using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RouteSearchApplication.SwgUI.Models;

namespace RouteSearchApplication.SwgUI
{
    public class SwgSearch : IDisposable
    {
        
        private bool Stop { get; set; }
        private string srcAirport { get; set; }
        private string destAirport { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Route { get; set; }
        private BlockingCollection<SwgAirline> Airlines;
        private BlockingCollection<SwgAirportItem> Airports;
        private Queue<string> _Route;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        CancellationToken token;

        public SwgSearch(string srcAirport, string destAirport)
        {
            this.srcAirport = srcAirport;
            this.destAirport = destAirport;
            Airlines = new BlockingCollection<SwgAirline>();
            Airports = new BlockingCollection<SwgAirportItem>();
            _Route = new Queue<string>();
            token = cancelTokenSource.Token;
        }

        private SwgAirline GetAirline(string alias)
        {
            SwgAirline airline;
            lock (Airlines)
            {
                airline = Airlines.FirstOrDefault(x => x.alias == alias);
                if (airline == null)
                {
                    var airlines = SwgCore<SwgAirline[]>.Get("Airline/" + alias);
                    if (airlines.Count() > 0)
                    {
                        airline = airlines[0];
                        Airlines.TryAdd(airline);
                    }
                    else
                    {
                        airline = new SwgAirline();
                        airline.alias = alias;
                        airline.active = false;
                    }
                }
            }
            return airline;
        }

        private SwgAirportItem GetAirportsItem(string airport)
        {
            var result = Airports.FirstOrDefault(x => x.Name == airport);
            if (result == null)
            {
                result = new SwgAirportItem(airport);
                Airports.Add(result);
            }
            return result;
        }

        private void ExploreAirport(SwgAirportItem frAirport)
        {
            try
            {
                frAirport.Explored = true;
                var routes = SwgCore<SwgRoute[]>.Get("Route/outgoing?airport=" + frAirport.Name);
                foreach (var route in routes)
                {
                    if (GetAirline(route.airline).active)
                    {
                        var toAirport = GetAirportsItem(route.destAirport);
                        frAirport.Outgoing.Add(toAirport);
                        toAirport.Incoming.Add(frAirport);
                    }
                    if (route.destAirport == destAirport)
                    {
                        Success = true;
                        cancelTokenSource.Cancel();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                cancelTokenSource.Cancel();
            }
        }

        private bool FindRoute(SwgAirportItem airport)
        {
            if (airport.Name == srcAirport)
            {
                _Route.Enqueue(airport.Name);
                return true;
            }
            else
            {
                foreach (var air in airport.Incoming)
                {
                    if (FindRoute(air))
                    {
                        _Route.Enqueue(airport.Name);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Execute()
        {
            Airports = new BlockingCollection<SwgAirportItem>();
            var frAirport = GetAirportsItem(srcAirport);
            while (!Stop)
            {
                var list = Airports.Where(x => x.Explored == false);
                if (list.Count() > 0)
                {
                    try
                    {
                        Parallel.ForEach<SwgAirportItem>(list, new ParallelOptions { CancellationToken = token }, ExploreAirport);
                    }
                    catch (OperationCanceledException)
                    {
                        Stop = true;
                        if (Success)
                        {
                            FindRoute(GetAirportsItem(destAirport));
                            Route = String.Join(',', _Route);
                        }
                    }
                }
                else
                {
                    Stop = true;
                    Message = "Route not found";
                }
                
            }
        }

        public void Dispose()
        {
            Airlines.Dispose();
            Airports.Dispose();
            cancelTokenSource.Dispose();
        }
    }
}
