using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Credentials
{
    public string TelegramApiKey { get; set; }
    public string AuthenticatedTelegramUserId { get; set; }
    public string TokenData { get; set; }
}

public class LocalDataManager
{
    public Credentials credentials;

    public LocalDataManager()
    {
        credentials = new Credentials
        {
            TelegramApiKey = "",
            AuthenticatedTelegramUserId = "",
            TokenData = ""
        };

        LoadData();
    }

    public void SetTelegramApiKey(string apiKey)
    {
        credentials.TelegramApiKey = apiKey;
    }

    public void SaveData()
    {
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

    public void LoadData()
    {
        string filePath = "credentials.json";

        try
        {
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                credentials = JsonConvert.DeserializeObject<Credentials>(jsonData);
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

    public bool IsAuthenticated()
    {
        if(credentials.AuthenticatedTelegramUserId.ToLower() == "null")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SetAuthenticatedTelegramUserId(string userId)
    {
        credentials.AuthenticatedTelegramUserId = userId;
        SaveData();
    }

}

