using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace MHRS_OtomatikRandevu.TelegramBotService
{
    internal class TelegramBotManager
    {
        public TelegramBotClient telegramBotClient;
        public CancellationToken _cancelToken;
        string TelegramBotName = string.Empty;
        public int ActvationCode = 0;
        public string BotToken = string.Empty;



        public bool TestApiKey(string apiKey)
        {
            bool result = false;
            telegramBotClient = new TelegramBotClient(apiKey);

            Console.WriteLine("Telegram API Key Test Ediliyor...");

            try
            {
                if (telegramBotClient.TestApiAsync().Result)
                {
                    var botInfo = telegramBotClient.GetMeAsync().Result;
                    TelegramBotName = botInfo.Username;
                    BotToken = apiKey;
                    Program._localDataManager.SetTelegramApiKey(apiKey);
                    CreateActivationCode();
                    result = true;
                    Start();
                }
                else
                {
                    Console.WriteLine("Telegram API Key Geçersiz.");
                }
            }
            catch (Exception)
            {
                result = false;
                Console.WriteLine("Telegram API Key Geçersiz.");
            }

            return result;
        }

        public string GetBotUsername()
        {
            if(TelegramBotName != string.Empty)
            {
                return TelegramBotName;
            }
            else
            {
                string botName = string.Empty;

                if (telegramBotClient == null)
                {
                    var botInfo = telegramBotClient.GetMeAsync().Result;
                    botName = botInfo.Username;
                }
                else
                {
                    botName = "Bot Bulunamadı";
                }

                return botName;
            }
            
        }

        public void CreateActivationCode()
        {
            Random random = new Random();
            ActvationCode = random.Next(100000, 999999);
        }

        public async void Start()
        {

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };


            telegramBotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            ); ;

            _cancelToken = cts.Token;

            var me = await telegramBotClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {

                if (update.CallbackQuery != null)
                {
                    BotOnCallbackQueryReceived(update.CallbackQuery, cancellationToken);
                }

                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                /*
                switch (messageText.ToLower(new CultureInfo("en-US")))
                {
                    case "/start":
                        
                        break;
                    case "/register":
                        

                        break;
                    case "/setup":
                        
                        break;
                    case "/testconnection":

                        break;
                    case "/help":
                        SendMessage("HELP WORK IN PROGRESS", chatId);
                        break;
                    case string data when data.Contains("/adminmessage"):

                        //SendMessage("Received Admin Command " + data, chatId);

                        string pattern = @"^/adminmessage ""(.*?)"" (\d+)$";
                        Match match = Regex.Match(data, pattern);

                        if (match.Success)
                        {
                            string _message = match.Groups[1].Value;
                            long number = long.Parse(match.Groups[2].Value);

                            Console.WriteLine("Message: " + _message);
                            Console.WriteLine("Number: " + number);

                            SendMessage(_message, number);
                        }
                        else
                        {
                            Console.WriteLine("Invalid command format.");
                        }

                        break;
                    default:


                        break;
                }
                */

                

                if (messageText == "/start")
                {

                    InlineKeyboardMarkup inlineKeyboard = new(
                     new[]
                     {
                        // first row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("1.1","13"),
                            InlineKeyboardButton.WithCallbackData("1.2", "12"),
                        },
                        // second row
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("2.1", "21"),
                            InlineKeyboardButton.WithCallbackData("2.2", "22"),
                        },
                     });


                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "A message with an inline keyboard markup",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);

                }




                //// Echo received message text
                //Message sentMessage = await botClient.SendTextMessageAsync(
                //    chatId: chatId,
                //    text: "You said:\n" + messageText,
                //    cancellationToken: cancellationToken);
            }

        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            Console.WriteLine("ID " + callbackQuery.From.Id);
            Console.WriteLine("CallBack Data " + callbackQuery.Data);


            /*
            bool timeDataSelected = false;
            UserConfig _userConfig = new UserConfig();
            _userConfig.UserId = callbackQuery.From.Id;

            string AnswerText = "";
            switch (callbackQuery.Data)
            {
                case var str when Regex.IsMatch(str, "^wh"):


                    if (str.StartsWith("wh_"))
                    {
                        string remainingText = str.Substring(3);

                        if (remainingText == "save")
                        {
                            AnswerText = "👍 Save Successful";
                            DeleteMessage(callbackQuery);
                            UserActions.SetWorkTime(_userConfig.UserId.ToString());
                        }
                        else
                        {
                            AnswerText = "Warehouse Selected";
                            UserActions.UpdateServiceAreaList(_userConfig.UserId.ToString(), remainingText, callbackQuery);
                        }
                    }

                    break;
                case var str when Regex.IsMatch(str, "^tz"):
                    Console.WriteLine("Metin 'tz' ile başlıyor.");
                    // 'tz' ile başlayan metin için yapılacak işlemler
                    break;
                case var str when Regex.IsMatch(str, "^wt"):

                    if (str.StartsWith("wt_"))
                    {
                        string workTimeText = str.Substring(3);

                        if (workTimeText == "save")
                        {
                            AnswerText = "👍 Save Successful";
                            DeleteMessage(callbackQuery);
                            UserActions.SetMinimumWorkStartTime(_userConfig.UserId.ToString());
                        }
                        else
                        {
                            AnswerText = "WorkTime Selected";
                            UserActions.UpdateWorktime(_userConfig.UserId.ToString(), workTimeText, callbackQuery);
                        }
                    }


                    break;
                case var str when Regex.IsMatch(str, "^mt"):

                    if (str.StartsWith("mt_"))
                    {
                        string workTimeText = str.Substring(3);

                        if (workTimeText == "save")
                        {
                            AnswerText = "👍 Save Successful";
                            DeleteMessage(callbackQuery);
                            //UserActions.SetMinimumWorkStartTime(_userConfig.UserId.ToString());
                        }
                        else
                        {
                            AnswerText = "WorkTime Selected";
                            //UserActions.UpdateWorktime(_userConfig.UserId.ToString(), workTimeText, callbackQuery);
                        }
                    }
                    break;


                default:
                    switch (callbackQuery.Data)
                    {
                        case string data when data.Contains("PDT"):
                            _userConfig.TimeZone = UserTimeZone.PDT;
                            timeDataSelected = true;
                            //Console.WriteLine("PDT SELECTED!");
                            UserActions.UpdateUserConfig(_userConfig);
                            DeleteMessage(callbackQuery);
                            break;

                        case string data when data.Contains("MDT"):
                            _userConfig.TimeZone = UserTimeZone.MDT;
                            timeDataSelected = true;
                            //Console.WriteLine("MDT SELECTED!");
                            UserActions.UpdateUserConfig(_userConfig);
                            DeleteMessage(callbackQuery);
                            break;

                        case string data when data.Contains("CDT"):
                            _userConfig.TimeZone = UserTimeZone.CDT;
                            timeDataSelected = true;
                            //Console.WriteLine("CDT SELECTED!");
                            UserActions.UpdateUserConfig(_userConfig);
                            DeleteMessage(callbackQuery);
                            break;

                        case string data when data.Contains("EDT"):
                            _userConfig.TimeZone = UserTimeZone.EDT;
                            timeDataSelected = true;
                            //Console.WriteLine("EDT SELECTED!");
                            UserActions.UpdateUserConfig(_userConfig);
                            DeleteMessage(callbackQuery);
                            break;

                            //await botClient.DeleteMessageAsync(
                            //chatId: callbackQuery.From.Id,
                            //messageId: callbackQuery.Message.MessageId,
                            //cancellationToken: cancellationToken
                            //);
                    }
                    break;

            }
            */

            string AnswerText = "Answer Comes Here";

            if (AnswerText.Length > 0)
            {
                await telegramBotClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: AnswerText,
                cancellationToken: cancellationToken);
            }


            //await botClient.SendTextMessageAsync(
            //    chatId: callbackQuery.Message!.Chat.Id,
            //    text: $"Received {callbackQuery.Data}",
            //    cancellationToken: cancellationToken);

            //InlineKeyboardMarkup updatedInlineKeyboard = null;

            //await botClient.EditMessageReplyMarkupAsync(
            //chatId: callbackQuery.From.Id,
            //messageId: callbackQuery.Message.MessageId,
            //replyMarkup: updatedInlineKeyboard,
            //cancellationToken: cancellationToken
            //);




        }

        public async void SendMessage(string message, long _chatId)
        {
            try
            {
                var botClient = new TelegramBotClient(BotToken);
                var me = await botClient.GetMeAsync();

                Message sentMessage = await botClient.SendTextMessageAsync(
                           chatId: _chatId,
                           text: message);
            }
            catch (ApiRequestException e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public async void DeleteMessage(long chatId, int messageId, CancellationToken cancellationToken)
        {
            try
            {
                await telegramBotClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
                Console.WriteLine("Mesaj başarıyla silindi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mesaj silinirken bir hata oluştu: " + ex.Message);
            }
        }

    }
}
