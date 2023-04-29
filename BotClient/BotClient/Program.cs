using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Domain.Models;
using Newtonsoft.Json;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //var botClient = new TelegramBotClient("{YOUR_ACCESS_TOKEN_HERE}");
            var botClient = new TelegramBotClient("6241133159:AAH2u8rcuVyumes6qWz3NMzkhciIw6QtNZI");

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receine all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            HttpClient client = new HttpClient();

            // Товар
            Console.WriteLine("Goods:");
            var result = await client.GetAsync("https://localhost:7252/api/Tovar");
            Console.WriteLine(result);

            var test = await result.Content.ReadAsStringAsync();
            Console.WriteLine(test);

            Tovar[] tovars = JsonConvert.DeserializeObject<Tovar[]>(test);

            foreach (var tovar in tovars)
            {
                Console.WriteLine(tovar.IdTovara + " " + tovar.IdKategorii + " " + tovar.Name + " " + tovar.Price);
            }

            // Просмотр всех категорий товаров
            Console.WriteLine("Categories:");

            var result1 = await client.GetAsync("https://localhost:7252/api/Haracterystica");
            Console.WriteLine(result1);

            var test1 = await result1.Content.ReadAsStringAsync();
            Console.WriteLine(test1);

            HaracterysticaTovarov[] har = JsonConvert.DeserializeObject<HaracterysticaTovarov[]>(test1);

            foreach (var h in har)
            {
                Console.WriteLine(h.IdKategorii + " " + h.NameKategorii);
            }

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https:\\core.telegram.org/bots/api#message
            if (update.Message is not { } message)
                return;
            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said: \n" + messageText,
                cancellationToken: cancellationToken);

            if (message.Text == "Проверка")
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Проверка: ОК!",
                    cancellationToken: cancellationToken);
            }

            if (message.Text == "Привет")
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Здраствуй, Настя",
                    cancellationToken: cancellationToken);
            }

            if (message.Text == "Картинка")
            {
                await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: "https://s.mediasole.ru/cache/content/data/images/1683/1683599/1472043884_02.jpg",
                    caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://s.mediasole.ru/cache/content/data/images/1683/1683599/1472043884_02.jpg\">Pixabay</a>",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            }

            if (message.Text == "Видео")
            {
                await botClient.SendVideoAsync(
                chatId: chatId,
                video: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-countdown.mp4",
                thumb: "https://raw.githubusercontent.com/TelegramBots/book/master/src/2/docs/thumb-clock.jpg",
                supportsStreaming: true,
                cancellationToken: cancellationToken);
            }

            if (message.Text == "Стикер")
            {
                await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: "https://github.com/TelegramBots/book/raw/master/src/docs/sticker-dali.webp",
                cancellationToken: cancellationToken);
            }

            // Кнопки
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                 new KeyboardButton[] {"📌 Мои списки", "➕ Список", "➕ Товар" },
                 new KeyboardButton[] { "⚙️ Настройки", "📚 Помощь"  },
                 new KeyboardButton[] { "➿ Обратная связь", "⭐️ Оценить бота"  },
             })
            {
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose a category",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
                 // first row
                 new []{InlineKeyboardButton.WithCallbackData(text: "Фломастеры 💳", callbackData: "Рисование"),},
                 new []{InlineKeyboardButton.WithCallbackData(text: "Карандаши 💳", callbackData: "Рисование"),},
                 new []{InlineKeyboardButton.WithCallbackData(text: "Альбом 💳", callbackData: "Рисование"),},
                 new []{InlineKeyboardButton.WithCallbackData(text: "Краски 💳", callbackData: "Рисование"),},
                 new []{InlineKeyboardButton.WithCallbackData(text: "Купить всё 💳 💳 💳", callbackData: "Всё"),},
             });

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❓ Вот товары из списка Для Рукоделия. Чтобы купить товар, просто нажми на него!\n" +
                "❗️ Можно так же купить всё сразу, нажав на 💳 💳 💳\n" +
                "А ещё можно дать доступ к списку другим с помощью команды /access",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            { 
                ApiRequestException apiRequestException 
                   => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
               _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}