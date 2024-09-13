using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        // 잡혀 있는 상태의 표현
        volatile int _locked = 0;

        public void Acquire()
        {   // Monitor Enter의 역할(획득)

            // 레퍼런스로 변수를 하나 넣어주고, 세팅할 값을 넣어줌
            // 최종적으로 주는 값은 이전에 원본 값을 줌
            // => 이것이 가지는 의미가 굉장히 중요함.
            // 만약 이전에 잠근 상태가 있었다면 1을 줄거임. 그것은 한번 더 잠궜다는 의미

            while (true)
            {
                // CompareExchange : 일반적인 버전. 
                // (조작하기 위한 값, 넣어줄 값, 비교할 값)
                // => 조작하기 위한 값과 비교할 값을 보고, 같다면 넣어줄 값을 넣음
                // 이런 계열의 함수를 CAS(Compare And Swap) / C++에도 있음
                int expected = 0;   // 예상되는 값
                int desired = 1;    // 넣고싶은 값

                if(Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                {
                    break;
                }
            }
        }

        public void Release()
        {   // Monitor Exit의 역할(내려놓기)
            _locked = 0;
        }
    }

    class Program
    {
        static int number = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for(int i = 0; i < 100000; i++)
            {
                _lock.Acquire();

                number++;

                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();

                number--;

                _lock.Release();
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