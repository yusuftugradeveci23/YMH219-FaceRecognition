using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace YMH219_FaceRecognition
{
    public class DetectedFaceInfo
    {
        public Rect BoundingBox { get; set; }
        public RecognitionResult Recognition { get; set; } = new();
    }

    public class FaceRecognitionService : IDisposable
    {
        private readonly CascadeClassifier _faceCascade;
        private readonly FaceSampleManager _sampleManager;
        private VideoCapture? _capture;

        public bool IsCameraRunning => _capture != null && _capture.IsOpened();
        public FaceSampleManager SampleManager => _sampleManager;

        public FaceRecognitionService(string cascadeFilePath, string baseDirectory)
        {
            if (!File.Exists(cascadeFilePath))
            {
                throw new FileNotFoundException(
                    $"Yüz algılama model dosyası bulunamadı:\n{cascadeFilePath}\nLütfen haarcascade_frontalface_default.xml dosyasını uygulama dizinine ekleyin.");
            }

            _faceCascade = new CascadeClassifier(cascadeFilePath);
            if (_faceCascade.Empty())
            {
                throw new InvalidOperationException("Haar Cascade yüz algılama modeli yüklenemedi.");
            }

            _sampleManager = new FaceSampleManager(baseDirectory);
        }

        public void StartCamera(int cameraIndex = 0)
        {
            StopCamera();
            _capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            if (!_capture.IsOpened())
            {
                _capture.Dispose();
                _capture = null;
                throw new InvalidOperationException("Bilgisayara bağlı kullanılabilir bir kamera bulunamadı veya kamera başka bir uygulama tarafından kullanılıyor.");
            }
        }

        public void StopCamera()
        {
            if (_capture != null)
            {
                if (_capture.IsOpened())
                    _capture.Release();
                _capture.Dispose();
                _capture = null;
            }
        }

        public Mat? CaptureFrame()
        {
            if (_capture == null || !_capture.IsOpened()) return null;

            Mat frame = new Mat();
            if (!_capture.Read(frame) || frame.Empty())
            {
                frame.Dispose();
                return null;
            }

            Cv2.Flip(frame, frame, FlipMode.Y);
            return frame;
        }

        public List<DetectedFaceInfo> DetectAndRecognize(Mat bgrFrame, bool drawAnnotations = true)
        {
            var results = new List<DetectedFaceInfo>();
            if (bgrFrame == null || bgrFrame.Empty()) return results;

            using Mat gray = new Mat();
            Cv2.CvtColor(bgrFrame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            Rect[] faces = _faceCascade.DetectMultiScale(
                image: gray,
                scaleFactor: 1.15,
                minNeighbors: 5,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new OpenCvSharp.Size(60, 60));

            foreach (Rect rect in faces)
            {
                using Mat faceRoi = new Mat(gray, rect);
                RecognitionResult rec = _sampleManager.Predict(faceRoi);

                results.Add(new DetectedFaceInfo
                {
                    BoundingBox = rect,
                    Recognition = rec
                });

                if (drawAnnotations)
                {
                    Scalar boxColor = rec.Confidence >= 55.0
                        ? new Scalar(46, 204, 113)
                        : new Scalar(52, 152, 219);

                    Cv2.Rectangle(bgrFrame, rect, boxColor, 2);

                    string caption = rec.Confidence > 0
                        ? $"{rec.Label} (%{rec.Confidence:0.0})"
                        : rec.Label;

                    int textY = Math.Max(22, rect.Y - 8);
                    Cv2.PutText(bgrFrame, caption, new OpenCvSharp.Point(rect.X, textY),
                        HersheyFonts.HersheySimplex, 0.65, boxColor, 2);
                }
            }

            return results;
        }

        public void AddTrainingSampleFromFrame(Mat bgrFrame, string label)
        {
            if (bgrFrame == null || bgrFrame.Empty())
                throw new InvalidOperationException("Örnek eklemek için geçerli bir görüntü bulunmuyor.");

            using Mat gray = new Mat();
            Cv2.CvtColor(bgrFrame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(gray, gray);

            Rect[] faces = _faceCascade.DetectMultiScale(gray, 1.15, 5, HaarDetectionTypes.ScaleImage, new OpenCvSharp.Size(60, 60));

            if (faces.Length == 0)
                throw new InvalidOperationException("Görüntüde yüz algılanamadı. Lütfen kameraya veya resme yüzünüzün net göründüğünden emin olun.");

            if (faces.Length > 1)
                throw new InvalidOperationException("Eğitim örneği eklerken karede yalnızca tek bir yüz bulunmalıdır.");

            using Mat faceRoi = new Mat(gray, faces[0]);
            _sampleManager.SaveTrainingSample(faceRoi, label);
        }

        public void Dispose()
        {
            StopCamera();
            _faceCascade.Dispose();
        }
    }
}
