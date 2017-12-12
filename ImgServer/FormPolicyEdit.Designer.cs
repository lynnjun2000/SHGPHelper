namespace ImgServer
{
    partial class FormPolicyEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label30 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.tbAutoCommitMS500 = new System.Windows.Forms.TextBox();
            this.dtAutoCommit500 = new System.Windows.Forms.DateTimePicker();
            this.label29 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.tbAutoCommitMS400 = new System.Windows.Forms.TextBox();
            this.dtAutoCommit400 = new System.Windows.Forms.DateTimePicker();
            this.label27 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.tbAutoCommitMS2 = new System.Windows.Forms.TextBox();
            this.dtAutoCommit2 = new System.Windows.Forms.DateTimePicker();
            this.label21 = new System.Windows.Forms.Label();
            this.ckSetAutoCommit = new System.Windows.Forms.CheckBox();
            this.cbAutoCommit = new System.Windows.Forms.CheckBox();
            this.cbAutoBid = new System.Windows.Forms.CheckBox();
            this.tbIncPrice = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.dtAutoBid = new System.Windows.Forms.DateTimePicker();
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label30);
            this.groupBox4.Controls.Add(this.label28);
            this.groupBox4.Controls.Add(this.tbAutoCommitMS500);
            this.groupBox4.Controls.Add(this.dtAutoCommit500);
            this.groupBox4.Controls.Add(this.label29);
            this.groupBox4.Controls.Add(this.label26);
            this.groupBox4.Controls.Add(this.tbAutoCommitMS400);
            this.groupBox4.Controls.Add(this.dtAutoCommit400);
            this.groupBox4.Controls.Add(this.label27);
            this.groupBox4.Controls.Add(this.label22);
            this.groupBox4.Controls.Add(this.tbAutoCommitMS2);
            this.groupBox4.Controls.Add(this.dtAutoCommit2);
            this.groupBox4.Controls.Add(this.label21);
            this.groupBox4.Controls.Add(this.ckSetAutoCommit);
            this.groupBox4.Controls.Add(this.cbAutoCommit);
            this.groupBox4.Controls.Add(this.cbAutoBid);
            this.groupBox4.Controls.Add(this.tbIncPrice);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.dtAutoBid);
            this.groupBox4.ForeColor = System.Drawing.Color.Black;
            this.groupBox4.Location = new System.Drawing.Point(2, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(337, 179);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "自动出价设定";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.ForeColor = System.Drawing.Color.Maroon;
            this.label30.Location = new System.Drawing.Point(151, 58);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(149, 12);
            this.label30.TabIndex = 25;
            this.label30.Text = "考虑50秒后价格延时或丢失";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(283, 107);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(29, 12);
            this.label28.TabIndex = 24;
            this.label28.Text = "毫秒";
            // 
            // tbAutoCommitMS500
            // 
            this.tbAutoCommitMS500.Location = new System.Drawing.Point(245, 101);
            this.tbAutoCommitMS500.Name = "tbAutoCommitMS500";
            this.tbAutoCommitMS500.Size = new System.Drawing.Size(33, 21);
            this.tbAutoCommitMS500.TabIndex = 23;
            this.tbAutoCommitMS500.Text = "700";
            // 
            // dtAutoCommit500
            // 
            this.dtAutoCommit500.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAutoCommit500.Location = new System.Drawing.Point(156, 102);
            this.dtAutoCommit500.Name = "dtAutoCommit500";
            this.dtAutoCommit500.ShowUpDown = true;
            this.dtAutoCommit500.Size = new System.Drawing.Size(85, 21);
            this.dtAutoCommit500.TabIndex = 22;
            this.dtAutoCommit500.Value = new System.DateTime(2015, 11, 10, 11, 29, 53, 0);
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(31, 108);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(95, 12);
            this.label29.TabIndex = 21;
            this.label29.Text = "提前500提交时间";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(283, 80);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(29, 12);
            this.label26.TabIndex = 20;
            this.label26.Text = "毫秒";
            // 
            // tbAutoCommitMS400
            // 
            this.tbAutoCommitMS400.Location = new System.Drawing.Point(245, 74);
            this.tbAutoCommitMS400.Name = "tbAutoCommitMS400";
            this.tbAutoCommitMS400.Size = new System.Drawing.Size(33, 21);
            this.tbAutoCommitMS400.TabIndex = 19;
            this.tbAutoCommitMS400.Text = "100";
            // 
            // dtAutoCommit400
            // 
            this.dtAutoCommit400.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAutoCommit400.Location = new System.Drawing.Point(156, 75);
            this.dtAutoCommit400.Name = "dtAutoCommit400";
            this.dtAutoCommit400.ShowUpDown = true;
            this.dtAutoCommit400.Size = new System.Drawing.Size(85, 21);
            this.dtAutoCommit400.TabIndex = 18;
            this.dtAutoCommit400.Value = new System.DateTime(2015, 11, 10, 11, 29, 53, 0);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(31, 81);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(95, 12);
            this.label27.TabIndex = 17;
            this.label27.Text = "提前400提交时间";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(283, 134);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(29, 12);
            this.label22.TabIndex = 16;
            this.label22.Text = "毫秒";
            // 
            // tbAutoCommitMS2
            // 
            this.tbAutoCommitMS2.Location = new System.Drawing.Point(245, 128);
            this.tbAutoCommitMS2.Name = "tbAutoCommitMS2";
            this.tbAutoCommitMS2.Size = new System.Drawing.Size(33, 21);
            this.tbAutoCommitMS2.TabIndex = 15;
            this.tbAutoCommitMS2.Text = "900";
            // 
            // dtAutoCommit2
            // 
            this.dtAutoCommit2.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAutoCommit2.Location = new System.Drawing.Point(156, 129);
            this.dtAutoCommit2.Name = "dtAutoCommit2";
            this.dtAutoCommit2.ShowUpDown = true;
            this.dtAutoCommit2.Size = new System.Drawing.Size(85, 21);
            this.dtAutoCommit2.TabIndex = 14;
            this.dtAutoCommit2.Value = new System.DateTime(2015, 11, 10, 11, 29, 53, 0);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(31, 135);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(77, 12);
            this.label21.TabIndex = 13;
            this.label21.Text = "最晚提交时间";
            // 
            // ckSetAutoCommit
            // 
            this.ckSetAutoCommit.AutoSize = true;
            this.ckSetAutoCommit.Location = new System.Drawing.Point(218, 157);
            this.ckSetAutoCommit.Name = "ckSetAutoCommit";
            this.ckSetAutoCommit.Size = new System.Drawing.Size(108, 16);
            this.ckSetAutoCommit.TabIndex = 11;
            this.ckSetAutoCommit.Text = "回车验证码完成";
            this.ckSetAutoCommit.UseVisualStyleBackColor = true;
            this.ckSetAutoCommit.Visible = false;
            // 
            // cbAutoCommit
            // 
            this.cbAutoCommit.AutoSize = true;
            this.cbAutoCommit.Enabled = false;
            this.cbAutoCommit.Location = new System.Drawing.Point(17, 58);
            this.cbAutoCommit.Name = "cbAutoCommit";
            this.cbAutoCommit.Size = new System.Drawing.Size(96, 16);
            this.cbAutoCommit.TabIndex = 7;
            this.cbAutoCommit.Text = "自动提交时间";
            this.cbAutoCommit.UseVisualStyleBackColor = true;
            // 
            // cbAutoBid
            // 
            this.cbAutoBid.AutoSize = true;
            this.cbAutoBid.Location = new System.Drawing.Point(17, 26);
            this.cbAutoBid.Name = "cbAutoBid";
            this.cbAutoBid.Size = new System.Drawing.Size(15, 14);
            this.cbAutoBid.TabIndex = 6;
            this.cbAutoBid.UseVisualStyleBackColor = true;
            // 
            // tbIncPrice
            // 
            this.tbIncPrice.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbIncPrice.ForeColor = System.Drawing.Color.Navy;
            this.tbIncPrice.Location = new System.Drawing.Point(199, 22);
            this.tbIncPrice.Name = "tbIncPrice";
            this.tbIncPrice.Size = new System.Drawing.Size(42, 23);
            this.tbIncPrice.TabIndex = 2;
            this.tbIncPrice.Text = "800";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(132, 26);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 12);
            this.label16.TabIndex = 1;
            this.label16.Text = "当前加价";
            // 
            // dtAutoBid
            // 
            this.dtAutoBid.CalendarForeColor = System.Drawing.Color.Blue;
            this.dtAutoBid.Font = new System.Drawing.Font("SimSun", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dtAutoBid.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtAutoBid.Location = new System.Drawing.Point(38, 22);
            this.dtAutoBid.Name = "dtAutoBid";
            this.dtAutoBid.ShowUpDown = true;
            this.dtAutoBid.Size = new System.Drawing.Size(88, 23);
            this.dtAutoBid.TabIndex = 0;
            this.dtAutoBid.Value = new System.DateTime(2015, 11, 20, 11, 29, 48, 0);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(175, 188);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 23);
            this.btOK.TabIndex = 8;
            this.btOK.Text = "确认";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(264, 188);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 9;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FormPolicyEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 240);
            this.ControlBox = false;
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.groupBox4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPolicyEdit";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "策略修改";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPolicyEdit_FormClosing);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox tbAutoCommitMS500;
        private System.Windows.Forms.DateTimePicker dtAutoCommit500;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.TextBox tbAutoCommitMS400;
        private System.Windows.Forms.DateTimePicker dtAutoCommit400;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tbAutoCommitMS2;
        private System.Windows.Forms.DateTimePicker dtAutoCommit2;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.CheckBox ckSetAutoCommit;
        private System.Windows.Forms.CheckBox cbAutoCommit;
        private System.Windows.Forms.CheckBox cbAutoBid;
        private System.Windows.Forms.TextBox tbIncPrice;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DateTimePicker dtAutoBid;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
    }
}