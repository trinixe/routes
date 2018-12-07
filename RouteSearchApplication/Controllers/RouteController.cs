using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RouteSearchApplication.Models;
using RouteSearchApplication.SwgUI;
using RouteSearchApplication.SwgUI.Models;

namespace RouteSearchApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "";
            //return RoutePa
        }
        // POST api/route
        [HttpPost]
        public RouteOut Post([FromBody] RouteIn route)
        {
            RouteOut Route = new RouteOut();
            using (var search = new SwgSearch(route.srcAirport, route.destAirport))
            {
                search.Execute();
                Route.Success = search.Success;
                Route.Error = search.Message;
                Route.Route = search.Route;
            }
            return Route;
        }
    }
}