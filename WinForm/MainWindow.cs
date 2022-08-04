using System;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WinForm;

public partial class MainWindow : Form
{
    private readonly LogWindow _logWindow;
    private readonly SpreadWindow _spreadWindow;

    public MainWindow(LogWindow logWindow, SpreadWindow spreadWindow)
    {
        _logWindow = logWindow;
        _spreadWindow = spreadWindow;
        InitializeComponent();
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        if (File.Exists("AppLayout.xml"))
        {
            dockPanel.LoadFromXml("AppLayout.xml", GetContentFromPersistString);
        }
        else
        {
            _logWindow.Show(dockPanel, DockState.DockBottom);
            _spreadWindow.Show(dockPanel, DockState.Document);
        }
    }

    private IDockContent GetContentFromPersistString(string persistString)
    {
        if (persistString == typeof(LogWindow).ToString()) return _logWindow;
        if (persistString == typeof(SpreadWindow).ToString()) return _spreadWindow;
        return null;
    }

    private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        dockPanel.SaveAsXml("AppLayout.xml");
    }
}
