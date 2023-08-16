using ATI.Services.Common.Behaviors;
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

    public Task<OperationResult<string>> SaveEventFileAsync(ITelegramBotClient botClient, string filePath, Guid eventId)
    {
        return SaveFileAsync(botClient, filePath, eventId);
    }

    public Task<OperationResult<byte[]>> GetEventFileAsync(string eventPath)
    {
        return GetFileAsync(eventPath);
    }
    
    public Task<OperationResult> DeleteEventFileAsync(string eventPath)
    {
        return DeleteFileAsync(eventPath);
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


    private async Task<OperationResult> DeleteFileAsync(string path)
    {
        try
        {
            return new(await Task.Run(() =>
            {
                if(File.Exists(path))
                    File.Delete(path);

                return ActionStatus.Ok;
            }));
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
    private async Task<OperationResult<string>> SaveFileAsync(ITelegramBotClient botClient, string telegramFilePath, Guid eventId)
    {
        try
        {
            var filePath = Path.Combine(_fileSystemOptions.EventsRootImageFilePath, eventId.ToString(), telegramFilePath);
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