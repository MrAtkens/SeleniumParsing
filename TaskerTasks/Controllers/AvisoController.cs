using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Models.System;
using Services.Business;

namespace TaskerTasks.Controllers
{
    [ApiController]
    [Route("api/aviso/[action]")]
    public class AvisoController : ControllerBase
    {
        private readonly AvisoService _avisoService;
       public AvisoController(AvisoService avisoService)
       {
            _avisoService = avisoService;
       }

        [HttpGet]
        public async Task InitialParse()
        {
            await _avisoService.ParseAllTasks();
            await _avisoService.ParseOnlyExtensions();
        }

        [HttpGet]
        public async Task ParseExtensions()
        {
            await _avisoService.ParseOnlyExtensions();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> StartTask(int id)
        {
            var answer = await _avisoService.StartTask(id);
            return Ok(answer);
        }
        
        [HttpPost]
        public async Task<IActionResult> CompleteTask(CompleteTaskDTO completeTaskDto)
        {
            var answer = await _avisoService.CompleteTask(completeTaskDto.Id, completeTaskDto.Answer);
            return Ok(answer);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> CancelTask(int id)
        {
            try
            {
                var answer = await _avisoService.CancelTask(id);
                return Ok(answer);
            }
            catch (Exception ex)
            {
                return Forbid();
            }
        }

        [HttpGet]
        public async Task<List<SimpleTask>> Get()
        {
            return await _avisoService.GetAllTasks();
        }

        [HttpGet]
        public async Task<int> GetCount()
        {
            return await _avisoService.GetCount();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> RemoveTask(int id)
        {
            await _avisoService.RemoveTask(id);
            return Ok();
        }
    }
}