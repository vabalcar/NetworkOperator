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
using System.Windows.Shapes;

using System.Threading;
using NetworkOperator.Core.UIMessanging.UIMessages;
using NetworkOperator.UserInterfaces.InformedControls;

namespace NetworkOperator.UserInterfaces
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            Application.Current.MainWindow = this;
            InitializeComponent();
        }
        private void TBStatus_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is StatusChangedMessage statusChange)
            {
                TBStatus.Text = statusChange.NewStatus;
            }
        }
        private void PBLoadingStatus_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is ProgressChangedMessage progressChange)
            {

                switch (progressChange.ChangeType)
                {
                    case ProgressChangeType.PercentageUpdate:
                        PBLoadingStatus.IsIndeterminate = false;
                        PBLoadingStatus.Value = progressChange.Percentage;
                        break;
                    case ProgressChangeType.ProcessIsIndeterminate:
                        PBLoadingStatus.IsIndeterminate = true;
                        break;
                    case ProgressChangeType.ProcessCompleted:
                        Session.Current.SwitchWindows(this, new ControlPanelWindow(), false);
                        break;
                    default:
                        break;
                }
            }
        }
        private void TBSubstatus_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message is SubstatusChangedMessage substatusChange)
            {
                TBSubstatus.Text = substatusChange.NewSubstatus;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
            => Session.Current.Init(this);
    }
}
