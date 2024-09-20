using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                session.Send(sendBuff);

                Thread.Sleep(1000);
                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            // DNS(Domain Name System)
            // IP주소를 매칭하는 이름이라고 생각
            // 지금 현재 컴퓨터로 서버를 동작시키니 서버 컴퓨터의 IP주소가 필요한 상황
            string host = Dns.GetHostName();
            Console.WriteLine(host);
            IPHostEntry ipHost = Dns.GetHostEntry(host);            
            IPAddress ipAddr = ipHost.AddressList[0];               // ip 주소
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);     // 최종 주소(ip, 포트 번호)

            // ipHost.AddressList -> 원하는 네트워크 주소(배열값)
            // 구글같이 트래픽이 많은 경우 부하 분산을 위해 DNS가 여러 주소를 가지는 경우도 많음
            // 포트 번호 : 정문, 후문 생각 -> 클라이언트가 엉뚱한 번호로 접근하면 못하도록 막음
            // 클라이언트가 접속할 주소도 똑같이 맞춰줘야 함

            listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");
        }
    }
}