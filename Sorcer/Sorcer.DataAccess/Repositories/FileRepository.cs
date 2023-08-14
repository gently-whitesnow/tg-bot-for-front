using System.Text.Json;
using ATI.Services.Common.Behaviors;
using Sorcer.Models.RwLock;

namespace Sorcer.DataAccess.Repositories;

public abstract class FileRepository<TEntity> where TEntity : class, new()
{
    private readonly string _filePath;

    private readonly RWLock _rwLock = new();

    protected FileRepository(string filePath)
    {
        _filePath = filePath;
    }

    protected Task<OperationResult<TEntity>> ReadAllAsync()
    {
        return Task.Run(() =>
        {
            using (_rwLock.ReadLock())
                return ReadAll();
        });
    }

    protected Task<OperationResult<TEntity>> WriteAllAsync(Action<TEntity> modificationAction)
    {
        return Task.Run(() =>
        {
            using (_rwLock.WriteLock())
            {
                var readOperation = ReadAll();
                if (!readOperation.Success)
                    return readOperation;

                var data = readOperation.Value;
                modificationAction(data);
                var writeOperation = WriteAll(data);
                if (!writeOperation.Success)
                    return new(writeOperation);

                return readOperation;
            }
        });
    }

    private OperationResult<TEntity> ReadAll()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new(new TEntity());
            
            return new(JsonSerializer.Deserialize<TEntity>(
                File.ReadAllText(_filePath)));
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    private OperationResult WriteAll(TEntity entity)
    {
        try
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(entity));
            return OperationResult.Ok;
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
}