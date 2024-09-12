using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        // volatile : 휘발성
        // -> stop은 휘발성 데이터(언제 변할지 모름 / 최적화하지 마라)
        volatile static bool stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("스레드 시작!");

            // 최적화를 위해 코드를 분석하다 보니까
            // 코드를 수정함. => 내부에서 stop이 수정없으니 로직은 동일하게 하므로
            if (stop == false)
            {
                while(true)
                {
                    // stop이 false이면 무한 루프
                }
            }

            while(stop == false)
            {
                // stop이 false이면 무한 루프
            }

            Console.WriteLine("스레드 종료!");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            // Sleep(1000)? : 해당 스레드가 잠시 1초 멈춤
            Thread.Sleep(1000);
            stop = true;

            Console.WriteLine("Stop 변경");
            Console.WriteLine("종료 대기중");
            // Wait : Thread의 Join과 동일한 역할
            t.Wait();   
            Console.WriteLine("종료 성공");
        }
    }
}