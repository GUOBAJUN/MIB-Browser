using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.Input;
using MIB_Browser.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MIB_Browser.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly MainPageViewModel viewModel = new();

    public MainPage()
    {
        this.InitializeComponent();
        InitViewModelCommand();
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        Properties.mibbrowser.Default.AgentIP = viewModel.AgentIP;
        Properties.mibbrowser.Default.Community = viewModel.Community;
        Properties.mibbrowser.Default.MaxRepetitions = viewModel.MaxRepetitions;
        Properties.mibbrowser.Default.Timeout = viewModel.Timeout;
        Properties.mibbrowser.Default.Save();
    }

    private void InitViewModelCommand()
    {
        viewModel.ScanCommand = new AsyncRelayCommand(async (cancellationToken) =>
        {
            Debug.WriteLine("Scan Command Invoked");
            viewModel.ProgressbarVisibility = Visibility.Visible;
            viewModel.ProgressbarIsIndeterminate = false;
            viewModel.ProgressbarValue = 0;
            MibBrowser browser = new(oid: "1.3.6.1.2.1.1.5.0", community: viewModel.Community);
            var l = MibBrowser.IPtoUINT(viewModel.IpBegin);
            var r = MibBrowser.IPtoUINT(viewModel.IpEnd);
            var pingsender = new Ping();
            for (var i = l; i <= r; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                var addr = MibBrowser.UINTtoIP(i);
                var reply = await pingsender.SendPingAsync(addr, 1000);
                if (reply.Status == IPStatus.Success)
                {
                    browser.AgentIP = addr;
                    try
                    {
                        var result = await browser.GetRequestAsync();
                        viewModel.AppendText($"[{addr}] is online. Hostname: {result.Last().Data.ToString()}");
                    }
                    catch
                    {
                        viewModel.AppendText($"[{addr}] is online. But we can't get its hostname with this SNMP config");
                    }
                }
                viewModel.ProgressbarValue = (double)(i - l + 1) / (r - l + 1) * 100;
            }
            viewModel.ProgressbarVisibility = Visibility.Collapsed;
        }, () =>
        {
            return !string.IsNullOrEmpty(viewModel.Community) && !string.IsNullOrEmpty(viewModel.IpBegin) && !string.IsNullOrEmpty(viewModel.IpEnd);
        });

        viewModel.GetValueCommand = new AsyncRelayCommand(async () =>
        {
            Debug.WriteLine("GetValue Command Invoked");
            MibBrowser browser = new(viewModel.AgentIP, viewModel.SelectedValue, viewModel.Community, viewModel.Timeout, viewModel.MaxRepetitions);
            try
            {
                viewModel.ProgressbarVisibility = Visibility.Visible;
                viewModel.ProgressbarIsIndeterminate = true;
                var result = await browser.GetRequestAsync();
                viewModel.ProgressbarVisibility = Visibility.Collapsed;
                foreach (var item in result)
                {
                    viewModel.AppendText($"[{item.Id}] {item.Data}");
                }
            }
            catch
            {
                viewModel.ProgressbarVisibility = Visibility.Collapsed;
            }
        }, () =>
        {
            return !string.IsNullOrEmpty(viewModel.AgentIP) && !string.IsNullOrEmpty(viewModel.Community) && viewModel.ScanCommand.IsRunning == false;
        });

        viewModel.GetNextCommand = new AsyncRelayCommand(async () =>
        {
            Debug.WriteLine("GetNext Command Invoked");
            MibBrowser browser = new(viewModel.AgentIP, viewModel.SelectedValue, viewModel.Community, viewModel.Timeout, viewModel.MaxRepetitions);
            try
            {
                viewModel.ProgressbarVisibility = Visibility.Visible;
                viewModel.ProgressbarIsIndeterminate = true;
                var result = await browser.GetNextRequestAsync();
                viewModel.ProgressbarVisibility = Visibility.Collapsed;
                foreach (var item in result)
                {
                    viewModel.AppendText($"[{item.Id}] {item.Data}");
                }
                viewModel.SelectedValue = result.Last().Id.ToString();
            }
            catch { viewModel.ProgressbarVisibility = Visibility.Collapsed; }
        }, () =>
        {
            return !string.IsNullOrEmpty(viewModel.AgentIP) && !string.IsNullOrEmpty(viewModel.Community) && viewModel.ScanCommand.IsRunning == false;
        });

        viewModel.GetBulkCommand = new AsyncRelayCommand(async () =>
        {

            Debug.WriteLine("GetBulk Command Invoked");
            MibBrowser browser = new(viewModel.AgentIP, viewModel.SelectedValue, viewModel.Community, viewModel.Timeout, viewModel.MaxRepetitions);
            try
            {
                viewModel.ProgressbarVisibility = Visibility.Visible;
                viewModel.ProgressbarIsIndeterminate = true;
                var result = await browser.GetBulkAsync();
                viewModel.ProgressbarVisibility = Visibility.Collapsed;
                foreach (var item in result)
                {
                    viewModel.AppendText($"[{item.Id}] {item.Data}");
                }
                viewModel.SelectedValue = result.Last().Id.ToString();
            }
            catch { viewModel.ProgressbarVisibility = Visibility.Collapsed; }
        }, () =>
        {
            return !string.IsNullOrEmpty(viewModel.AgentIP) && !string.IsNullOrEmpty(viewModel.Community) && viewModel.ScanCommand.IsRunning == false;
        });

        viewModel.GetTreeCommand = new AsyncRelayCommand(async () =>
        {
            Debug.WriteLine("GetTree Command Invoked");
            MibBrowser browser = new(viewModel.AgentIP, viewModel.SelectedValue, viewModel.Community, viewModel.Timeout, viewModel.MaxRepetitions);
            try
            {
                viewModel.ProgressbarVisibility = Visibility.Visible;
                viewModel.ProgressbarIsIndeterminate = true;
                var result = await browser.GetTreeAsync();
                viewModel.ProgressbarVisibility = Visibility.Collapsed;
                foreach (var item in result)
                {
                    viewModel.AppendText($"[{item.Id}] {item.Data}");
                }
            }
            catch { viewModel.ProgressbarVisibility = Visibility.Collapsed; }
        }, () =>
        {
            return !string.IsNullOrEmpty(viewModel.AgentIP) && !string.IsNullOrEmpty(viewModel.Community) && viewModel.ScanCommand.IsRunning == false;
        });

        viewModel.CancelCommand = viewModel.ScanCommand.CreateCancelCommand();
    }

    private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        TextBlock textBlock = sender as TextBlock;
        ScrollViewer scrollViewer = textBlock.Parent as ScrollViewer;
        scrollViewer.ScrollToVerticalOffset(textBlock.ActualHeight);
    }
}
