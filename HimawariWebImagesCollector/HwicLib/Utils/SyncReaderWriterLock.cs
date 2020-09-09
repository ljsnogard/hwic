namespace Hwic.Utils
{
    using System;
    using System.Threading;


    public sealed class SyncReaderWriterLock
    {
        private readonly ReaderWriterLockSlim rwlock_;


        public SyncReaderWriterLock()
            => this.rwlock_ = new ReaderWriterLockSlim();


        public sealed class UpgradeLock : IDisposable
        {
            private ReaderWriterLockSlim rwlock_;


            internal UpgradeLock(ReaderWriterLockSlim rwlock)
                => this.rwlock_ = rwlock;


            public X Enter<X>(Func<X> upgradedWork)
            {
                if (this.rwlock_ == null)
                    throw new ObjectDisposedException(objectName: nameof(UpgradeLock));

                this.rwlock_.EnterWriteLock();
                try
                {
                    return upgradedWork();
                }
                finally
                {
                    this.rwlock_.ExitWriteLock();
                }
            }


            public void Dispose()
                => this.rwlock_ = null;
        }



        public X EnterReaderLock<X>(Func<X> readerFn)
        {
            this.rwlock_.EnterReadLock();
            try
            {
                return readerFn();
            }
            finally
            {
                this.rwlock_.ExitReadLock();
            }
        }


        public X EnterWriterLock<X>(Func<X> writerFn)
        {
            this.rwlock_.EnterWriteLock();
            try
            {
                return writerFn();
            }
            finally
            {
                this.rwlock_.ExitWriteLock();
            }
        }


        public X EnterUpgradableLock<X>(Func<UpgradeLock, X> upgradableFn)
        {
            this.rwlock_.EnterUpgradeableReadLock();
            try
            {
                using var upgLock = new UpgradeLock(this.rwlock_);
                return upgradableFn(upgLock);
            }
            finally
            {
                this.rwlock_.ExitUpgradeableReadLock();
            }
        }
    }
}
