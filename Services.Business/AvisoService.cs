using DataAccess.Providers.Abstract;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using Models.Models;
using Models.System;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;

namespace Services.Business
{
    public class AvisoService
    {
        private readonly IBaseTaskProvider _taskProvider;

        public AvisoService(IBaseTaskProvider taskProvider)
        {
            _taskProvider = taskProvider;
        }

        public async Task ParseTaskFromAuthorization(SiteConfiguration siteConfiguration)
        {
            //Initialize webdriver
            WebDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("username")).SendKeys(siteConfiguration.Username);
            driver.FindElement(By.Name("password")).SendKeys(siteConfiguration.Password + Keys.Enter);

            //Navigate to footer of page count to get all in one time without get ban
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var navigationEnd = wait.Until(e => e.FindElement(By.Id("navi-end")));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            var pageCount = int.Parse(pageCountSplit[1]);
            //Iteration of page for parse all pages start from 1 because we start from first page
            for (var i = 1; i < pageCount; i++)
            {
                driver.Navigate().GoToUrl("https://aviso.bz/work-task?plagin=" + i);
                IWebElement parsedTasks = wait.Until(e => e.FindElement(By.Id("work-task")));
                IList<IWebElement> elements = parsedTasks.FindElements(By.TagName("tr"));
                await _taskProvider.AddRange(ParseTasks(elements));
            }
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

        private List<BaseTask> ParseTasks(IList<IWebElement> elements)
        {
            int count = 0;
            var tasks = new List<BaseTask>();
            foreach (IWebElement e in elements)
            {
                var task = new BaseTask();
                var lines = e.Text.Split("\r\n");
                try
                {
                    //Title, url
                    task.Title = lines[0];
                    task.Url = lines[1];
                    var elementId = e.GetAttribute("id").Split("block-task")[1];
                    task.TaskId = int.Parse(elementId);
                    IWebElement idElement;
                    try
                    {
                        //If it's first 3 element in page
                        idElement = e.FindElement(By.ClassName("serfinfotext"));
                    }
                    catch (Exception ex)
                    {
                        //It's all common tasks
                        idElement = e.FindElement(By.CssSelector("td:nth-child(2)")).FindElement(By.TagName("span"));
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
                    var authorElement = idElement.FindElement(By.TagName("i")).FindElement(By.TagName("a"));
                    var authorName = authorElement.Text;
                    var authorId = authorElement.GetAttribute("href").Split("/wall?uid=");
                    task.AuthorName = authorName;
                    task.AuthorId = int.Parse(authorId[1]);
                    Console.WriteLine(e.TagName);
                    var priceElement = e.FindElement(By.CssSelector("td:nth-child(3)")).FindElement(By.TagName("span"));
                    task.Price = priceElement.Text;

                    //Add to list and finalize task
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    count++;
                    Console.WriteLine(count);
                    continue;
                }
            }
            return tasks;
        }
    }
}
