using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Extensions;
using Sorcer.DataAccess.Helpers;
using Sorcer.DataAccess.Repositories;
using Sorcer.Models.Options;
using Sorcer.Models.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sorcer.DataAccess.Managers;

public class AuthorizationManager
{
    private readonly AuthorizationRepository _authorizationRepository;
    private readonly string _password;
    private const string PasswordRequiredMessage = "Для доступа необходим пароль";
    private const string AccessProvidedMessage = "Доступ предоставлен";

    public AuthorizationManager(AuthorizationRepository authorizationRepository, TelegramBotOptions options)
    {
        _password = options.PASSWORD;
        _authorizationRepository = authorizationRepository;
    }

    public async Task<OperationResult<UserDto>> GetAuthModelAsync(ITelegramBotClient bot, User? user, long chatId,
        Message? message = null)
    {
        if (user == null) return new (ActionStatus.BadRequest);
        
        var userOperation = await _authorizationRepository.GetUserAsync(user.Id);
        if (userOperation.ActionStatus != ActionStatus.Ok)
        {
            bot.LogErrorAsync(chatId, userOperation).Forget();
            return userOperation;
        }

        if (userOperation.Value != null)
            return userOperation;

        if (message == null || string.IsNullOrEmpty(message.Text) || !message.Text.Trim().Equals(_password, StringComparison.InvariantCultureIgnoreCase))
        {
            bot.SendTextMessageAsync(chatId, PasswordRequiredMessage).Forget();
            return new(ActionStatus.Forbidden);
        }

        var telegramUser = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username
        };
        var addOperation = await _authorizationRepository.AddUserAsync(telegramUser);
        if (!addOperation.Success)
        {
            bot.LogErrorAsync(chatId, addOperation).Forget();
            return new(addOperation);
        }

        bot.SendTextMessageAsync(chatId, AccessProvidedMessage).Forget();
        return new(telegramUser);
    }
}