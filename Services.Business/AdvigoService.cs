using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccess.Providers.Abstract;
using Helpers;
using Microsoft.Extensions.Configuration;
using Models.Enums;
using Models.System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace Services.Business
{
    public class AdvigoService
    {
        private readonly ITaskProvider _taskProvider;
        private static SiteConfiguration _siteConfiguration;
        public AdvigoService(ITaskProvider taskProvider)
        {
            _taskProvider = taskProvider;
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.CoreConfigurations.json")
                .Build();

            IConfigurationSection configurationSection = config.GetSection("Sites").GetSection("Advigo");
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
        
        public async Task<int> GetCount()
        {
            return await _taskProvider.GetCountSiteId(_siteConfiguration.SiteId);
        }

        public async Task<List<SimpleTask>> GetAllTasks()
        {
            return await _taskProvider.GetAllBySiteId(_siteConfiguration.SiteId);
        }
        
        public async Task ParseAllTasks()
        {
            //Initialize webdriver and authorize 
            var driver = Authorize(_siteConfiguration);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(8);

            var navigationTasks = driver.FindElement(By.CssSelector("#submenu_standart > div > div:nth-child(1) > div > a"));
            navigationTasks.Click();
            //Navigate to footer of page count to get count of pages
            var navigationEnd = driver.FindElement(By.CssSelector("#top > div.middle > div.ie-zaebal > div > div.inmain > div.pages > a:nth-child(12)"));
            var pageCount = int.Parse(navigationEnd.Text);
            //Iteration of page for parse all pages start from 1 because we start from first page
            var refreshCounter = 0;
            for (var i = 1; i < pageCount; i++)
            {
                if (refreshCounter == 11)
                {
                    refreshCounter = 0;
                    LogOutAndAuthorize(_siteConfiguration, driver);
                    
                }
                driver.Navigate().GoToUrl(_siteConfiguration.TasksUrl + i);
                var parsedTasks = driver.FindElement(By.ClassName("list"));
                IList<IWebElement> elements = parsedTasks.FindElements(By.ClassName("list_item"));
                //Parse tasks on one page and add range(10 tasks)
                var baseTasks = await ParseTasks(elements);
                await _taskProvider.AddRange(baseTasks);  

                refreshCounter++;
            }
            
            driver.Quit();
        }
        
          private async Task<List<SimpleTask>> ParseTasks(IEnumerable<IWebElement> elements)
        {
            var tasks = new List<SimpleTask>();
            var count = 0;
            foreach (var e in elements)
            {
                var task = new SimpleTask();
                var divList = e.FindElement(By.TagName("div"));
                try
                {
                    var taskIdElement = divList.GetAttribute("id").Split("job_");
                    task.TaskId = int.Parse(taskIdElement[1]);
                    if (!await _taskProvider.CheckByTaskId(task.TaskId, _siteConfiguration.SiteId))
                    {
                        task.Title = divList.FindElement(By.ClassName("order-title")).Text;
                        var headerElement = divList.FindElement(By.CssSelector("div:nth-child(5) > table > tbody > tr"));
                        var priceElement = headerElement.FindElement(By.TagName("td")).FindElement(By.TagName("div"))
                            .GetAttribute("data-price").Split(" ");
                        IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
                        task.Price = double.Parse(priceElement[0], formatter);
                        task.Url = "https://advego.com/job/order/" + task.TaskId;
                        var authorImage = headerElement.FindElement(By.CssSelector("td.pictable_text > div > a > img"));
                        task.AuthorName = authorImage.GetAttribute("alt");
                        var bodyElement = divList.FindElement(By.ClassName("job_header"));
                        var descriptionElement = bodyElement.FindElement(By.ClassName("job_desc"));
                        var creationDateElement = bodyElement.FindElement(By.ClassName("over-title-panel")).FindElement(By.TagName("span"));
                        task.CreationDate = DateTime.Parse(creationDateElement.GetAttribute("data-last-modify-date"));
                        try
                        {
                            var showAllButton = descriptionElement.FindElement(By.TagName("a"));
                            showAllButton.Click();
                        }
                        catch (Exception exception)
                        {
                            // ignored
                        }

                        task.Description = descriptionElement.Text;
                        var showParameter = divList.FindElement(By.CssSelector($"#params-{task.TaskId} > ul > li:nth-child(2)"));
                        var showParameterA = showParameter.FindElement(By.TagName("a"));
                        showParameterA.Click();
                        
                        var taskTypeElement = divList.FindElement(By.ClassName("order-status-panel"));
                        var table = e.FindElement(By.ClassName("params")).FindElement(By.TagName("tbody"));
                        var lines = table.FindElements(By.TagName("tr"));
                        task.TaskType = taskTypeElement.FindElement(By.ClassName("cat-tags")).Text;
                
                        foreach (var line in lines)
                        {
                            if (line.Text.Contains("Настройки модерации:"))
                                task.WorkCount = line.FindElements(By.TagName("td"))[1].Text;
                            if (line.Text.Contains("Время на выполнение работы:"))
                            {
                                var workTime = line.Text.Split(" ");
                                var workTimeDate = workTime[4].Split(".");
                                task.WorkTime = int.Parse(workTimeDate[0]) * DateHelper.OneHour;
                            }

                            if(line.Text.Contains("Время на проверку работы:"))
                                task.CheckTime = int.Parse(line.Text.Split(" ")[4].Split(".")[0]) * DateHelper.OneHour;
                        }

                        task.Status = Status.Available;
                        task.SiteId = _siteConfiguration.SiteId;
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
                    Console.WriteLine(count);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(task.TaskId);
                    Console.WriteLine(divList.Text);
                    Console.WriteLine(divList.GetAttribute("id"));
                    continue;
                }
                GC.Collect();
            }
            return tasks;
        }

        private static WebDriver Authorize(SiteConfiguration siteConfiguration)
        {

            WebDriver driver = new ChromeDriver(AppDomain.CurrentDomain.BaseDirectory);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("login")).SendKeys(siteConfiguration.Username);
            driver.FindElement(By.Name("pwd")).SendKeys(siteConfiguration.Password + Keys.Enter);
            return driver;
        }

        private static void LogOutAndAuthorize(SiteConfiguration siteConfiguration, IWebDriver driver)
        {
            driver.Navigate().GoToUrl(siteConfiguration.Url + "logout");
            driver.Navigate().GoToUrl(siteConfiguration.AuthUrl);
            //Authorization
            driver.FindElement(By.Name("login")).SendKeys(siteConfiguration.Username);
            driver.FindElement(By.Name("pwd")).SendKeys(siteConfiguration.Password + Keys.Enter);
            var navigationTasks = driver.FindElement(By.CssSelector("#submenu_standart > div > div:nth-child(1) > div > a"));
            navigationTasks.Click();
        }

    }
}