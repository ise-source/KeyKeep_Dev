using LogParser.Engine.Classes;
using System;
using System.Collections.Generic;
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

namespace LogParser.Engine.Dialogs
{
    /// <summary>
    /// Логика взаимодействия для UpdatesDialog.xaml
    /// </summary>
    public partial class UpdatesDialog : UserControl
    {
        public event EventHandler DialogClosed;

        private UpdatesModel model { get; set; } = new UpdatesModel();

        public UpdatesDialog()
        {
            InitializeComponent();

            Loaded += (sender, args) => LoadDialog();

            Wrapper.DataContext = model;
        }

        private void LoadDialog()
        {
            Wrapper.SelectedIndex = 0;
        }

        private void BTN_UpdatesSkip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Functions.CloseDialog();
        }

        private async void BTN_UpdateNow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Wrapper.SelectedIndex = 1;
            await Functions.UpdateApplication(model);
        }

        private void OnDialogClosed()
        {
            DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            OnDialogClosed();
        }
    }
}
