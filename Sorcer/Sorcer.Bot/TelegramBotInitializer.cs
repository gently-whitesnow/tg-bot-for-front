using ATI.Services.Common.Initializers.Interfaces;
using Microsoft.Extensions.Options;
using Sorcer.Models.Options;

namespace Sorcer.Bot;

public class TelegramBotInitializer : IInitializer
{
    private readonly TelegramBot _bot;
    private bool isInitialized;

    public TelegramBotInitializer(TelegramBotOptions options,
        UpdateHandler updateHandler)
    {
        _bot = new TelegramBot(options.TOKEN, updateHandler);
    }
    
    public Task InitializeAsync()
    {
        if (isInitialized) return Task.CompletedTask;
        
        _bot.Execute();
        isInitialized = true;
        return Task.CompletedTask;
    }

    public string InitStartConsoleMessage()
    {
        return $"Start {nameof(TelegramBotInitializer)} initialize";
    }

    public string InitEndConsoleMessage()
    {
        return $"End {nameof(TelegramBotInitializer)} initialize";
    }
}