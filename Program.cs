using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class NetTest
{
    static async Task Main()
    {
        Server s = new();
        s.Start();

        bool running = true;
        while (running)
        {
            Console.WriteLine("press q to quit");
            if (char.TryParse(Console.ReadLine(), out char x)) // or out int x
            {
                switch (char.ToLower(x))
                {
                    case 'q':
                        {
                            await s.Stop();
                            running = false;
                            break;
                        }
                    case 't':
                        {
                            var hostName = Dns.GetHostName();
                            IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
                            // This is the IP address of the local machine
                            IPAddress localIpAddress = localhost.AddressList[0];
                            TcpClient tclient = new();
                            tclient.Connect(localIpAddress, 9001);
                            MessageClient client = new(tclient);
                            client.Send(7, Encoding.ASCII.GetBytes("hello"));
                        }
                        break;
                    default:
                        {
                            Console.WriteLine($"Unknown command {x}");
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }

}
