using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MHRS_OtomatikRandevu.TelegramBotService
{
    internal class TelegramBotManager
    {
        public TelegramBotClient telegramBotClient;
        string TelegramBotName = string.Empty;

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
                    //Console.WriteLine($"Bot Bulundu Ve Başlatıldı: {botInfo.Username}");

                    TelegramBotName = botInfo.Username;
                    result = true;
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

        

    }
}
