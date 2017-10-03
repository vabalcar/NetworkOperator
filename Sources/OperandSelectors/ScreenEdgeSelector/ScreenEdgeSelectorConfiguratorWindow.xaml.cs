using NetworkOperator.UserInterfaces;
using System.Windows;

namespace NetworkOperator.OperandSelectors
{
    /// <summary>
    /// Interaction logic for ScreenEdgeSelectorConfiguratorWindow.xaml
    /// </summary>
    public partial class ScreenEdgeSelectorConfiguratorWindow : Window
    {
        private const double LOW_OPACITY = 0.25;
        private const double FULL_OPACITY = 1;
        private bool wrongCellSelected = false;
        private bool swapInProgress = false;
        private Cell swappingCell;
        private MatrixView<string> matrixView;
        ScreenEdgeSelector screenEdgeSelector;
        public ScreenEdgeSelectorConfiguratorWindow(ScreenEdgeSelector screenEdgeSelector)
        {
            this.screenEdgeSelector = screenEdgeSelector;
            InitializeComponent();
            view.OnClick += View_OnClick;
            view.OnMouseEnter += View_OnMouseEnter;
            view.OnMouseLeave += View_OnMouseLeave;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var extendedMatrix = MatrixProcessor.ExtendForView(screenEdgeSelector.OperandMatrix, screenEdgeSelector.OperandCount, view.Rows, view.Columns);
            matrixView = new MatrixView<string>(extendedMatrix)
            {
                Y = 0,
                X = 0,
                Height = view.Rows,
                Width = view.Columns
            };
            matrixView.FontSize = 15;
            matrixView.TextAlignment = HorizontalAlignment.Center;
            matrixView.PreviewNonEmptyCellShow += MatrixView_PreviewNonEmptyCellShow;
            matrixView.PreviewSwappingCellShow += MatrixView_PreviewSwappingCellShow;
            view.AssignCells(matrixView.GetView());
            if (matrixView.EndReached(Direction.Up))
            {
                BUp.Opacity = LOW_OPACITY;
            }
            if (matrixView.EndReached(Direction.Left))
            {
                BLeft.Opacity = LOW_OPACITY;
            }
            if (matrixView.EndReached(Direction.Right))
            {
                BRight.Opacity = LOW_OPACITY;
            }
            if (matrixView.EndReached(Direction.Down))
            {
                BDown.Opacity = LOW_OPACITY;
            }
        }
        private void MatrixView_PreviewNonEmptyCellShow(Cell cell)
        {
            cell.BackgroundColor = Session.Current.SessionColors.MildHighlight;
        }
        private void MatrixView_PreviewSwappingCellShow(Cell cell)
        {
            cell.BackgroundColor = Session.Current.SessionColors.Highlight;
            swappingCell = cell;
        }
        private void BBack_Click(object sender, RoutedEventArgs e)
            => Session.Current.SwitchToPreviousWindow();
        private void BUp_Click(object sender, RoutedEventArgs e)
        {
            view.AssignCells(matrixView.Move(Direction.Up).GetView());
            if (matrixView.EndReached(Direction.Up))
            {
                BUp.Opacity = LOW_OPACITY;
            }
            if (!matrixView.EndReached(Direction.Down))
            {
                BDown.Opacity = FULL_OPACITY;
            }
        }
        private void BLeft_Click(object sender, RoutedEventArgs e)
        {
            view.AssignCells(matrixView.Move(Direction.Left).GetView());
            if (matrixView.EndReached(Direction.Left))
            {
                BLeft.Opacity = LOW_OPACITY;
            }
            if (!matrixView.EndReached(Direction.Right))
            {
                BRight.Opacity = FULL_OPACITY;
            }
        }
        private void BRight_Click(object sender, RoutedEventArgs e)
        {
            view.AssignCells(matrixView.Move(Direction.Right).GetView());
            if (matrixView.EndReached(Direction.Right))
            {
                BRight.Opacity = LOW_OPACITY;
            }
            if (!matrixView.EndReached(Direction.Left))
            {
                BLeft.Opacity = FULL_OPACITY;
            }
        }
        private void BDown_Click(object sender, RoutedEventArgs e)
        {
            view.AssignCells(matrixView.Move(Direction.Down).GetView());
            if (matrixView.EndReached(Direction.Down))
            {
                BDown.Opacity = LOW_OPACITY;
            }
            if (!matrixView.EndReached(Direction.Up))
            {
                BUp.Opacity = FULL_OPACITY;
            }
        }
        private void View_OnClick(Cell cell)
        {
            if (!swapInProgress)
            {
                if (cell.IsEmpty)
                {
                    return;
                }
                matrixView.BeginSwap(cell.Y, cell.X);
                cell.BackgroundColor = Session.Current.SessionColors.Highlight;
                swappingCell = cell;
                swapInProgress = true;
            }
            else
            {
                if (wrongCellSelected)
                {
                    cell = swappingCell;
                }
                view.AssignCells(matrixView.CompleteSwap(cell.Y, cell.X).GetView());
                swapInProgress = false;
                if (cell != swappingCell)
                {
                    BApply.IsEnabled = true;
                }
            }
        }
        private void BApply_Click(object sender, RoutedEventArgs e)
        {
            BApply.IsEnabled = false;
            screenEdgeSelector.OperandMatrix = MatrixProcessor.MinimizeMatrix(matrixView.Matrix);
        }
        private void View_OnMouseEnter(Cell cell)
        {
            if (swapInProgress && (cell.X != swappingCell.X || cell.Y != swappingCell.Y))
            {
                wrongCellSelected = cell.IsEmpty;
                if (wrongCellSelected
                    && (!matrixView.IsArticulationPoint(swappingCell, false, false) || matrixView.IsArticulationPoint(cell, true, false)))
                {
                    foreach (var neighbour in matrixView.GetNeighbours(cell, swappingCell, false))
                    {
                        if (neighbour.Length > 0)
                        {
                            wrongCellSelected = false;
                            break;
                        }
                    }
                }
                if (wrongCellSelected)
                {
                    cell.BackgroundColor = Session.Current.SessionColors.MildFailure;
                }
                else
                {
                    cell.BackgroundColor = Session.Current.SessionColors.MildSuccess;
                }
            }
        }
        private void View_OnMouseLeave(Cell cell)
        {
            if (swapInProgress && (cell.X != swappingCell.X || cell.Y != swappingCell.Y))
            {
                cell.BackgroundColor = cell.PreviousColor;
                wrongCellSelected = false;
            }
        }
    }
}
