using Microsoft.AspNetCore.Identity;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sorcer.Bot;

public class TelegramBot
{
    private readonly ITelegramBotClient _bot;
    private readonly UpdateHandler _updateHandler;

    public TelegramBot(string botToken, UpdateHandler updateHandler)
    {
        _bot = new TelegramBotClient(botToken);
        _updateHandler = updateHandler;
    }

    public Task Execute()
    {
        Console.WriteLine("Запущен бот " + _bot.GetMeAsync().Result);
        
        return _bot.ReceiveAsync(_updateHandler,
            new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery,
                }
            }
        );
    }
}