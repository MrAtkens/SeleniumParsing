using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models.System;
using Services.Business;

namespace TaskerTasks.Controllers
{
    [ApiController]
    [Route("api/advigo/[action]")]
    public class AdvigoController
    {
        private readonly AdvigoService _advigoService;
        public AdvigoController(AdvigoService advigoService)
        {
            _advigoService = advigoService;
        }
        
        
        [HttpGet]
        public async Task InitialParse()
        {
            await _advigoService.ParseAllTasks();
        }
        
        [HttpGet]
        public async Task<List<SimpleTask>> Get()
        {
            return await _advigoService.GetAllTasks();
        }

        [HttpGet]
        public async Task<int> GetCount()
        {
            return await _advigoService.GetCount();
        }

        [HttpGet("Url")]
        public async Task BuyAvia([FromQuery] string url)
        {
            await _advigoService.Buy(url);
        }

    }
}