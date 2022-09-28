using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

            var cveCol = new CVE_collection();

            cveCol.GetCVEById("CVE-2022-3272");

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
            if (message.Type != MessageType.Text) return;
            
            switch (message.Text)
            {


            }

        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }

     }
}