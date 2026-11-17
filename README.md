# YMH 219 - OpenCV ile Gercek Zamanli Yuz Algilama ve Tanima Sistemi

C# (.NET WinForms) ve OpenCvSharp4 kutuphanesi kullanilarak gelistirilmis gercek zamanli yuz algilama ve yerel egitim tabanli yuz tanima masaustu uygulamasidir.

## Ozellikler
- **Gercek Zamanli Yuz Algilama:** Haar Cascade siniflandiricisi (haarcascade_frontalface_default.xml) ile kamera akisi veya secilen gorseller uzerinden hizli yuz tespiti.
- **Yerel Yuz Tanima ve Egitim:** 4x4 uzamsal izgara histogrami (Spatial Grid Histogram) ve histogram korelasyonu (Cv2.CompareHist) ile coklu kisi tanima destegi.
- **Isik Normalizasyonu:** 120x120 gri tonlamali on isleme ve histogram esitleme (Cv2.EqualizeHist).
- **Yuz Disa Aktarma:** Algilanan yuzleri orijinal renkleriyle ayri dosya olarak kaydetme.

## Kullanilan Teknolojiler
- C# / .NET Windows Forms
- OpenCvSharp4 & OpenCvSharp4.Extensions
- Haar Cascade Frontal Face Modeli


## Uygulama Ekran Goruntuleri
![Uygulama Ana Arayuzu](YMH219-FaceRecognition/ana_arayuz.png)

![Uyari ve Durum Yonetimi](YMH219-FaceRecognition/durum_uyari.png)


## Uygulama Ekran Goruntuleri
![Uygulama Ana Arayuzu](YMH219-FaceRecognition/ana_arayuz.png)

![Uyari ve Durum Yonetimi](YMH219-FaceRecognition/durum_uyari.png)
