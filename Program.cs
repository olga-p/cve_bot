using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;

namespace Bot
{
    internal class Program
    {
        private const string _botToken = "5387781101:AAGBdqrUJxpXXPhvbxmYjXEODEk9IT8mhWA";

        private static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            var bot = new TelegramBotClient(_botToken);
            var me = await bot.GetMeAsync();

            bot.StartReceiving(HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>(),
                },
                cts.Token);

             Console.ReadLine();
            cts.Cancel();

        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await BotOnMessageReceived(botClient, update.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            if (message.Type != MessageType.Text || message.From.IsBot) return;

            var cveCol = new CVECollection();
            Regex regex = new Regex(@"CVE-[0-9]{4}-[0-9]+");

            switch (message.Text)
            {
                case "/start":
                    ReplyKeyboardMarkup keyboard = new(new[]
                    {
                        new KeyboardButton[] {"Загрузить новые CVE в БД"},
                        new KeyboardButton[] {"Найти CVE по ID", "Найти CVE по ключевому слову" }
                    })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Вас приветствует CVE-бот. Выберите действие:", replyMarkup: keyboard);
                    break;
                case "Найти CVE по ID":
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введите CVE Id:", replyMarkup: new ForceReplyMarkup());                  
                    break;
                case "Найти CVE по ключевому слову":
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введите ключевое слово:", replyMarkup: new ForceReplyMarkup());
                    break;
                case "Загрузить новые CVE в БД":
                    var count = cveCol.GetNewCves();
                    if (count > 0)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Готово! Загружено {count} новых CVE");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Новых CVE не найдено");
                    }
                    break;
                default:
                    CreateKeyboard(botClient, message.Chat.Id);
                    break;
            }

            if(message.ReplyToMessage != null)
            {
                var notFoundMessage = "По вашему запросу ничего не найдено";
                MatchCollection matches = regex.Matches(message.Text);
                if (matches.Count > 0)
                {
                    var wantedCve = cveCol.GetCVEById(message.Text);
                    if(wantedCve == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, notFoundMessage);
                        CreateKeyboard(botClient, message.Chat.Id);
                        return;
                    }
                    await botClient.SendTextMessageAsync(message.Chat.Id, wantedCve.PrintCVEInfo());
                    CreateKeyboard(botClient, message.Chat.Id);
                    return;

                }
                var wantedCves = cveCol.GetCVEByKeyword(message.Text);
                if (wantedCves.Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, notFoundMessage);
                    CreateKeyboard(botClient, message.Chat.Id);
                    return;
                }
                foreach (var cve in wantedCves)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, cve.PrintCVEInfo());
                    
                }
                CreateKeyboard(botClient, message.Chat.Id);
            }

        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }

        public static async void CreateKeyboard(ITelegramBotClient botClient, long chatId)
        {
            ReplyKeyboardMarkup keyboard = new(new[]
{
                        new KeyboardButton[] {"Загрузить новые CVE в БД"},
                        new KeyboardButton[] {"Найти CVE по ID", "Найти CVE по ключевому слову" }
                    })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(chatId, $"Выберите действие:", replyMarkup: keyboard);
        }


    }
}