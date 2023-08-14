using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using MIB_Browser.Utilities;
using Microsoft.UI.Xaml;

namespace MIB_Browser.ViewModel;
public class MainPageViewModel : INotifyPropertyChanged
{
    public MainPageViewModel()
    {
        ObjectIDs = new();
    }

    private string _agentIP;
    private string _community;
    private List<string> _objectIDs;
    private string _selectedValue;
    private int _selectedIndex;
    private int _timeout;
    private int _maxRepetitions;
    private string _ipBegin;
    private string _ipEnd;
    private Visibility _progressbarVisibility;
    private bool _progressbarIsIndeterminate;
    private string _text;
    private double _scrollOffset;
    private int _progressbarValue; 
    
    public AsyncRelayCommand GetValueCommand { get; set; }
    public AsyncRelayCommand GetNextCommand { get; set; }
    public AsyncRelayCommand GetBulkCommand { get; set; }
    public AsyncRelayCommand GetTreeCommand { get; set; }
    public AsyncRelayCommand ScanCommand { get; set; }


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
    public List<string> ObjectIDs
    {
        get => _objectIDs;
        set
        {
            if(_objectIDs != value)
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
                if (_objectIDs.Contains(_selectedValue))
                {
                    if (_objectIDs[_selectedIndex] != value)
                    {
                        _objectIDs.Remove(_selectedValue);
                        _objectIDs.Insert(0, _selectedValue);
                        _selectedIndex = 0;

                        OnPropertyChanged(nameof(SelectedIndex));
                        OnPropertyChanged(nameof(ObjectIDs));
                        OnPropertyChanged();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _objectIDs.Insert(0, _selectedValue);
                    _selectedIndex = 0;

                    OnPropertyChanged(nameof(SelectedIndex));
                    OnPropertyChanged(nameof(ObjectIDs));
                    OnPropertyChanged();
                }
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
    public double ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            if (value != _scrollOffset)
            {
                _scrollOffset = value;
                OnPropertyChanged();
            }
        }
    }

    [XmlAttribute]
    public int ProgressbarValue
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
