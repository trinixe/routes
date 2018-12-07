using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RouteSearchApplication.SwgUI.Models;

namespace RouteSearchApplication.SwgUI
{
    public static class SwgCore<T>
    {
        static Semaphore sem = new Semaphore(10, 10);
        private static readonly string baseUri = "https://homework.appulate.com/api/";
        private static short max_tries = 3;

        private static async Task<T> Run(string path)
        {
            sem.WaitOne();
            T result = default(T);
            short tries = 1;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                while (true)
                {
                    using (var response = await client.GetAsync(path))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsAsync<T>();
                            break;
                        }
                        else if (tries >= max_tries)
                        {
                            throw new Exception(response.ReasonPhrase);
                        }
                    }
                    tries++;
                }
            }
            sem.Release();
            return result;
        }

        public static T Get(string path)
        {
            return Run(path).GetAwaiter().GetResult();
        }
    }
}
