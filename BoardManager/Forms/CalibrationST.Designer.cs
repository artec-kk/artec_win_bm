namespace ScratchConnection.Forms
{
    partial class CalibrationST
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
            this.saD2 = new ScratchConnection.Forms.ServoAngle();
            this.saD4 = new ScratchConnection.Forms.ServoAngle();
            this.saD7 = new ScratchConnection.Forms.ServoAngle();
            this.saD8 = new ScratchConnection.Forms.ServoAngle();
            this.saD9 = new ScratchConnection.Forms.ServoAngle();
            this.saD10 = new ScratchConnection.Forms.ServoAngle();
            this.saD11 = new ScratchConnection.Forms.ServoAngle();
            this.saD12 = new ScratchConnection.Forms.ServoAngle();
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).BeginInit();
            this.gbServo.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbServo
            // 
            this.gbServo.Controls.Add(this.saD12);
            this.gbServo.Controls.Add(this.saD8);
            this.gbServo.Controls.Add(this.saD11);
            this.gbServo.Controls.Add(this.saD7);
            this.gbServo.Controls.Add(this.saD10);
            this.gbServo.Controls.Add(this.saD4);
            this.gbServo.Controls.Add(this.saD9);
            this.gbServo.Controls.Add(this.saD2);
            this.gbServo.Controls.SetChildIndex(this.saD2, 0);
            this.gbServo.Controls.SetChildIndex(this.saD9, 0);
            this.gbServo.Controls.SetChildIndex(this.saD4, 0);
            this.gbServo.Controls.SetChildIndex(this.saD10, 0);
            this.gbServo.Controls.SetChildIndex(this.saD7, 0);
            this.gbServo.Controls.SetChildIndex(this.saD11, 0);
            this.gbServo.Controls.SetChildIndex(this.saD8, 0);
            this.gbServo.Controls.SetChildIndex(this.saD12, 0);
            // 
            // saD2
            // 
            this.saD2.Location = new System.Drawing.Point(24, 46);
            this.saD2.Name = "saD2";
            this.saD2.portName = "D2";
            this.saD2.Size = new System.Drawing.Size(119, 25);
            this.saD2.TabIndex = 11;
            // 
            // saD4
            // 
            this.saD4.Location = new System.Drawing.Point(24, 71);
            this.saD4.Name = "saD4";
            this.saD4.portName = "D4";
            this.saD4.Size = new System.Drawing.Size(119, 25);
            this.saD4.TabIndex = 11;
            // 
            // saD7
            // 
            this.saD7.Location = new System.Drawing.Point(24, 96);
            this.saD7.Name = "saD7";
            this.saD7.portName = "D7";
            this.saD7.Size = new System.Drawing.Size(119, 25);
            this.saD7.TabIndex = 11;
            // 
            // saD8
            // 
            this.saD8.Location = new System.Drawing.Point(24, 121);
            this.saD8.Name = "saD8";
            this.saD8.portName = "D8";
            this.saD8.Size = new System.Drawing.Size(119, 25);
            this.saD8.TabIndex = 11;
            // 
            // saD9
            // 
            this.saD9.Location = new System.Drawing.Point(149, 46);
            this.saD9.Name = "saD9";
            this.saD9.portName = "D9";
            this.saD9.Size = new System.Drawing.Size(119, 25);
            this.saD9.TabIndex = 11;
            // 
            // saD10
            // 
            this.saD10.Location = new System.Drawing.Point(149, 71);
            this.saD10.Name = "saD10";
            this.saD10.portName = "D10";
            this.saD10.Size = new System.Drawing.Size(119, 25);
            this.saD10.TabIndex = 11;
            // 
            // saD11
            // 
            this.saD11.Location = new System.Drawing.Point(149, 96);
            this.saD11.Name = "saD11";
            this.saD11.portName = "D11";
            this.saD11.Size = new System.Drawing.Size(119, 25);
            this.saD11.TabIndex = 11;
            // 
            // saD12
            // 
            this.saD12.Location = new System.Drawing.Point(149, 121);
            this.saD12.Name = "saD12";
            this.saD12.portName = "D12";
            this.saD12.Size = new System.Drawing.Size(119, 25);
            this.saD12.TabIndex = 11;
            // 
            // CalibrationST
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 370);
            this.Name = "CalibrationST";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CalibrationST_FormClosing);
            this.Load += new System.EventHandler(this.CalibrationST_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).EndInit();
            this.gbServo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ServoAngle saD2;
        private ServoAngle saD7;
        private ServoAngle saD4;
        private ServoAngle saD12;
        private ServoAngle saD8;
        private ServoAngle saD11;
        private ServoAngle saD10;
        private ServoAngle saD9;
    }
}