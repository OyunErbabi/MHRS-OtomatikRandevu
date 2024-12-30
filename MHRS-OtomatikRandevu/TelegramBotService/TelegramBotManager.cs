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
using System.Threading;
using MHRS_OtomatikRandevu.Models.ResponseModels;

namespace MHRS_OtomatikRandevu.TelegramBotService
{
    internal class TelegramBotManager
    {
        public TelegramBotClient telegramBotClient;
        public CancellationToken _cancelToken;
        string TelegramBotName = string.Empty;
        public int ActvationCode = 0;
        public string BotToken = string.Empty;

        bool Started = false;

        bool waitingForActivationCode = false;
        bool waitingTCNumber = false;
        bool waitingPassword = false;

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
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
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
                        
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {

                if (update.CallbackQuery != null)
                {
                    BotOnCallbackQueryReceived(update.CallbackQuery, cancellationToken);
                }

                
                if (update.Message is not { } message)
                    return;
                
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                
                
                switch (messageText.ToLower(new CultureInfo("en-US")))
                {
                    case "/start":
                        Started = true;                        
                        if (Program._localDataManager.IsAuthenticated())
                        {
                            if(chatId.ToString() != Program._localDataManager.credentials.AuthenticatedTelegramUserId)
                            {
                                SendMessage("Bu Bot Sadece Bir Telegram Hesabı İle Kullanılabilmektedir! Yeni Kurulum İçin Bilgisayarınızdaki Programı Sıfırlayın", chatId);
                            }
                            else
                            {
                                SendMessage("Hoş Geldiniz\nGiriş Yapmak İçin MHRS TC Kimlik Numaranızı Girin:", chatId);
                                waitingTCNumber = true;
                            }
                        }
                        else
                        {
                            SendMessage("Lütfen Aktivasyon Kodunuzu Giriniz:", chatId);
                            waitingForActivationCode = true;
                        }

                        break;
                    default:
                        if (waitingForActivationCode)
                        {
                            if (messageText == ActvationCode.ToString())
                            {
                                SendMessage("Aktivasyon Başarılı ✅\nLütfen tekrar /start komutu ile kuruluma devam ediniz.", chatId);
                                Program._localDataManager.SetAuthenticatedTelegramUserId(chatId.ToString());
                                waitingForActivationCode = false;
                            }
                            else
                            {
                                SendMessage("Kod Hatalı⛔\nLütfen Tekrar Aktivasyon Kodunu Girin:", chatId);
                            }
                        }
                        else
                        {
                            if (chatId.ToString() != Program._localDataManager.credentials.AuthenticatedTelegramUserId)
                            {
                                SendMessage("Bu Bot Sadece Bir Telegram Hesabı İle Kullanılabilmektedir! Yeni Kurulum İçin Bilgisayarınızdaki Programı Sıfırlayın", chatId);
                            }
                            else
                            {
                                if (waitingTCNumber)
                                {
                                    if(messageText.ToString().Length != 11)
                                    {
                                        SendMessage("TC Kimlik Numarası 11 Haneli Olmalıdır. Lütfen Tekrar Girin ⛔", chatId);
                                    }
                                    else
                                    {
                                        Program.TC_NO = messageText;
                                        waitingTCNumber = false;
                                        waitingPassword = true;
                                        SendMessage("MHRS Şifrenizi Girin:", chatId);
                                    }
                                }
                                else if (waitingPassword)
                                {
                                    Program.SIFRE = messageText;
                                    waitingPassword = false;
                                    Program.LoginPhese();
                                }
                                else
                                {
                                    SendMessage(messageText + "Bilinmeyen Komut\nLütfen /start Komutu İle Başlayın.", chatId);
                                }
                             }
                        }
                        break;
                }

            }

        }

        public async void AskProvince(List<GenericResponseModel> provinceList)
        {
            int columns = 3;
            int rows = (int)Math.Ceiling((double)provinceList.Count / columns);

         
            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {         
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < provinceList.Count)
                    {
                        var item = provinceList[index];
                        string _buttonData = String.Format("province_{0}", (index+1));
                        inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                    }
                    else
                    {
                        inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(" ", " "); // Boş buton
                    }
                }
            }

            
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Randevu Almak İstediğiniz İli Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);            
        }

        public async void AskIstanbulLocation()
        {
            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[3][];
            inlineKeyboard[0] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("İSTANBUL", "ist_340") };
            inlineKeyboard[1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("İSTANBUL AVRUPA", "ist_341") };
            inlineKeyboard[2] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("İSTANBUL ANADOLU", "ist_342") };

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "İstanbul İçin Kıta Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }

        public async void AskDistrict(List<GenericResponseModel> districtList)
        {
            
            districtList.Insert(0, new GenericResponseModel { Text = "FARKETMEZ" });

            int columns = 3;
            int rows = (int)Math.Ceiling((double)districtList.Count / columns);

            
            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < districtList.Count)
                    {
                        var item = districtList[index];
                        string _buttonData = String.Format("district_{0}", index);                         
                        inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                    }
                    else
                    {
                        inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(" ", " ");
                    }
                }
            }


            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Randevu Almak İstediğiniz İlçeyi Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }

        public async void AskClinic(List<GenericResponseModel> ClinicList)
        {

            int columns = 1;
            int rows = ClinicList.Count;

            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    var item = ClinicList[row];
                    string _buttonData = String.Format("clinic_{0}", (row+1));                    
                    inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                }
            }

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Klinik Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }

        public async void AskHospital(List<GenericResponseModel> HospitalList)
        {
            HospitalList.Insert(0, new GenericResponseModel { Text = "FARKETMEZ" });

            int columns = 1;
            int rows = HospitalList.Count;

            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    var item = HospitalList[row];
                    string _buttonData = String.Format("hospital_{0}", row);
                    inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                }
            }

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Hastane Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }

        public async void AskPlace(List<ClinicResponseModel> placeList)
        {
            placeList.Insert(0, new ClinicResponseModel { Text = "FARKETMEZ" });

            int columns = 1;
            int rows = placeList.Count;

            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    var item = placeList[row];
                    string _buttonData = String.Format("place_{0}", row);
                    inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                }
            }

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Muayne Yeri Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }

        public async void AskDoctor(List<GenericResponseModel> DoctorList)
        {
            DoctorList.Insert(0, new GenericResponseModel { Text = "FARKETMEZ" });

            int columns = 1;
            int rows = DoctorList.Count;

            InlineKeyboardButton[][] inlineKeyboard = new InlineKeyboardButton[rows][];

            for (int row = 0; row < rows; row++)
            {
                inlineKeyboard[row] = new InlineKeyboardButton[columns];
                for (int col = 0; col < columns; col++)
                {
                    var item = DoctorList[row];
                    string _buttonData = String.Format("doctor_{0}", row);
                    inlineKeyboard[row][col] = InlineKeyboardButton.WithCallbackData(item.Text, _buttonData);
                }
            }

            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboard);

            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId),
                text: "Doktor Seçin:",
                replyMarkup: inlineKeyboardMarkup,
                cancellationToken: _cancelToken);
        }


        public void WrongPasswordOrIdEntered()
        {
            SendMessage("Hatalı TC Kimlik Numarası veya Şifre Girdiniz. Lütfen Tekrar Deneyin.", long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId));
            SendMessage("MHRS TC Kimlik Numaranızı Girin:", long.Parse(Program._localDataManager.credentials.AuthenticatedTelegramUserId));

            waitingTCNumber = true;
            waitingPassword = false;
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
            if (!Started)
            {
                SendMessage("Henüz giriş yapılmamış. Lütfen /start komutu ile giriş işlemini başlatın.", callbackQuery.Message.Chat.Id);
                return;
            }

            Console.WriteLine("CallBack Data " + callbackQuery.Data);
                        
            switch (callbackQuery.Data)
            {
                case var str when Regex.IsMatch(str, "^province"):

                    if (str.StartsWith("province_"))
                    {
                        string remainingText = str.Substring(9);

                        if (remainingText == "34")
                        {
                            DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                            AskIstanbulLocation();
                        }
                        else
                        {
                            Int32 province = Convert.ToInt32(remainingText);
                            DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                            Program.GetDistricts(province);
                        }
                    }
                    break;
                case var str when Regex.IsMatch(str, "^ist"):
                    Int32 subProvince = Convert.ToInt32(str.Substring(4));                    
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetDistricts(subProvince);
                    break;
                case var str when Regex.IsMatch(str, "^district"):
                    Int32 district = Convert.ToInt32(str.Substring(9));                    
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetClinics(district);
                    break;
                case var str when Regex.IsMatch(str, "^clinic"):
                    Console.WriteLine("Klinik tıklandı");
                    Int32 clinic = Convert.ToInt32(str.Substring(7));                    
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetHospitals(clinic);
                    break;
                case var str when Regex.IsMatch(str, "^hospital"):
                    Console.WriteLine("Hastane tıklandı");
                    Int32 hospital = Convert.ToInt32(str.Substring(9));
                    Console.WriteLine("Clicked Hospital: " + hospital);
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetPlace(hospital);
                    break;
                case var str when Regex.IsMatch(str, "^place"):
                    Console.WriteLine("Place tıklandı");
                    Int32 place = Convert.ToInt32(str.Substring(6));
                    Console.WriteLine("Clicked Place: " + place);
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetDoctors(place);
                    break;
                case var str when Regex.IsMatch(str, "^doctor"):
                    Console.WriteLine("Doktor tıklandı");
                    Int32 doctor = Convert.ToInt32(str.Substring(7));
                    Console.WriteLine("Clicked Doctor: " + doctor);
                    DeleteMessage(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
                    Program.GetAppointment(doctor);
                    break;
                default:
                    break;
            }

                    string AnswerText = "Başarılı";

            if (AnswerText.Length > 0)
            {
                await telegramBotClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: AnswerText,
                cancellationToken: cancellationToken);
            }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mesaj silinirken bir hata oluştu: " + ex.Message);
            }
        }

    }
}
