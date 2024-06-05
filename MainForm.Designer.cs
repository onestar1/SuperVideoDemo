﻿namespace SuperVideo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox textBoxInputVideo;
        private System.Windows.Forms.TextBox textBoxOutputVideo;
        private System.Windows.Forms.TextBox textBoxFramesDirectory;
        private System.Windows.Forms.Button buttonBrowseInput;
        private System.Windows.Forms.Button buttonBrowseOutput;
        private System.Windows.Forms.Button buttonBrowseFrames;
        private System.Windows.Forms.Button buttonProcess;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelTotalFrames;
        private System.Windows.Forms.Label labelCurrentStep;
        private System.Windows.Forms.Label labelGpuInfo;
        private System.Windows.Forms.CheckBox checkBoxUseGpu;
        private System.Windows.Forms.Button buttonChangeBackgroundColor;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label labelTotalProgress;
        private System.Windows.Forms.CheckBox checkBoxExtractFrames;
        private System.Windows.Forms.Button BrowseWarter;
        private System.Windows.Forms.TextBox textBoxWarter;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            textBoxInputVideo = new TextBox();
            textBoxOutputVideo = new TextBox();
            textBoxFramesDirectory = new TextBox();
            buttonBrowseInput = new Button();
            buttonBrowseOutput = new Button();
            buttonBrowseFrames = new Button();
            buttonProcess = new Button();
            labelStatus = new Label();
            progressBar = new ProgressBar();
            labelTime = new Label();
            labelTotalFrames = new Label();
            labelCurrentStep = new Label();
            labelGpuInfo = new Label();
            checkBoxUseGpu = new CheckBox();
            buttonChangeBackgroundColor = new Button();
            buttonPause = new Button();
            buttonStop = new Button();
            colorDialog = new ColorDialog();
            labelTotalProgress = new Label();
            checkBoxExtractFrames = new CheckBox();
            BrowseWarter = new Button();
            textBoxWarter = new TextBox();
            checkBoxCreateVideo = new CheckBox();
            SuspendLayout();
            // 
            // textBoxInputVideo
            // 
            textBoxInputVideo.Location = new Point(25, 10);
            textBoxInputVideo.Name = "textBoxInputVideo";
            textBoxInputVideo.Size = new Size(300, 23);
            textBoxInputVideo.TabIndex = 0;
            // 
            // textBoxOutputVideo
            // 
            textBoxOutputVideo.Location = new Point(25, 36);
            textBoxOutputVideo.Name = "textBoxOutputVideo";
            textBoxOutputVideo.Size = new Size(300, 23);
            textBoxOutputVideo.TabIndex = 1;
            // 
            // textBoxFramesDirectory
            // 
            textBoxFramesDirectory.Location = new Point(25, 62);
            textBoxFramesDirectory.Name = "textBoxFramesDirectory";
            textBoxFramesDirectory.Size = new Size(300, 23);
            textBoxFramesDirectory.TabIndex = 2;
            // 
            // buttonBrowseInput
            // 
            buttonBrowseInput.Location = new Point(365, 10);
            buttonBrowseInput.Name = "buttonBrowseInput";
            buttonBrowseInput.Size = new Size(180, 23);
            buttonBrowseInput.TabIndex = 3;
            buttonBrowseInput.Text = "BrowseInput";
            buttonBrowseInput.UseVisualStyleBackColor = true;
            buttonBrowseInput.Click += buttonBrowseInput_Click;
            // 
            // buttonBrowseOutput
            // 
            buttonBrowseOutput.Location = new Point(365, 36);
            buttonBrowseOutput.Name = "buttonBrowseOutput";
            buttonBrowseOutput.Size = new Size(180, 23);
            buttonBrowseOutput.TabIndex = 4;
            buttonBrowseOutput.Text = "BrowseOutput";
            buttonBrowseOutput.UseVisualStyleBackColor = true;
            buttonBrowseOutput.Click += buttonBrowseOutput_Click;
            // 
            // buttonBrowseFrames
            // 
            buttonBrowseFrames.Location = new Point(365, 62);
            buttonBrowseFrames.Name = "buttonBrowseFrames";
            buttonBrowseFrames.Size = new Size(180, 23);
            buttonBrowseFrames.TabIndex = 5;
            buttonBrowseFrames.Text = "BrowseFrames";
            buttonBrowseFrames.UseVisualStyleBackColor = true;
            buttonBrowseFrames.Click += buttonBrowseFrames_Click;
            // 
            // buttonProcess
            // 
            buttonProcess.BackColor = SystemColors.MenuHighlight;
            buttonProcess.ForeColor = SystemColors.ControlText;
            buttonProcess.Location = new Point(400, 132);
            buttonProcess.Name = "buttonProcess";
            buttonProcess.Size = new Size(75, 23);
            buttonProcess.TabIndex = 6;
            buttonProcess.Text = "Process";
            buttonProcess.UseVisualStyleBackColor = false;
            buttonProcess.Click += buttonProcess_Click;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(12, 116);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(43, 17);
            labelStatus.TabIndex = 7;
            labelStatus.Text = "Status";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(25, 132);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(292, 23);
            progressBar.TabIndex = 8;
            // 
            // labelTime
            // 
            labelTime.AutoSize = true;
            labelTime.Location = new Point(12, 158);
            labelTime.Name = "labelTime";
            labelTime.Size = new Size(36, 17);
            labelTime.TabIndex = 9;
            labelTime.Text = "Time";
            // 
            // labelTotalFrames
            // 
            labelTotalFrames.AutoSize = true;
            labelTotalFrames.Location = new Point(12, 180);
            labelTotalFrames.Name = "labelTotalFrames";
            labelTotalFrames.Size = new Size(83, 17);
            labelTotalFrames.TabIndex = 10;
            labelTotalFrames.Text = "Total Frames";
            // 
            // labelCurrentStep
            // 
            labelCurrentStep.AutoSize = true;
            labelCurrentStep.Location = new Point(12, 200);
            labelCurrentStep.Name = "labelCurrentStep";
            labelCurrentStep.Size = new Size(81, 17);
            labelCurrentStep.TabIndex = 11;
            labelCurrentStep.Text = "Current Step";
            // 
            // labelGpuInfo
            // 
            labelGpuInfo.AutoSize = true;
            labelGpuInfo.Location = new Point(12, 220);
            labelGpuInfo.Name = "labelGpuInfo";
            labelGpuInfo.Size = new Size(60, 17);
            labelGpuInfo.TabIndex = 12;
            labelGpuInfo.Text = "GPU Info";
            // 
            // checkBoxUseGpu
            // 
            checkBoxUseGpu.AutoSize = true;
            checkBoxUseGpu.Location = new Point(12, 240);
            checkBoxUseGpu.Name = "checkBoxUseGpu";
            checkBoxUseGpu.Size = new Size(78, 21);
            checkBoxUseGpu.TabIndex = 13;
            checkBoxUseGpu.Text = "Use GPU";
            checkBoxUseGpu.UseVisualStyleBackColor = true;
            checkBoxUseGpu.CheckedChanged += checkBoxUseGpu_CheckedChanged;
            // 
            // buttonChangeBackgroundColor
            // 
            buttonChangeBackgroundColor.Location = new Point(12, 260);
            buttonChangeBackgroundColor.Name = "buttonChangeBackgroundColor";
            buttonChangeBackgroundColor.Size = new Size(150, 23);
            buttonChangeBackgroundColor.TabIndex = 14;
            buttonChangeBackgroundColor.Text = "Change Background Color";
            buttonChangeBackgroundColor.UseVisualStyleBackColor = true;
            buttonChangeBackgroundColor.Click += buttonChangeBackgroundColor_Click;
            // 
            // buttonPause
            // 
            buttonPause.Location = new Point(12, 290);
            buttonPause.Name = "buttonPause";
            buttonPause.Size = new Size(75, 23);
            buttonPause.TabIndex = 15;
            buttonPause.Text = "Pause";
            buttonPause.UseVisualStyleBackColor = true;
            buttonPause.Click += buttonPause_Click;
            // 
            // buttonStop
            // 
            buttonStop.Location = new Point(93, 290);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(75, 23);
            buttonStop.TabIndex = 16;
            buttonStop.Text = "Stop";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            // 
            // labelTotalProgress
            // 
            labelTotalProgress.AutoSize = true;
            labelTotalProgress.Location = new Point(25, 400);
            labelTotalProgress.Name = "labelTotalProgress";
            labelTotalProgress.Size = new Size(41, 17);
            labelTotalProgress.TabIndex = 10;
            labelTotalProgress.Text = "Total ";
            // 
            // checkBoxExtractFrames
            // 
            checkBoxExtractFrames.AutoSize = true;
            checkBoxExtractFrames.Checked = true;
            checkBoxExtractFrames.CheckState = CheckState.Checked;
            checkBoxExtractFrames.Location = new Point(170, 180);
            checkBoxExtractFrames.Name = "checkBoxExtractFrames";
            checkBoxExtractFrames.Size = new Size(134, 21);
            checkBoxExtractFrames.TabIndex = 13;
            checkBoxExtractFrames.Text = "Use ExtractFrames";
            checkBoxExtractFrames.UseVisualStyleBackColor = true;
            checkBoxExtractFrames.CheckedChanged += checkBoxExtractFrames_CheckedChanged;
            // 
            // BrowseWarter
            // 
            BrowseWarter.Location = new Point(365, 91);
            BrowseWarter.Name = "BrowseWarter";
            BrowseWarter.Size = new Size(180, 23);
            BrowseWarter.TabIndex = 18;
            BrowseWarter.Text = "BrowseWarter";
            BrowseWarter.UseVisualStyleBackColor = true;
            BrowseWarter.Click += BrowseWarter_Click;
            // 
            // textBoxWarter
            // 
            textBoxWarter.Location = new Point(25, 91);
            textBoxWarter.Name = "textBoxWarter";
            textBoxWarter.Size = new Size(300, 23);
            textBoxWarter.TabIndex = 17;
            // 
            // checkBoxCreateVideo
            // 
            checkBoxCreateVideo.AutoSize = true;
            checkBoxCreateVideo.Checked = true;
            checkBoxCreateVideo.CheckState = CheckState.Checked;
            checkBoxCreateVideo.Location = new Point(322, 179);
            checkBoxCreateVideo.Name = "checkBoxCreateVideo";
            checkBoxCreateVideo.Size = new Size(111, 21);
            checkBoxCreateVideo.TabIndex = 19;
            checkBoxCreateVideo.Text = "是否生成视频？";
            checkBoxCreateVideo.UseVisualStyleBackColor = true;
            checkBoxCreateVideo.CheckedChanged += checkBoxCreateVideo_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(566, 330);
            Controls.Add(checkBoxCreateVideo);
            Controls.Add(BrowseWarter);
            Controls.Add(textBoxWarter);
            Controls.Add(buttonStop);
            Controls.Add(buttonPause);
            Controls.Add(buttonChangeBackgroundColor);
            Controls.Add(checkBoxUseGpu);
            Controls.Add(labelGpuInfo);
            Controls.Add(labelCurrentStep);
            Controls.Add(labelTotalFrames);
            Controls.Add(labelTime);
            Controls.Add(progressBar);
            Controls.Add(labelStatus);
            Controls.Add(buttonProcess);
            Controls.Add(buttonBrowseFrames);
            Controls.Add(buttonBrowseOutput);
            Controls.Add(buttonBrowseInput);
            Controls.Add(textBoxFramesDirectory);
            Controls.Add(textBoxOutputVideo);
            Controls.Add(textBoxInputVideo);
            Controls.Add(labelTotalProgress);
            Controls.Add(checkBoxExtractFrames);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "视频每帧图片提取与水印处理V1.0";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private CheckBox checkBoxCreateVideo;
    }
}