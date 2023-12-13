using MIB_Browser.Pages;
using Microsoft.UI.Xaml;

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
