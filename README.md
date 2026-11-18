# YMH 219 - OpenCV ile Gerçek Zamanlı Yüz Algılama ve Tanıma Sistemi

C# (.NET WinForms) ve **OpenCvSharp4** kütüphanesi kullanılarak geliştirilmiş, gerçek zamanlı yüz algılama ve yerel eğitim tabanlı yüz tanıma masaüstü uygulamasıdır.

## Özellikler
- **Gerçek Zamanlı Yüz Algılama:** Haar Cascade sınıflandırıcısı (`haarcascade_frontalface_default.xml`) ile kamera akışı veya seçilen görseller üzerinden hızlı yüz tespiti.
- **Yerel Yüz Tanıma ve Eğitim:** `4x4` uzamsal ızgara histogramı (Spatial Grid Histogram) ve histogram korelasyonu (`Cv2.CompareHist`) ile çoklu kişi tanıma desteği.
- **Işık Normalizasyonu:** `120x120` gri tonlamalı ön işleme ve histogram eşitleme (`Cv2.EqualizeHist`).
- **Yüz Dışa Aktarma:** Algılanan yüzleri orijinal renkleriyle ayrı dosya olarak kaydetme.

## Kullanılan Teknolojiler / NuGet Paketleri
- C# / .NET Windows Forms
- OpenCvSharp4 & OpenCvSharp4.Extensions
- Haar Cascade Frontal Face Modeli (`haarcascade_frontalface_default.xml`)

## Kurulum ve Çalıştırma Adımları
1. Depoyu bilgisayarınıza klonlayın:
   `git clone https://github.com/yusuftugradeveci23/YMH219-FaceRecognition.git`
2. İndirilen klasör içindeki `YMH219-FaceRecognition/YMH219-FaceRecognition.slnx` çözüm dosyasını Visual Studio ile açın.
3. NuGet paketlerinin (`OpenCvSharp4`) otomatik olarak geri yüklenmesini bekleyin.
4. Çözümü derleyin (`Ctrl + Shift + B`) ve `F5` tuşuna basarak uygulamayı çalıştırın.
5. **Kamerayı Başlat** veya **Görüntü Seç** butonları ile görüntü aldıktan sonra en az iki farklı gönüllü/yerel etiket için eğitim örnekleri ekleyerek tanıma işlemini test edin.

## Bilinen Sınırlamalar / Geliştirme Notları
- Tanıma işlemi derin öğrenme tabanlı gömme (embedding) vektörleri yerine yerel örnekler ve uzamsal histogram benzerliği üzerinden gerçekleştirilir.
- Ortam ışığı, yüz açısı ve kamera görüntü kalitesi benzerlik skorunu doğrudan etkileyebilir.
- Eğitim örnekleri yerel `FaceSamples` klasöründe saklanır; gizlilik gereği depoya dahil edilmemiştir.
- Uygulama yalnızca ders kapsamında gönüllü ve yerel örneklerle kullanılmak üzere tasarlanmıştır.

## Uygulama Ekran Görüntüleri

### Ana Arayüz
![Uygulama Ana Arayüzü](YMH219-FaceRecognition/ana_arayuz.png)

### Uyarı ve Durum Yönetimi
![Uyarı ve Durum Yönetimi](YMH219-FaceRecognition/durum_uyari.png)

---

- **Öğrenci:** Yusuf Tuğra Deveci
- **Öğrenci No:** 250541095