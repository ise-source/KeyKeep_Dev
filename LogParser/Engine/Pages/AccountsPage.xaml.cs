using LogParser.Engine.Classes;
using LogParser.Engine.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

namespace LogParser.Engine.Pages
{
    /// <summary>
    /// Логика взаимодействия для AccountsPage.xaml
    /// </summary>
    public partial class AccountsPage : Page
    {
        private int SelectFilters = 1;

        private int MaxAccountsToPage = 5;
        private int CurrentPage = 1;

        private readonly SolidColorBrush BG_ColorSelect = Brushes.Black;
        private readonly SolidColorBrush BG_ColorDefault = Brushes.Transparent;
        private readonly SolidColorBrush FG_ColorSelect = Brushes.White;
        private readonly SolidColorBrush FG_ColorDefault = Brushes.White;

        private readonly List<ButtonsClass> buttons = new List<ButtonsClass>
        {
            new ButtonsClass { Name = "BTN_AllAccounts", ButtonNumber = 1 },
            new ButtonsClass { Name = "BTN_FavoritAccounts", ButtonNumber = 2 },
        };

        private AccountsModel model { get; set; } = new AccountsModel();

        public AccountsPage()
        {
            InitializeComponent();

            Loaded += (sender, e) => SetAllElements();
        }

        private void SetAllElements()
        {
            LB_Count.DataContext = model;
            MainPanel.DataContext = null;
            SelectFilter();
        }

        private int GetButtonNumber(Border button)
        {
            ButtonsClass btn = buttons.FirstOrDefault(u => u.Name == button.Name);
            return btn?.ButtonNumber ?? -1;
        }

        private void LoadDataAsync()
        {
            int startIndex = MaxAccountsToPage * (CurrentPage - 1);

            var data = Handler.AllData?.Select(u => new DataModel {
                Data = u, 
                Icon = Functions.directoryPath + u.Icon, 
                IsFavorite = u.IsFavorit,
                Password = u.Password,
                IsHide = true
            }).OrderByDescending(u => u.Data.Name).ToList();

            if (SelectFilters == 2) data = data.Where(u => u.Data.IsFavorit).ToList();

            string searchText = TB_Search.Text.ToLower().Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                data = data?.Where(u => u.Data.Name.Trim().ToLower().Contains(searchText) || u.Data.Login.Trim().ToLower().Contains(searchText)).ToList();
            }

            model.Count = data.Count;

            IC_View.ItemsSource = data?.Skip(startIndex).Take(MaxAccountsToPage);

            if (data != null)
            {
                UpdatePageButtons(data.Count);
            }
        }

        private void UpdatePageButtons(double count)
        {
            IC_Buttons.Items.Clear();

            int totalPages = (int)Math.Ceiling(count / MaxAccountsToPage);
            int VisibleButtonsToPage = 9;
            int halfVisibleButtons = (VisibleButtonsToPage - 1) / 2;

            int startPage = Math.Max(CurrentPage - halfVisibleButtons, 1);
            int endPage = Math.Min(startPage + VisibleButtonsToPage - 1, totalPages);

            if (totalPages < VisibleButtonsToPage)
            {
                startPage = 1;
                endPage = totalPages;
            }
            else if (CurrentPage + halfVisibleButtons > totalPages)
            {
                endPage = totalPages;
                startPage = endPage - (VisibleButtonsToPage - 1);
            }

            if (count > 0)
            {
                AddNavigationButton("<", () =>
                {
                    CurrentPage--;
                    LoadDataAsync();
                }, CurrentPage > 1);

                for (int i = startPage; i <= endPage; i++)
                {
                    AddPageButton(i.ToString(), i == CurrentPage);
                }

                AddNavigationButton(">", () =>
                {
                    CurrentPage++;
                    LoadDataAsync();
                }, CurrentPage < totalPages);
            }
        }

        private void AddNavigationButton(string content, Action onClick, bool isEnabled)
        {
            Label label = new Label
            {
                Content = content,
                Foreground = isEnabled ? Brushes.White : Brushes.Black,
            };

            Border button = new Border
            {
                IsEnabled = isEnabled,
                Margin = new Thickness(3),
                CornerRadius = new CornerRadius(5),
                Child = label
            };

            label.MouseDown += (sender, e) =>
            {
                onClick();
            };

            IC_Buttons.Items.Add(button);
        }

        void AddPageButton(string content, bool isSelected)
        {
            Label label = new Label
            {
                Content = content,
                Foreground = isSelected ? Brushes.White : Brushes.Gray,
            };

            Border button = new Border
            {
                Background = isSelected ? (SolidColorBrush)(new BrushConverter().ConvertFrom("#33808080")) : Brushes.Transparent,
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(3),
                Child = label
            };

            label.MouseDown += PageButton_Click;
            IC_Buttons.Items.Add(button);
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Label button)
            {
                CurrentPage = Convert.ToInt32(button.Content);
                LoadDataAsync();
            }
        }

        private void SelectFilter()
        {
            foreach (var button in buttons)
            {
                Border Button = FindName(button.Name) as Border;
                TextBlock text = Button.Child as TextBlock;

                Button.Background = SelectFilters == button.ButtonNumber ? BG_ColorSelect : BG_ColorDefault;
                text.Foreground = SelectFilters == button.ButtonNumber ? FG_ColorSelect : FG_ColorDefault;
            }

            LoadDataAsync();
        }

        private void IC_View_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IC_View.SelectedItem is DataModel model)
            {
                MainPanel.DataContext = model;
            }
        }

        private void BTN_SwitchFilter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border button = sender as Border;

            SelectFilters = GetButtonNumber(button);

            SelectFilter();
            LoadDataAsync();
        }

        private void TB_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadDataAsync();
        }

        private void BTN_EditFavorit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            try
            {
                if ((sender as Border).DataContext is DataModel model)
                {
                    if (model.IsFavorite) Functions.RemoveToFavorite(model);
                    else Functions.AddToFavorite(model);
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        private void BTN_HidePassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((sender as Border).DataContext is DataModel model)
                {
                    if (model.IsHide) model.IsHide = false;
                    else model.IsHide = true;
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        private void BTN_OpenWebSite_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((sender as Border).DataContext is DataModel model)
                {
                    Process.Start("https://" + model.Data.WebSite);
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        private void BTN_CopyLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((sender as Border).DataContext is DataModel model)
                {
                    Clipboard.SetText(model.Data.Login);
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        private void BTN_CopyPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((sender as Border).DataContext is DataModel model)
                {
                    Clipboard.SetText(model.Data.Password);
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }

        }

        private async void BTN_EditAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if((sender as Border).DataContext is DataModel model)
                {
                    var dialog = new ControlDataDialog(model);
                    dialog.DialogClosed += (Sender, Args) =>
                    {
                        SetAllElements();
                    };

                    await Functions.OpenDialog(dialog);
                }
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        private async void BTN_AddData_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
               var dialog = new ControlDataDialog();
                dialog.DialogClosed += (Sender, Args) =>
                {
                    SetAllElements();
                };

                await Functions.OpenDialog(dialog);
            }
            catch (Exception ex) { Notify.Send(ex.Message, TypeNotify.Error); }
        }

        /*
         * 
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = WebsiteUrlTextBox.Text;

            try
            {
                Uri uri = new Uri(url);
                Uri go_Uri =  new Uri($"https://www.google.com/s2/favicons?sz=64&domain={uri.Host}");

                string imageName = $"{uri.Host}_icon.png";

                await Functions.GetIconFromWeb(go_Uri, imageName);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Invalid URL format.");
            }
        }
        */
    }
}
