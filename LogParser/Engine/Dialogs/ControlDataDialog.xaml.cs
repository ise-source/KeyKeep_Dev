using LogParser.Engine.Classes;
using LogParser.Engine.DataBase;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LogParser.Engine.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для ControlDataDialog.xaml
    /// </summary>
    public partial class ControlDataDialog : UserControl
    {
        public event EventHandler DialogClosed;

        private DataModel model { get; set; } = new DataModel();
        private string SelectPhoto { get; set; } = null;
        private string SelectedWebSite { get; set; } = null;
        private bool inProgress { get; set; } = false;

        public ControlDataDialog(DataModel data = null)
        {
            InitializeComponent();

            if (data != null)
            {
                model = data;
                SelectPhoto = data.Icon;
            }

            Loaded += (sender, args) => LoadDialog();

            DataContext = model;
        }

        private void LoadDialog()
        {
            Wrapper.SelectedIndex = 0;

            TB_Login.Text = model.Data.Login;
            TB_Name.Text = model.Data.Name;
            TB_Password.Text = model.Data.Password;
            TB_WebSite.Text = model.Data.WebSite;
        }

        private async void ChangeStatus()
        {
            Wrapper.SelectedIndex = 1;
        }

        private void BTN_RandomPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TB_Password.Text = Functions.CreatePassword();
        }

        private async void TB_WebSite_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = TB_WebSite.Text;

            if (url.Length <= 5 || inProgress) return;

            try
            {
                Uri uri = new Uri(url.Contains("https://") ? url : $"https://{url}");
                Uri goUri = new Uri($"https://www.google.com/s2/favicons?sz=64&domain={uri.Host}");
                string imageName = $"{uri.Host}_icon.png";

                ToggleProgress(true);

                string imagePath = await Functions.GetIconFromWeb(goUri, imageName);

                if (!string.IsNullOrEmpty(imagePath))
                {
                    IM_Icon.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                    SelectPhoto = $"Images\\{imageName}";
                    SelectedWebSite = uri.Host;

                    Notify.Send("Изображение загружено!", TypeNotify.Success);
                }
                else
                {
                    Notify.Send("Не удалось загрузить изображение.", TypeNotify.Error);
                }
            }
            catch (Exception ex)
            {
                Notify.Send($"Ошибка загрузки изображения: {ex.Message}", TypeNotify.Error);
            }
            finally
            {
                ToggleProgress(false);
            }
        }

        private void ToggleProgress(bool state)
        {
            inProgress = state;
            TB_WebSite.IsEnabled = !state;
        }

        private void OnDialogClosed()
        {
            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDialogClosed();
        }

        private async void BTN_Sumbit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(TB_Login.Text) && !string.IsNullOrEmpty(TB_Name.Text) && !string.IsNullOrEmpty(TB_Password.Text))
            {
                model.Data.Name = TB_Name.Text;
                model.Data.Login = TB_Login.Text;
                model.Data.Password = TB_Password.Text;
                model.Data.Icon = SelectPhoto;
                model.Data.WebSite = SelectedWebSite;


                if (model.Data.ID == 0)
                {
                    model.Data.DateCreate = DateTime.Now;

                    DataManager.AddData(model.Data);
                }
                else model.Data.DateUpdate = DateTime.Now;

                Notify.Send("Успешно сохранено!", TypeNotify.Success);
                await DataManager.SaveBD();

                Functions.CloseDialog();
            }
            else Notify.Send("Есть пустые поля!", TypeNotify.Error);
        }

        private async void BTN_Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (model.Data.ID != 0)
                {
                    DataManager.RemoveData(model.Data);

                    Notify.Send($"Успешно удалено!", TypeNotify.Success);
                    await DataManager.SaveBD();

                    Functions.CloseDialog();
                }
            }
            catch (Exception ex) { Notify.Send($"{ex.Message}!", TypeNotify.Error); }
        }
    }
}
