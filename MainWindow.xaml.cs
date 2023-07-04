using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MIB_Browser;
/// <summary>
/// MIB Browser主界面
/// </summary>
public sealed partial class MainWindow : Window
{
    private MIB_Browser Browser
    {
        get; set;
    }
    private SynchronizationContext Context
    {
        get; set;
    }
    private readonly BackgroundWorker _worker = new();

    public MainWindow()
    {
        this.InitializeComponent();
        // 启用Acyclic材质
        this.SystemBackdrop = new DesktopAcrylicBackdrop();
        // 配置BackgroundWorker
        _worker.WorkerSupportsCancellation = true;
        _worker.WorkerReportsProgress = true;
        _worker.DoWork += _worker_ScanIPRange;
        _worker.ProgressChanged += _worker_ProgressChanged;
        _worker.RunWorkerCompleted += _worker_RunTaskCompleted;
        // 获取UI线程上下文
        Context = SynchronizationContext.Current;
        // 设置标题栏
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar);
        // 重设窗口大小
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId WndID = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(WndID);
        appWindow.Resize(new Windows.Graphics.SizeInt32(820, 600));
        appWindow.Title = "MIB Browser";
        // 创建MIB Browser实例
        Browser = new MIB_Browser(ip: Properties.mibbrowser.Default.AgentIP, community: Properties.mibbrowser.Default.Community, timeout: (int)Properties.mibbrowser.Default.TimeOut, maxRepetitions: (int)Properties.mibbrowser.Default.MaxRepetitions);
        this.OID_ComboBox.SelectedIndex = 0;
        // 绑定事件响应函数
        this.Closed += MainWindow_Closed;
        this.AgentIP.TextChanged += AgentIP_Changed;
        this.OID_ComboBox.TextSubmitted += OID_Changed;
        this.Community.TextChanged += Community_Changed;
        this.TimeOut.ValueChanged += TimeOut_Changed;
        this.MaxRepetitions.ValueChanged += MaxRepetition_ValueChanged;
        this.GetValueBtn.Click += GetValueBtn_Click;
        this.GetNextBtn.Click += GetNextBtn_Click;
        this.ScanIPBtn.Click += ScanIPBtn_Click;
        this.GetBulkBtn.Click += GetBulkBtn_Click;
        this.GetTreeBtn.Click += GetTreeBtn_Click;
    }

    private async void GetTreeBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var variables = await Browser.GetTreeAsync();
            if (variables == null) return;
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
            }
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = ex.Message,
                Content = ex.GetType().ToString(),
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };
            await dialog.ShowAsync();
        }
    }

    private async void GetBulkBtn_Click(object sender, RoutedEventArgs e)
    {
        ControllerStateChange(false);
        try
        {
            var variables = await Browser.GetBulkAsync();
            if (variables == null) return;
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
            }
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = ex.Message,
                Content = ex.GetType().ToString(),
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };
            await dialog.ShowAsync();
        }
        finally { ControllerStateChange(true); }
    }

    private async void GetValueBtn_Click(object sender, RoutedEventArgs e)
    {
        ControllerStateChange(false);
        try
        {
            var variables = await Browser.GetRequestAsync();
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
                this.Browser.OID_History.Remove(variable.Id.ToString());
                this.Browser.OID_History.Insert(0, variable.Id.ToString());
            }
            this.OID_ComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = ex.Message,
                Content = ex.GetType().ToString(),
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };
            await dialog.ShowAsync();
        }
        finally { ControllerStateChange(true); }
    }

    private async void GetNextBtn_Click(object sender, RoutedEventArgs e)
    {
        ControllerStateChange(false);
        try
        {
            var variables = await Browser.GetNextRequestAsync();
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
                this.Browser.OID_History.Remove(variable.Id.ToString());
                this.Browser.OID_History.Insert(0, variable.Id.ToString());
                this.OID_ComboBox.SelectedIndex = 0;
                this.Browser.OID = variable.Id.ToString();
            }
            this.OID_ComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.Content.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = ex.Message,
                Content = ex.GetType().ToString(),
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };
            await dialog.ShowAsync();
        }
        finally { ControllerStateChange(true); }
    }

    private void ScanIPBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!_worker.IsBusy)
        {
            ControllerStateChange(false);
            this.ProgressBar.Visibility = Visibility.Visible;
            this.ScanIPBtn.Content = "Stop";
            this.ScanIPBtn.IsEnabled = true;
            var args = new object[2];
            args[0] = this.IPBegin.Text;
            args[1] = this.IPEnd.Text;
            _worker.RunWorkerAsync(args);
        }
        else
        {
            _worker.CancelAsync();
            this.ScanIPBtn.IsEnabled = false;
        }
    }

    private void AppendResult(object state)
    {
        var text = (string)state;
        this.ResultsTextBlock.Text += text;
        this.ResultsScollViewer.ScrollToVerticalOffset(ResultsTextBlock.ActualHeight);
    }

    private void OID_Changed(ComboBox sender, ComboBoxTextSubmittedEventArgs e) => Browser.OID =this.OID_ComboBox.Text;
    private void AgentIP_Changed(object sender, TextChangedEventArgs e) => Browser.IP=this.AgentIP.Text;
    private void Community_Changed(object sender, TextChangedEventArgs e) => Browser.Community = this.Community.Text;
    private void TimeOut_Changed(NumberBox sender, NumberBoxValueChangedEventArgs e) => Browser.Timeout = (int)this.TimeOut.Value;
    private void MaxRepetition_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) => this.Browser.MaxRepetitions = (int)this.MaxRepetitions.Value;


    private void _worker_ScanIPRange(object sender, DoWorkEventArgs e)
    {
        if (sender is not BackgroundWorker worker) return;
        var args = e.Argument as object[]; ;
        var ip_begin = MIB_Browser.IPtoUINT((string)args[0]);
        var ip_end = MIB_Browser.IPtoUINT((string)args[1]);
        if (ip_begin == 0 || ip_end == 0)
        {
            return;
        }
        if (ip_begin > ip_end) { return; }
        Browser.OID = "1.3.6.1.2.1.1.5.0";
        Ping pingsender = new();
        for (var i = ip_begin; i <= ip_end; i++)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                break;
            }
            var addr = MIB_Browser.UINTtoIP(i);

            var reply = pingsender.Send(addr, 500);
            if (reply.Status == IPStatus.Success) { 
                Browser.IP = addr;
                try
                {
                    var variables = Browser.GetRequest();
                    foreach (var variable in variables)
                        Context.Post(AppendResult, addr + " is online. Hostname: " + variable.Data + "\n");
                }
                catch {
                    Context.Post(AppendResult, addr + " is online. But SNMP request can NOT reach.\n");
                }
                finally
                {
                    worker.ReportProgress((int)((i - ip_begin + 1.0) / (ip_end - ip_begin + 1.0) * 100));
                }
            }
            else
                worker.ReportProgress((int)((i - ip_begin + 1.0) / (ip_end - ip_begin + 1.0) * 100));
        }
        Browser.OID = Browser.OID_History[0];
    }

    private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        this.ProgressBar.Value = e.ProgressPercentage;
    }

    private void _worker_RunTaskCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ControllerStateChange(true);
        this.ScanIPBtn.Content = "Scan";
        this.ProgressBar.Value = 0;
        this.ProgressBar.Visibility = Visibility.Collapsed;
        if (e.Cancelled)
        {
            this.ScanIPBtn.IsEnabled = true;
        }
        Browser.IP = this.AgentIP.Text;
        Browser.OID = OID_ComboBox.SelectedValue.ToString();
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        Properties.mibbrowser.Default.AgentIP = this.AgentIP.Text;
        Properties.mibbrowser.Default.Community = this.Community.Text;
        Properties.mibbrowser.Default.TimeOut = (int)this.TimeOut.Value;
        Properties.mibbrowser.Default.MaxRepetitions = (int)this.MaxRepetitions.Value;
        Properties.mibbrowser.Default.Save();
    }

    private void ControllerStateChange(bool state)
    {
        this.ScanIPBtn.IsEnabled = state;
        this.GetValueBtn.IsEnabled = state;
        this.GetNextBtn.IsEnabled = state;
        this.AgentIP.IsEnabled = state;
        this.OID_ComboBox.IsEnabled = state;
        this.TimeOut.IsEnabled = state;
        this.Community.IsEnabled = state;
        this.IPBegin.IsEnabled = state;
        this.IPEnd.IsEnabled = state;
        this.GetTreeBtn.IsEnabled = state;
        this.GetBulkBtn.IsEnabled = state;
    }
}
