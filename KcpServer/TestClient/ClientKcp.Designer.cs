namespace TestClient
{
    partial class ClientKcp
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
            this.components = new System.ComponentModel.Container();
            this.button_close = new System.Windows.Forms.Button();
            this.button_Send = new System.Windows.Forms.Button();
            this.textBox_remote = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_init = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button_pingpong_init = new System.Windows.Forms.Button();
            this.button_pingpong_loop = new System.Windows.Forms.Button();
            this.checkBox_withflush = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(12, 124);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(75, 23);
            this.button_close.TabIndex = 38;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // button_Send
            // 
            this.button_Send.Location = new System.Drawing.Point(13, 95);
            this.button_Send.Name = "button_Send";
            this.button_Send.Size = new System.Drawing.Size(75, 23);
            this.button_Send.TabIndex = 37;
            this.button_Send.Text = "Send";
            this.button_Send.UseVisualStyleBackColor = true;
            this.button_Send.Click += new System.EventHandler(this.button_Send_Click);
            // 
            // textBox_remote
            // 
            this.textBox_remote.Location = new System.Drawing.Point(123, 21);
            this.textBox_remote.Name = "textBox_remote";
            this.textBox_remote.Size = new System.Drawing.Size(129, 21);
            this.textBox_remote.TabIndex = 36;
            this.textBox_remote.Text = "127.0.0.1:1000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 35;
            this.label2.Text = "remote ipep";
            // 
            // button_init
            // 
            this.button_init.Location = new System.Drawing.Point(12, 66);
            this.button_init.Name = "button_init";
            this.button_init.Size = new System.Drawing.Size(75, 23);
            this.button_init.TabIndex = 34;
            this.button_init.Text = "Init";
            this.button_init.UseVisualStyleBackColor = true;
            this.button_init.Click += new System.EventHandler(this.button_init_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button_pingpong_init
            // 
            this.button_pingpong_init.Location = new System.Drawing.Point(167, 66);
            this.button_pingpong_init.Name = "button_pingpong_init";
            this.button_pingpong_init.Size = new System.Drawing.Size(142, 23);
            this.button_pingpong_init.TabIndex = 39;
            this.button_pingpong_init.Text = "PingPongInit";
            this.button_pingpong_init.UseVisualStyleBackColor = true;
            this.button_pingpong_init.Click += new System.EventHandler(this.button_pingpong_init_Click);
            // 
            // button_pingpong_loop
            // 
            this.button_pingpong_loop.Location = new System.Drawing.Point(167, 95);
            this.button_pingpong_loop.Name = "button_pingpong_loop";
            this.button_pingpong_loop.Size = new System.Drawing.Size(142, 23);
            this.button_pingpong_loop.TabIndex = 40;
            this.button_pingpong_loop.Text = "PingPongLoop";
            this.button_pingpong_loop.UseVisualStyleBackColor = true;
            this.button_pingpong_loop.Click += new System.EventHandler(this.button_pingpong_loop_Click);
            // 
            // checkBox_withflush
            // 
            this.checkBox_withflush.AutoSize = true;
            this.checkBox_withflush.Checked = true;
            this.checkBox_withflush.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_withflush.Location = new System.Drawing.Point(231, 128);
            this.checkBox_withflush.Name = "checkBox_withflush";
            this.checkBox_withflush.Size = new System.Drawing.Size(84, 16);
            this.checkBox_withflush.TabIndex = 41;
            this.checkBox_withflush.Text = "With flush";
            this.checkBox_withflush.UseVisualStyleBackColor = true;
            // 
            // ClientKcp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 174);
            this.Controls.Add(this.checkBox_withflush);
            this.Controls.Add(this.button_pingpong_loop);
            this.Controls.Add(this.button_pingpong_init);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.button_Send);
            this.Controls.Add(this.textBox_remote);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_init);
            this.Name = "ClientKcp";
            this.Text = "ClientKcp";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Button button_Send;
        private System.Windows.Forms.TextBox textBox_remote;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_init;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button_pingpong_init;
        private System.Windows.Forms.Button button_pingpong_loop;
        private System.Windows.Forms.CheckBox checkBox_withflush;
    }
}