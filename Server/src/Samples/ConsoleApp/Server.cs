using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fleck.Samples.ConsoleApp
{
    class Server
    {
        static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();
        static void Main()
        {
            FleckLog.Level = LogLevel.Debug;
            var server = new WebSocketServer("ws://0.0.0.0:8081");
            server.Start(socket =>
            {
                var id = socket.ConnectionInfo.Id;
                socket.OnOpen = () =>
                {
                    allSockets.Add(socket);
                    Console.WriteLine(id + ": Connected");
                };
                socket.OnClose = () =>
                {
                    allSockets.Remove(socket);
                    Console.WriteLine(id + ": Closed");
                };
                socket.OnBinary = bytes =>
                {
                    //Console.WriteLine(id + ": Received: bytes(" + bytes.Length + ")");
                    //socket.Send(bytes);
                    BroacastMsg(bytes);
                };
                socket.OnMessage = message =>
                {
                    var allSocketsList = allSockets.ToList();
                    Console.WriteLine($"======当前要广播的客户端数量:{allSocketsList.Count}");
                    foreach (var s in allSocketsList)
                    {
                        Console.WriteLine("======" + id + ": Send: " + message + "");
                        s.Send(message);
                    }
                };
                socket.OnError = error =>
                {
                    Console.WriteLine(socket.ConnectionInfo.Id + ": " + error.Message);
                };
            });

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets.ToList())
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }
        }

        static void BroacastMsg(byte[] bytes)
        {
            Console.WriteLine($"======当前要广播的客户端数量:{allSockets.Count}");
            foreach (var socket in allSockets)
            {
                Console.WriteLine($"======给{socket.ConnectionInfo.Id}广播消息");
                socket.Send(bytes);
            }
        }
    }
}
