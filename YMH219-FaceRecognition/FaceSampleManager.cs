using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YMH219_FaceRecognition
{
    public class RecognitionResult
    {
        public string Label { get; set; } = "Bilinmeyen";
        public double Confidence { get; set; } = 0.0;
    }

    public class FaceSampleManager
    {
        private readonly string _samplesRootDir;
        private readonly List<(string Label, Mat FeatureVector)> _trainedSamples = new();

        public FaceSampleManager(string rootDirectory)
        {
            _samplesRootDir = Path.Combine(rootDirectory, "FaceSamples");
            if (!Directory.Exists(_samplesRootDir))
            {
                Directory.CreateDirectory(_samplesRootDir);
            }
            LoadExistingSamples();
        }

        public int TrainedSampleCount => _trainedSamples.Count;
        public int DistinctLabelCount => _trainedSamples.Select(x => x.Label).Distinct().Count();

        public void SaveTrainingSample(Mat grayFaceRoi, string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Etiket adı boş olamaz.");

            string safeLabel = string.Concat(label.Trim().Split(Path.GetInvalidFileNameChars()));
            string labelDir = Path.Combine(_samplesRootDir, safeLabel);
            Directory.CreateDirectory(labelDir);

            using Mat processed = PreprocessFace(grayFaceRoi);

            string fileName = $"{safeLabel}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            string fullPath = Path.Combine(labelDir, fileName);
            Cv2.ImWrite(fullPath, processed);

            Mat feat = ComputeSpatialHistogram(processed);
            _trainedSamples.Add((safeLabel, feat));
        }

        public void LoadExistingSamples()
        {
            foreach (var item in _trainedSamples)
            {
                item.FeatureVector.Dispose();
            }
            _trainedSamples.Clear();

            if (!Directory.Exists(_samplesRootDir)) return;

            foreach (string dir in Directory.GetDirectories(_samplesRootDir))
            {
                string label = Path.GetFileName(dir);
                foreach (string file in Directory.GetFiles(dir, "*.png"))
                {
                    using Mat img = Cv2.ImRead(file, ImreadModes.Grayscale);
                    if (img.Empty()) continue;

                    using Mat resized = new Mat();
                    Cv2.Resize(img, resized, new OpenCvSharp.Size(120, 120));

                    Mat feat = ComputeSpatialHistogram(resized);
                    _trainedSamples.Add((label, feat));
                }
            }
        }

        public RecognitionResult Predict(Mat grayFaceRoi)
        {
            if (_trainedSamples.Count == 0)
            {
                return new RecognitionResult { Label = "Egitimsiz (Ornek Yok)", Confidence = 0 };
            }

            using Mat processed = PreprocessFace(grayFaceRoi);
            using Mat queryFeat = ComputeSpatialHistogram(processed);

            var labelScores = new Dictionary<string, List<double>>();

            foreach (var sample in _trainedSamples)
            {
                double correlation = Cv2.CompareHist(queryFeat, sample.FeatureVector, HistCompMethods.Correl);
                double score = Math.Max(0, Math.Min(100, correlation * 100.0));

                if (!labelScores.ContainsKey(sample.Label))
                    labelScores[sample.Label] = new List<double>();

                labelScores[sample.Label].Add(score);
            }

            string bestLabel = "Bilinmeyen";
            double bestScore = 0;

            foreach (var kvp in labelScores)
            {
                double topAvg = kvp.Value.OrderByDescending(s => s).Take(2).Average();
                if (topAvg > bestScore)
                {
                    bestScore = topAvg;
                    bestLabel = kvp.Key;
                }
            }

            if (bestScore < 75.0)
            {
                return new RecognitionResult { Label = "Taninmayan Yuz", Confidence = Math.Round(bestScore, 1) };
            }

            return new RecognitionResult
            {
                Label = bestLabel,
                Confidence = Math.Round(bestScore, 1)
            };
        }

        private static Mat PreprocessFace(Mat grayFaceRoi)
        {
            Mat resized = new Mat();
            Cv2.Resize(grayFaceRoi, resized, new OpenCvSharp.Size(120, 120));
            Cv2.EqualizeHist(resized, resized);
            return resized;
        }

        private static Mat ComputeSpatialHistogram(Mat gray120)
        {
            OpenCvSharp.Rect centerCrop = new OpenCvSharp.Rect(12, 12, 96, 96);
            using Mat centerFace = new Mat(gray120, centerCrop);

            int grid = 4;
            int cellSize = 96 / grid;
            int bins = 16;
            Mat spatialHist = new Mat(grid * grid * bins, 1, MatType.CV_32F);

            int[] histSize = { bins };
            Rangef[] ranges = { new Rangef(0, 256) };

            int offset = 0;
            for (int r = 0; r < grid; r++)
            {
                for (int c = 0; c < grid; c++)
                {
                    OpenCvSharp.Rect cellRect = new OpenCvSharp.Rect(c * cellSize, r * cellSize, cellSize, cellSize);
                    using Mat cell = new Mat(centerFace, cellRect);
                    using Mat cellHist = new Mat();

                    Cv2.CalcHist(new[] { cell }, new[] { 0 }, null, cellHist, 1, histSize, ranges);
                    Cv2.Normalize(cellHist, cellHist, 0, 1, NormTypes.MinMax);

                    for (int b = 0; b < bins; b++)
                    {
                        spatialHist.Set(offset + b, 0, cellHist.Get<float>(b, 0));
                    }
                    offset += bins;
                }
            }

            return spatialHist;
        }
    }
}