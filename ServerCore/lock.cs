using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ServerCore
{
    // lock의 정책
    // 1. 재귀적 락을 허용할지 (No)
    // -> writeLock을 허용한 상태에서 다시 한번 같은 Thread에서 또 acquire할 때 허용할지
    // 2. 스핀락 정책 (5000번 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;  // 0
        const int WRITE_MASK = 0x7FFF0000;  // Write용 비트
        const int READ_MASK = 0x0000FFFF;   // Read용 비트
        const int MAX_SPIN_COUNT = 5000;    // 최대 스핀 수

        // 32bit [Unused(1)] [WriteThreadID(15)] [ReadCount(16)]
        // -> 첫번째 비트는 사용하지 않음(음수가 될 가능성이 있기 때문에)
        // -> WriteThreadId로 15bit / ReadCount로 16비트 사용
        // WriteLock은 한번에 한 스레드만 획득 가능.(기록은 WriteThreadID로)
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {   // 아무도 WriteLock, ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다

            // 재귀적 Lock 구현
            // 동일 스레드가 이미 라이트락을 획득하고 있는지 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if (lockThreadID == Thread.CurrentThread.ManagedThreadId)
            {   // 만약 스레드가 라이트락을 잡고 있었다면
                _writeCount++;
                return;
            }

            // 1부터 늘어나는 숫자. 어떤 Thread인지 구분하기 편함
            // Read 16비트만큼 밀어줘야 함 + 혹시 남아있을 수 있으니 WriteMask로 아랫자리 밀어버림
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 또한 멀티 스레딩에서 동시에 접근하는 것을 막기 위해서 InterLock 활용
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;

                        return;
                    }
                }

                // 횟수안에 성공하지 못하면 휴식(양보)
                Thread.Yield();
            }
        }
        public void WriteUnLock()
        {   // 잡고 있던 flag만 풀어주면 됨

            // 재귀적 구현에서는 writeCount 확인하기
            int lockCount = --_writeCount;

            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {   // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1늘림

            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if (lockThreadID == Thread.CurrentThread.ManagedThreadId)
            {   // 만약 스레드가 라이트락을 잡고 있었다면
                Interlocked.Increment(ref _flag);
                return;
            }

            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 예상되는 값 : flag값에 ReadMask로 걸러낸 값
                    // lock-free 프로그래밍의 기초 : 원하는 값을 넣어 한방에 통과할 수도, 안할 수도 있음
                    // 1. Write는 무조건 걸러짐.
                    // 2. 동시 다발적으로 ReadLock을 걸면? -> 경합해서 하나씩 들어감
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                    {
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void ReadUnLock()
        {   // 1하나 줄이기
            Interlocked.Decrement(ref _flag);
        }
    }
}
