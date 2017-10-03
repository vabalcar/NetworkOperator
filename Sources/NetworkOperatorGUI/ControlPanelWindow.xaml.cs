using NetworkOperator.Core.OperationDescription;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for ControlPanelWindow.xaml
    /// </summary>
    public partial class ControlPanelWindow : Window
    {
        private bool operationsButtonSelected = false;
        private ListBoxItem selectedListViewItem;

        public ControlPanelWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var opertionsInfoList = new List<VisualOperationInfo>();
            foreach (var operation in NetowrkOperatorInterface.Current.Operations)
            {
                opertionsInfoList.Add(new VisualOperationInfo(operation.Info));
            }
            LVOperations.ItemsSource = opertionsInfoList;
            OperationAutoStartManager.Current.Autostart(NetowrkOperatorInterface.Current.Operations);
        }
        private void LVIButton_MouseEnter(object sender, MouseEventArgs e)
        {
            operationsButtonSelected = true;
        }
        private void LVIButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CancelLVIHighlight();
            selectedListViewItem = null;
            operationsButtonSelected = false;
        }
        private void LVIButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            var info = button.DataContext as VisualOperationInfo;
            info.Operation.ToNextStatus();
            info.NotifyChanges();
            e.Handled = true;
        }
        private void ListViewItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is ListViewItem listViewItem && selectedListViewItem != listViewItem && !(sender is Button))
            {
                CancelLVIHighlight();
                selectedListViewItem = listViewItem;
                SetLVIHighlight(Session.Current.SessionColors.Highlight);
            }
        }
        private void LVOperations_MouseLeave(object sender, MouseEventArgs e)
        {
            CancelLVIHighlight();
            selectedListViewItem = null;
        }
        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (operationsButtonSelected || selectedListViewItem == null)
            {
                return;
            }
            var operationInfo = selectedListViewItem.Content as VisualOperationInfo;
            Session.Current.SwitchWindows(this, new OperationOverviewWindow(operationInfo));
        }

        private void SetLVIHighlight(Color highlightColor)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            var currentlySelectedLVI = selectedListViewItem.Content as VisualOperationInfo;
            currentlySelectedLVI.BackgroundColor = highlightColor;
        }
        private void CancelLVIHighlight()
        {
            if (selectedListViewItem != null)
            {
                Mouse.OverrideCursor = null;
                var previouslySelectedLVI = selectedListViewItem.Content as VisualOperationInfo;
                previouslySelectedLVI.BackgroundColor = previouslySelectedLVI.PreviousColor;
            }
        }

        private void BHelp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.google.com");
        }
    }
}
