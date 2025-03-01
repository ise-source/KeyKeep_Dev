using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using MaterialDesignThemes.Wpf;
using LogParser.Engine.DataBase;
using System.Collections.ObjectModel;
using LogParser.Engine.Pages;
using System.ComponentModel;

namespace LogParser.Engine.Classes
{
    public class Handler
    {
        public static Window MainWindow { get; set; } = null;
        public static Frame MainFrame { get; set; } = null;

        public static DialogHost MainDialogControl { get; set; } = null;

        public static ObservableCollection<Data> AllData { get; set; } = new ObservableCollection<Data>();


        // Pages

        public static Page P_Accounts { get; set; } = new AccountsPage();

    }

    public class ButtonsClass
    {
        public string Name { get; set; }
        public int ButtonNumber { get; set; }
    }

    public class AccountsModel : INotifyPropertyChanged
    {
        private int count;
        public int Count
        {
            get { return count; }
            set
            {
                if (value != count)
                {
                    count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }
        public List<Data> DataList { get; set; } = new List<Data>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UpdatesModel : INotifyPropertyChanged
    {
        private string status;

        public string Status
        {
            get { return status; }
            set
            {
                if (value != status)
                {
                    status = value;

                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DataModel : INotifyPropertyChanged
    {
        private bool isFavorite, isHide;
        private string password, icon;
        public Data Data { get; set; } = new Data();

        public string Password
        {
            get { return password; }
            set
            {
                if (value != password)
                {
                    password = value;

                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string Icon
        {
            get { return icon; }
            set
            {
                if (value != icon)
                {
                    icon = value;

                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public bool IsHide
        {
            get { return isHide; }
            set
            {
                if (value != isHide)
                {
                    isHide = value;

                    OnPropertyChanged(nameof(IsHide));
                    UpdatePassword();
                }
            }
        }

        public bool IsFavorite
        {
            get { return isFavorite; }
            set
            {
                if (value != isFavorite)
                {
                    isFavorite = value;
                    Data.IsFavorit = value;

                    OnPropertyChanged(nameof(IsFavorite));
                    Functions.SaveToBD();
                }
            }
        }

        private void UpdatePassword()
        {
            if (IsHide) Password = Functions.EncryptPassword(Data.Password.Length);
            else Password = Data.Password;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
