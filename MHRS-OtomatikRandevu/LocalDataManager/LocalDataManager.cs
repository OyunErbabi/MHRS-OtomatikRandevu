using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHRS_OtomatikRandevu.LocalDataManager
{
    internal class LocalDataManager
    {
        
        public class Credentials
        {
            public string TelegramApiKey { get; set; }
            public string AuthenticatedTelegramUserId { get; set; }
        }

        public static Credentials credentials;
        public static void SaveData()
        {

            credentials = new Credentials();
            credentials.TelegramApiKey = "API_KEY";
            credentials.AuthenticatedTelegramUserId = "USER_ID";

            string filePath = "credentials.json";

            try
            {
                string jsonData = JsonConvert.SerializeObject(credentials, Formatting.Indented);
                File.WriteAllText(filePath, jsonData);

                //Console.WriteLine("Verileriniz başarıyla kaydedildi.\n"+jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }

        public static void TestLoadData()
        {
            string filePath = "credentials.json";

            try
            {
                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    credentials = JsonConvert.DeserializeObject<Credentials>(jsonData);

                    Console.WriteLine("Your Data: " + jsonData);
                    Console.WriteLine("Telegram API Key: " + credentials.TelegramApiKey);
                }
                else
                {
                    Console.WriteLine("Dosya bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
            }
        }

    }
}
