using ATI.Services.Common.Behaviors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sorcer.Models.Options;
using Telegram.Bot;

namespace Sorcer.DataAccess.Helpers;

public class FileSystemHelper
{
    private readonly FileSystemOptions _fileSystemOptions;

    public FileSystemHelper(IOptions<FileSystemOptions> fileSystemOptions)
    {
        _fileSystemOptions = fileSystemOptions.Value;
    }

    public Task<OperationResult<string>> SaveEventFileAsync(Guid eventId,ITelegramBotClient botClient, string filePath)
    {
        return SaveFileAsync($"{eventId}", botClient, filePath);
    }

    public Task<OperationResult<byte[]>> GetEventFileAsync(string eventPath)
    {
        return GetFileAsync(eventPath);
    }
    
    public Task<OperationResult> DeleteEventDirectoryAsync(Guid eventId)
    {
        return DeleteDirectoryAsync($"{eventId}");
    }

    private async Task<OperationResult<byte[]>> GetFileAsync(string path)
    {
        try
        {
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                return new(ActionStatus.BadRequest, "file_not_found", "изображение не найдено");
            
            byte[] fileBytes;
            await using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }

            return new(fileBytes);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }


    private async Task<OperationResult> DeleteDirectoryAsync(string path)
    {
        try
        {
            return new(await Task.Run(() =>
            {
                var directory = new DirectoryInfo($"{_fileSystemOptions.EventsRootImageFilePath}/{path}");
                if (!directory.Exists)
                    return ActionStatus.Ok;

                directory.Delete(true);

                return ActionStatus.Ok;
            }));
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
    private async Task<OperationResult<string>> SaveFileAsync(string path, ITelegramBotClient botClient, string telegramFilePath)
    {
        try
        {
            var filePath = Path.Combine(_fileSystemOptions.EventsRootImageFilePath, path, telegramFilePath);
            new FileInfo(filePath).Directory?.Create();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await botClient.DownloadFileAsync(telegramFilePath, stream);
            }
            return new(filePath);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
}