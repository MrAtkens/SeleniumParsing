using DataAccess.Providers.Abstract;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Models.System;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using Helpers;
using Helpers.ServiceHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Services.Business
{
    public class AvisoService
    {
        private readonly ITaskProvider _taskProvider;
        private readonly IWorkingTaskProvider _workingTaskProvider;
        private static SiteConfiguration _siteConfiguration;
        public AvisoService(ITaskProvider taskProvider, IWorkingTaskProvider workingTaskProvider)
        {
            _taskProvider = taskProvider;
            _workingTaskProvider = workingTaskProvider;
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
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var navigationEnd = wait.Until(e => e.FindElement(By.Id("navi-end")));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            await AvisoServiceHelper.StartParseTasks(driver, pageCountSplit, _siteConfiguration, _taskProvider, false);
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
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Get new tasks
            var newTasksButton = wait.Until(e => e.FindElement(By.CssSelector(
                "#contentwrapper > div:nth-child(6) > a:nth-child(2)")));
            newTasksButton.Click();
            var navigationEnd = driver.FindElement(By.Id("navi-end"));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            await AvisoServiceHelper.StartParseTasks(driver, pageCountSplit, _siteConfiguration, _taskProvider, true);
            var newTasks = await _taskProvider.GetAllNew();
            await AvisoServiceHelper.ParseTasksExtensions(newTasks, driver, _siteConfiguration, _taskProvider);
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
            await _workingTaskProvider.Add(new WorkingTask(task));
            button.Click();
            driver.Quit();
            return task.Description + "\n" + task.Url;
        }
        
        public async Task<string> CompleteTask(int id, string answer, List<IFormFile> files )
        {
            if (!await _workingTaskProvider.CheckByTaskId(id, _siteConfiguration.SiteId))
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
            var task = await _workingTaskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            await _workingTaskProvider.Remove(task);
            button.Click();
            driver.Quit();
            return "Отчёт об выполнение задания был отправлен автору, пожалуйста ожидайте ответа";
        }

        public async Task<string> CancelTask(int id)
        {
            if (!await _workingTaskProvider.CheckByTaskId(id, _siteConfiguration.SiteId))
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
            var task = await _workingTaskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            await _workingTaskProvider.Remove(task);
            driver.Quit();
            return "Данное задание отменено";
        }

        public async Task RemoveTask(int id)
        {
            var task = await _workingTaskProvider.GetByTaskId(id, _siteConfiguration.SiteId);
            await _workingTaskProvider.Remove(task);
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
    }
}
