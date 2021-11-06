using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DTOs
{
    public class CompleteTaskDTO
    {
        public int Id { get; set; }
        public string Answer { get; set; }
    }
}