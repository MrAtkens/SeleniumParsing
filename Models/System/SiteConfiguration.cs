using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.System
{
    public class SiteConfiguration
    {
        public string Url { get; set; }
        public string AuthUrl { get; set; }
        public string TasksUrl { get; set; }
        public string TaskUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int SiteId { get; set; }
    }
}
