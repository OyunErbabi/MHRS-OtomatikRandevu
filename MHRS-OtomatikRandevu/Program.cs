using MHRS_OtomatikRandevu.Models;
using MHRS_OtomatikRandevu.Models.RequestModels;
using MHRS_OtomatikRandevu.Models.ResponseModels;
using MHRS_OtomatikRandevu.Services;
using MHRS_OtomatikRandevu.Services.Abstracts;
using MHRS_OtomatikRandevu.TelegramBotService;
using MHRS_OtomatikRandevu.Urls;
using MHRS_OtomatikRandevu.Utils;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace MHRS_OtomatikRandevu
{
    public class Program
    {
        public static string TC_NO;
        public static string SIFRE;
        static string TelegramBotToken;

        const string TOKEN_FILE_NAME = "token.txt";
        static string JWT_TOKEN;
        static DateTime TOKEN_END_DATE;

        static IClientService _client;
        static TelegramBotManager _telegramBotManager;
        public static LocalDataManager _localDataManager;
        public static List<GenericResponseModel> provinceList;
        public static List<GenericResponseModel> districtList;
        public static List<GenericResponseModel> clinicList;
        public static List<GenericResponseModel> hospitalList;
        public static List<ClinicResponseModel> placeList;
        public static List<GenericResponseModel> doctorList;

        static Int32 provinceIndex;
        static Int32 districtIndex;
        static Int32 clinicIndex;
        static Int32 hospitalIndex;
        static Int32 placeIndex;
        static Int32 doctorIndex;


        static void Main(string[] args)
        {
            _client = new ClientService();
            _telegramBotManager = new TelegramBotManager();
            _localDataManager = new LocalDataManager();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("MHRS Otomatik Randevu Sistemine Hoşgeldiniz.");
            Console.WriteLine("");
            Console.WriteLine(String.Format("Yeni Bir Telegram Bot'u Oluşturmak İçin Aşağıdaki Adrese Gidin: \n{0}",TelegramUrls.BotFatherUrl));
            Console.WriteLine("");

            if(_localDataManager.credentials.TelegramApiKey.ToLower() != "")
            {
                Console.WriteLine("Kayıtlı Telegram Bot Api Keyiniz: " + _localDataManager.credentials.TelegramApiKey);
                
                if(_telegramBotManager.TestApiKey(_localDataManager.credentials.TelegramApiKey))
                {
                    BotStarted();
                }
                else
                {
                    Console.WriteLine("Kayıtlı API Anahtarı Geçersiz! Lütfen Tekrar Girin.");
                }
            } 
            else
            {
                bool isValidApiKey = false;
                while (!isValidApiKey)
                {
                    Console.WriteLine("Telegram Bot Api Keyinizi Girin:");
                    TelegramBotToken = Console.ReadLine();

                    if (_telegramBotManager.TestApiKey(TelegramBotToken))
                    {
                        BotStarted();
                        isValidApiKey = true;
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Geçersiz API Anahtarı! Tekrar Deneyin.");
                    }

                }
            }
           
            _localDataManager.SaveData();

            Console.ReadKey();
        }

        static void BotStarted()
        {
            Console.Clear();
            Console.WriteLine($"Bot Bulundu Ve Başlatıldı: {_telegramBotManager.GetBotUsername()}");
            string finalMessage = $"Bot Kurulumu Başarılı.\n\nLÜTFEN BU PROGRAMI KAPATMAYIN!\n\nLütfen {TelegramUrls.BaseBotUrl}{_telegramBotManager.GetBotUsername()} Adresine Gidin Ve /start Mesajı İle Telegram Üzerinden Devam Edin.";
            Console.WriteLine(finalMessage);
            Console.WriteLine("BOTU SIFIRLAMAK İÇİN DOSYALAR İÇİNDEKİ 'credentials.json' DOSYASINI SİLİN VE PROGRAMI TEKRAR BAŞLATIN");

            Console.WriteLine($"Telegram Aktivasyon Kodunuz : {_telegramBotManager.ActvationCode}");
        }

        public static void LoginPhese()
        {
            var tokenData = GetToken(_client);
            if (tokenData == null || string.IsNullOrEmpty(tokenData.Token))
            {
                _telegramBotManager.WrongPasswordOrIdEntered();
            }
            else
            {
                JWT_TOKEN = tokenData.Token;
                TOKEN_END_DATE = tokenData.Expiration;

                _client.AddOrUpdateAuthorizationHeader(JWT_TOKEN);
                GetAllGetProvinces();
            }
             
        }

        static void GetAllGetProvinces()
        {
            
            var provinceListResponse = _client.GetSimple<List<GenericResponseModel>>(MHRSUrls.BaseUrl, MHRSUrls.GetProvinces);
            if (provinceListResponse == null || !provinceListResponse.Any())
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }
            provinceList = provinceListResponse
                                    .DistinctBy(x => x.Value)
                                    .OrderBy(x => x.Value)
                                    .ToList();
            _telegramBotManager.AskProvince(provinceList);
        }

        public static void GetDistricts(int _provinceIndex)
        {
            if(provinceList.IsNullOrEmpty())
            {
                return;
            }

            provinceIndex = _provinceIndex;

            districtList = _client.GetSimple<List<GenericResponseModel>>(MHRSUrls.BaseUrl, string.Format(MHRSUrls.GetDistricts, provinceIndex));
            if (districtList == null || !districtList.Any())
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }

            _telegramBotManager.AskDistrict(districtList);
        }


        public static async Task GetClinics(int district)
        {
            districtIndex = district;

            if (districtList.IsNullOrEmpty())
            {
                return;
            }

            if (districtIndex != 0)
                districtIndex = districtList[districtIndex].Value;
            else
                districtIndex = -1;

            var clinicListResponse = await _client.GetAsync<List<GenericResponseModel>>(MHRSUrls.BaseUrl, string.Format(MHRSUrls.GetClinics, provinceIndex, districtIndex));
            if (!clinicListResponse.Success && (clinicListResponse.Data == null || !clinicListResponse.Data.Any()))
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }
            else
            {
                Console.WriteLine("Başarılı: " + clinicListResponse.Data.Count);
            }

            clinicList = clinicListResponse.Data;
            _telegramBotManager.AskClinic(clinicList);
            
        }
        
        public static async Task GetHospitals(int clinic)
        {   
            if (clinicList.IsNullOrEmpty())
            {
                Console.WriteLine("Klinik Listesi Boş");
                return;
            }

            if (clinic != 0)
                clinicIndex = clinicList[clinic-1].Value;
            else
                clinicIndex = -1;

            var hospitalListResponse = _client.Get<List<GenericResponseModel>>(MHRSUrls.BaseUrl, string.Format(MHRSUrls.GetHospitals, provinceIndex, districtIndex, clinicIndex));
            if (!hospitalListResponse.Success && (hospitalListResponse.Data == null || !hospitalListResponse.Data.Any()))
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }
            else
            {
                Console.WriteLine("Başarılı: " + hospitalListResponse.Data.Count);
            }
            hospitalList = hospitalListResponse.Data;

            _telegramBotManager.AskHospital(hospitalList);
        }

        public static async Task GetPlace(int place)
        {
            if (hospitalList.IsNullOrEmpty())
            {
                Console.WriteLine("Hastane Listesi Boş");
                return;
            }

            hospitalIndex = hospitalList[place].Value;

            var placeListResponse = _client.Get<List<ClinicResponseModel>>(MHRSUrls.BaseUrl, string.Format(MHRSUrls.GetPlaces, hospitalIndex, clinicIndex));
            if (!placeListResponse.Success && (placeListResponse.Data == null || !placeListResponse.Data.Any()))
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }

            placeList = placeListResponse.Data;
            foreach (var item in placeList)
            {
                Console.WriteLine("Muayne Yeri: " + item.Text);
            }
            _telegramBotManager.AskPlace(placeList);
        }

        public static async Task GetDoctors(int place)
        {
            if (placeList.IsNullOrEmpty())
            {
                Console.WriteLine("Hastane Listesi Boş");
                return;
            }

            if (placeIndex != 0)
                placeIndex = placeList[place - 1].Value;
            else
                placeIndex = -1;

            foreach (var item in placeList)
            {
                Console.WriteLine("Hastane: " + item.Text);
                Console.WriteLine("Hastane Kodu: " + item.Value);
            }


            var doctorListResponse = _client.Get<List<GenericResponseModel>>(MHRSUrls.BaseUrl, string.Format(MHRSUrls.GetDoctors, hospitalIndex, clinicIndex));
            if (!doctorListResponse.Success && (doctorListResponse.Data == null || !doctorListResponse.Data.Any()))
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return;
            }

            doctorList = doctorListResponse.Data;
            foreach (var item in doctorList)
            {
                Console.WriteLine("Doktor: " + item.Text);
            }
            _telegramBotManager.AskDoctor(doctorList);
        }

        public static async Task GetAppointment(int DoctorIndex)
        {
            if (doctorIndex != 0)               
               doctorIndex = doctorList[DoctorIndex-1].Value;
            else
                doctorIndex = -1;


            bool appointmentState = false;
            bool isNotified = false;
            _telegramBotManager.SendMessage("Randevu aranıyor. Uygun randevu bulunduğunda buradan bilgilendirileceksiniz. Lütfen bu bot'un çalıştığı bilgisayarı kapatmayınız.", long.Parse(_localDataManager.credentials.AuthenticatedTelegramUserId));
            do
            {
                if (TOKEN_END_DATE == default || TOKEN_END_DATE < DateTime.Now)
                {
                    var tokenData = GetToken(_client);
                    if (string.IsNullOrEmpty(tokenData.Token))
                    {
                        ConsoleUtil.WriteText("Yeniden giriş yapılırken bir hata meydana geldi!", 2000);
                        return;
                    }
                    JWT_TOKEN = tokenData.Token;
                    _client.AddOrUpdateAuthorizationHeader(JWT_TOKEN);
                }

                var slotRequestModel = new SlotRequestModel
                {
                    MhrsHekimId = doctorIndex,
                    MhrsIlId = provinceIndex,
                    MhrsIlceId = districtIndex,
                    MhrsKlinikId = clinicIndex,
                    MhrsKurumId = hospitalIndex,
                    MuayeneYeriId = placeIndex
                };

                var slot = GetSlot(_client, slotRequestModel);
                if (slot == null || slot == default)
                {
                    Console.WriteLine($"Müsait randevu bulunamadı | Kontrol Saati: {DateTime.Now.ToShortTimeString()}");
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                    continue;
                }

                var appointmentRequestModel = new AppointmentRequestModel
                {
                    FkSlotId = slot.Id,
                    FkCetvelId = slot.FkCetvelId,
                    MuayeneYeriId = slot.MuayeneYeriId,
                    BaslangicZamani = slot.BaslangicZamani,
                    BitisZamani = slot.BitisZamani
                };

                Console.WriteLine($"Randevu bulundu - Müsait Tarih: {slot.BaslangicZamani}");
                if (!isNotified)
                {
                    _telegramBotManager.SendMessage($"Randevu bulundu - Müsait Tarih: {slot.BaslangicZamani}",long.Parse(_localDataManager.credentials.AuthenticatedTelegramUserId));
                    
                }
               
                
                Console.WriteLine("Id: "+slot.Id);
                Console.WriteLine("FkCetvelId: "+slot.FkCetvelId);
                Console.WriteLine("MuayeneYeriId: "+slot.MuayeneYeriId);
                Console.WriteLine("Başlangıç Zamanı: "+slot.BaslangicZamani);
                Console.WriteLine("Bitiş Zamanı: "+slot.BitisZamani);
                
                Thread.Sleep(TimeSpan.FromMinutes(1));


                appointmentState = MakeAppointment(_client, appointmentRequestModel, true);
            } while (!appointmentState);
        }


        static JwtTokenModel GetToken(IClientService client)
        {            
            var tokenData = _localDataManager.credentials.TokenData;
            try
            {   
                tokenData = _localDataManager.credentials.TokenData;
                if (string.IsNullOrEmpty(tokenData) || JwtTokenUtil.GetTokenExpireTime(tokenData) < DateTime.Now)
                    throw new Exception();                
                return new() { Token = tokenData, Expiration = JwtTokenUtil.GetTokenExpireTime(tokenData) };
            }
            catch (Exception)
            {
                var loginRequestModel = new LoginRequestModel
                {
                    KullaniciAdi = TC_NO,
                    Parola = SIFRE
                };

                var loginResponse = client.Post<LoginResponseModel>(MHRSUrls.BaseUrl, MHRSUrls.Login, loginRequestModel).Result;
                if (loginResponse.Data == null || (loginResponse.Data != null && string.IsNullOrEmpty(loginResponse.Data?.Jwt)))
                {                    
                    return null;
                }

                if (!string.IsNullOrEmpty(tokenData))
                {
                    _localDataManager.credentials.TokenData = loginResponse.Data!.Jwt;
                    _localDataManager.SaveData();
                }
                return new() { Token = loginResponse.Data!.Jwt, Expiration = JwtTokenUtil.GetTokenExpireTime(loginResponse.Data!.Jwt) };
            }

        }
                
        static SubSlot GetSlot(IClientService client, SlotRequestModel slotRequestModel)
        {
            var slotListResponse = client.Post<List<SlotResponseModel>>(MHRSUrls.BaseUrl, MHRSUrls.GetSlots, slotRequestModel).Result;
            if (slotListResponse.Data is null)
            {
                ConsoleUtil.WriteText("Bir hata meydana geldi!", 2000);
                return null;
            }

            var saatSlotList = slotListResponse.Data.FirstOrDefault()?.HekimSlotList.FirstOrDefault()?.MuayeneYeriSlotList.FirstOrDefault()?.SaatSlotList;
            if (saatSlotList == null || !saatSlotList.Any())
                return null;

            var slot = saatSlotList.FirstOrDefault(x => x.Bos)?.SlotList.FirstOrDefault(x => x.Bos)?.SubSlot;
            if (slot == default)
                return null;

            return slot;
        }

        static bool MakeAppointment(IClientService client, AppointmentRequestModel appointmentRequestModel, bool sendNotification)
        {
            var randevuResp = client.PostSimple(MHRSUrls.BaseUrl, MHRSUrls.MakeAppointment, appointmentRequestModel);
            if (randevuResp.StatusCode != HttpStatusCode.OK)
            {
                _telegramBotManager.SendMessage($"Randevu alırken bir problem ile karşılaşıldı! \nRandevu Tarihi -> {appointmentRequestModel.BaslangicZamani}", long.Parse(_localDataManager.credentials.AuthenticatedTelegramUserId));
                return false;
            }

            var message = $"Randevu alındı! \nRandevu Tarihi -> {appointmentRequestModel.BaslangicZamani}";
            Console.WriteLine(message);

            
            string appointmentMessage = string.Format("Randevu alındı! \nRandevu Tarihi -> {0}", appointmentRequestModel.BaslangicZamani);
            _telegramBotManager.SendMessage(appointmentMessage, long.Parse(_localDataManager.credentials.AuthenticatedTelegramUserId));

            return true;
        }

    }
}
