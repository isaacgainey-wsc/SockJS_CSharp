using System;

namespace SockJS_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpStomp stomp = new SimpStomp("flapxtest:8080/ws");
            stomp.Subscribe("/app/fix.market", (sf) =>
            {
                Console.WriteLine($"<<<{sf.Command.CMD}\nMarket\n{sf.Headers.ToString("\n")}\n\n{sf.Content}\n");
            });
            stomp.Subscribe("/topic/fix.new", (sf) =>
            {
                Console.WriteLine($"<<<{sf.Command.CMD}\nNew Offering\n{sf.Headers.ToString("\n")}\n\n{sf.Content}\n");
            });
            stomp.Subscribe("/topic/status", (sf) =>
            {
                Console.WriteLine($"<<<{sf.Command.CMD}\nStatus\n{sf.Headers.ToString("\n")}\n\n{sf.Content}\n");
            });

            stomp.Connect(
                (connected_frame) =>
                {
                    Console.WriteLine($"<<<{connected_frame.Command.CMD}\n{connected_frame.Headers.ToString("\n")}\n\n{connected_frame.Content}\n");
                },
                (sending_frame) =>
                {
                    Console.WriteLine($">>>{sending_frame.Command.CMD}\n{sending_frame.Headers.ToString("\n")}\n\n{sending_frame.Content}\n");
                }
                , (error_message) =>
                {
                    Console.WriteLine($"\n<<< Error\n{error_message}\n\n");
                }
            );
            Console.ReadLine();
            stomp.Close();
        }
    }
}
