using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace YMH219_FaceRecognition
{
    public partial class Form1 : Form
    {
        private FaceRecognitionService? _service;
        private readonly System.Windows.Forms.Timer _cameraTimer;
        private Mat? _currentCleanFrame;

        private PictureBox picPreview = null!;
        private Button btnStartCam = null!;
        private Button btnStopCam = null!;
        private Button btnSelectImage = null!;
        private Button btnAddSample = null!;
        private Button btnSaveFaces = null!;
        private TextBox txtLabelName = null!;
        private Label lblStatus = null!;
        private Label lblRecognitionInfo = null!;
        private Label lblDatasetStats = null!;

        public Form1()
        {
            InitializeComponent();
            BuildModernUI();

            _cameraTimer = new System.Windows.Forms.Timer { Interval = 33 };
            _cameraTimer.Tick += CameraTimer_Tick;

            InitializeService();
        }

        private void BuildModernUI()
        {
            this.Text = "YMH 219 - OpenCV ile Yüz Algılama ve Tanıma";
            this.Size = new System.Drawing.Size(1040, 700);
            this.MinimumSize = new System.Drawing.Size(920, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.White,
                Padding = new Padding(14, 12, 14, 12)
            };

            btnStartCam = CreateStyledButton("Kamerayı Başlat", Color.FromArgb(39, 174, 96), 145);
            btnStartCam.Location = new System.Drawing.Point(14, 14);
            btnStartCam.Click += BtnStartCam_Click;

            btnStopCam = CreateStyledButton("Kamerayı Durdur", Color.FromArgb(192, 57, 43), 145);
            btnStopCam.Location = new System.Drawing.Point(168, 14);
            btnStopCam.Enabled = false;
            btnStopCam.Click += BtnStopCam_Click;

            btnSelectImage = CreateStyledButton("Görüntü Seç", Color.FromArgb(41, 128, 185), 130);
            btnSelectImage.Location = new System.Drawing.Point(322, 14);
            btnSelectImage.Click += BtnSelectImage_Click;

            btnSaveFaces = CreateStyledButton("Algılanan Yüzü Kaydet", Color.FromArgb(142, 68, 173), 175);
            btnSaveFaces.Location = new System.Drawing.Point(462, 14);
            btnSaveFaces.Click += BtnSaveFaces_Click;

            lblStatus = new Label
            {
                Text = "● Hazır",
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(655, 22)
            };

            topPanel.Controls.AddRange(new Control[] { btnStartCam, btnStopCam, btnSelectImage, btnSaveFaces, lblStatus });

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 115,
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            Label lblTrainTitle = new Label
            {
                Text = "Yerel Yüz Tanıma Eğitimi (En az 2 farklı kişi/etiket ekleyerek tanımayı test edebilirsiniz):",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new System.Drawing.Point(14, 10)
            };

            Label lblInput = new Label
            {
                Text = "Kişi / Etiket Adı:",
                AutoSize = true,
                Location = new System.Drawing.Point(14, 42)
            };

            txtLabelName = new TextBox
            {
                Location = new System.Drawing.Point(125, 39),
                Width = 180,
                PlaceholderText = "Örn: Tugra veya Gonullu1"
            };

            btnAddSample = CreateStyledButton("Mevcut Yüzü Eğitim Örneği Ekle", Color.FromArgb(230, 126, 34), 230);
            btnAddSample.Location = new System.Drawing.Point(315, 36);
            btnAddSample.Click += BtnAddSample_Click;

            lblDatasetStats = new Label
            {
                Text = "Kayıtlı Sınıf: 0 | Toplam Örnek: 0",
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = true,
                Location = new System.Drawing.Point(560, 42)
            };

            lblRecognitionInfo = new Label
            {
                Text = "Tahmin Sonucu: Henüz yüz algılanmadı.",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 160, 133),
                AutoSize = true,
                Location = new System.Drawing.Point(14, 78)
            };

            bottomPanel.Controls.AddRange(new Control[]
            {
                lblTrainTitle, lblInput, txtLabelName, btnAddSample, lblDatasetStats, lblRecognitionInfo
            });

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 34, 40),
                SizeMode = PictureBoxSizeMode.Zoom
            };

            this.Controls.Add(picPreview);
            this.Controls.Add(topPanel);
            this.Controls.Add(bottomPanel);
        }

        private static Button CreateStyledButton(string text, Color backColor, int width)
        {
            return new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new System.Drawing.Size(width, 38),
                Cursor = Cursors.Hand
            };
        }

        private void InitializeService()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string cascadePath = Path.Combine(baseDir, "haarcascade_frontalface_default.xml");
                _service = new FaceRecognitionService(cascadePath, baseDir);
                UpdateDatasetStats();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "● Model Hatası";
                lblStatus.ForeColor = Color.Crimson;
                MessageBox.Show(ex.Message, "Model Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDatasetStats()
        {
            if (_service == null) return;
            lblDatasetStats.Text = $"Kayıtlı Sınıf: {_service.SampleManager.DistinctLabelCount} | Toplam Örnek: {_service.SampleManager.TrainedSampleCount}";
        }

        private void BtnStartCam_Click(object? sender, EventArgs e)
        {
            if (_service == null) return;
            try
            {
                _service.StartCamera(0);
                _cameraTimer.Start();
                btnStartCam.Enabled = false;
                btnStopCam.Enabled = true;
                lblStatus.Text = "● Kamera Aktif (Canlı Algılama)";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Kamera Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnStopCam_Click(object? sender, EventArgs e)
        {
            _cameraTimer.Stop();
            _service?.StopCamera();
            btnStartCam.Enabled = true;
            btnStopCam.Enabled = false;
            lblStatus.Text = "● Kamera Durduruldu";
            lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
        }

        private void CameraTimer_Tick(object? sender, EventArgs e)
        {
            if (_service == null || !_service.IsCameraRunning) return;

            using Mat? frame = _service.CaptureFrame();
            if (frame == null) return;

            ProcessAndDisplayFrame(frame);
        }

        private void BtnSelectImage_Click(object? sender, EventArgs e)
        {
            if (_service == null) return;

            if (_service.IsCameraRunning)
            {
                BtnStopCam_Click(null, EventArgs.Empty);
            }

            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Yüz Algılanacak Görüntüyü Seçin",
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using Mat img = Cv2.ImRead(ofd.FileName, ImreadModes.Color);
                if (img.Empty())
                    throw new InvalidOperationException("Seçilen görüntü dosyası okunamadı.");

                lblStatus.Text = $"● Dosya Yüklendi: {Path.GetFileName(ofd.FileName)}";
                lblStatus.ForeColor = Color.FromArgb(41, 128, 185);
                ProcessAndDisplayFrame(img);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Görüntü Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessAndDisplayFrame(Mat sourceFrame)
        {
            if (_service == null) return;

            _currentCleanFrame?.Dispose();
            _currentCleanFrame = sourceFrame.Clone();

            using Mat displayMat = sourceFrame.Clone();
            List<DetectedFaceInfo> faces = _service.DetectAndRecognize(displayMat, drawAnnotations: true);

            Bitmap bmp = BitmapConverter.ToBitmap(displayMat);
            Image? oldImg = picPreview.Image;
            picPreview.Image = bmp;
            oldImg?.Dispose();

            if (faces.Count == 0)
            {
                lblRecognitionInfo.Text = "Tahmin Sonucu: Karede yüz algılanmadı.";
                lblRecognitionInfo.ForeColor = Color.Gray;
            }
            else
            {
                var summary = faces.Select((f, idx) =>
                    $"Yüz #{idx + 1}: {f.Recognition.Label} (Güven: %{f.Recognition.Confidence:0.0})");
                lblRecognitionInfo.Text = "Tahmin Sonucu: " + string.Join("   |   ", summary);
                lblRecognitionInfo.ForeColor = Color.FromArgb(22, 160, 133);
            }
        }

        private void BtnAddSample_Click(object? sender, EventArgs e)
        {
            if (_service == null) return;

            try
            {
                if (_currentCleanFrame == null || _currentCleanFrame.Empty())
                    throw new InvalidOperationException("Önce kamerayı başlatın veya bir görüntü dosyası seçin.");

                _service.AddTrainingSampleFromFrame(_currentCleanFrame, txtLabelName.Text);
                UpdateDatasetStats();

                if (!_service.IsCameraRunning)
                {
                    ProcessAndDisplayFrame(_currentCleanFrame);
                }

                MessageBox.Show($"'{txtLabelName.Text.Trim()}' etiketi için yüz örneği başarıyla kaydedildi!",
                    "Eğitim Örneği Eklendi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Eğitim Örneği Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSaveFaces_Click(object? sender, EventArgs e)
        {
            if (_service == null || _currentCleanFrame == null || _currentCleanFrame.Empty())
            {
                MessageBox.Show("Kaydedilecek aktif bir görüntü bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using Mat copy = _currentCleanFrame.Clone();
                var faces = _service.DetectAndRecognize(copy, drawAnnotations: false);
                if (faces.Count == 0)
                {
                    MessageBox.Show("Mevcut karede kaydedilecek yüz algılanmadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DetectedFaces");
                Directory.CreateDirectory(exportDir);

                foreach (var face in faces)
                {
                    using Mat crop = new Mat(_currentCleanFrame, face.BoundingBox);
                    string file = Path.Combine(exportDir, $"face_{face.Recognition.Label}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
                    Cv2.ImWrite(file, crop);
                }

                MessageBox.Show($"{faces.Count} adet algılanan yüz '{exportDir}' klasörüne kaydedildi.",
                    "Kayıt Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Kaydetme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cameraTimer.Stop();
            _currentCleanFrame?.Dispose();
            _service?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
