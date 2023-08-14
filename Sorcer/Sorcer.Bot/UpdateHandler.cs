using ATI.Services.Common.Extensions;
using Sorcer.Bot.CallbackHandler;
using Sorcer.Bot.MessageHandler;
using Sorcer.DataAccess.Helpers;
using Sorcer.Models.CallbackHandler;
using Sorcer.Models.MessageHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sorcer.Bot;

public class UpdateHandler : IUpdateHandler
{
    private readonly MessageReceiver _messageReceiver;
    private readonly CallbackReceiver _callbackReceiver;
    private readonly AuthorizationManager _authorizationManager;

    public UpdateHandler(MessageReceiver messageReceiver,
        CallbackReceiver callbackReceiver,
        AuthorizationManager authorizationManager)
    {
        _messageReceiver = messageReceiver;
        _callbackReceiver = callbackReceiver;
        _authorizationManager = authorizationManager;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    var message = update.Message;
                    if (message == null)
                        return;

                    var authorizedUser =
                        await _authorizationManager.GetAuthModelAsync(botClient, message.From, message.Chat.Id,
                            message);
                    if (!authorizedUser.Success)
                        return;

                    _messageReceiver.Handle(new MessageContext(botClient, authorizedUser.Value, message));
                    break;
                }

                case UpdateType.CallbackQuery:
                {
                    var query = update.CallbackQuery;
                    if (query == null)
                        return;

                    var authorizedUser =
                        await _authorizationManager.GetAuthModelAsync(botClient, query.From, query.From.Id);
                    if (!authorizedUser.Success)
                        return;

                    _callbackReceiver.Handle(new CallbackContext(botClient, authorizedUser.Value, query));
                    break;
                }
            }
        }
        catch
        {
            // todo some logs
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        return Task.CompletedTask;
    }
}