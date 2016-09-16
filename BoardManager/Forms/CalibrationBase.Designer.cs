namespace ScratchConnection.Forms
{
    partial class CalibrationBase
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalibrationBase));
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.gbDC = new System.Windows.Forms.GroupBox();
            this.lbDCM2 = new System.Windows.Forms.Label();
            this.lbDCM1 = new System.Windows.Forms.Label();
            this.lbDCM2Value = new System.Windows.Forms.Label();
            this.lbDCM1Value = new System.Windows.Forms.Label();
            this.tbM2 = new System.Windows.Forms.TrackBar();
            this.tbM1 = new System.Windows.Forms.TrackBar();
            this.btDCCalibStop = new System.Windows.Forms.Button();
            this.btDCCalibStart = new System.Windows.Forms.Button();
            this.gbServo = new System.Windows.Forms.GroupBox();
            this.btReset = new System.Windows.Forms.Button();
            this.gbDC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).BeginInit();
            this.gbServo.SuspendLayout();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            resources.ApplyResources(this.btCancel, "btCancel");
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Name = "btCancel";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btOK
            // 
            resources.ApplyResources(this.btOK, "btOK");
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Name = "btOK";
            this.btOK.UseVisualStyleBackColor = true;
            // 
            // gbDC
            // 
            resources.ApplyResources(this.gbDC, "gbDC");
            this.gbDC.Controls.Add(this.lbDCM2);
            this.gbDC.Controls.Add(this.lbDCM1);
            this.gbDC.Controls.Add(this.lbDCM2Value);
            this.gbDC.Controls.Add(this.lbDCM1Value);
            this.gbDC.Controls.Add(this.tbM2);
            this.gbDC.Controls.Add(this.tbM1);
            this.gbDC.Controls.Add(this.btDCCalibStop);
            this.gbDC.Controls.Add(this.btDCCalibStart);
            this.gbDC.Name = "gbDC";
            this.gbDC.TabStop = false;
            // 
            // lbDCM2
            // 
            resources.ApplyResources(this.lbDCM2, "lbDCM2");
            this.lbDCM2.Name = "lbDCM2";
            // 
            // lbDCM1
            // 
            resources.ApplyResources(this.lbDCM1, "lbDCM1");
            this.lbDCM1.Name = "lbDCM1";
            // 
            // lbDCM2Value
            // 
            resources.ApplyResources(this.lbDCM2Value, "lbDCM2Value");
            this.lbDCM2Value.Name = "lbDCM2Value";
            // 
            // lbDCM1Value
            // 
            resources.ApplyResources(this.lbDCM1Value, "lbDCM1Value");
            this.lbDCM1Value.Name = "lbDCM1Value";
            // 
            // tbM2
            // 
            resources.ApplyResources(this.tbM2, "tbM2");
            this.tbM2.Maximum = 255;
            this.tbM2.Minimum = 127;
            this.tbM2.Name = "tbM2";
            this.tbM2.TickFrequency = 10;
            this.tbM2.Value = 255;
            // 
            // tbM1
            // 
            resources.ApplyResources(this.tbM1, "tbM1");
            this.tbM1.Maximum = 255;
            this.tbM1.Minimum = 127;
            this.tbM1.Name = "tbM1";
            this.tbM1.TickFrequency = 10;
            this.tbM1.Value = 255;
            // 
            // btDCCalibStop
            // 
            resources.ApplyResources(this.btDCCalibStop, "btDCCalibStop");
            this.btDCCalibStop.Name = "btDCCalibStop";
            this.btDCCalibStop.UseVisualStyleBackColor = true;
            this.btDCCalibStop.Click += new System.EventHandler(this.btDCCalibStart_Click);
            // 
            // btDCCalibStart
            // 
            resources.ApplyResources(this.btDCCalibStart, "btDCCalibStart");
            this.btDCCalibStart.Name = "btDCCalibStart";
            this.btDCCalibStart.UseVisualStyleBackColor = true;
            this.btDCCalibStart.Click += new System.EventHandler(this.btDCCalibStart_Click);
            // 
            // gbServo
            // 
            resources.ApplyResources(this.gbServo, "gbServo");
            this.gbServo.Controls.Add(this.btReset);
            this.gbServo.Name = "gbServo";
            this.gbServo.TabStop = false;
            // 
            // btReset
            // 
            resources.ApplyResources(this.btReset, "btReset");
            this.btReset.Name = "btReset";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.Click += new System.EventHandler(this.btReset_Click);
            // 
            // CalibrationBase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.gbServo);
            this.Controls.Add(this.gbDC);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CalibrationBase";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalibrationBase_FormClosing);
            this.gbDC.ResumeLayout(false);
            this.gbDC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).EndInit();
            this.gbServo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.GroupBox gbDC;
        private System.Windows.Forms.Label lbDCM2;
        private System.Windows.Forms.Label lbDCM1;
        private System.Windows.Forms.Label lbDCM2Value;
        private System.Windows.Forms.Label lbDCM1Value;
        protected System.Windows.Forms.TrackBar tbM2;
        protected System.Windows.Forms.TrackBar tbM1;
        protected System.Windows.Forms.Button btDCCalibStop;
        protected System.Windows.Forms.Button btDCCalibStart;
        private System.Windows.Forms.Button btReset;
        protected System.Windows.Forms.GroupBox gbServo;
    }
}