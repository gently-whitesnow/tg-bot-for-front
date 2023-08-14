using System.Runtime.CompilerServices;
using ATI.Services.Common.Behaviors;
using Telegram.Bot;

namespace Sorcer.DataAccess.Helpers;

public static class ITelegramBotClientExtension
{
    public static Task LogErrorAsync<TValue>(this ITelegramBotClient bot, long chatId, OperationResult<TValue> operationResult, [CallerMemberName] string callerMethod = null) => 
        bot.SendTextMessageAsync(chatId,
            $"Ошибка! {callerMethod} Перешлите https://t.me/gently_whitesnow \n{operationResult.DumpAllErrors()}");
    
    public static Task LogErrorAsync(this ITelegramBotClient bot, long chatId, OperationResult operationResult, [CallerMemberName] string callerMethod = null) => 
        bot.SendTextMessageAsync(chatId,
            $"Ошибка! {callerMethod} Перешлите https://t.me/gently_whitesnow \n{operationResult.DumpAllErrors()}");

    
    public static Task LogErrorAsync(this ITelegramBotClient bot, long chatId, Exception ex, [CallerMemberName] string callerMethod = null) => 
        bot.SendTextMessageAsync(chatId,
            $"Ошибка! {callerMethod} Перешлите https://t.me/gently_whitesnow \n{ex.Message}");
    
    public static Task LogErrorAsync(this ITelegramBotClient bot, long chatId, string message) => 
        bot.SendTextMessageAsync(chatId,
            $"Ошибка! Перешлите https://t.me/gently_whitesnow \n{message}");
}