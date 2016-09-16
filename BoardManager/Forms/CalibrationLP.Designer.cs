namespace ScratchConnection.Forms
{
    partial class CalibrationLP
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
            this.saD5 = new ScratchConnection.Forms.ServoAngle();
            this.saD6 = new ScratchConnection.Forms.ServoAngle();
            this.saD9 = new ScratchConnection.Forms.ServoAngle();
            this.saD10 = new ScratchConnection.Forms.ServoAngle();
            this.saD11 = new ScratchConnection.Forms.ServoAngle();
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).BeginInit();
            this.gbServo.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbServo
            // 
            this.gbServo.Controls.Add(this.saD9);
            this.gbServo.Controls.Add(this.saD11);
            this.gbServo.Controls.Add(this.saD10);
            this.gbServo.Controls.Add(this.saD6);
            this.gbServo.Controls.Add(this.saD5);
            this.gbServo.Controls.SetChildIndex(this.saD5, 0);
            this.gbServo.Controls.SetChildIndex(this.saD6, 0);
            this.gbServo.Controls.SetChildIndex(this.saD10, 0);
            this.gbServo.Controls.SetChildIndex(this.saD11, 0);
            this.gbServo.Controls.SetChildIndex(this.saD9, 0);
            // 
            // saD5
            // 
            this.saD5.Location = new System.Drawing.Point(24, 46);
            this.saD5.Name = "saD5";
            this.saD5.portName = "D5";
            this.saD5.Size = new System.Drawing.Size(119, 25);
            this.saD5.TabIndex = 10;
            // 
            // saD6
            // 
            this.saD6.Location = new System.Drawing.Point(24, 71);
            this.saD6.Name = "saD6";
            this.saD6.portName = "D6";
            this.saD6.Size = new System.Drawing.Size(119, 25);
            this.saD6.TabIndex = 10;
            // 
            // saD9
            // 
            this.saD9.Location = new System.Drawing.Point(149, 46);
            this.saD9.Name = "saD9";
            this.saD9.portName = "D9";
            this.saD9.Size = new System.Drawing.Size(119, 25);
            this.saD9.TabIndex = 10;
            // 
            // saD10
            // 
            this.saD10.Location = new System.Drawing.Point(149, 71);
            this.saD10.Name = "saD10";
            this.saD10.portName = "D10";
            this.saD10.Size = new System.Drawing.Size(119, 25);
            this.saD10.TabIndex = 10;
            // 
            // saD11
            // 
            this.saD11.Location = new System.Drawing.Point(149, 96);
            this.saD11.Name = "saD11";
            this.saD11.portName = "D11";
            this.saD11.Size = new System.Drawing.Size(119, 25);
            this.saD11.TabIndex = 10;
            // 
            // CalibrationLP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 370);
            this.Name = "CalibrationLP";
            ((System.ComponentModel.ISupportInitialize)(this.tbM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbM1)).EndInit();
            this.gbServo.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ServoAngle saD5;
        private ServoAngle saD6;
        private ServoAngle saD9;
        private ServoAngle saD11;
        private ServoAngle saD10;
    }
}