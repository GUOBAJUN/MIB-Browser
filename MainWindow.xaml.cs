using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;
using System.Diagnostics;
using WinRT.Interop;
using Microsoft.UI;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Windows.ApplicationModel.Resources.Core;
using System.Threading;
using System.ComponentModel;
using Lextm.SharpSnmpLib.Security;
using System.Runtime.InteropServices;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MIB_Browser;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MIB_Browser Browser
    {
        get; set;
    }

    private readonly BackgroundWorker _worker = new();
    private SynchronizationContext Context{ get; set; }

    private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
    private Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
    private Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configuarationSource;

    public MainWindow()
    {
        this.InitializeComponent();

        TrySetAcrylicBackdrop();
        
        // 配置BackgroundWorker
        _worker.WorkerSupportsCancellation = true;
        _worker.WorkerReportsProgress = true;
        _worker.DoWork += _worker_ScanIPRange;
        _worker.ProgressChanged += _worker_ProgressChanged;
        _worker.RunWorkerCompleted += _worker_RunTaskCompleted;

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
        Browser = new MIB_Browser(ip: this.AgentIP.Text, community: this.Community.Text, timeout: (int)this.TimeOut.Value, maxRepetitions:(int)MaxRepetitions.Value);
        this.OID_ComboBox.SelectedIndex = 0;
        // 绑定事件响应函数
        this.AgentIP.TextChanged += AgentIP_Changed;
        this.OID_ComboBox.TextSubmitted += OID_Changed;
        this.Community.TextChanged += Community_Changed;
        this.TimeOut.ValueChanged += TimeOut_Changed;
        this.MaxRepetitions.ValueChanged += MaxRepetition_ValueChanged;
        this.GetValueBtn.Click += GetValueBtn_Click;
        this.GetNextBtn.Click += GetNextBtn_Click;
        this.ScanIPBtn.Click += ScanIPBtn_Click;
        this.GetBulkBtn.Click += GetBulkBtn_Click;
    }

    private void GetBulkBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var variables = Browser.GetBulk();
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
            }
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new();
            dialog.XamlRoot = this.Content.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = ex.Message;
            dialog.Content = ex.GetType().ToString();
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            _ = dialog.ShowAsync();
        }
    }

    private void GetValueBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var variables = Browser.GetRequest();
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
                this.Browser.OID_History.Remove(variable.Id.ToString());
                this.Browser.OID_History.Insert(0, variable.Id.ToString());
            }
            this.OID_ComboBox.SelectedIndex = 0;
        }
        catch(Exception ex)
        {
            ContentDialog dialog = new();
            dialog.XamlRoot = this.Content.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = ex.Message;
            dialog.Content = ex.GetType().ToString();
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            _ = dialog.ShowAsync();
        }
    }

    private void GetNextBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var variables = Browser.GetNextRequest();
            foreach (var variable in variables)
            {
                AppendResult("[" + variable.Id + "] " + variable.Data + "\n");
                this.Browser.OID_History.Remove(variable.Id.ToString());
                this.Browser.OID_History.Insert(0, variable.Id.ToString());
                this.OID_ComboBox.SelectedIndex = 0;
                this.Browser.SetOID(variable.Id.ToString());
            }
            this.OID_ComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new();
            dialog.XamlRoot = this.Content.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = ex.Message;
            dialog.Content = ex.GetType().ToString();
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            _ = dialog.ShowAsync();
        }
    }

    private void ScanIPBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!_worker.IsBusy)
        {
            this.GetValueBtn.IsEnabled = false;
            this.GetNextBtn.IsEnabled = false;
            this.AgentIP.IsEnabled = false;
            this.OID_ComboBox.IsEnabled = false;
            this.TimeOut.IsEnabled = false;
            this.Community.IsEnabled = false;
            this.IPBegin.IsEnabled = false;
            this.IPEnd.IsEnabled = false;
            this.GetBulkBtn.IsEnabled = false;
            this.ProgressBar.Visibility = Visibility.Visible;
            this.ScanIPBtn.Content = "Stop";
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

    private void OID_Changed(ComboBox sender, ComboBoxTextSubmittedEventArgs e) => Browser?.SetOID(this.OID_ComboBox.Text);
    private void AgentIP_Changed(object sender, TextChangedEventArgs e) => Browser?.SetIP(this.AgentIP.Text);
    private void Community_Changed(object sender, TextChangedEventArgs e) => Browser?.SetCommunity(this.Community.Text);
    private void TimeOut_Changed(NumberBox sender, NumberBoxValueChangedEventArgs e) => Browser?.SetTimeout((int)this.TimeOut.Value);
    private void MaxRepetition_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs e) => this.Browser.SetMaxRepetitions((int)this.MaxRepetitions.Value);


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
        Browser.SetOID("1.3.6.1.2.1.1.5.0");
        for (var i = ip_begin; i <= ip_end; i++)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                break;
            }
            var addr = MIB_Browser.UINTtoIP(i);
            var addr4 = addr.Split('.')[3];
            if (addr4 == "0" || addr4 == "255")
                continue;
            Browser.SetIP(addr);
            try
            {
                var variables = Browser.GetRequest();
                foreach (var variable in variables)
                    Context.Post(AppendResult,addr + " is online. Hostname: " + variable.Data + "\n");
            }
            catch { }
            finally
            {
                worker.ReportProgress((int)((i-ip_begin+1.0) / (ip_end - ip_begin + 1.0) * 100));
            }
        }
    }

    private void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        this.ProgressBar.Value = e.ProgressPercentage;
    }

    private void _worker_RunTaskCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        this.GetValueBtn.IsEnabled = true;
        this.GetNextBtn.IsEnabled = true;
        this.AgentIP.IsEnabled = true;
        this.OID_ComboBox.IsEnabled = true;
        this.TimeOut.IsEnabled = true;
        this.Community.IsEnabled = true;
        this.IPBegin.IsEnabled = true;
        this.IPEnd.IsEnabled = true;
        this.GetBulkBtn.IsEnabled = true;
        this.ScanIPBtn.Content = "Scan";
        this.ProgressBar.Value = 0;
        this.ProgressBar.Visibility = Visibility.Collapsed;
        if(e.Cancelled)
        {
            this.ScanIPBtn.IsEnabled = true;
        }
        Browser.SetIP(this.AgentIP.Text);
        Browser.SetOID(OID_ComboBox.SelectedValue.ToString());
    }

    private void SetConfiguationSourecTheme()
    {
        switch (((FrameworkElement)this.Content).ActualTheme)
        {
            case ElementTheme.Dark: m_configuarationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: m_configuarationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
            case ElementTheme.Default: m_configuarationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
        }
    }

    private bool TrySetAcrylicBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            m_wsdqHelper = new();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

            m_configuarationSource = new();
            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;
            ((FrameworkElement)this.Content).ActualThemeChanged += MainWindow_ActualThemeChanged;

            m_configuarationSource.IsInputActive = true;
            SetConfiguationSourecTheme();

            m_acrylicController = new();
            m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_acrylicController.SetSystemBackdropConfiguration(m_configuarationSource);
            return true;
        }
        return false;
    }

    private void MainWindow_ActualThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configuarationSource != null)
            SetConfiguationSourecTheme();
    }
    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        if(m_acrylicController != null)
        {
            m_acrylicController.Dispose();
            m_acrylicController = null;
        }
        this.Activated -= MainWindow_Activated;
        m_configuarationSource = null;
    }
    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        m_configuarationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }
}

public class MIB_Browser
{
    private string IP
    {
        get; set;
    }
    private string OID
    {
        get; set;
    }
    private string Community
    {
        get; set;
    }
    private int Timeout
    {
        get; set;
    }

    private int MaxRepetitions
    {
        get; set; 
    }

    private int request_id = 0;

    public ObservableCollection<string> OID_History
    {
        get; set;
    }
    public MIB_Browser(string ip = "127.0.0.1", string oid = "1.3.6.1.2.1.1.5.0", string community = "public", int timeout = 2000, int maxRepetitions = 10)
    {
        IP = ip;
        OID = oid;
        Community = community;
        Timeout = timeout;
        OID_History = new ObservableCollection<string>();
        OID_History.Insert(0, oid);
        MaxRepetitions = maxRepetitions;
    }

    public IList<Variable> GetRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetRequestMessage request = new GetRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
        }
        catch (Exception)
        {
            throw;
        }
        if (response != null)
        {
            return response.Pdu().Variables;
        }
        return null;
    }

    public IList<Variable> GetNextRequest()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
        {
            new Variable(new ObjectIdentifier(OID))
        };
        GetNextRequestMessage request = new GetNextRequestMessage(request_id++, VersionCode.V1, new OctetString(Community), variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
        }
        catch (Exception)
        {
            throw;
        }
        if (response != null)
        {
            return response.Pdu().Variables;
        }
        return null;
    }

    public IList<Variable> GetBulk()
    {
        IPEndPoint receiver = new IPEndPoint(IPAddress.Parse(IP), 161);
        List<Variable> variables = new List<Variable>
        {
            new Variable (new ObjectIdentifier(OID))
        };
        GetBulkRequestMessage request = new GetBulkRequestMessage(request_id++,VersionCode.V2,new OctetString(Community),0,MaxRepetitions,variables);
        ISnmpMessage response;
        try
        {
            response = request.GetResponse(Timeout, receiver);
        }
        catch(Exception) { throw; }
        if (response != null)
        {
            return response.Pdu().Variables;
        }
        return null;
    }

    public static uint IPtoUINT(string addr)
    {
        if (!IPAddress.TryParse(addr, out var ip)) return 0;
        var bInt = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bInt);
        return BitConverter.ToUInt32(bInt, 0);
    }

    public static string UINTtoIP(uint addr)
    {
        var addr1 = (byte)((addr & 0xFF000000) >> 0x18);
        var addr2 = (byte)((addr & 0x00FF0000) >> 0x10);
        var addr3 = (byte)((addr & 0x0000FF00) >> 0x08);
        var addr4 = (byte)((addr & 0x000000FF));
        return string.Format("{0}.{1}.{2}.{3}", addr1, addr2, addr3, addr4);
    }

    public void SetIP(string ip) => IP = ip;
    public void SetOID(string oid) => OID = oid;
    public void SetCommunity(string community) => Community = community;
    public void SetTimeout(int timeout) => Timeout = timeout;
    public void SetMaxRepetitions(int maxRepetitions) => MaxRepetitions = maxRepetitions;
    public string GetOID() => OID;
    public override string ToString()
    {
        return IP + ":" + OID + ":" + Community + ":" + Timeout;
    }
}

public class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }
    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

    object m_dispatcherQueueController = null;
    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            return;
        }
        if (m_dispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;
            options.apartmentType = 2;
            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}