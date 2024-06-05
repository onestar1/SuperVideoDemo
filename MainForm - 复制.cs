using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SuperVideo
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;
        private int totalProgress;
        private int totalSteps;
        private bool useGpu;
        private bool isPaused;
        private ManualResetEventSlim pauseEvent;

        public MainForm()
        {
            InitializeComponent();
            InitializeGpuInfo();
            pauseEvent = new ManualResetEventSlim(true);
        }

        private void InitializeGpuInfo()
        {
            if (CudaInvoke.HasCuda) {
                var deviceInfo = new CudaDeviceInfo();
                labelGpuInfo.Text = $"GPU: {deviceInfo.Name}, 显存: {deviceInfo.TotalMemory / (1024 * 1024)} MB";
                checkBoxUseGpu.Checked = true;
                useGpu = true;
            } else {
                labelGpuInfo.Text = "GPU: 不支持CUDA";
                checkBoxUseGpu.Checked = false;
                useGpu = false;
            }
        }

        private void checkBoxUseGpu_CheckedChanged(object sender, EventArgs e)
        {
            useGpu = checkBoxUseGpu.Checked;
        }

        private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mkv";
                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    textBoxInputVideo.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.Filter = "MP4 Files|*.mp4";
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    textBoxOutputVideo.Text = saveFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseFrames_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()) {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                    textBoxFramesDirectory.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void buttonChangeBackgroundColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog()) {
                if (colorDialog.ShowDialog() == DialogResult.OK) {
                    this.BackColor = colorDialog.Color;
                }
            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused) {
                pauseEvent.Reset();
                buttonPause.Text = "继续";
            } else {
                pauseEvent.Set();
                buttonPause.Text = "暂停";
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private async void buttonProcess_Click(object sender, EventArgs e)
        {
            string inputVideoPath = textBoxInputVideo.Text;
            string outputVideoPath = textBoxOutputVideo.Text;
            string framesDirectory = textBoxFramesDirectory.Text;

            if (string.IsNullOrEmpty(inputVideoPath) || string.IsNullOrEmpty(outputVideoPath) || string.IsNullOrEmpty(framesDirectory)) {
                MessageBox.Show("请填写所有路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try {
                // 创建帧目录
                Directory.CreateDirectory(framesDirectory);

                // 初始化总进度和总步骤
                totalProgress = 0;
                totalSteps = 4; // 提取帧、去除水印、修改帧、生成视频

                // 提取视频帧
                UpdateCurrentStep("正在提取帧...");
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                var progressTask = Task.Run(() => AnimateProgressBar(token), token);

                await ExtractFramesAsync(inputVideoPath, framesDirectory);

                // 停止进度条轮动动画
                cancellationTokenSource.Cancel();
                await progressTask;

                // 获取帧数
                int frameCount = Directory.GetFiles(framesDirectory, "*.png").Length;
                progressBar.Maximum = frameCount;
                UpdateTotalFrames(frameCount);

                // 开始计时
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // 使用多线程处理视频
                await Task.Run(() => {
                    // 去除每一帧的文字水印
                   // UpdateCurrentStep("正在去除水印...");
                    //RemoveWatermark(framesDirectory, frameCount);

                    // 修改每一帧
                    UpdateCurrentStep("正在修改帧...");
                    ModifyFrames(framesDirectory, frameCount);

                    // 重新生成视频
                    UpdateCurrentStep("正在生成新的视频...");
                    CreateVideoFromFrames(framesDirectory, outputVideoPath);
                });

                // 停止计时
                stopwatch.Stop();
                labelTime.Text = $"处理时间: {stopwatch.Elapsed}";

                // 清理帧目录
                Directory.Delete(framesDirectory, true);

                UpdateCurrentStep("处理完成！");
            } catch (Exception ex) {
                MessageBox.Show($"处理过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ExtractFramesAsync(string inputVideoPath, string framesDirectory)
        {
            try {
                if (!Directory.Exists(framesDirectory)) {
                    Directory.CreateDirectory(framesDirectory);
                }

                await Task.Run(() => {
                    Process ffmpeg = new Process();
                    ffmpeg.StartInfo.FileName = "ffmpeg";
                    ffmpeg.StartInfo.Arguments = $"-i \"{inputVideoPath}\" \"{framesDirectory}\\frame_%04d.png\"";
                    ffmpeg.StartInfo.RedirectStandardOutput = true;
                    ffmpeg.StartInfo.RedirectStandardError = true; // 捕获标准错误输出
                    ffmpeg.StartInfo.UseShellExecute = false;
                    ffmpeg.StartInfo.CreateNoWindow = true;
                    ffmpeg.StartInfo.WorkingDirectory = framesDirectory; // 设置工作目录

                    ffmpeg.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                    ffmpeg.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                    ffmpeg.Start();
                    ffmpeg.BeginOutputReadLine();
                    ffmpeg.BeginErrorReadLine();
                    ffmpeg.WaitForExit();
                });

                // 更新总进度
                UpdateTotalProgress();
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred while extracting frames: {ex.Message}");
                throw;
            }
        }

        private void AnimateProgressBar(CancellationToken token)
        {
            while (!token.IsCancellationRequested) {
                for (int i = 0; i <= 100; i++) {
                    if (token.IsCancellationRequested)
                        break;

                    UpdateProgress(i, 100);
                    Thread.Sleep(50); // 控制进度条轮动速度
                }
            }
        }

        void RemoveWatermark(string framesDirectory, int frameCount)
        {
            try {
                int processedFrames = 0;
                Parallel.ForEach(Directory.GetFiles(framesDirectory, "*.png"), new ParallelOptions { CancellationToken = cancellationTokenSource.Token }, framePath => {
                    try {
                        pauseEvent.Wait(cancellationTokenSource.Token);

                        using (Mat img = CvInvoke.Imread(framePath, ImreadModes.Color)) {
                            if (img.IsEmpty) {
                                throw new Exception($"Failed to load image: {framePath}");
                            }

                            if (useGpu && CudaInvoke.HasCuda) {
                                // 使用 GPU 进行处理
                                using (CudaImage<Bgr, Byte> cudaImg = new CudaImage<Bgr, Byte>(img)) {
                                    // 转换为灰度图像
                                    using (CudaImage<Gray, Byte> gray = new CudaImage<Gray, Byte>(cudaImg.Size)) {
                                        CudaInvoke.CvtColor(cudaImg, gray, ColorConversion.Bgr2Gray);

                                        // 二值化处理
                                        using (CudaImage<Gray, Byte> binary = new CudaImage<Gray, Byte>(gray.Size)) {
                                            CudaInvoke.Threshold(gray, binary, 200, 255, ThresholdType.Binary);

                                            // 边缘检测
                                            using (CudaImage<Gray, Byte> edges = new CudaImage<Gray, Byte>(binary.Size)) {
                                                using (CudaCannyEdgeDetector canny = new CudaCannyEdgeDetector(100, 200)) {
                                                    canny.Detect(binary, edges);

                                                    // 调试输出
                                                    Mat edgesMat = new Mat();
                                                    edges.Download(edgesMat);
                                                    string edgePath = Path.Combine(framesDirectory, $"edges_{Interlocked.Increment(ref processedFrames)}.png");
                                                    edgesMat.Save(edgePath);

                                                    // 轮廓检测
                                                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {
                                                        CvInvoke.FindContours(edgesMat, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                                                        // 遍历所有轮廓，找到可能的水印区域
                                                        for (int i = 0; i < contours.Size; i++) {
                                                            using (VectorOfPoint contour = contours[i]) {
                                                                Rectangle rect = CvInvoke.BoundingRectangle(contour);
                                                                if (IsWatermark(rect, img.Size)) {
                                                                    // 去除水印
                                                                    Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
                                                                    mask.SetTo(new MCvScalar(255));
                                                                    CvInvoke.Rectangle(mask, rect, new MCvScalar(0), -1);
                                                                    CvInvoke.Inpaint(img, mask, img, 3, InpaintType.Telea);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            } else {
                                // 使用 CPU 进行处理
                                // 转换为灰度图像
                                Mat gray = new Mat();
                                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);

                                // 二值化处理
                                Mat binary = new Mat();
                                CvInvoke.Threshold(gray, binary, 200, 255, ThresholdType.Binary);

                                // 边缘检测
                                Mat edges = new Mat();
                                CvInvoke.Canny(binary, edges, 100, 200);

                                // 调试输出
                                string edgePath = Path.Combine(framesDirectory, $"edges_{Interlocked.Increment(ref processedFrames)}.png");
                                CvInvoke.Imwrite(edgePath, edges);

                                // 轮廓检测
                                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {
                                    CvInvoke.FindContours(edges, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                                    // 遍历所有轮廓，找到可能的水印区域
                                    for (int i = 0; i < contours.Size; i++) {
                                        using (VectorOfPoint contour = contours[i]) {
                                            Rectangle rect = CvInvoke.BoundingRectangle(contour);
                                            if (IsWatermark(rect, img.Size)) {
                                                // 去除水印
                                                Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
                                                mask.SetTo(new MCvScalar(255));
                                                CvInvoke.Rectangle(mask, rect, new MCvScalar(0), -1);
                                                CvInvoke.Inpaint(img, mask, img, 3, InpaintType.Telea);
                                            }
                                        }
                                    }
                                }
                            }

                            img.Save(framePath);
                        }
                        UpdateProgress(Interlocked.Increment(ref processedFrames), frameCount);
                    } catch (Exception ex) {
                        Console.WriteLine($"An error occurred while processing frame {framePath}: {ex.Message}");
                    }
                });

                // 更新总进度
                UpdateTotalProgress();
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred while removing watermark: {ex.Message}");
                throw;
            }
        }

        bool IsWatermark(Rectangle rect, Size imgSize)
        {
            // 根据矩形的大小和位置判断是否为水印
            // 你可以根据实际情况调整这个判断逻辑
            return rect.Width > 50 && rect.Height > 20 && rect.Width < imgSize.Width / 2 && rect.Height < imgSize.Height / 2;
        }

        void ModifyFrames(string framesDirectory, int frameCount)
        {
            try {
                Random rand = new Random();
                int processedFrames = 0;
                Parallel.ForEach(Directory.GetFiles(framesDirectory, "*.png"), new ParallelOptions { CancellationToken = cancellationTokenSource.Token }, framePath => {
                    try {
                        pauseEvent.Wait(cancellationTokenSource.Token);

                        using (Bitmap bitmap = new Bitmap(framePath)) {
                            //for (int y = 0; y < bitmap.Height; y++) {
                            //    for (int x = 0; x < bitmap.Width; x++) {
                            //        Color pixelColor = bitmap.GetPixel(x, y);
                            //        // 修改颜色
                            //        Color newColor = Color.FromArgb(pixelColor.A, rand.Next(256), rand.Next(256), rand.Next(256));
                            //        bitmap.SetPixel(x, y, newColor);
                            //    }
                            //}
                            // 添加水印
                            using (Graphics g = Graphics.FromImage(bitmap)) {
                                g.DrawString("监控下的万花筒", new Font("Arial", 20), Brushes.Red, new PointF(10, 10));
                            }
                            bitmap.Save(framePath);
                        }
                        UpdateProgress(Interlocked.Increment(ref processedFrames), frameCount);
                    } catch (Exception ex) {
                        Console.WriteLine($"An error occurred while modifying frame {framePath}: {ex.Message}");
                    }
                });

                // 更新总进度
                UpdateTotalProgress();
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred while modifying frames: {ex.Message}");
                throw;
            }
        }

        void CreateVideoFromFrames(string framesDirectory, string outputVideoPath)
        {
            try {
                string tempFileList = Path.Combine(framesDirectory, "filelist.txt");
                using (StreamWriter writer = new StreamWriter(tempFileList)) {
                    int frameIndex = 1;
                    while (true) {
                        string framePath = Path.Combine(framesDirectory, $"frame_{frameIndex:D4}.png");
                        if (File.Exists(framePath)) {
                            writer.WriteLine($"file '{framePath}'");
                        } else {
                            break;
                        }
                        frameIndex++;
                    }
                }

                Process ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = "ffmpeg";
                ffmpeg.StartInfo.Arguments = $"-f concat -safe 0 -i \"{tempFileList}\" -c:v libx264 -pix_fmt yuv420p \"{outputVideoPath}\"";
                ffmpeg.StartInfo.RedirectStandardOutput = true;
                ffmpeg.StartInfo.RedirectStandardError = true; // 捕获标准错误输出
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.CreateNoWindow = true;

                ffmpeg.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                ffmpeg.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                ffmpeg.Start();
                ffmpeg.BeginOutputReadLine();
                ffmpeg.BeginErrorReadLine();
                ffmpeg.WaitForExit();

                // 删除临时文件
                File.Delete(tempFileList);

                // 更新总进度
                UpdateTotalProgress();
            } catch (OperationCanceledException) {
                Console.WriteLine("操作已取消！");
            } catch (Exception ex) {
                Console.WriteLine($"An error occurred while creating video: {ex.Message}");
                throw;
            }
        }
        private void UpdateProgress(int processedFrames, int frameCount)
        {
            if (InvokeRequired) {
                Invoke(new Action<int, int>(UpdateProgress), processedFrames, frameCount);
            } else {
                progressBar.Value = processedFrames;
                labelStatus.Text = $"处理进度: {processedFrames}/{frameCount}";
            }
        }

        private void UpdateTotalProgress()
        {
            if (InvokeRequired) {
                Invoke(new Action(UpdateTotalProgress));
            } else {
                totalProgress++;
                int progressPercentage = Math.Min((totalProgress * 100) / totalSteps, 100);
                progressBar.Value = progressPercentage;
                labelStatus.Text = $"总进度: {progressPercentage}%";
                labelTotalFrames.Text = $"总进度: {progressPercentage}%"; // 更新前台UI的Label控件
            }
        }

        private void UpdateTotalFrames(int frameCount)
        {
            if (InvokeRequired) {
                Invoke(new Action<int>(UpdateTotalFrames), frameCount);
            } else {
                labelTotalFrames.Text = $"总帧数: {frameCount}";
            }
        }

        private void UpdateCurrentStep(string step)
        {
            if (InvokeRequired) {
                Invoke(new Action<string>(UpdateCurrentStep), step);
            } else {
                labelCurrentStep.Text = step;
            }
        }
    }
}