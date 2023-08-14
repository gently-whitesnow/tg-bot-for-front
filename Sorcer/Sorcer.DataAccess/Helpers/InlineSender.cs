using System.Text.Json;
using Sorcer.Models.CallbackHandler;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sorcer.DataAccess.Helpers;

public class InlineSender
{
    public async Task<Telegram.Bot.Types.Message> SendMenuInlineKeyboard(ITelegramBotClient botClient, long chatId)
    {
        InlineKeyboardMarkup inlineKeyboard = new(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Все события", JsonSerializer.Serialize(new CallbackData
                    {
                        BT = Buttons.Show,
                    })),
                    InlineKeyboardButton.WithCallbackData("Добавить событие", JsonSerializer.Serialize(new CallbackData
                    {
                        BT = Buttons.Add,
                    })),
                }
            });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "<b>Выберите действие:</b>",
            replyMarkup: inlineKeyboard, parseMode: ParseMode.Html);
    }

    public async Task<Message> SendEventImageWithInlineAsync(
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
                    InlineKeyboardButton.WithCallbackData("Удалить", 
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
                    InlineKeyboardButton.WithCallbackData("Сбросить", 
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
                    InlineKeyboardButton.WithCallbackData("Сбросить", 
                        JsonSerializer.Serialize(new CallbackData
                        {
                            BT = Buttons.Cancel
                        })),
                    InlineKeyboardButton.WithCallbackData("Сохранить", 
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