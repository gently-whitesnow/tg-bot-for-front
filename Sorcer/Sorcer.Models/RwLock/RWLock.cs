namespace Sorcer.Models.RwLock;

public class RWLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    
    public ReadLock ReadLock() => new(_lock);
    public WriteLock WriteLock() => new(_lock);
    public ReadUpgradeableLock ReadUpgradeableLock() => new(_lock);

    public void Dispose() => _lock.Dispose();
}