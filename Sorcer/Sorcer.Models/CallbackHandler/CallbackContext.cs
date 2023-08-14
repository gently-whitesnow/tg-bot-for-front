using Sorcer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sorcer.Models.CallbackHandler;

public record CallbackContext(ITelegramBotClient BotClient, UserDto UserDto, CallbackQuery Callback);