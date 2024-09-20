using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    class Session
    {
        Socket clientSocket;
        int _disconnected = 0;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        object _lock = new object();
        bool _pending = false;
        Queue<byte[]> _sendQueue = new Queue<byte[]>();

        public void Start(Socket clientSocket)
        {
            this.clientSocket = clientSocket;

            SocketAsyncEventArgs recvargs = new SocketAsyncEventArgs();
            recvargs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // receive의 경우 버퍼를 통하여 데이터를 받아오므로
            // 버퍼 설정이 필요함
            // 추가적인 정보가 필요할 때 recvargs.UserToken = this;
            recvargs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            ResisterRecv(recvargs);
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {   
                _sendQueue.Enqueue(sendBuff);

                if (_pending == false)
                    ResisterSend();
            }
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

        void ResisterSend()
        {   // Send 등록
            _pending = true;    // 현재 하나의 Send 진행 중
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = clientSocket.SendAsync(_sendArgs);

            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {   // Send 처리
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {   // Send 진행 종료

                        if (_sendQueue.Count > 0)
                        {   // pending 중인 상황에서 누군가 Send를 또 하게되면
                            // Send에 대한 등록, 처리가 이루어지지 않고 큐에만 데이터가 들어감
                            // 추가 처리가 필요함
                            ResisterSend();
                        }
                        else
                        {   // 그 사이에 아무도 패킷을 추가하지 않음
                            _pending = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void ResisterRecv(SocketAsyncEventArgs args)
        {   // receive 요청
            bool pending = clientSocket.ReceiveAsync(args);

            if(pending == false)
            {
                OnRecvCompleted(null, args);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
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
                Disconnect();
            }
        }
        #endregion
    }
}
