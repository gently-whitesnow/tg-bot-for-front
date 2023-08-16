using System.Globalization;
using System.Text.Json;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Extensions;
using Microsoft.Extensions.Options;
using Sorcer.DataAccess.Helpers;
using Sorcer.DataAccess.Repositories;
using Sorcer.Models.CallbackHandler;
using Sorcer.Models.Event;
using Sorcer.Models.MessageHandler;
using Sorcer.Models.Options;
using Sorcer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sorcer.DataAccess.Managers;

public class EventsManager
{
    private readonly EventsRepository _eventsRepository;
    private readonly InlineSender _inlineSender;
    private readonly FileSystemHelper _fileSystemHelper;
    private readonly UserStateRepository _userStateRepository;
    private readonly FileSystemOptions _options;    

    public EventsManager(EventsRepository eventsRepository,
        IOptions<FileSystemOptions> options,
        InlineSender inlineSender,
        FileSystemHelper fileSystemHelper,
        UserStateRepository userStateRepository)
    {
        _eventsRepository = eventsRepository;
        _inlineSender = inlineSender;
        _fileSystemHelper = fileSystemHelper;
        _userStateRepository = userStateRepository;
        _options = options.Value;
    }

    public async Task<OperationResult<List<EventPublic>>> GetEventsForApiAsync()
    {
        var eventsOperation = await _eventsRepository.GetEventsAsync();
        if (!eventsOperation.Success)
            return new(eventsOperation);

        var events = eventsOperation.Value
            .OrderBy(e => e.EventDateTime)
            .Select(e => new EventPublic
            {
                DateTimeOffset = e.EventDateTime,
                EventDescription = e.EventDescription,
                ImageUrl = _options.EventsImagePrefixUrl + e.ImagePath,
                StringifyEventDateTime = e.StringifyEventDateTime
            }).ToList();

        bool isFound = false;
        foreach (var publicEnvent in events)
        {
            if(publicEnvent.DateTimeOffset <= DateTimeOffset.Now)
                continue;

            publicEnvent.IsNextEvent = true;
            isFound = true;
            break;
        }

        if (!isFound) events[^1].IsNextEvent = true;
  
        return new(events);
    }

    public async Task ShowEventsAsync(CallbackContext context)
    {
        var eventsOperation = await _eventsRepository.GetEventsAsync();
        var chatId = context.Callback.From.Id;
        if (!eventsOperation.Success)
        {
            context.BotClient.LogErrorAsync(chatId, eventsOperation);
            return;
        }

        foreach (var eventDto in eventsOperation.Value.OrderBy(e => e.EventDateTime).ToArray())
        {
            var fileOperation = await _fileSystemHelper.GetEventFileAsync(eventDto.ImagePath);
            if (!fileOperation.Success)
            {
                context.BotClient.LogErrorAsync(chatId, fileOperation);
                return;
            }

            var caption = $"{eventDto.StringifyEventDateTime}\n{eventDto.EventDescription}";
            var image = GetInputFile(fileOperation.Value);
            await _inlineSender.SendEventImageWithInlineAsync(
                    context.BotClient, chatId, caption, image, eventDto.Id);
        }
        await context.BotClient.SendTextMessageAsync(chatId, $"Количество событий: {eventsOperation.Value.Count}");
        await _inlineSender.SendMenuInlineKeyboard(context.BotClient, chatId);
    }
    
    public void AddEventAsync(CallbackContext context)
    {
        var chatId = context.Callback.From.Id;
        _userStateRepository.SetTempUserData(context.UserDto.Id, (tempUserData)=>
        {
            tempUserData.State = UserState.GettingEventImage;
        });
        
        _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
            "<b>Пришлите изображение события:</b>").Forget();
    }
    
    public void CancelAddingEventAsync(CallbackContext context)
    {
        var chatId = context.Callback.From.Id;
        _userStateRepository.ClearTempUserData(context.UserDto.Id);
        _inlineSender.SendMenuInlineKeyboard(context.BotClient, chatId).Forget();
    }
    public async Task DeleteEventAsync(CallbackContext context, string rawGuid)
    {
        var chatId = context.Callback.From.Id;
        if (!Guid.TryParse(rawGuid, out var eventId))
        {
            context.BotClient.LogErrorAsync(chatId, $"Ошибка чтения ключа события {rawGuid}").Forget();
            return;
        }
        
        var eventsOperation = await _eventsRepository.RemoveEventAsync(eventId);
        if (!eventsOperation.Success)
        {
            context.BotClient.LogErrorAsync(chatId, eventsOperation);
            return;
        }
        if(eventsOperation.Value.Id == eventId)
        {
            var fileOperation = await _fileSystemHelper.DeleteEventFileAsync(eventsOperation.Value.ImagePath);
            if (!fileOperation.Success)
            {
                context.BotClient.LogErrorAsync(chatId, fileOperation).Forget();
                return;
            }
        }

        await context.BotClient.SendTextMessageAsync(chatId,
            $"<b>Успешно удалено</b>", parseMode: ParseMode.Html);
        await _inlineSender.SendMenuInlineKeyboard(context.BotClient, chatId);
    }
    
    public async Task SaveImageAsync(MessageContext context)
    {
        var chatId = context.Message.Chat.Id;
        if (context.Message.Type != MessageType.Photo)
        {
            _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
                "<b>Пришлите изображение события</b>").Forget();
            return;
        }

        // Get the last available size, usually the largest one
        var photo = context.Message.Photo[^1];

        var file = await context.BotClient.GetFileAsync(photo.FileId);
        var eventId = Guid.NewGuid();
        var saveOperation = await _fileSystemHelper.SaveEventFileAsync(context.BotClient, file.FilePath, eventId);
        if (!saveOperation.Success)
        {
            context.BotClient.LogErrorAsync(chatId, saveOperation).Forget();
            return;
        }
        
        _userStateRepository.SetTempUserData(context.UserDto.Id, tempUserData =>
        {
            tempUserData.State = UserState.GettingEventDateTime;
            tempUserData.EventDto.Id = eventId;
            tempUserData.EventDto.ImagePath = saveOperation.Value;
        } );
        
        _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
            "<b>Пришлите дату события в формате дд.ММ.гггг чч:мм, например:\n12.08.2023 21:30</b>").Forget();
    }
    
    public void SaveEventDateTimeAsync(MessageContext context)
    {
        var chatId = context.Message.Chat.Id;
        if (context.Message.Type != MessageType.Text 
            || !DateTime.TryParseExact(context.Message.Text, "g", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var dateTimeOffset))
        {
            _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
                "<b>Пришлите дату события в формате дд.ММ.гггг чч:мм, например:\n12.08.2023 21:30</b>").Forget();
            return;
        }
        
        _userStateRepository.SetTempUserData(context.UserDto.Id, tempUserData =>
        {
            tempUserData.State = UserState.GettingDescription;
            tempUserData.EventDto.EventDateTime = dateTimeOffset;
            tempUserData.EventDto.StringifyEventDateTime = $"{dateTimeOffset.Day} {MonthDictionary.Months[dateTimeOffset.Month]} {dateTimeOffset.Year}";
        } );
        
        _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
            "<b>Пришлите описание события</b>").Forget();
    }
    
    public async Task  SaveEventDescriptionAsync(MessageContext context)
    {
        var chatId = context.Message.Chat.Id;
        if (context.Message.Type != MessageType.Text 
            || string.IsNullOrEmpty(context.Message.Text))
        {
            _inlineSender.SendCancelInlineAsync(context.BotClient, chatId,
                "<b>Пришлите описание события</b>").Forget();
            return;
        }
        
        _userStateRepository.SetTempUserData(context.UserDto.Id, tempUserData =>
        {
            tempUserData.State = UserState.GettingSaveApproval;
            tempUserData.EventDto.EventDescription = context.Message.Text;
        } );
        
        var userData = _userStateRepository.GetTempUserData(context.UserDto.Id);
        var fileOperation = await _fileSystemHelper.GetEventFileAsync(userData.EventDto.ImagePath);
        if (!fileOperation.Success)
        {
            context.BotClient.LogErrorAsync(chatId, fileOperation);
            return;
        }
        var image = GetInputFile(fileOperation.Value);
        
        _inlineSender.SendSaveCancelInlineAsync(context.BotClient, chatId,
            "<b>Подтвердите сохранение события:</b>\n" +
            $"{userData.EventDto.StringifyEventDateTime}\n" +
            $"{userData.EventDto.EventDescription}",
            image).Forget();
    }
    
    public async Task SaveEventAsync(CallbackContext context)
    {
        var chatId = context.Callback.From.Id;
        var userData = _userStateRepository.GetTempUserData(context.UserDto.Id);
        if (string.IsNullOrEmpty(userData.EventDto.EventDescription)
            || string.IsNullOrEmpty(userData.EventDto.StringifyEventDateTime)
            || string.IsNullOrEmpty(userData.EventDto.ImagePath))
        {
            context.BotClient.LogErrorAsync(chatId, $"Что-то пошло не так:\n{JsonSerializer.Serialize(userData)}").Forget();
            return;
        }

        var eventsOperation = await _eventsRepository.AddEventAsync(userData.EventDto);
        if (!eventsOperation.Success)
        {
            context.BotClient.LogErrorAsync(chatId, eventsOperation);
            return;
        }
        _userStateRepository.ClearTempUserData(context.UserDto.Id);
        
        await context.BotClient.SendTextMessageAsync(chatId,
            $"<b>Успешно сохранено</b>", parseMode: ParseMode.Html);
        _inlineSender.SendMenuInlineKeyboard(context.BotClient, chatId).Forget();
    }

    private static InputFile GetInputFile(byte[] bytes)
    {
        var memoryStream = new MemoryStream();

        memoryStream.Write(bytes, 0, bytes.Length);
        memoryStream.Position = 0;
        return InputFile.FromStream(memoryStream);
    }
}