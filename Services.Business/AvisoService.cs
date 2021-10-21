using DataAccess.Providers.Abstract;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Models.System;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using Helpers;

namespace Services.Business
{
    public class AvisoService
    {
        private readonly IBaseTaskProvider _taskProvider;

        public AvisoService(IBaseTaskProvider taskProvider)
        {
            _taskProvider = taskProvider;
        }

        public async Task ParseAllTasks(SiteConfiguration siteConfiguration)
        {
            //Initialize webdriver and authorize 
            var driver = Authorize(siteConfiguration);
            //Navigate to footer of page count to get all in one time without get ban
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var navigationEnd = wait.Until(e => e.FindElement(By.Id("navi-end")));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            var pageCount = int.Parse(pageCountSplit[1]);
            //Iteration of page for parse all pages start from 1 because we start from first page
            for (var i = 1; i < pageCount; i++)
            {
                var waitTask = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
                driver.Navigate().GoToUrl("https://aviso.bz/work-task?plagin=" + i);
                var parsedTasks = waitTask.Until(e => e.FindElement(By.Id("work-task")));
                IList<IWebElement> elements = parsedTasks.FindElements(By.TagName("tr"));
                //Parse tasks on one page and add range(30-35 tasks)
                var baseTasks = await ParseTasks(elements);
                await _taskProvider.AddRange(baseTasks);
            }
            
            driver.Quit();
        }

        public async Task ParseOnlyExtensions(SiteConfiguration siteConfiguration)
        {
            var driver = Authorize(siteConfiguration);
            var tasks = await _taskProvider.GetAllExtensionsNull();
            await ParseTasksExtensions(tasks, driver);
            driver.Quit();
        }

        public async Task<int> GetCount()
        {
            return await _taskProvider.GetCount();
        }

        public async Task<List<BaseTask>> GetAllTasks()
        {
            return await _taskProvider.GetAll();
        }

        private static WebDriver Authorize(SiteConfiguration siteConfiguration)
        {
            WebDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("username")).SendKeys(siteConfiguration.Username);
            driver.FindElement(By.Name("password")).SendKeys(siteConfiguration.Password + Keys.Enter);
            return driver;
        }
        
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Char[]")]
        private async Task<List<BaseTask>> ParseTasks(IEnumerable<IWebElement> elements)
        {
            var tasks = new List<BaseTask>();
            int count = 0;
            foreach (var e in elements)
            {
                var task = new BaseTask();
                try
                {
                    var lines = e.Text.Split("\r\n");
                    var elementId = e.GetAttribute("id");
                    
                    task.TaskId = int.Parse(elementId.Split("block-task").Last());
                    if (!await _taskProvider.CheckByTaskId(task.TaskId))
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
                        var priceElement = e.FindElement(By.CssSelector("td:nth-child(3)"))
                            .FindElement(By.TagName("span"));
                        task.Price = priceElement.Text;
                        task.Status = true;
                        //Add to list and finalize task
                        tasks.Add(task);
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
            }
            return tasks;
        }

        private async Task ParseTasksExtensions(IEnumerable<BaseTask> tasks, IWebDriver driver)
        {
            var count = 0;
            var editedTasks = new List<BaseTask>();
            foreach (var task in tasks)
            {
                try
                {
                  driver.Navigate().GoToUrl("https://aviso.bz/work-task-read?adv=" + task.TaskId);
                  IWebElement taskCompleteTimeElement = null;
                  //Get elements(Url, CompleteTime, WorkCount, FullDescription, Created date)
                  try
                  {
                      taskCompleteTimeElement = driver.FindElement(By.CssSelector("#contentwrapper > div:nth-child(14) > b > font"));
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
                      editedTasks = new List<BaseTask>();
                  }
                }
                catch (Exception ex)
                {
                    count++;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(count);
                    Console.WriteLine(task.TaskId);
                    task.Status = false;
                    editedTasks.Add(task);
                    continue;
                }
            }

        }
    }
}
