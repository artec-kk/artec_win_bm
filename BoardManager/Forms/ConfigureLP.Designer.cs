namespace ScratchConnection.Forms
{
    partial class ConfigureLP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigureLP));
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.cbLEDD6 = new System.Windows.Forms.CheckBox();
            this.cbLEDD9 = new System.Windows.Forms.CheckBox();
            this.cbLEDD5 = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.cbClock = new System.Windows.Forms.CheckBox();
            this.cbbSIn4 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbbSIn6 = new System.Windows.Forms.ComboBox();
            this.cbbSIn1 = new System.Windows.Forms.ComboBox();
            this.cbSIn1 = new System.Windows.Forms.CheckBox();
            this.cbbSIn5 = new System.Windows.Forms.ComboBox();
            this.cbSIn6 = new System.Windows.Forms.CheckBox();
            this.cbSIn5 = new System.Windows.Forms.CheckBox();
            this.cbSIn4 = new System.Windows.Forms.CheckBox();
            this.cbbSIn3 = new System.Windows.Forms.ComboBox();
            this.cbbSIn2 = new System.Windows.Forms.ComboBox();
            this.cbSIn3 = new System.Windows.Forms.CheckBox();
            this.cbSIn2 = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cbSVD6 = new System.Windows.Forms.CheckBox();
            this.cbSVD5 = new System.Windows.Forms.CheckBox();
            this.cbSVD11 = new System.Windows.Forms.CheckBox();
            this.cbSVD10 = new System.Windows.Forms.CheckBox();
            this.cbSVD9 = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDCM2 = new System.Windows.Forms.CheckBox();
            this.cbDCM1 = new System.Windows.Forms.CheckBox();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.SandyBrown;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.cbLEDD6);
            this.panel4.Controls.Add(this.cbLEDD9);
            this.panel4.Controls.Add(this.cbLEDD5);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cbLEDD6
            // 
            resources.ApplyResources(this.cbLEDD6, "cbLEDD6");
            this.cbLEDD6.Name = "cbLEDD6";
            this.cbLEDD6.UseVisualStyleBackColor = true;
            this.cbLEDD6.CheckedChanged += new System.EventHandler(this.cbLEDD569_CheckedChanged);
            // 
            // cbLEDD9
            // 
            resources.ApplyResources(this.cbLEDD9, "cbLEDD9");
            this.cbLEDD9.Name = "cbLEDD9";
            this.cbLEDD9.UseVisualStyleBackColor = true;
            this.cbLEDD9.CheckedChanged += new System.EventHandler(this.cbLEDD569_CheckedChanged);
            // 
            // cbLEDD5
            // 
            resources.ApplyResources(this.cbLEDD5, "cbLEDD5");
            this.cbLEDD5.Name = "cbLEDD5";
            this.cbLEDD5.UseVisualStyleBackColor = true;
            this.cbLEDD5.CheckedChanged += new System.EventHandler(this.cbLEDD569_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.cbClock);
            this.panel3.Controls.Add(this.cbbSIn4);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.cbbSIn6);
            this.panel3.Controls.Add(this.cbbSIn1);
            this.panel3.Controls.Add(this.cbSIn1);
            this.panel3.Controls.Add(this.cbbSIn5);
            this.panel3.Controls.Add(this.cbSIn6);
            this.panel3.Controls.Add(this.cbSIn5);
            this.panel3.Controls.Add(this.cbSIn4);
            this.panel3.Controls.Add(this.cbbSIn3);
            this.panel3.Controls.Add(this.cbbSIn2);
            this.panel3.Controls.Add(this.cbSIn3);
            this.panel3.Controls.Add(this.cbSIn2);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // cbClock
            // 
            resources.ApplyResources(this.cbClock, "cbClock");
            this.cbClock.Name = "cbClock";
            this.cbClock.UseVisualStyleBackColor = true;
            this.cbClock.CheckedChanged += new System.EventHandler(this.cbClock_CheckedChanged);
            // 
            // cbbSIn4
            // 
            this.cbbSIn4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn4, "cbbSIn4");
            this.cbbSIn4.FormattingEnabled = true;
            this.cbbSIn4.Name = "cbbSIn4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cbbSIn6
            // 
            this.cbbSIn6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn6, "cbbSIn6");
            this.cbbSIn6.FormattingEnabled = true;
            this.cbbSIn6.Name = "cbbSIn6";
            this.cbbSIn6.SelectedIndexChanged += new System.EventHandler(this.cbbSIn56_SelectedIndexChanged);
            // 
            // cbbSIn1
            // 
            this.cbbSIn1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn1, "cbbSIn1");
            this.cbbSIn1.FormattingEnabled = true;
            this.cbbSIn1.Name = "cbbSIn1";
            // 
            // cbSIn1
            // 
            resources.ApplyResources(this.cbSIn1, "cbSIn1");
            this.cbSIn1.Name = "cbSIn1";
            this.cbSIn1.UseVisualStyleBackColor = true;
            this.cbSIn1.CheckedChanged += new System.EventHandler(this.cbSIn_CheckedChanged);
            // 
            // cbbSIn5
            // 
            this.cbbSIn5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn5, "cbbSIn5");
            this.cbbSIn5.FormattingEnabled = true;
            this.cbbSIn5.Name = "cbbSIn5";
            this.cbbSIn5.SelectedIndexChanged += new System.EventHandler(this.cbbSIn56_SelectedIndexChanged);
            // 
            // cbSIn6
            // 
            resources.ApplyResources(this.cbSIn6, "cbSIn6");
            this.cbSIn6.Name = "cbSIn6";
            this.cbSIn6.UseVisualStyleBackColor = true;
            this.cbSIn6.CheckedChanged += new System.EventHandler(this.cbSIn56_CheckedChanged);
            // 
            // cbSIn5
            // 
            resources.ApplyResources(this.cbSIn5, "cbSIn5");
            this.cbSIn5.Name = "cbSIn5";
            this.cbSIn5.UseVisualStyleBackColor = true;
            this.cbSIn5.CheckedChanged += new System.EventHandler(this.cbSIn56_CheckedChanged);
            // 
            // cbSIn4
            // 
            resources.ApplyResources(this.cbSIn4, "cbSIn4");
            this.cbSIn4.Name = "cbSIn4";
            this.cbSIn4.UseVisualStyleBackColor = true;
            this.cbSIn4.CheckedChanged += new System.EventHandler(this.cbSIn34_CheckedChanged);
            // 
            // cbbSIn3
            // 
            this.cbbSIn3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn3, "cbbSIn3");
            this.cbbSIn3.FormattingEnabled = true;
            this.cbbSIn3.Name = "cbbSIn3";
            // 
            // cbbSIn2
            // 
            this.cbbSIn2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbbSIn2, "cbbSIn2");
            this.cbbSIn2.FormattingEnabled = true;
            this.cbbSIn2.Name = "cbbSIn2";
            // 
            // cbSIn3
            // 
            resources.ApplyResources(this.cbSIn3, "cbSIn3");
            this.cbSIn3.Name = "cbSIn3";
            this.cbSIn3.UseVisualStyleBackColor = true;
            this.cbSIn3.CheckedChanged += new System.EventHandler(this.cbSIn34_CheckedChanged);
            // 
            // cbSIn2
            // 
            resources.ApplyResources(this.cbSIn2, "cbSIn2");
            this.cbSIn2.Name = "cbSIn2";
            this.cbSIn2.UseVisualStyleBackColor = true;
            this.cbSIn2.CheckedChanged += new System.EventHandler(this.cbSIn_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightCoral;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.cbSVD6);
            this.panel2.Controls.Add(this.cbSVD5);
            this.panel2.Controls.Add(this.cbSVD11);
            this.panel2.Controls.Add(this.cbSVD10);
            this.panel2.Controls.Add(this.cbSVD9);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // cbSVD6
            // 
            resources.ApplyResources(this.cbSVD6, "cbSVD6");
            this.cbSVD6.Name = "cbSVD6";
            this.cbSVD6.UseVisualStyleBackColor = true;
            this.cbSVD6.CheckedChanged += new System.EventHandler(this.cbSVOut56910_CheckedChanged);
            // 
            // cbSVD5
            // 
            resources.ApplyResources(this.cbSVD5, "cbSVD5");
            this.cbSVD5.Name = "cbSVD5";
            this.cbSVD5.UseVisualStyleBackColor = true;
            this.cbSVD5.CheckedChanged += new System.EventHandler(this.cbSVOut56910_CheckedChanged);
            // 
            // cbSVD11
            // 
            resources.ApplyResources(this.cbSVD11, "cbSVD11");
            this.cbSVD11.Name = "cbSVD11";
            this.cbSVD11.UseVisualStyleBackColor = true;
            this.cbSVD11.CheckedChanged += new System.EventHandler(this.cbSVOut11_CheckedChanged);
            // 
            // cbSVD10
            // 
            resources.ApplyResources(this.cbSVD10, "cbSVD10");
            this.cbSVD10.Name = "cbSVD10";
            this.cbSVD10.UseVisualStyleBackColor = true;
            this.cbSVD10.CheckedChanged += new System.EventHandler(this.cbSVOut56910_CheckedChanged);
            // 
            // cbSVD9
            // 
            resources.ApplyResources(this.cbSVD9, "cbSVD9");
            this.cbSVD9.Name = "cbSVD9";
            this.cbSVD9.UseVisualStyleBackColor = true;
            this.cbSVD9.CheckedChanged += new System.EventHandler(this.cbSVOut56910_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cbDCM2);
            this.panel1.Controls.Add(this.cbDCM1);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cbDCM2
            // 
            resources.ApplyResources(this.cbDCM2, "cbDCM2");
            this.cbDCM2.Name = "cbDCM2";
            this.cbDCM2.UseVisualStyleBackColor = true;
            this.cbDCM2.CheckedChanged += new System.EventHandler(this.cbDCOut1_CheckedChanged);
            // 
            // cbDCM1
            // 
            resources.ApplyResources(this.cbDCM1, "cbDCM1");
            this.cbDCM1.Name = "cbDCM1";
            this.cbDCM1.UseVisualStyleBackColor = true;
            this.cbDCM1.CheckedChanged += new System.EventHandler(this.cbDCOut1_CheckedChanged);
            // 
            // ConfigureLP
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ConfigureLP";
            this.Controls.SetChildIndex(this.panel1, 0);
            this.Controls.SetChildIndex(this.panel2, 0);
            this.Controls.SetChildIndex(this.panel3, 0);
            this.Controls.SetChildIndex(this.panel4, 0);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbLEDD6;
        private System.Windows.Forms.CheckBox cbLEDD9;
        private System.Windows.Forms.CheckBox cbLEDD5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ComboBox cbbSIn4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbbSIn6;
        private System.Windows.Forms.ComboBox cbbSIn1;
        private System.Windows.Forms.CheckBox cbSIn1;
        private System.Windows.Forms.ComboBox cbbSIn5;
        private System.Windows.Forms.CheckBox cbSIn6;
        private System.Windows.Forms.CheckBox cbSIn5;
        private System.Windows.Forms.CheckBox cbSIn4;
        private System.Windows.Forms.ComboBox cbbSIn3;
        private System.Windows.Forms.ComboBox cbbSIn2;
        private System.Windows.Forms.CheckBox cbSIn3;
        private System.Windows.Forms.CheckBox cbSIn2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbSVD6;
        private System.Windows.Forms.CheckBox cbSVD5;
        private System.Windows.Forms.CheckBox cbSVD11;
        private System.Windows.Forms.CheckBox cbSVD10;
        private System.Windows.Forms.CheckBox cbSVD9;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbDCM2;
        private System.Windows.Forms.CheckBox cbDCM1;
        private System.Windows.Forms.CheckBox cbClock;
    }
}