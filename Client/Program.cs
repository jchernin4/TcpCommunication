using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client {
    public class Program {
        public static void Main(string[] args) {
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
            
            Console.WriteLine("Connected!");
            while (true) {
                byte type = (byte) 1;
                byte[] textBytes = Encoding.ASCII.GetBytes("This is a test!");
                byte[] length = BitConverter.GetBytes(textBytes.Length);
                byte[] final = new byte[5 + textBytes.Length];
                final[0] = type;
                
                Array.Copy(length, 0, final, 1, 4);
                Array.Copy(textBytes, 0, final, 5, textBytes.Length);
                
                socket.Send(final);
                
                Thread.Sleep(5000);
            }
        }
    }
}