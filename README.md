# YMH 219 - OpenCV ile Gerçek Zamanlı Yüz Algılama ve Tanıma Sistemi

C# (.NET WinForms) ve **OpenCvSharp4** kütüphanesi kullanılarak geliştirilmiş, gerçek zamanlı yüz algılama ve yerel eğitim tabanlı yüz tanıma masaüstü uygulamasıdır.

## Özellikler
- **Gerçek Zamanlı Yüz Algılama:** Haar Cascade sınıflandırıcısı (`haarcascade_frontalface_default.xml`) ile kamera akışı veya seçilen görseller üzerinden hızlı yüz tespiti.
- **Yerel Yüz Tanıma ve Eğitim:** `4x4` uzamsal ızgara histogramı (Spatial Grid Histogram) ve histogram korelasyonu (`Cv2.CompareHist`) ile çoklu kişi tanıma desteği.
- **Işık Normalizasyonu:** `120x120` gri tonlamalı ön işleme ve histogram eşitleme (`Cv2.EqualizeHist`).
- **Yüz Dışa Aktarma:** Algılanan yüzleri orijinal renkleriyle ayrı dosya olarak kaydetme.

## Kullanılan Teknolojiler
- C# / .NET Windows Forms
- OpenCvSharp4 & OpenCvSharp4.Extensions
- Haar Cascade Frontal Face Modeli

## Uygulama Ekran Görüntüleri

### Ana Arayüz
![Uygulama Ana Arayüzü](YMH219-FaceRecognition/ana_arayuz.png)

### Uyarı ve Durum Yönetimi
![Uyarı ve Durum Yönetimi](YMH219-FaceRecognition/durum_uyari.png)

---

- **Öğrenci:** Yusuf Tuğra Deveci
- **Öğrenci No:** 250541095
