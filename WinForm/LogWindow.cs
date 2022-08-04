using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Shared.Models;
using WeifenLuo.WinFormsUI.Docking;
using static log4net.Core.Level;

namespace WinForm;

public partial class LogWindow : DockContent, IAppender
{
    private enum MsgColumnType
    {
        Time,
        LogType,
        Source,
        Message
    }

    private readonly List<LoggingEvent> _messages = new();
    private readonly int _maxMsgCount = 500;
    private bool _update;

    public LogWindow()
    {
        ((Hierarchy) LogManager.GetRepository()).Root.AddAppender(this);
        InitializeComponent();
    }

    public void DoAppend(LoggingEvent loggingEvent)
    {
        if (loggingEvent.MessageObject is ObserverUpdate log) return;
        if (InvokeRequired)
        {
            BeginInvoke(new Action<LoggingEvent>(DoAppend), loggingEvent);
        }
        else
        {
            if (loggingEvent.Level != Debug)
            {
                _messages.Insert(0, loggingEvent);
                if (_messages.Count > _maxMsgCount) _messages.RemoveAt(_maxMsgCount);

                _update = true;
            }
        }
    }

    private void SetRowColor(Level level, int rowIndex)
    {
        if (level == Error)
            log_dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Red;
        else if (level == Warn)
            log_dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
        else
            log_dataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.White;
    }

    private void log_dataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _messages.Count) return;

        var myLog = _messages[e.RowIndex];
        switch (e.ColumnIndex)
        {
            case (int) MsgColumnType.Time:
                e.Value = myLog.TimeStamp.ToString(CultureInfo.InvariantCulture);
                break;
            case (int) MsgColumnType.LogType:
                e.Value = myLog.Level.ToString();
                break;
            case (int) MsgColumnType.Source:
                e.Value = myLog.Identity;
                break;
            case (int) MsgColumnType.Message:
                e.Value = myLog.GetLoggingEventData().Message;
                break;
        }

        SetRowColor(myLog.Level, e.RowIndex);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (_update == false) return;
        log_dataGridView.RowCount = _messages.Count;
        log_dataGridView.Refresh();
        _update = false;
    }

    private void LogWindow_Load(object sender, EventArgs e)
    {
        ((Hierarchy) LogManager.GetRepository()).Root.AddAppender(this);
        timer1.Enabled = true;
        timer1.Start();
    }

    private void LogWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        timer1.Stop();
        timer1.Dispose();
    }
}
