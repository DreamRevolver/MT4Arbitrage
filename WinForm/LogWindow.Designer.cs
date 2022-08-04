using System.ComponentModel;

namespace WinForm
{
    partial class LogWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.log_dataGridView = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LogType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize) (this.log_dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // log_dataGridView
            // 
            this.log_dataGridView.AllowUserToAddRows = false;
            this.log_dataGridView.AllowUserToDeleteRows = false;
            this.log_dataGridView.AllowUserToResizeColumns = false;
            this.log_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.log_dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {this.Time, this.LogType, this.Source, this.Message});
            this.log_dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.log_dataGridView.Location = new System.Drawing.Point(0, 0);
            this.log_dataGridView.Name = "log_dataGridView";
            this.log_dataGridView.RowHeadersVisible = false;
            this.log_dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.log_dataGridView.Size = new System.Drawing.Size(800, 450);
            this.log_dataGridView.TabIndex = 0;
            this.log_dataGridView.VirtualMode = true;
            this.log_dataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.log_dataGridView_CellValueNeeded);
            // 
            // Time
            // 
            this.Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Time.FillWeight = 10F;
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            // 
            // LogType
            // 
            this.LogType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LogType.FillWeight = 10F;
            this.LogType.HeaderText = "Type";
            this.LogType.Name = "LogType";
            this.LogType.ReadOnly = true;
            // 
            // Source
            // 
            this.Source.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Source.FillWeight = 20F;
            this.Source.HeaderText = "Source";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            // 
            // Message
            // 
            this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Message.FillWeight = 60F;
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // LogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.log_dataGridView);
            this.Name = "LogWindow";
            this.TabText = "LogWindow";
            this.Text = "LogWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LogWindow_FormClosing);
            this.Load += new System.EventHandler(this.LogWindow_Load);
            ((System.ComponentModel.ISupportInitialize) (this.log_dataGridView)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Timer timer1;

        private System.Windows.Forms.DataGridViewTextBoxColumn Message;

        private System.Windows.Forms.DataGridViewTextBoxColumn Source;

        private System.Windows.Forms.DataGridViewTextBoxColumn LogType;

        private System.Windows.Forms.DataGridViewTextBoxColumn Time;

        private System.Windows.Forms.DataGridView log_dataGridView;

        #endregion
    }
}