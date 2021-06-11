using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Client {
    public class Program {
        private static byte[] buffer;
        public static void Main(string[] args) {
            buffer = new byte[5];
            Console.WriteLine("Starting...");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (!socket.Connected) {
                try {
                    socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4342));
                } catch (Exception) {
                    // ignored
                }
                Thread.Sleep(5000);
            }

            socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);
            
            Console.WriteLine("Connected!");
            while (true) {
                
            }
        }
        
        private static void ReceiveCallback(IAsyncResult ar) {
            Socket socket = (Socket)ar.AsyncState;

            if (!socket.Connected) {
                return;
            }
            
            int bytesRead = socket.EndReceive(ar);
            if (bytesRead > 0) {
                byte type = buffer[0];
                int length = BitConverter.ToInt32(buffer, 1); // Skip first byte, find a 4 byte int
                
                Console.WriteLine("Type " + type);
                Console.WriteLine("Length " + length);
                
                // Kept separate to show that you can do different things when receiving each type
                switch (type) {
                    case 1:
                        buffer = new byte[length];
                        socket.Receive(buffer, SocketFlags.None);
                        string result = Encoding.ASCII.GetString(buffer);
                        Console.WriteLine("Received " + result);
                        
                        byte responseType = 1;
                        byte[] responseTextBytes = Encoding.ASCII.GetBytes("This is a test response!");
                        byte[] responseLength = BitConverter.GetBytes(responseTextBytes.Length);
                        byte[] responseFinal = new byte[5 + responseTextBytes.Length];
                        responseFinal[0] = responseType;
                
                        Array.Copy(responseLength, 0, responseFinal, 1, 4);
                        Array.Copy(responseTextBytes, 0, responseFinal, 5, responseTextBytes.Length);
                        socket.Send(responseFinal);
                        break;
                    
                    case 2:
                        buffer = new byte[length];
                        socket.Receive(buffer, SocketFlags.None);
                        result = Encoding.ASCII.GetString(buffer);
                        Console.WriteLine("Received " + result);
                        
                        responseType = 2;
                        responseTextBytes = Encoding.ASCII.GetBytes("This is a test response for type 2!");
                        responseLength = BitConverter.GetBytes(responseTextBytes.Length);
                        responseFinal = new byte[5 + responseTextBytes.Length];
                        responseFinal[0] = responseType;
                
                        Array.Copy(responseLength, 0, responseFinal, 1, 4);
                        Array.Copy(responseTextBytes, 0, responseFinal, 5, responseTextBytes.Length);
                        socket.Send(responseFinal);
                        break;
                    
                    default:
                        Console.WriteLine("Unknown type " + type);
                        socket.Disconnect(false);
                        break;
                }

                buffer = new byte[5];
                socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);
            }
        }
    }
}