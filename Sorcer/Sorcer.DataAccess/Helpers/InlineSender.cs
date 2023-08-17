using System.Text.Json;
using Sorcer.Models.CallbackHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sorcer.DataAccess.Helpers;

public class InlineSender
{
    private const string AllEventsText = "Все события";
    private const string AddEventText = "Добавить событие";
    private const string CancelText = "Сбросить";
    private const string DeleteText = "Удалить";
    private const string SaveText = "Сохранить";
    private const string ChoiceActionText = "<b>Выберите действие:</b>";
    
    public async Task<Message> SendMenuInlineAsync(ITelegramBotClient botClient, long chatId)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(AllEventsText, JsonSerializer.Serialize(new CallbackData
                    {
                        BT = Buttons.Show,
                    })),
                    InlineKeyboardButton.WithCallbackData(AddEventText, JsonSerializer.Serialize(new CallbackData
                    {
                        BT = Buttons.Add,
                    })),
                }
            });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: ChoiceActionText,
            replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
    }

    public async Task<Message> SendEventImageWithDeleteInlineAsync(
        ITelegramBotClient botClient,
        long chatId,
        string caption,
        InputFile image,
        Guid eventId)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(DeleteText, 
                        JsonSerializer.Serialize(new CallbackData
                    {
                        BT = Buttons.Delete,
                        AD = eventId.ToString()
                    }))
                }
            });

        return await botClient.SendPhotoAsync(
            chatId: chatId,
            caption: caption,
            photo: image,
            replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
    }
    
    public Task SendCancelInlineAsync(ITelegramBotClient botClient,
        long chatId,
        string text)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(CancelText, 
                        JsonSerializer.Serialize(new CallbackData
                        {
                            BT = Buttons.Cancel
                        }))
                }
            });

        return botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
    }
    
    public Task SendSaveCancelInlineAsync(ITelegramBotClient botClient,
        long chatId,
        string text,
        InputFile photo)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(CancelText, 
                        JsonSerializer.Serialize(new CallbackData
                        {
                            BT = Buttons.Cancel
                        })),
                    InlineKeyboardButton.WithCallbackData(SaveText, 
                        JsonSerializer.Serialize(new CallbackData
                        {
                            BT = Buttons.Save
                        }))
                }
            });

        return botClient.SendPhotoAsync(
            chatId: chatId,
            caption: text,
            photo:photo,
            replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
    }
}