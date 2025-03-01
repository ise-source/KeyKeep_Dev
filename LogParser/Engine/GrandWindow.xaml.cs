using LogParser.Engine.Classes;
using LogParser.Engine.Dialogs;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogParser.Engine
{
    /// <summary>
    /// Логика взаимодействия для GrandWindow.xaml
    /// </summary>
    public partial class GrandWindow : Window
    {
        private bool isDragging = false;
        private Point lastPosition;

        private int SelectPage = 1;

        private readonly SolidColorBrush BG_ColorSelect = Brushes.White;
        private readonly SolidColorBrush BG_ColorDefault = Brushes.Transparent;
        private readonly SolidColorBrush FG_ColorSelect = Brushes.White;
        private readonly SolidColorBrush FG_ColorDefault = Brushes.Gray;

        private readonly List<ButtonsClass> buttons = new List<ButtonsClass>
        {
            new ButtonsClass { Name = "BTN_Accounts", ButtonNumber = 1 },
            new ButtonsClass { Name = "BTN_Settings", ButtonNumber = 2 },
        };

        public GrandWindow()
        {
            InitializeComponent();

            DataContext = new Config();

            Functions.CheckOutDatesVersionApp();

            Handler.MainWindow = this;
            Handler.MainFrame = MainFrame;
            Handler.MainDialogControl = M_DialogControl;
            NotifyContainer.ItemsSource = Notify.Notifications;

            LoadWindow();
        }
        private void SelectButtom()
        {
            foreach (var button in buttons)
            {
                Border Button = FindName(button.Name) as Border;
                PackIcon icon = Button.Child as PackIcon;

                Button.BorderBrush = SelectPage == button.ButtonNumber ? BG_ColorSelect : BG_ColorDefault;
                icon.Foreground = SelectPage == button.ButtonNumber ? FG_ColorSelect : FG_ColorDefault;
            }
        }

        private int GetButtonNumber(Border button)
        {
            ButtonsClass btn = buttons.FirstOrDefault(u => u.Name == button.Name);
            return btn?.ButtonNumber ?? -1;
        }

        void GoPage(int i, Page page)
        {
            SelectPage = i;
            Functions.NavigatePage(page);
            SelectButtom();
        }

        async void LoadWindow()
        {
            try
            {
                MainContainer.SelectedIndex = 0;

                await Functions.LoadData();
                await Functions.LoadConfig();
                GoPage(1, Handler.P_Accounts);

                await Task.Delay(2000);
                await LoadContentAsync();

            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        async Task LoadContentAsync()
        {
            bool HasUpdates = await Functions.CheckUpdates();

            if (HasUpdates) await Functions.OpenDialog(new UpdatesDialog());

            await Task.Delay(1000);
            MainContainer.SelectedIndex = 1;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Functions.CheckBackups();
        }

        private void BTN_Accounts_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GoPage(1, Handler.P_Accounts);
        }

        private void BTN_MouseEnter(object sender, MouseEventArgs e)
        {
            Border button = sender as Border;
            PackIcon icon = button.Child as PackIcon;

            var colorAnimation = new ColorAnimation { To = Colors.White, Duration = TimeSpan.FromSeconds(0.1) };

            var newBrush = new SolidColorBrush(((SolidColorBrush)icon.Foreground).Color);
            icon.Foreground = newBrush;

            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void BTN_MouseLeave(object sender, MouseEventArgs e)
        {
            Border button = sender as Border;
            PackIcon icon = button.Child as PackIcon;

            var originalColor = SelectPage == GetButtonNumber(button) ? Colors.White : Colors.LightGray;

            var colorAnimation = new ColorAnimation { To = originalColor, Duration = TimeSpan.FromSeconds(0.1) };

            var newBrush = new SolidColorBrush(((SolidColorBrush)icon.Foreground).Color);
            icon.Foreground = newBrush;

            newBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void BTN_MoveWindow_Down_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                lastPosition = e.GetPosition(this);
                DragMove();
            }
        }

        private void BTN_MoveWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - lastPosition.X;
                double deltaY = currentPosition.Y - lastPosition.Y;

                Left += deltaX;
                Top += deltaY;

                lastPosition = currentPosition;
            }
        }

        private void BTN_MoveWindow_Up_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
            }
        }

        private void BTN_CloseWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
