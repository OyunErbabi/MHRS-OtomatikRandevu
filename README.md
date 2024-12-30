![Banner](https://i.hizliresim.com/jv6ah3o.jpg)
# MHRS Otomatik Randevu
Gerekli kurulumları yaptıktan sonra MHRS kullanıcı bilgileriniz ile giriş yapıp İl-İlçe-Klinik-Doktor gibi filtrelemeler yaparak aradığınız randevuyu otomatik olarak alabilirsiniz.
Bu program telegram bot'u ile birlikte çalışmaktadır. Bu sayede bilgisayarınızdan uzakta olsanız bile telefon üzerinden randevu arayabilir ve uygun randevuyu aldıktan sonra bilgilendirilirsiniz.

## Neden Buna İhtiyaç Duyuldu?
Devlet hastanelerinde randevu bulmak oldukça zorlaştı, MHRS'nin vatandaşlara sunmuş olduğu randevu müsaitlik bildirim sistemi yeteri kadar hızlı ve sağlıklı çalışmadığı için bu konsol uygulaması geliştirildi.

## Randevu Tarama Sıklığı Nedir?
Aradığınız kriterlere uygun randevu sisteme düştüğünde 5 dakika içerisinde otomatik olarak randevu tarafınıza bildirilir ve alınır.

## Telegram Bot Sistemi Nasıl İşlemektedir?
Ücretsiz ve kolay bir şekilde Telegram üzerinden [BotFather](https://telegram.me/BotFather) ile kendinize ait bir bot oluşturduktan sonra hem randevu alma işlemi hem de uygun randevu bulunduğunda bildirim almak için kullanılır.

## Neden Telegram Botunu Kendim Oluşturmam Gerekiyor?
Kişisel verilerinizi sadece size ait olan bir bot ile paylaşmanız aklınızda soru işareti bırakmayacak şekilde güvenliğinizi sağlamak için düşünülmüştür. Ayrıca bu proje açık kaynaklı olduğu için teknik olarak programın nasıl çalıştığını gönül rahatlığıyla inceleyebilirsiniz.

## Uygulamanın Kurulumu
1-[Releases](https://github.com/OyunErbabi/MHRS-OtomatikRandevu/releases) kısmından mevcut versiyonu indirip zip dosyasını herhangi bir konuma çıkartın.

2-MHRS-OtomatikRandevu.exe ile programı başlatın.

3-Eğer ilk defa kurulum yapıyorsanız telegramdan @BotFather [BotFather](https://telegram.me/BotFather) adresine gidiniz.

	3.1-BotFather'a "/newbot" komutu girerek yeni bir bot oluşturun.	 
 	3.2-Oluşturmak istediğiniz bota bir isim verin
    3.3-Oluşturmak istediğiniz bota bir kullanıcı ismi verin(Bot kelimesi ile bitmek zorundadır! Örn: TetrisBot veya tetris_bot)
    3.4-Bot'u başarılı bir şekilde oluşturduğunuzda size bir token vermektedir. Bu tokeni programa giriniz. (Örn: 0123456789:XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX)
	
4-Bir önceki adımda oluşturduğunuz botun tokenini programa giriniz ve işlemlerinize telegram üzerinden devam ediniz.

5-Telegram üzerinden oluşturuduğunuz bota ulaşmak için 3.3 adımında verdiğiniz kullanıcı adını aratarak ulaşın ve \start komutunu giriniz.

6-İlk defa bot ile etkileşim kurmak için masaüstü programının size verdiği "Telegram Aktivasyon Kodunu" mesaj olarak gönderiniz. Bu adımda numaranız bot ile eşleşmekte ve başka bir numaradan gelen taleplere bot yanıt vermemektedir.

7-Eşleşme tamamlandıktan sonra artık Tc No ve MHRS şifreniz(E-Devlet şifreniz değil!) ile giriş adımlarını tamamlayarak İl-İlçe-Hastane-Klinik-Doktor adımları ile randevu arama işlemini başlatabilirsiniz.

## Uygulamayı Sıfırlama
Eğer başka bir telefon numarası ile eşleştirme yapmak istiyorsanız programın içinde bulunan "credentials.json" dosyasını siliniz. Bu sayede kurulum adımlarından 4. adıma dönmüş olacaksınız.

## Sorumluluk Reddi
Bu açık kaynaklı bir projedir ve içerisinde bir çok hata bulunabilmektedir. Bu programın kullanımından dolayı oluşabilecek her türlü maddi ve manevi zarardan sadece kullanıcı sorumludur ve bu sorumluluğu kabul ederek programı kullanmaktadır. Oluşabilecek herhangi bir zarardan projenin geliştirilmesine katkı sağlayan hiçkimse sorumlu tutulamaz.
