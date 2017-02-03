using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpConsoleFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            Search().Wait();
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static async Task Search()
        {
            var addrs = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            var subNets = addrs.Select(t => t.GetAddressBytes())
                .Where(t => t.Length == 4)
                .Where(t => t[0] == 192 && t[1] == 168)
                .SelectMany(t => GetSubnetHosts(t[2]));

            await Task.WhenAll(subNets.Select(async t =>
            {
                if (!await CheckHttp(t)) return;
                Console.WriteLine($"OK {t}");
            }));
        }

        private static IEnumerable<string> GetSubnetHosts(byte subnet)
        {
            return Enumerable.Range(1, 254).Select(t => $"192.168.{subnet}.{t}");
        }

        private static async Task<bool> CheckHttp(string host)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(host, 80);
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
