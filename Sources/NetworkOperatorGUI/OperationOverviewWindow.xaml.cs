using NetworkOperator.Core.OperationDescription;
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

namespace NetworkOperator.UserInterfaces
{
    /// <summary>
    /// Interaction logic for OperationOverviewWindow.xaml
    /// </summary>
    public partial class OperationOverviewWindow : Window
    {
        private VisualOperationInfo operationInfo;
        
        public OperationOverviewWindow(VisualOperationInfo operationInfo)
        {
            InitializeComponent();
            this.operationInfo = operationInfo;
            DataContext = operationInfo;
        }

        private void BOperationStatusChange_Click(object sender, RoutedEventArgs e)
        {
            operationInfo.Operation.ToNextStatus();
            operationInfo.NotifyChanges();
        }

        private void BBack_Click(object sender, RoutedEventArgs e)
            => Session.Current.SwitchToPreviousWindow();

        private void AutostartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OperationAutoStartManager.Current.SetAutostartProperty(operationInfo.Operation, true);
        }

        private void AutostartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OperationAutoStartManager.Current.SetAutostartProperty(operationInfo.Operation, false);
        }

        private void BAdditionalOperationSettings_Click(object sender, RoutedEventArgs e)
            => Session.Current.SwitchWindows(this, LoadAdditionalOperationSettings(false));

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OperationAutoStartManager.Current.IsAutostartedOperation(operationInfo.Operation))
            {
                AutostartCheckBox.IsChecked = true;
            }
            LoadAdditionalOperationSettings(true);
        }
        private Window LoadAdditionalOperationSettings(bool initLoad)
        {
            var configuration = operationInfo.Operation.Configuration;
            if (configuration != null)
            {
                foreach (var implementation in configuration.ConfigurationImplementations)
                {
                    if (implementation is Window implementationWindow)
                    {
                        if (initLoad)
                        {
                            TBAdditionalOperationSettings.Text = configuration.Description;
                            implementationWindow.Close();
                            break;
                        }
                        else
                        {
                            return implementationWindow;
                        }
                    }
                }
            }
            else if (initLoad)
            {
                BAdditionalOperationSettings.Visibility = Visibility.Hidden;
            }
            return null;
        }
    }
}
