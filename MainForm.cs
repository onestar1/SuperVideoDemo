
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
using static Emgu.CV.DISOpticalFlow;

namespace SuperVideo
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;
        private int totalProgress;
        private int totalSteps;
        private bool useGpu;
        private bool isPaused;
        private bool useExtractFrames;
        private ManualResetEventSlim pauseEvent;
        private bool isCreateVoide;

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
            useExtractFrames = checkBoxExtractFrames.Checked;
            isCreateVoide = checkBoxCreateVideo.Checked;
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
        private void BrowseWarter_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Water Png|*.PNG";
                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    textBoxWarter.Text = openFileDialog.FileName;
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

        private void checkBoxExtractFrames_CheckedChanged(object sender, EventArgs e)
        {
            useExtractFrames = checkBoxExtractFrames.Checked;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        private async void buttonProcess_Click(object sender, EventArgs e)
        {
            string inputVideoPath = textBoxInputVideo.Text;
            string outputVideoPath = textBoxOutputVideo.Text;
            string framesDirectory = textBoxFramesDirectory.Text;
            string templateImagePath = textBoxWarter.Text;

            if (string.IsNullOrEmpty(inputVideoPath) || string.IsNullOrEmpty(outputVideoPath) || string.IsNullOrEmpty(framesDirectory)) {
                MessageBox.Show("请填写所有路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try {
                // 创建帧目录
                Directory.CreateDirectory(framesDirectory);

                // 创建灰度图像和处理后图像的目录
                string grayscaleDirectory = Path.Combine(Path.GetDirectoryName(outputVideoPath), "GrayscaleFrames");
                string processedDirectory = Path.Combine(Path.GetDirectoryName(outputVideoPath), "ProcessedFrames");
                Directory.CreateDirectory(grayscaleDirectory);
                Directory.CreateDirectory(processedDirectory);

                // 初始化总进度和总步骤
                totalProgress = 0;
                totalSteps = 4; // 提取帧、去除水印、修改帧、生成视频

                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;
                // 提取视频帧
                if (useExtractFrames) {
                    UpdateCurrentStep("正在提取帧...");

                    var progressTask = Task.Run(() => AnimateProgressBar(token), token);

                    await ExtractFramesAsync(inputVideoPath, framesDirectory, token);
                    // 停止进度条轮动动画
                    cancellationTokenSource.Cancel();
                    await progressTask;
                } else {

                }
                // 获取帧数
                int frameCount = Directory.GetFiles(framesDirectory, "*.png").Length;
                progressBar.Maximum = frameCount;
                UpdateTotalFrames(frameCount);

                // 开始计时
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                // 使用多线程处理视频
                ////await Task.Run(() => {
                //    // 去除每一帧的文字水印
                //    UpdateCurrentStep("正在去除水印...");
                //    RemoveWatermark(framesDirectory, grayscaleDirectory, processedDirectory, frameCount, token);

                //    // 修改每一帧
                //    UpdateCurrentStep("正在修改帧...");
                //    ModifyFrames(processedDirectory, frameCount, token);

                //    // 重新生成视频
                //    UpdateCurrentStep("正在生成新的视频...");
                //    CreateVideoFromFrames(processedDirectory, outputVideoPath, token);
                ////});


                // 去除每一帧的文字水印
                cancellationTokenSource = new CancellationTokenSource();
                token = cancellationTokenSource.Token;
                UpdateCurrentStep("正在去除水印...");
                await Task.Run(() => RemoveWatermark(framesDirectory, grayscaleDirectory, processedDirectory, frameCount, templateImagePath, token), token);
                // RemoveWatermark(framesDirectory, grayscaleDirectory, processedDirectory, frameCount, templateImagePath, token);
                // UpdateCurrentStep("正在去除水印...");
                //RemoveWatermarkWithTemplate(framesDirectory, templateImagePath, processedDirectory);
                //// 修改每一帧
                //cancellationTokenSource = new CancellationTokenSource();
                //token = cancellationTokenSource.Token;
                //UpdateCurrentStep("正在修改帧...");
                //await Task.Run(() => ModifyFrames(processedDirectory, frameCount, token), token);

                // 重新生成视频
                if (isCreateVoide) {
                    cancellationTokenSource = new CancellationTokenSource();
                    token = cancellationTokenSource.Token;
                    UpdateCurrentStep("正在生成新的视频...");
                    await Task.Run(() => CreateVideoFromFrames(processedDirectory, outputVideoPath, token), token);
                }
                // 停止计时
                stopwatch.Stop();
                labelTime.Text = $"处理时间: {stopwatch.Elapsed}";

                UpdateCurrentStep("处理完成！");
            } catch (OperationCanceledException) {
                MessageBox.Show("操作已取消。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show($"处理过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ExtractFramesAsync(string inputVideoPath, string framesDirectory, CancellationToken token)
        {
            try {
                if (!Directory.Exists(framesDirectory)) {
                    Directory.CreateDirectory(framesDirectory);
                }
                string audioOutputPath = Path.Combine(framesDirectory, "音频.mp3");
                string subtitlesOutputPath = Path.Combine(framesDirectory, "字幕.srt");
                await Task.Run(() => {
                    Process ffmpeg = new Process();
                    ffmpeg.StartInfo.FileName = "ffmpeg";
                    ffmpeg.StartInfo.Arguments = $"-i \"{inputVideoPath}\" \"{framesDirectory}\\frame_%04d.png\"";
                    //ffmpeg.StartInfo.Arguments = $"-i \"{inputVideoPath}\" -vf \"fps=1\" \"{framesDirectory}\\frame_%04d.png\" -vn -acodec copy \"{audioOutputPath}\" -an -scodec copy \"{subtitlesOutputPath}\"";
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
                }, token);

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
        ///// <summary>
        ///// 采用模板方法去除水印
        ///// </summary>
        ///// <param name="inputImagePath"></param>
        ///// <param name="templateImagePath"></param>
        ///// <param name="outputImagePath"></param>
        //public void RemoveWatermarkWithTemplate(string inputImagePath, string templateImagePath, string outputImagePath)
        //{
        //    // 读取原图和模板图像
        //    Mat img = CvInvoke.Imread(inputImagePath, ImreadModes.Color);
        //    Mat template = CvInvoke.Imread(templateImagePath, ImreadModes.Color);

        //    // 创建结果矩阵
        //    Mat result = new Mat();
        //    int resultCols = img.Cols - template.Cols + 1;
        //    int resultRows = img.Rows - template.Rows + 1;
        //    result.Create(resultRows, resultCols, DepthType.Cv32F, 1);

        //    // 执行模板匹配
        //    CvInvoke.MatchTemplate(img, template, result, TemplateMatchingType.CcoeffNormed);

        //    // 找到匹配位置
        //    double minVal = 0, maxVal = 0;
        //    Point minLoc = new Point(0, 0), maxLoc = new Point(0, 0);
        //    CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

        //    // 如果匹配度高于某个阈值，则认为找到水印
        //    double threshold = 0.8; // 可以根据具体情况调整阈值
        //    if (maxVal >= threshold) {
        //        // 创建一个与原图像大小相同的掩膜
        //        Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
        //        mask.SetTo(new MCvScalar(255));

        //        // 在掩膜上标记水印区域
        //        Rectangle rect = new Rectangle(maxLoc, template.Size);
        //        CvInvoke.Rectangle(mask, rect, new MCvScalar(0), -1);

        //        // 应用掩膜去除水印
        //        CvInvoke.Inpaint(img, mask, img, 3, InpaintType.Telea);
        //    }

        //    // 保存处理后的图像
        //    img.Save(outputImagePath);
        //}
        static bool FindTemplate(Mat img, Mat template, out Rectangle matchRect)
        {
            using (Mat result = new Mat()) {
                // Perform template matching
                CvInvoke.MatchTemplate(img, template, result, TemplateMatchingType.CcoeffNormed);

                // Find the best match location
                double minVal = 0, maxVal = 0;
                Point minLoc = new Point(0, 0), maxLoc = new Point(0, 0);
                CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

                // Define a threshold for matching
                double threshold = 0.8; // Adjust this value as needed

                if (maxVal >= threshold) {
                    matchRect = new Rectangle(maxLoc, template.Size);
                    return true;
                } else {
                    matchRect = Rectangle.Empty;
                    return false;
                }
            }
        }

        void RemoveWatermark(string framesDirectory, string grayscaleDirectory, string processedDirectory, int frameCount, string templateImagePath, CancellationToken token)
        {
            try {
                Mat template = new Mat();
                int processedFrames = 0;
                if (!string.IsNullOrWhiteSpace(templateImagePath)) {
                    template = CvInvoke.Imread(templateImagePath, ImreadModes.Color);
                }
                //Mat template = new Mat();
                //CvInvoke.CvtColor(pathtemplate, template, ColorConversion.Bgr2Gray);

                Parallel.ForEach(Directory.GetFiles(framesDirectory, "*.png"), new ParallelOptions { CancellationToken = token }, framePath => {
                    try {
                        // 停止事件
                        // pauseEvent.Wait(token);

                        using (Mat img = CvInvoke.Imread(framePath, ImreadModes.Color)) {
                            if (img.IsEmpty) {
                                throw new Exception($"Failed to load image: {framePath}");
                            }

                            string fileName = Path.GetFileName(framePath);

                            // 保存灰度图像
                            Mat gray = new Mat();
                            CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
                            gray.Save(Path.Combine(grayscaleDirectory, fileName));

                            // 检测并去除水印
                            if (useGpu && CudaInvoke.HasCuda) {
                                // 使用 GPU 进行处理
                                using (CudaImage<Bgr, Byte> cudaImg = new CudaImage<Bgr, Byte>(img)) {
                                    // 转换为灰度图像
                                    using (CudaImage<Gray, Byte> grayCuda = new CudaImage<Gray, Byte>(cudaImg.Size)) {
                                        CudaInvoke.CvtColor(cudaImg, grayCuda, ColorConversion.Bgr2Gray);

                                        // 二值化处理
                                        using (CudaImage<Gray, Byte> binary = new CudaImage<Gray, Byte>(grayCuda.Size)) {
                                            CudaInvoke.Threshold(grayCuda, binary, 200, 255, ThresholdType.Binary);

                                            // 边缘检测
                                            using (CudaImage<Gray, Byte> edges = new CudaImage<Gray, Byte>(binary.Size)) {
                                                using (CudaCannyEdgeDetector canny = new CudaCannyEdgeDetector(100, 200)) {
                                                    canny.Detect(binary, edges);

                                                    // 轮廓检测
                                                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {
                                                        Mat edgesMat = new Mat();
                                                        edges.Download(edgesMat);
                                                        CvInvoke.FindContours(edgesMat, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                                                        // 遍历所有轮廓，找到可能的水印区域
                                                        for (int i = 0; i < contours.Size; i++) {
                                                            using (VectorOfPoint contour = contours[i]) {
                                                                Rectangle rect = CvInvoke.BoundingRectangle(contour);
                                                                if (IsWatermark(rect, img.Size)) {
                                                                    // 去除水印
                                                                    Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
                                                                    mask.SetTo(new MCvScalar(0)); // 其他区域为黑色
                                                                    CvInvoke.Rectangle(mask, rect, new MCvScalar(255), -1); // 水印区域为白色
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
                                // 检测并去除水印
                                Rectangle matchRect;
                                if (string.IsNullOrWhiteSpace(templateImagePath)) {
                                    if (FindWatermarkBySize(img, out matchRect)) {
                                        Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
                                        mask.SetTo(new MCvScalar(0)); // 其他区域为黑色
                                        CvInvoke.Rectangle(mask, matchRect, new MCvScalar(255), -1); // 水印区域为白色

                                        // Debug: Save the mask to check the mask region
                                        // mask.Save(Path.Combine(processedDirectory, "mask_" + fileName));

                                        // 修复图像
                                        CvInvoke.Inpaint(img, mask, img, 5, InpaintType.Telea);

                                    }
                                } else if (FindTemplate(img, template, out matchRect)) {
                                    Mat mask = new Mat(img.Size, DepthType.Cv8U, 1);
                                    mask.SetTo(new MCvScalar(0)); // 其他区域为黑色
                                    CvInvoke.Rectangle(mask, matchRect, new MCvScalar(255), -1); // 水印区域为白色

                                    // Debug: Save the mask to check the mask region
                                    mask.Save(Path.Combine(processedDirectory, "mask_" + fileName));

                                    // 修复图像
                                    CvInvoke.Inpaint(img, mask, img, 5, InpaintType.Telea);
                                }


                            }
                            // 保存处理后的彩色图像
                            img.Save(Path.Combine(processedDirectory, fileName));
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
            //return rect.Width > 50 && rect.Height > 10 && rect.X > imgSize.Width * 0.1 && rect.Y > imgSize.Height * 0.1;

            // 假设水印的大小在图像总大小的一定比例范围内
            double minWidthRatio = 0.05; // 最小宽度比例
            double maxWidthRatio = 0.3; // 最大宽度比例
            double minHeightRatio = 0.05; // 最小高度比例
            double maxHeightRatio = 0.3; // 最大高度比例

            double imgWidth = imgSize.Width;
            double imgHeight = imgSize.Height;

            double rectWidthRatio = (double)rect.Width / imgWidth;
            double rectHeightRatio = (double)rect.Height / imgHeight;

            // 判断轮廓是否符合水印的大小比例
            if (rectWidthRatio >= minWidthRatio && rectWidthRatio <= maxWidthRatio &&
                rectHeightRatio >= minHeightRatio && rectHeightRatio <= maxHeightRatio) {
                return true;
            }

            return false;
        }
        bool FindWatermarkBySize(Mat img, out Rectangle matchRect)
        {
            // 假设水印的大小在图像总大小的一定比例范围内
            double minWidthRatio = 0.05; // 最小宽度比例
            double maxWidthRatio = 0.3; // 最大宽度比例
            double minHeightRatio = 0.05; // 最小高度比例
            double maxHeightRatio = 0.3; // 最大高度比例

            double imgWidth = img.Width;
            double imgHeight = img.Height;

            using (Mat gray = new Mat()) {
                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
                using (Mat binary = new Mat()) {
                    CvInvoke.Threshold(gray, binary, 240, 255, ThresholdType.Binary);

                    using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint()) {
                        CvInvoke.FindContours(binary, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                        for (int i = 0; i < contours.Size; i++) {
                            using (VectorOfPoint contour = contours[i]) {
                                Rectangle rect = CvInvoke.BoundingRectangle(contour);
                                double rectWidthRatio = (double)rect.Width / imgWidth;
                                double rectHeightRatio = (double)rect.Height / imgHeight;

                                // 判断轮廓是否符合水印的大小比例
                                if (rectWidthRatio >= minWidthRatio && rectWidthRatio <= maxWidthRatio &&
                                    rectHeightRatio >= minHeightRatio && rectHeightRatio <= maxHeightRatio) {
                                    matchRect = rect;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            matchRect = Rectangle.Empty;
            return false;
        }

        void ModifyFrames(string framesDirectory, int frameCount, CancellationToken token)
        {
            try {
                Random rand = new Random();
                int processedFrames = 0;
                Parallel.ForEach(Directory.GetFiles(framesDirectory, "*.png"), new ParallelOptions { CancellationToken = token }, framePath => {
                    try {
                        pauseEvent.Wait(token);

                        using (Bitmap bitmap = new Bitmap(framePath)) {
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

        void CreateVideoFromFrames(string framesDirectory, string outputVideoPath, CancellationToken token)
        {
            try {
                // 创建 filelist.txt 文件 考虑 随机删除一些文件
                Random random = new Random(); // Create a new instance of the Random class for generating random numbers
                var frameFiles = Directory.GetFiles(framesDirectory, "frame_*.png");
                string tempFileList = Path.Combine(framesDirectory, "filelist.txt");
                using (StreamWriter writer = new StreamWriter(tempFileList)) {
                    // 获取所有图片文件

                    Array.Sort(frameFiles); // 确保文件按名称排序

                    foreach (string framePath in frameFiles) {
                        if (random.NextDouble() <= 0.92) // Generate a random number between 0.0 and 1.0; if it's less than or equal to 0.95 (95% probability)
                        {
                            writer.WriteLine($"file '{framePath}'"); // Write the frame file path to the filelist.txt

                        } else {
                            File.Delete(framePath);
                        }
                    }
                }

                // 获取剩余的文件并重新排序
                frameFiles = Directory.GetFiles(framesDirectory, "frame_*.png");
                Array.Sort(frameFiles);

                // 重新命名剩余的文件
                for (int i = 0; i < frameFiles.Length; i++) {
                    string newFileName = Path.Combine(framesDirectory, $"frame_{i + 1:D4}.png");
                    File.Move(frameFiles[i], newFileName);
                }


                // Define the resolution, aspect ratio, and format as parameters
                string resolution = "1920"; // Example resolution height
                string aspectRatio = "9:16"; // Example aspect ratio (16:9)
                string format = "libx264"; // Example format (codec)

                // Calculate the width based on the aspect ratio
                string[] aspectRatioParts = aspectRatio.Split(':'); // Split the aspect ratio string by ':'
                int aspectRatioWidth = int.Parse(aspectRatioParts[0]); // Parse the width part of the aspect ratio
                int aspectRatioHeight = int.Parse(aspectRatioParts[1]); // Parse the height part of the aspect ratio
                int width = int.Parse(resolution) * aspectRatioWidth / aspectRatioHeight; // Calculate the width based on the resolution and aspect ratio
                //ffmpeg -framerate 30 -i frame_%04d.png -c:v libx264 -r 30 -pix_fmt yuv420p output.mp4
                Process ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = "ffmpeg";
                ffmpeg.StartInfo.Arguments = $"-framerate 30 -i \"{Path.Combine(framesDirectory, "frame_%04d.png")}\" -pix_fmt yuv420p -c:v libx264 -crf 18 -preset slow \"{outputVideoPath}\"";
                ffmpeg.StartInfo.RedirectStandardOutput = true;
                ffmpeg.StartInfo.RedirectStandardError = true; // 捕获标准错误输出
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.CreateNoWindow = true;

                ffmpeg.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                ffmpeg.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                // 打印命令行参数以调试
                Console.WriteLine(ffmpeg.StartInfo.Arguments);

                ffmpeg.Start();
                ffmpeg.BeginOutputReadLine();
                ffmpeg.BeginErrorReadLine();
                ffmpeg.WaitForExit();

                // 删除临时文件
                // File.Delete(tempFileList);

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

        private void checkBoxCreateVideo_CheckedChanged(object sender, EventArgs e)
        {
            isCreateVoide = checkBoxCreateVideo.Checked;
        }
    }
}