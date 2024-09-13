using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static int number = 0;
        static object obj = new object();
        

        static void Thread_1()
        {
            // 상호배제(Mutual Exclusive)

            for(int i = 0; i < 1000000; i++)
            {
                // 1. return에서 잠구기
                // Monitor.Enter(obj);
                // number++;
                // 
                // Monitor.Exit(obj);
                // return;

                // 2. try - finally 이용
                // try
                // {
                //     Monitor.Enter(obj);
                //     number++;
                // 
                //     return;
                // }
                // finally
                // {
                //     Monitor.Exit(obj);
                // }

                // 3. lock 키워드 이용
                lock(obj)
                {
                    number++;
                }
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                Monitor.Enter(obj);

                number--;

                Monitor.Exit(obj);
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine($"{number}");
        }
    }
}