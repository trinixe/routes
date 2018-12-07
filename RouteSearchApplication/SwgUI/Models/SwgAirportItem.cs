using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteSearchApplication.SwgUI.Models
{
    public class SwgAirportItem
    {
        public string Name { get; set; }
        public bool Explored { get; set; }
        public BlockingCollection<SwgAirportItem> Outgoing { get; set; }
        public BlockingCollection<SwgAirportItem> Incoming { get; set; }
        public SwgAirportItem()
        {
            Outgoing = new BlockingCollection<SwgAirportItem>();
            Incoming = new BlockingCollection<SwgAirportItem>();
        }
        public SwgAirportItem(string airport) : this()
        {
            Name = airport;
        }
    }
}
