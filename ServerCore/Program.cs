
using System.Runtime.CompilerServices;

namespace ServerCore
{
    class Program
    {
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My Name Is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAmI()
        {   // ThreadLocal.Value를 통해 값에 접근 및 수정 가능
            // ThreadName.Value = $"My Name Is {Thread.CurrentThread.ManagedThreadId}";

            // IsValueCreate : 이미 만들어져 있다면 true로 할당
            bool repeat = ThreadName.IsValueCreated;

            Thread.Sleep(1000);

            if (repeat)
            {
                Console.WriteLine($"{ThreadName.Value} (repeat)");
            }
            else
                Console.WriteLine($"{ThreadName.Value}");
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(2, 2);

            // Invoke에 넣어주는 액션 만큼을 태스크로 만들어 실행
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            // 필요없다면 날려주는 메소드
            ThreadName.Dispose();
        }
    }
}