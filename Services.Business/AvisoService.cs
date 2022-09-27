using DataAccess.Providers.Abstract;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using Models.System;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using Helpers;
using Helpers.ServiceHelpers;
using Microsoft.Extensions.Configuration;
using Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Services.Business
{
    public class AvisoService
    {
        private readonly ITaskProvider _taskProvider;
        private static SiteConfiguration _siteConfiguration;
        public AvisoService(ITaskProvider taskProvider)
        {
            _taskProvider = taskProvider;
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.CoreConfigurations.json")
                .Build();

            var configurationSection = config.GetSection("Sites").GetSection("Aviso");
            _siteConfiguration = new SiteConfiguration()
            {
                Url = configurationSection.GetSection("Url").Value,
                AuthUrl = configurationSection.GetSection("AuthUrl").Value,
                TasksUrl = configurationSection.GetSection("TasksUrl").Value,
                TaskUrl = configurationSection.GetSection("TaskUrl").Value,
                Username = configurationSection.GetSection("Username").Value,
                Password = configurationSection.GetSection("Password").Value,
                SiteId = int.Parse(configurationSection.GetSection("SiteId").Value)
            };
        }

        //Tasks parsing
        public async Task ParseAllTasks()
        {
            //Initialize webdriver and authorize 
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            //Navigate to footer of page count to get all in one time without get ban
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var navigationEnd = wait.Until(e => e.FindElement(By.Id("navi-end")));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            var pageCount = int.Parse(pageCountSplit[1]);
            await AvisoServiceHelper.StartParseTasks(driver, pageCount, _siteConfiguration, _taskProvider, false, Status.Available);
            driver.Quit();
        }
        public async Task ParseOnlyExtensions()
        {
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            var tasks = await _taskProvider.GetAllExtensionsNull();
            await AvisoServiceHelper.ParseTasksExtensions(tasks, driver, _siteConfiguration, _taskProvider);
            driver.Quit();
        }

        public async Task ParseNew()
        {
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            DefaultWait<IWebDriver> fluentWait = new DefaultWait<IWebDriver>(driver);
            fluentWait.Timeout = TimeSpan.FromSeconds(5);
            fluentWait.PollingInterval = TimeSpan.FromMilliseconds(250);
            /* Ignore the exception - NoSuchElementException that indicates that the element is not present */
            fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            fluentWait.Message = "Element to be searched not found";

            //Get new tasks
            var newTasksButton = fluentWait.Until(e => e.FindElement(By.CssSelector(
                "#contentwrapper > div:nth-child(6) > a:nth-child(2)")));
            newTasksButton.Click();
            await AvisoServiceHelper.StartParseTasks(driver, 20, _siteConfiguration, _taskProvider, true, Status.Available);
            var newTasks = await _taskProvider.GetAllNew();
            await AvisoServiceHelper.ParseTasksExtensions(newTasks, driver, _siteConfiguration, _taskProvider);
            driver.Quit();
        }
        
        public async Task UpdateStatus()
        {
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Get new tasks
            foreach (var taskStatusUpdateDto in AvisoServiceHelper.StatusUpdateDtos)
            {
                var inWork = wait.Until(e => e.FindElement(By.CssSelector(taskStatusUpdateDto.ParsingQuery)));
                inWork.Click();
                await AvisoServiceHelper.EditTaskStatus(driver, _siteConfiguration, _taskProvider, taskStatusUpdateDto.TaskStatus);   
            }
            driver.Quit();
        }

        //Tasks operations like subscribe, unsubscribe, complete
        public async Task<string> StartTask(int id)
        {
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            var task = await _taskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            if (task == null)
                return "К сожалению данное задание не было найдено";
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            var navigationEnd = driver.FindElement(By.Id("navi-end"));
            driver.Navigate().GoToUrl(_siteConfiguration.TaskUrl + id);
            var button = driver.FindElement(By.Name("goform")).FindElement(By.TagName("button"));
            task.Status = Status.InWork;
            await _taskProvider.Edit(task);
            button.Click();
            driver.Quit();
            return task.Description + "\n" + task.Url;
        }
        
        public async Task<string> CompleteTask(int id, string answer, List<IFormFile> files )
        {
            if (!await _taskProvider.CheckByTaskId(id, _siteConfiguration.SiteId))
            {
                return "Данное задание не выполняется";
            }
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            var navigationEnd = driver.FindElement(By.Id("navi-end"));
            driver.Navigate().GoToUrl(_siteConfiguration.TaskUrl + id);
            if (answer != null)
            {
                var input = driver.FindElement(By.Name("ask_reply"));
                input.SendKeys(answer);
            }
            if (files.Count != 0)
            {
                try
                {
                    var fileUploadElement = driver.FindElement(By.Id("file-load-patch"));
                    foreach (var file in files)
                    {
                        string fileName = Path.GetRandomFileName();
                        fileName = Path.ChangeExtension(fileName, FileHelper.GetExtension(file.ContentType));
                        
                        var filePath = Path.Combine(Path.GetTempPath(), fileName);
                        await using (var stream = File.Create(filePath))
                        {
                            // The formFile is the method parameter which type is IFormFile
                            // Saves the files to the local file system using a file name generated by the app.
                            await file.CopyToAsync(stream);
                        }
                        fileUploadElement.SendKeys(filePath);
                    }
                    //Loading file if need
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }

            var button = driver.FindElement(By.ClassName("button_theme_blue"));
            var task = await _taskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            task.Status = Status.InCheck;
            await _taskProvider.Edit(task);
            button.Click();
            driver.Quit();
            return "Отчёт об выполнение задания был отправлен автору, пожалуйста ожидайте ответа";
        }

        public async Task<string> CancelTask(int id)
        {
            if (!await _taskProvider.CheckByTaskId(id, _siteConfiguration.SiteId))
            {
                return "Данное задание не выполняется";
            }
            var driver = AvisoServiceHelper.Authorize(_siteConfiguration);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
            var navigationEnd = driver.FindElement(By.Id("navi-end"));
            driver.Navigate().GoToUrl(_siteConfiguration.TaskUrl + id);
            var button = driver.FindElement(By.ClassName("button_theme_red"));
            button.Click();
            var acceptButton = driver.FindElement(By.Id("js-popup")).FindElement(By.TagName("form")).FindElement(By.TagName("div")).FindElement(By.TagName("button"));
            acceptButton.Click();
            var task = await _taskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            task.Status = Status.Available;
            await _taskProvider.Edit(task);
            driver.Quit();
            return "Данное задание отменено";
        }
        public async Task RemoveTask(int id)
        {
            var task = await _taskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            await _taskProvider.Remove(task);
        }
        
        //Get Tasks for watch results of parsing
        public async Task<int> GetCount()
        {
            return await _taskProvider.GetCountSiteId(_siteConfiguration.SiteId);
        }

        public async Task<List<SimpleTask>> GetAllTasks()
        {
            return await _taskProvider.GetAllBySiteId(_siteConfiguration.SiteId);
        }

        public async Task<SimpleTask> GetTaskByTaskId(int taskId)
        {
            return await _taskProvider.GetByTaskId(taskId, _siteConfiguration.SiteId);
        }
    }
}
