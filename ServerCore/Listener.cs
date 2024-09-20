using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
	class Listener
	{
		// 리슨 소켓을 관리할 리스너(문지기)
		Socket listenSocket;
		// Accept가 완료되었을 때 호출할 액션
		Action<Socket> onAcceptHandler;

		public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
		{	// 시작 초기화 메소드
			listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.onAcceptHandler += onAcceptHandler;
			
			// Bind, Listen 진행
			listenSocket.Bind(endPoint);
			listenSocket.Listen(10);

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			// EventHandler(이벤트 핸들러) 방식
			// -> 콜백으로 전달하는 방식 / 클라이언트에서 커넥트 요청이 오면 OnAcceptCompleted가 호출
			args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			
			// 최초에 초기화를 진행하며 한번 등록
			RegisterAccept(args);
		}

		void RegisterAccept(SocketAsyncEventArgs args)
		{   // 등록 -> 요청만 진행
			// 기존의 소켓은 비워두기
			args.AcceptSocket = null;

            bool pending = listenSocket.AcceptAsync(args);

			// pending -> false 작업 완료
			// pending -> true 작업 진행 중
            if (pending == false)
            {   // 실행하는 동시에 클라이언트가 접속
				// => 이 경우에는 args에서 Completed 이벤트가 호출되지 않아 직접 호출
				OnAcceptCompleted(null, args);
            }

        }

		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{	// 실제로 클라에 대한 처리를 진행할 메소드
			if(args.SocketError == SocketError.Success)
			{   // 소켓에러가 없이 모든게 잘 처리되었을 때

				// args에 소켓 활용(AcceptSocket)
				onAcceptHandler?.Invoke(args.AcceptSocket);
			}
			else
                Console.WriteLine(args.SocketError.ToString());

			// 다음 클라이언트를 위한 등록
			RegisterAccept(args);
		}
	}
}
