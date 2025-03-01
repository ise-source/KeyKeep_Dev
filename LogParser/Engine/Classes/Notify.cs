using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LogParser.Engine.Classes
{
    public enum TypeNotify
    {
        Success,
        Error,
        Info
    }

    public class Notify
    {
        public static ObservableCollection<Notification> Notifications { get; } = new ObservableCollection<Notification>();

        public static async void Send(string message, TypeNotify type)
        {
            if (Notifications.Any(u => u.Message == message))
                return;

            Notification noty = new Notification
            {
                Message = message,
                Type = type,
                IsActive = true
            };

            Notifications.Add(noty);

            await Task.Delay(3000);
            noty.IsActive = false;

            await Task.Delay(500);
            Notifications.Remove(noty);
        }

        public class Notification : INotifyPropertyChanged
        {
            private bool _isActive;
            public string Message { get; set; }
            public TypeNotify Type { get; set; }

            public PackIconKind Icon => Type == TypeNotify.Success ? PackIconKind.Success :
                                        Type == TypeNotify.Error ? PackIconKind.Error :
                                        Type == TypeNotify.Info ? PackIconKind.Info : PackIconKind.None;
            public bool IsActive
            {
                get { return _isActive; }
                set
                {
                    if (_isActive != value)
                    {
                        _isActive = value;
                        OnPropertyChanged(nameof(IsActive));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
