using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class NetTest
{
    static async Task Main()
    {
        GameCoordinator gc = new();
        gc.Start();
        Server s = new(gc);
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
                            s.Stop();
                            gc.Stop();
                            running = false;
                            break;
                        }
                    case 't':
                        {
                            List<Task> tasks = new();
                            for (int i = 0; i < 10; i++)
                            {
                                int d = i;
                                tasks.Add(Task.Run(() =>
                                {
                                    var hostName = Dns.GetHostName();
                                    IPHostEntry localhost = Dns.GetHostEntryAsync(hostName).Result;
                                    // This is the IP address of the local machine
                                    IPAddress localIpAddress = localhost.AddressList[0];

                                    MessageClient client = new(localIpAddress, 9001);
                                    ConnectionRequest r = new() { User = "Thisuser", Game = 1 };
                                    client.Send((Int16)MessageTypes.ConnectionRequest, Serializer.Serialize(r)).Wait();
                                }));
                            }
                            for (int i = 0; i < 10; i++)
                            {
                                await tasks[i];
                            }
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
