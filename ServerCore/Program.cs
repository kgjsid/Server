using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        int answer;
        bool complete;

        void A()
        {   // Write만 하는 스레드
            answer = 123;
            Thread.MemoryBarrier(); // Barrier 1
            complete = true;
            Thread.MemoryBarrier(); // Barrier 2
            // 마지막에 넣는 이유? -> Write를 할 때마다 써줌
            // -> 확실히 가시성을 챙기기 위함
        }

        void B()
        {   // Read만 하는 스레드
            // 처음에 넣는 이유? -> Read를 하기 전 써줌
            // -> 또한 확실히 가시성을 챙기기 위함
            Thread.MemoryBarrier(); // Barrier 3
            if(complete)
            {
                Thread.MemoryBarrier(); // Barrier 4
                Console.WriteLine(answer);
            }
        }
    }
}