using ATI.Services.Common.Behaviors;
using Microsoft.Extensions.Options;
using Sorcer.Models.Options;
using Sorcer.Models.User;

namespace Sorcer.DataAccess.Repositories;

public class AuthorizationRepository : FileRepository<List<UserDto>>
{
    public AuthorizationRepository(IOptions<FileSystemOptions> options) 
        : base(options.Value.AuthorizationRepositoryFilePath)
    {
    }

    public async Task<OperationResult<UserDto?>> GetUserAsync(long id)
    {
        var readOperation = await ReadAllAsync();
        if (!readOperation.Success)
            return new(readOperation);
        
        return new(readOperation.Value.FirstOrDefault(u => u.Id == id));
    }
    
    public Task<OperationResult<List<UserDto>>> AddUserAsync(UserDto userDto)
    {
        return WriteAllAsync((users) =>
        {
            if(!users.Exists(u => u.Id == userDto.Id))
                users.Add(userDto);
        });
    }
}