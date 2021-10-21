using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models.System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Business;

namespace TaskerTasks.Controllers
{
    [ApiController]
    [Route("api/aviso/[action]")]
    public class AvisoController : ControllerBase
    {
        private readonly AvisoService _avisoService;
        private SiteConfiguration siteConfiguration;
       public AvisoController(AvisoService avisoService)
       {
            _avisoService = avisoService;
            IConfiguration config = new ConfigurationBuilder()
                       .AddJsonFile("appsettings.CoreConfigurations.json")
                       .Build();

            IConfigurationSection configurationSection = config.GetSection("Sites").GetSection("Aviso");
            siteConfiguration = new SiteConfiguration()
            {
                Url = configurationSection.GetSection("Url").Value,
                AuthUrl = configurationSection.GetSection("AuthUrl").Value,
                Username = configurationSection.GetSection("Username").Value,
                Password = configurationSection.GetSection("Password").Value
            };
        }

        [HttpGet]
        public async Task InitialParse()
        {
            await _avisoService.ParseAllTasks(siteConfiguration);
            await _avisoService.ParseOnlyExtensions(siteConfiguration);
        }

        [HttpGet]
        public async Task ParseExtensions()
        {
            await _avisoService.ParseOnlyExtensions(siteConfiguration);
        }

        [HttpGet]
        public async Task<List<BaseTask>> Get()
        {
            return await _avisoService.GetAllTasks();
        }

        [HttpGet]
        public async Task<int> GetCount()
        {
            return await _avisoService.GetCount();
        }
    }
}