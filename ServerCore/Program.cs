using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static object _lock1 = new object();
        static SpinLock _lock2 = new SpinLock();
        static Mutex _lock3 = new Mutex();

        static ReaderWriterLockSlim _lock4 = new ReaderWriterLockSlim();

        class Reward
        {

        }

        static Reward GetRewardById(int id)
        {   // id로 Reward를 조회하는 함수
            _lock4.EnterReadLock();

            _lock4.ExitReadLock();

            return null;
        }

        static void AddReward(Reward reward)
        {   // 보상을 추가할 함수
            _lock4.EnterWriteLock();

            _lock4.ExitWriteLock();
        }

        static void Main(string[] args)
        {
            lock(_lock1)
            {

            }

            bool lockTaken = false;
            try
            {   
                _lock2.Enter(ref lockTaken); 
            }
            finally
            {
                if (lockTaken)
                {
                    _lock2.Exit();
                }
            }

            _lock3.WaitOne();
            _lock3.ReleaseMutex();
        }
    }
}