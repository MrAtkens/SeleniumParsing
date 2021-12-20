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
using Models.Enums;

namespace Services.Business
{
    public class WMailService
    {
        private readonly ITaskProvider _taskProvider;
        private static SiteConfiguration _siteConfiguration;
        public WMailService(ITaskProvider taskProvider)
        {
            _taskProvider = taskProvider;
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.CoreConfigurations.json")
                .Build();

            var configurationSection = config.GetSection("Sites").GetSection("WMail");
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
            var driver = WMailServiceHelper.Authorize(_siteConfiguration);
            //Navigate to footer of page count to get all in one time without get ban
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var navigationEnd = wait.Until(e => e.FindElement(By.Id("navi-end")));
            var href = navigationEnd.GetAttribute("href");
            var pageCountSplit = href.Split("#page-");
            var pageCount = int.Parse(pageCountSplit[1]);
            await WMailServiceHelper.StartParseTasks(driver, pageCount, _siteConfiguration, _taskProvider, false, Status.Available);
            driver.Quit();
        }
        public async Task ParseOnlyExtensions()
        {
            var driver = WMailServiceHelper.Authorize(_siteConfiguration);
            var tasks = await _taskProvider.GetAllExtensionsNull();
            await WMailServiceHelper.ParseTasksExtensions(tasks, driver, _siteConfiguration, _taskProvider);
            driver.Quit();
        }

        public async Task ParseNew()
        {
            var driver = WMailServiceHelper.Authorize(_siteConfiguration);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //Get new tasks
            var newTasksButton = wait.Until(e => e.FindElement(By.CssSelector(
                "#contentwrapper > div:nth-child(6) > a:nth-child(2)")));
            newTasksButton.Click();
            await WMailServiceHelper.StartParseTasks(driver, 20, _siteConfiguration, _taskProvider, true, Status.Available);
            var newTasks = await _taskProvider.GetAllNew();
            await WMailServiceHelper.ParseTasksExtensions(newTasks, driver, _siteConfiguration, _taskProvider);
            driver.Quit();
        }
        

        
        
        // Common operations
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
