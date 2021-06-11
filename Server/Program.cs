using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace Server {
    public class Program {
        private class StateObject {
            public byte[] buffer;
            public Socket socket;
        }

        private static List<Socket> clientList = new List<Socket>();
        
        public static void Main(string[] args) {
            Console.WriteLine("Starting...");

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 4342));
            listener.Listen(10);

            listener.BeginAccept(AcceptCallback, listener);

            while (true) {
                if (Console.KeyAvailable) {
                    string command = Console.ReadLine();
                    switch (command) {
                        case "1":
                            byte type = 1;
                            byte[] textBytes = Encoding.ASCII.GetBytes("Test text for type 1!");
                            byte[] length = BitConverter.GetBytes(textBytes.Length);
                            byte[] final = new byte[5 + textBytes.Length];
                            final[0] = type;
                            Array.Copy(length, 0, final, 1, 4);
                            Array.Copy(textBytes, 0, final, 5, textBytes.Length);

                            foreach (Socket socket in clientList) {
                                socket.Send(final);
                            }
                            break;
                        
                        case "2":
                            type = 2;
                            textBytes = Encoding.ASCII.GetBytes("Text for type 2!");
                            length = BitConverter.GetBytes(textBytes.Length);
                            final = new byte[5 + textBytes.Length];
                            final[0] = type;
                            Array.Copy(length, 0, final, 1, 4);
                            Array.Copy(textBytes, 0, final, 5, textBytes.Length);

                            foreach (Socket socket in clientList) {
                                socket.Send(final);
                            }
                            break;
                        
                        case "3":
                            type = 3;
                            textBytes = Encoding.ASCII.GetBytes("Text for type 3!");
                            length = BitConverter.GetBytes(textBytes.Length);
                            final = new byte[5 + textBytes.Length];
                            final[0] = type;
                            Array.Copy(length, 0, final, 1, 4);
                            Array.Copy(textBytes, 0, final, 5, textBytes.Length);

                            foreach (Socket socket in clientList) {
                                socket.Send(final);
                            }
                            break;
                        
                        default:
                            break;
                    }
                }
            }
        }

        private static void AcceptCallback(IAsyncResult ar) {
            Socket listener = (Socket)ar.AsyncState;
            if (listener != null) {
                Console.WriteLine("Client connected");
                
                Socket handler = listener.EndAccept(ar);
                StateObject so = new StateObject();
                so.socket = handler;
                so.buffer = new byte[5];
                handler.BeginReceive(so.buffer, 0, so.buffer.Length, 0, ReceiveCallback, so);

                clientList.Add(so.socket);
                
                listener.BeginAccept(AcceptCallback, listener);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar) {
            StateObject so = (StateObject)ar.AsyncState;
            Socket handler = so.socket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0) {
                byte type = so.buffer[0];
                int length = BitConverter.ToInt32(so.buffer, 1); // Skip first byte, find a 4 byte int
                
                Console.WriteLine("Type " + type);
                Console.WriteLine("Length " + length);
                
                // Kept separate to show that you can do different things when receiving each type
                switch (type) {
                    case 1:
                        so.buffer = new byte[length];
                        handler.Receive(so.buffer, SocketFlags.None);
                        string result = Encoding.ASCII.GetString(so.buffer);
                        Console.WriteLine("Received " + result);
                        break;
                    
                    case 2:
                        so.buffer = new byte[length];
                        handler.Receive(so.buffer, SocketFlags.None);
                        result = Encoding.ASCII.GetString(so.buffer);
                        Console.WriteLine("Received " + result);
                        break;
                    
                    default:
                        Console.WriteLine("Unknown type " + type);
                        so.socket.Disconnect(false);
                        clientList.Remove(so.socket);
                        break;
                }

                so.buffer = new byte[5];
                handler.BeginReceive(so.buffer, 0, so.buffer.Length, 0, ReceiveCallback, so);
            }
        }
    }
}