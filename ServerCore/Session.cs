using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket clientSocket;
        int _disconnected = 0;

        public void Start(Socket clientSocket)
        {
            this.clientSocket = clientSocket;

            SocketAsyncEventArgs recvargs = new SocketAsyncEventArgs();
            recvargs.Completed += new EventHandler<SocketAsyncEventArgs>(OnResisterCompleted);

            // receive의 경우 버퍼를 통하여 데이터를 받아오므로
            // 버퍼 설정이 필요함
            // 추가적인 정보가 필요할 때 recvargs.UserToken = this;
            recvargs.SetBuffer(new byte[1024], 0, 1024);

            ResisterRecv(recvargs);
        }

        public void Send(byte[] sendBuff)
        {
            clientSocket.Send(sendBuff);
        }

        public void Disconnect()
        {
            // 예외처리1. 멀티 스레딩 환경에서 동시에 접속을 끊는 경우
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        #region 네트워크 통신(Recv)
        void ResisterRecv(SocketAsyncEventArgs args)
        {   // receive 요청
            bool pending = clientSocket.ReceiveAsync(args);

            if(pending == false)
            {
                OnResisterCompleted(null, args);
            }
        }

        void OnResisterCompleted(object sender, SocketAsyncEventArgs args)
        {   // receive 처리
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {   // 상대방이 내용을 보내고 성공했다면
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 다시 요청으로
                    ResisterRecv(args);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
                }
            }
            else
            {

            }
        }
        #endregion
    }
}
