namespace Sorcer.Models.RwLock;

public class ReadUpgradeableLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;
    public ReadUpgradeableLock(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
        _lock.EnterUpgradeableReadLock();
    }

    public void EnterWriteLock() => _lock.EnterWriteLock();
    public void ExitWriteLock() => _lock.ExitWriteLock();
    public void Dispose() => _lock.ExitUpgradeableReadLock();
}