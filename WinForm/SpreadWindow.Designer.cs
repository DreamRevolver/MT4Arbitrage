using System.ComponentModel;

namespace WinForm
{
    partial class SpreadWindow
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
            this.InfoDataGridView = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Pair = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Spread = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FastBroker = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FastBid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FastAsk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SlowBroker = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SlowBid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SlowAsk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delta = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.InfoDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // InfoDataGridView
            // 
            this.InfoDataGridView.AllowUserToAddRows = false;
            this.InfoDataGridView.AllowUserToDeleteRows = false;
            this.InfoDataGridView.AllowUserToResizeColumns = false;
            this.InfoDataGridView.AllowUserToResizeRows = false;
            this.InfoDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.InfoDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.Time, this.Pair, this.Spread, this.FastBroker, this.FastBid, this.FastAsk, this.SlowBroker, this.SlowBid, this.SlowAsk, this.Delta });
            this.InfoDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoDataGridView.Location = new System.Drawing.Point(0, 0);
            this.InfoDataGridView.MultiSelect = false;
            this.InfoDataGridView.Name = "InfoDataGridView";
            this.InfoDataGridView.ReadOnly = true;
            this.InfoDataGridView.RowHeadersVisible = false;
            this.InfoDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.InfoDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.InfoDataGridView.Size = new System.Drawing.Size(800, 450);
            this.InfoDataGridView.TabIndex = 0;
            this.InfoDataGridView.VirtualMode = true;
            this.InfoDataGridView.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.InfoDataGridView_CellMouseClick);
            this.InfoDataGridView.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.InfoDataGridView_CellValueNeeded);
            // 
            // Time
            // 
            this.Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Time.FillWeight = 5F;
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            // 
            // Pair
            // 
            this.Pair.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Pair.FillWeight = 5F;
            this.Pair.HeaderText = "Pair";
            this.Pair.Name = "Pair";
            this.Pair.ReadOnly = true;
            // 
            // Spread
            // 
            this.Spread.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Spread.FillWeight = 5F;
            this.Spread.HeaderText = "Spread";
            this.Spread.Name = "Spread";
            this.Spread.ReadOnly = true;
            // 
            // FastBroker
            // 
            this.FastBroker.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FastBroker.FillWeight = 10F;
            this.FastBroker.HeaderText = "Fast Broker";
            this.FastBroker.Name = "FastBroker";
            this.FastBroker.ReadOnly = true;
            // 
            // FastBid
            // 
            this.FastBid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FastBid.FillWeight = 5F;
            this.FastBid.HeaderText = "Fast Bid";
            this.FastBid.Name = "FastBid";
            this.FastBid.ReadOnly = true;
            // 
            // FastAsk
            // 
            this.FastAsk.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FastAsk.FillWeight = 5F;
            this.FastAsk.HeaderText = "Fast Ask";
            this.FastAsk.Name = "FastAsk";
            this.FastAsk.ReadOnly = true;
            // 
            // SlowBroker
            // 
            this.SlowBroker.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SlowBroker.FillWeight = 10F;
            this.SlowBroker.HeaderText = "Slow Broker";
            this.SlowBroker.Name = "SlowBroker";
            this.SlowBroker.ReadOnly = true;
            // 
            // SlowBid
            // 
            this.SlowBid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SlowBid.FillWeight = 5F;
            this.SlowBid.HeaderText = "Slow Bid";
            this.SlowBid.Name = "SlowBid";
            this.SlowBid.ReadOnly = true;
            // 
            // SlowAsk
            // 
            this.SlowAsk.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SlowAsk.FillWeight = 5F;
            this.SlowAsk.HeaderText = "Slow Ask";
            this.SlowAsk.Name = "SlowAsk";
            this.SlowAsk.ReadOnly = true;
            // 
            // Delta
            // 
            this.Delta.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Delta.FillWeight = 5F;
            this.Delta.HeaderText = "Delta";
            this.Delta.Name = "Delta";
            this.Delta.ReadOnly = true;
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // SpreadWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.InfoDataGridView);
            this.Name = "SpreadWindow";
            this.TabText = "SpreadWindow";
            this.Text = "SpreadWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpreadWindow_FormClosing);
            this.Load += new System.EventHandler(this.SpreadWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.InfoDataGridView)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn Delta;

        private System.Windows.Forms.DataGridViewTextBoxColumn FastBroker;
        private System.Windows.Forms.DataGridViewTextBoxColumn FastBid;
        private System.Windows.Forms.DataGridViewTextBoxColumn FastAsk;
        private System.Windows.Forms.DataGridViewTextBoxColumn SlowBroker;
        private System.Windows.Forms.DataGridViewTextBoxColumn SlowBid;
        private System.Windows.Forms.DataGridViewTextBoxColumn SlowAsk;

        private System.Windows.Forms.Timer timer1;

        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Spread;
        private System.Windows.Forms.DataGridViewTextBoxColumn Pair;

        private System.Windows.Forms.DataGridView InfoDataGridView;

        #endregion
    }
}
