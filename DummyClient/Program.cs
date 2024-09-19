using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            Console.WriteLine(host);
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];               // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 소켓 설정(휴대전화)
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 입장 가능 여부 확인
                socket.Connect(endPoint);

                // RemoteEndPoint : 연결된 서버라고 생각
                Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                // 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Hello World");
                int sendBytes = socket.Send(sendBuff);

                // 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = socket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);

                Console.WriteLine($"[From Server] {recvData}");

                // 나가기
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString()); ;
            }
        }
    }
}