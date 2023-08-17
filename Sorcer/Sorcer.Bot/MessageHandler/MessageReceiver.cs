using ATI.Services.Common.Extensions;
using Sorcer.DataAccess.Helpers;
using Sorcer.DataAccess.Managers;
using Sorcer.DataAccess.Repositories;
using Sorcer.Models.MessageHandler;
using Sorcer.Models.User;

namespace Sorcer.Bot.MessageHandler;

public class MessageReceiver
{
    private readonly UserStateRepository _userStateRepository;
    private readonly EventsManager _eventsManager;  
    private readonly InlineSender _inlineSender;    

    public MessageReceiver(UserStateRepository userStateRepository,
        InlineSender inlineSender,
        EventsManager eventsManager)
    {
        _userStateRepository = userStateRepository;
        _inlineSender = inlineSender;
        _eventsManager = eventsManager;
    }

    public void Handle(MessageContext context)
    {
        var chatId = context.Message.Chat.Id;
        var tempUserData = _userStateRepository.GetTempUserData(context.UserDto.Id);
        if (tempUserData == null || tempUserData.State == UserState.None)
        {
            _inlineSender.SendMenuInlineAsync(context.BotClient, chatId);
            return;
        }

        switch (tempUserData.State)
        {
            case UserState.GettingEventImage:
            {
                _eventsManager.SaveImageAsync(context).Forget();
                break;
            }
            case UserState.GettingEventDateTime:
            {
                _eventsManager.SaveEventDateTime(context);
                break;
            }
            case UserState.GettingDescription:
            {
                _eventsManager.SaveEventDescriptionAsync(context).Forget();
                break;
            }
        }
    }
}