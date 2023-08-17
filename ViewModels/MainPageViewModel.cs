using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace MIB_Browser.ViewModel;
public class MainPageViewModel : INotifyPropertyChanged
{
    public MainPageViewModel()
    {
        AgentIP = Properties.mibbrowser.Default.AgentIP;
        Community = Properties.mibbrowser.Default.Community;
        ObjectIDs = new()
        {
            "1.3.6.1.2.1.1.1.0"
        };
        SelectedIndex = 0;
        SelectedValue = "1.3.6.1.2.1.1.1.0";
        Timeout = Properties.mibbrowser.Default.Timeout;
        MaxRepetitions = Properties.mibbrowser.Default.MaxRepetitions;
        IpBegin = "192.168.0.1";
        IpEnd = "192.168.0.255";
        ProgressbarVisibility = Visibility.Collapsed;
        ProgressbarIsIndeterminate = true;
        Text = string.Empty;
        ProgressbarValue = 0;
    }

    private string _agentIP;
    private string _community;
    private ObservableCollection<string> _objectIDs;
    private string _selectedValue;
    private int _selectedIndex;
    private int _timeout;
    private int _maxRepetitions;
    private string _ipBegin;
    private string _ipEnd;
    private Visibility _progressbarVisibility;
    private bool _progressbarIsIndeterminate;
    private string _text;
    private double _progressbarValue;

    public IAsyncRelayCommand GetValueCommand { get; set; }
    public IAsyncRelayCommand GetNextCommand { get; set; }
    public IAsyncRelayCommand GetBulkCommand { get; set; }
    public IAsyncRelayCommand GetTreeCommand { get; set; }
    public IAsyncRelayCommand ScanCommand { get; set; }
    public ICommand CancelCommand { get; set; }


    [XmlAttribute]
    public Visibility ProgressbarVisibility
    {
        get => _progressbarVisibility;
        set
        {
            if (_progressbarVisibility != value)
            {
                _progressbarVisibility = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public bool ProgressbarIsIndeterminate
    {
        get => _progressbarIsIndeterminate;
        set
        {
            if(_progressbarIsIndeterminate != value)
            {
                _progressbarIsIndeterminate = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string AgentIP
    {
        get => _agentIP;
        set
        {
            if (_agentIP != value)
            {
                _agentIP = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string Community
    {
        get => _community;
        set
        {
            if (_community != value) { 
                _community = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public ObservableCollection<string> ObjectIDs
    {
        get => _objectIDs;
        set
        {
            if (_objectIDs != value)
            {
                _objectIDs = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string SelectedValue
    {
        get => _selectedValue;
        set
        {
            if (_selectedValue != value)
            {
                _selectedValue = value;

                if(!_objectIDs.Contains(value))
                {
                    _objectIDs.Insert(0, value);
                    _selectedIndex = 0;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                if (_selectedIndex != -1)
                {
                    _selectedValue = _objectIDs[_selectedIndex];
                    OnPropertyChanged(nameof(SelectedValue));
                }
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public int Timeout
    {
        get => _timeout;
        set
        {
            if (_timeout != value)
            {

                _timeout = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public int MaxRepetitions
    {
        get => _maxRepetitions;
        set
        {
            if(_maxRepetitions != value)
            {
                _maxRepetitions = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string IpBegin
    {
        get => _ipBegin;
        set
        {
            if(_ipBegin != value)
            {
                _ipBegin = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string IpEnd
    {
        get => _ipEnd;
        set
        {
            if (_ipEnd != value)
            {
                _ipEnd = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }


    [XmlAttribute]
    public double ProgressbarValue
    {
        get => _progressbarValue;
        set
        {
            if(value != _progressbarValue)
            {
                _progressbarValue = value;
                OnPropertyChanged();
            }
        }
    }


    public void AppendText(string text)
    {
        Text += text + "\n";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")=>
        PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(propertyName));
}
