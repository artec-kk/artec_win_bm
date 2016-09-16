namespace ScratchConnection.Forms
{
    partial class ServoAngle
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServoAngle));
            this.lbPort = new System.Windows.Forms.Label();
            this.lbDegree = new System.Windows.Forms.Label();
            this.nudAngle = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudAngle)).BeginInit();
            this.SuspendLayout();
            // 
            // lbPort
            // 
            resources.ApplyResources(this.lbPort, "lbPort");
            this.lbPort.Name = "lbPort";
            // 
            // lbDegree
            // 
            resources.ApplyResources(this.lbDegree, "lbDegree");
            this.lbDegree.Name = "lbDegree";
            // 
            // nudAngle
            // 
            resources.ApplyResources(this.nudAngle, "nudAngle");
            this.nudAngle.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudAngle.Minimum = new decimal(new int[] {
            15,
            0,
            0,
            -2147483648});
            this.nudAngle.Name = "nudAngle";
            // 
            // ServoAngle
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbDegree);
            this.Controls.Add(this.nudAngle);
            this.Controls.Add(this.lbPort);
            this.Name = "ServoAngle";
            ((System.ComponentModel.ISupportInitialize)(this.nudAngle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbDegree;
        protected System.Windows.Forms.Label lbPort;
        public System.Windows.Forms.NumericUpDown nudAngle;
    }
}
