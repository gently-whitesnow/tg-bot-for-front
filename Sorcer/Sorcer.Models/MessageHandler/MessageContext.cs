using Sorcer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sorcer.Models.MessageHandler;

public record MessageContext(ITelegramBotClient BotClient, UserDto UserDto, Message Message);