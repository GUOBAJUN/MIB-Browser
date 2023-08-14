using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using MIB_Browser.Pages;
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
/// MIB BrowserÖ÷½çÃæ
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.ExtendsContentIntoTitleBar = true;
        this.Title = "MIB Browser - SNMP Get";
        this.SetTitleBar(AppTitleBar);
        MainFrame.Navigate(typeof(MainPage));
    }
}
