using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server {
    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
    public class Program {
        public class StateObject {
            public byte[] buffer;
            public Socket socket;
        }
        
        public static void Main(string[] args) {
            Console.WriteLine("Starting...");

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 4342));
            listener.Listen(10);
            while (true) {
                listener.BeginAccept(AcceptCallback, listener);
                Thread.Sleep(5000);
            }
        }

        private static void AcceptCallback(IAsyncResult ar) {
            Socket listener = (Socket)ar.AsyncState;
            if (listener != null) {
                Socket handler = listener.EndAccept(ar);

                Console.WriteLine("Client connected HERE");

                StateObject so = new StateObject();
                so.socket = handler;
                so.buffer = new byte[5];
                handler.BeginReceive(so.buffer, 0, so.buffer.Length, 0, ReceiveCallback, so);
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
                
                switch (type) {
                    case 1:
                        so.buffer = new byte[length];
                        handler.Receive(so.buffer, SocketFlags.None);
                        string result = Encoding.ASCII.GetString(so.buffer);
                        Console.WriteLine("Received " + result);
                        break;
                    
                    default:
                        Console.WriteLine("Unknown type " + type);
                        break;
                }

                so.buffer = new byte[5];
                handler.BeginReceive(so.buffer, 0, so.buffer.Length, 0, ReceiveCallback, so);
            }
        }
    }
}