using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Providers.Abstract;
using DataAccess.Providers.Abstract.Base;
using DTOs;
using Models.Enums;
using Models.System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Helpers.ServiceHelpers
{
    public static class AvisoServiceHelper
    {
        
        public static List<TaskStatusUpdateDTO> StatusUpdateDtos = new List<TaskStatusUpdateDTO>()
        {
            new TaskStatusUpdateDTO("#contentwrapper > div:nth-child(7) > a:nth-child(3)", Status.InWork),
            new TaskStatusUpdateDTO("#contentwrapper > div:nth-child(7) > a:nth-child(4)", Status.InCheck),
            new TaskStatusUpdateDTO("#contentwrapper > div:nth-child(7) > a:nth-child(5)", Status.Paid),
            new TaskStatusUpdateDTO("#contentwrapper > div:nth-child(7) > a:nth-child(6)", Status.Rejected)
        };
        public static async Task StartParseTasks(IWebDriver driver, int count, SiteConfiguration siteConfiguration, ITaskProvider taskProvider, 
            bool isNew, Status status)
        {
            //Iteration of page for parse all pages start from 1 because we start from first page
            for (var i = 1; i < count; i++)
            {
                var waitTask = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
                driver.Navigate().GoToUrl(siteConfiguration.TasksUrl + i);
                var parsedTasks = waitTask.Until(e => e.FindElement(By.Id("work-task")));
                IList<IWebElement> elements = parsedTasks.FindElements(By.TagName("tr"));
                //Parse tasks on one page and add range(30-35 tasks)
                var baseTasks = await ParseTasks(elements, taskProvider, siteConfiguration.SiteId, isNew, status);
                await taskProvider.AddRange(baseTasks);
            }
        }
        
        public static async Task EditTaskStatus(IWebDriver driver, SiteConfiguration siteConfiguration, ITaskProvider taskProvider, Status status)
        {
            try
            {
                var waitTask = new WebDriverWait(driver, TimeSpan.FromSeconds(7));
                var parsedTasks = waitTask.Until(e => e.FindElement(By.Id("work-task")));
                IList<IWebElement> elements = parsedTasks.FindElements(By.TagName("tr"));
                //Update only task status
                var baseTasks = await UpdateTaskStatus(elements, taskProvider, siteConfiguration.SiteId, status);
                await taskProvider.EditRange(baseTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        public static async Task ParseTasksExtensions(IEnumerable<SimpleTask> tasks, IWebDriver driver, SiteConfiguration siteConfiguration, ITaskProvider taskProvider)
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
                  task.isNew = false;
                  editedTasks.Add(task);
                  if (editedTasks.Count != 30) continue;
                  await taskProvider.EditRange(editedTasks);
                  editedTasks = new List<SimpleTask>();
                }
                catch (Exception ex)
                {
                    task.Status = Status.Rejected;
                    editedTasks.Add(task);
                }
            }

        }
        
        public static IWebDriver Authorize(SiteConfiguration siteConfiguration)
        {
            IWebDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("username")).SendKeys(siteConfiguration.Username);
            driver.FindElement(By.Name("password")).SendKeys(siteConfiguration.Password);
            driver.FindElement(By.Id("button-login")).Click();
            return driver;
        }
        
         private static async Task<List<SimpleTask>> UpdateTaskStatus(IEnumerable<IWebElement> elements, ITaskProvider taskProvider, int siteId, Status status)
        {
            var tasks = new List<SimpleTask>();
            foreach (var e in elements)
            {
                try
                {
                    var elementId = e.GetAttribute("id");
                    var id = int.Parse(elementId.Split("block-task").Last());
                    if (!await taskProvider.CheckByTaskId(id, siteId))
                    {
                        var task = await taskProvider.GetByTaskId(id, siteId);
                        task.Status = status;
                        //Add to list
                        tasks.Add(task);
                    }
                    else 
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
                GC.Collect();
            }
            return tasks;
        }
        
        //Helper private function to parsing aviso tasks
        private static async Task<List<SimpleTask>> ParseTasks(IEnumerable<IWebElement> elements, ITaskProvider taskProvider, int siteId, bool isNew, Status status)
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
                    if (!await taskProvider.CheckByTaskId(task.TaskId, siteId))
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
                        task.Status = status;
                        task.SiteId = siteId;
                        task.isNew = isNew;
                        //Add to list
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
                GC.Collect();
            }
            return tasks;
        }

    }
}