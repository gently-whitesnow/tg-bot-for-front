using System.Text.Json;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Extensions;
using Sorcer.DataAccess.Helpers;
using Sorcer.DataAccess.Managers;
using Sorcer.Models.CallbackHandler;

namespace Sorcer.Bot.CallbackHandler;

public class CallbackReceiver
{

    private readonly EventsManager _eventsManager;

    public CallbackReceiver(EventsManager eventsManager)
    {
        _eventsManager = eventsManager;
    }

    public void Handle(CallbackContext context)
    {
        var callbackData = TryGetCallbackData(context.Callback.Data);
        if (!callbackData.Success)
        {
            context.BotClient.LogErrorAsync(context.Callback.From.Id, callbackData).Forget();
            return;
        }
        
        switch (callbackData.Value.BT)
        {
            case Buttons.Show:
            {
                _eventsManager.ShowEventsAsync(context).Forget();
                break;
            }
            case Buttons.Add:
            {
                _eventsManager.AddEventAsync(context);
                break;
            }
            case Buttons.Delete:
            {
                _eventsManager.DeleteEventAsync(context, callbackData.Value.AD).Forget();
                break;
            }
            case Buttons.Cancel:
            {
                _eventsManager.CancelAddingEventAsync(context);
                break;
            }
            case Buttons.Save:
            {
                _eventsManager.SaveEventAsync(context);
                break;
            }
        }
    }

    private OperationResult<CallbackData> TryGetCallbackData(string rawCallbackData)
    {
        try
        {
            if(string.IsNullOrEmpty(rawCallbackData))
                throw new JsonException($"Callback data empty");
            
            var data = JsonSerializer.Deserialize<CallbackData>(rawCallbackData);
            if (data == null)
                throw new JsonException($"Invalid callback data {rawCallbackData}");
            
            return new(data);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
}