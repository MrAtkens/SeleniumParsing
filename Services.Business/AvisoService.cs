using DataAccess.Providers.Abstract;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using Models.System;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Services.Business
{
    public class AvisoService
    {
        private readonly ITaskProvider _taskProvider;
        private readonly IWorkingTaskProvider _workingTaskProvider;
        private static SiteConfiguration _siteConfiguration;
        private const int SiteId = 3;

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
                Password = configurationSection.GetSection("Password").Value
            };
        }

        public async System.Threading.Tasks.Task ParseAllTasks()
        {
            //Initialize webdriver and authorize 
            var driver = Authorize();
            //Navigate to footer of page count to get all in one time without get ban
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(8);
            var navigationEnd = driver.FindElement(By.Id("navi-end"));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            var pageCount = int.Parse(pageCountSplit[1]);
            //Iteration of page for parse all pages start from 1 because we start from first page
            for (var i = 1; i < pageCount; i++)
            {
                var waitTask = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
                driver.Navigate().GoToUrl(_siteConfiguration.TasksUrl + i);
                var parsedTasks = waitTask.Until(e => e.FindElement(By.Id("work-task")));
                IList<IWebElement> elements = parsedTasks.FindElements(By.TagName("tr"));
                //Parse tasks on one page and add range(30-35 tasks)
                var baseTasks = await ParseTasks(elements);
                await _taskProvider.AddRange(baseTasks);
            }
            
            driver.Quit();
        }

        public async Task<string> StartTask(int id)
        {
         
                var driver = Authorize();
                var task = await _taskProvider.GetByTaskId(id, SiteId);
                driver.Navigate().GoToUrl(_siteConfiguration.TaskUrl + task.TaskId);
                var button = driver.FindElement(By.Name("submit"));
                button.Click();
                await _workingTaskProvider.Add(new WorkingTask(task));
                return task.Description;
        }
        
        public async Task CompleteTask(int id, string answer)
        {
            var driver = Authorize();
            var task = await _workingTaskProvider.GetByTaskId(id, SiteId);
            driver.Navigate().GoToUrl(_siteConfiguration.TaskUrl + task.TaskId);
            driver.FindElement(By.Name("ask_reply")).SendKeys(answer);
            try
            {
                //Loading file if need
                driver.FindElement(By.Id("file-load-patch")).SendKeys(answer);
            }
            catch (Exception ex)
            {
                // ignored
            }

            var button = driver.FindElement(By.Name("submit"));
            button.Click();
            await _workingTaskProvider.Remove(task);
        }

        public async Task ParseOnlyExtensions()
        {
            var driver = Authorize();
            var tasks = await _taskProvider.GetAllExtensionsNull();
            await ParseTasksExtensions(tasks, driver, _siteConfiguration);
            driver.Quit();
        }

        public async Task<int> GetCount()
        {
            return await _taskProvider.GetCountSiteId(SiteId);
        }

        public async Task<List<SimpleTask>> GetAllTasks()
        {
            return await _taskProvider.GetAllBySiteId(SiteId);
        }

        private static WebDriver Authorize()
        {
            WebDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(_siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("username")).SendKeys(_siteConfiguration.Username);
            driver.FindElement(By.Name("password")).SendKeys(_siteConfiguration.Password + Keys.Enter);
            return driver;
        }
        
        private async Task<List<SimpleTask>> ParseTasks(IEnumerable<IWebElement> elements)
        {
            var tasks = new List<SimpleTask>();
            var count = 0;
            foreach (var e in elements)
            {
                var task = new SimpleTask();
                try
                {
                    var lines = e.Text.Split("\r\n");
                    var elementId = e.GetAttribute("id");
                    task.TaskId = int.Parse(elementId.Split("block-task").Last());
                    if (!await _taskProvider.CheckByTaskId(task.TaskId, SiteId))
                    {
                        //Title
                        task.Title = lines[0];
                        task.Url = e.FindElement(By.ClassName("task-url")).Text;
                        IWebElement idElement;
                        try
                        {
                            //If it's first 3 element in page
                            idElement = e.FindElement(By.ClassName("serfinfotext"));
                        }
                        catch (Exception ex)
                        {
                            //It's all common tasks
                            idElement = e.FindElement(By.CssSelector("td:nth-child(2)"))
                                .FindElement(By.TagName("span"));
                        }

                        var taskLine = idElement.Text.Split(" ");
                        //Get TaskType
                        for (var i = 0; i < taskLine.Length; i++)
                        {
                            if (taskLine[i] == "[")
                                break;
                            if (i > 2)
                                task.TaskType += taskLine[i] + " ";
                        }

                        //Author Name with Author Id
                        var authorElement = e.FindElement(By.TagName("i")).FindElement(By.TagName("a"));
                        var authorId = authorElement.GetAttribute("href");
                        task.AuthorName = authorElement.Text;
                        task.AuthorId = int.Parse(authorId.Split("/wall?uid=")[1]);
                        var priceElementText = e.FindElement(By.CssSelector("td:nth-child(3)"))
                            .FindElement(By.TagName("span")).Text;
                        var priceLine = priceElementText.Split(" ");
                        task.Price = float.Parse(priceLine[0]) + float.Parse(priceLine[2]) / 100;
                        task.Status = true;
                        task.SiteId = SiteId;
                        //Add to list
                        tasks.Add(task);
                        task.Dispose();
                    }
                    else 
                        continue;
                }
                catch (Exception ex)
                {
                    count++;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(count);
                    Console.WriteLine(task.TaskId);
                    continue;
                }
                GC.Collect();
            }
            return tasks;
        }

        private async System.Threading.Tasks.Task ParseTasksExtensions(IEnumerable<SimpleTask> tasks, IWebDriver driver, SiteConfiguration siteConfiguration)
        {
            var editedTasks = new List<SimpleTask>();
            foreach (var task in tasks)
            {
                try
                {
                  driver.Navigate().GoToUrl(siteConfiguration.TaskUrl + task.TaskId);
                  //Get elements(Url, CompleteTime, WorkCount, FullDescription, Created date)
                  try
                  {
                      var taskCompleteTimeElement = driver.FindElement(By.CssSelector("#contentwrapper > div:nth-child(14) > b > font"));
                      task.WorkTime = DateHelper.StringDateToInt(taskCompleteTimeElement.Text);
                  }
                  catch (Exception ex)
                  {
                      task.WorkTime = 30;
                  }

                  var descriptionElement = driver.FindElement(By.CssSelector("#contentwrapper > div:nth-child(9)"));
                  var descriptionCompleteElement = driver.FindElement(By.CssSelector("#contentwrapper > div:nth-child(11)"));
                  var workCountElement = driver.FindElement(By.CssSelector(
                      "#contentwrapper > table:nth-child(7) > tbody > tr:nth-child(3) > td:nth-child(1) > font"));
                  var creationDateElement = driver.FindElement(By.CssSelector("#contentwrapper > div:nth-child(6) > span"));
                  var timeLines = creationDateElement.Text.Split(" ");
                  //Write elements
                  task.Description = descriptionElement.Text + descriptionCompleteElement.Text;
                  task.CreationDate = DateTime.Parse(timeLines[1] + " " + timeLines[3]);
                  task.WorkCount = workCountElement.Text;
                  task.CheckTime = DateHelper.CheckTime;
                  
                  editedTasks.Add(task);
                  if (editedTasks.Count == 30)
                  {
                      await _taskProvider.EditRange(editedTasks);
                      editedTasks = new List<SimpleTask>();
                  }
                }
                catch (Exception ex)
                {
                    task.Status = false;
                    editedTasks.Add(task);
                    continue;
                }
            }

        }
    }
}
