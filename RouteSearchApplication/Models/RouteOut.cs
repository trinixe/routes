using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteSearchApplication.Models
{
    public class RouteOut
    {
        public bool Success { get; set; }
        public string Route { get; set; }
        public string Error { get; set; }
    }
}
