using NetworkOperator.Core.OperationDescription;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkOperator.UserInterfaces
{
    public class VisualOperationInfo : OperationInfo, INotifyPropertyChanged
    {
        private PropertyChangedEventHandler propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => propertyChanged += value;
            remove => propertyChanged -= value;
        }
        public Color PreviousColor { get; private set; }
        private Color backgroundColor = Colors.White;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                if (value != backgroundColor)
                {
                    PreviousColor = backgroundColor;
                    backgroundColor = value;
                    NotifyPropertyChanged(nameof(BackgroundColor));
                }
            }
        }
        private OperationStatus lastProcessedStatus;
        public ImageSource ControlPanelButtonIcon
        {
            get
            {
                lastProcessedStatus = Operation.Status;
                switch (Operation.Status)
                {
                    case OperationStatus.Running:
                        return ToImageSource(Properties.Resources.pauseIcon);
                    case OperationStatus.Stopped:
                        return ToImageSource(Properties.Resources.startIcon);
                    default:
                        return null;
                }
            }
        }
        public string Status
        {
            get => Operation.IsRunning ? "Running" : "Stopped";
        }
        public Brush StatusColor
        {
            get => Operation.IsRunning ? new SolidColorBrush(Session.Current.SessionColors.Success) : new SolidColorBrush(Session.Current.SessionColors.Failure);
        }
        public string NextStatus
        {
            get => Operation.IsRunning ? "Stop" : "Start";
        }

        public VisualOperationInfo(OperationInfo info) : base(info.Operation)
        {
            Name = info.Name;
            Description = info.Description;
        }
        private void NotifyPropertyChanged(string propertyName) 
            => propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        private BitmapImage ToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        public void NotifyChanges()
        {
            if (Operation.Status != lastProcessedStatus)
            {
                NotifyPropertyChanged(nameof(ControlPanelButtonIcon));
                NotifyPropertyChanged(nameof(Status));
                NotifyPropertyChanged(nameof(StatusColor));
                NotifyPropertyChanged(nameof(NextStatus));
            }
        }
    }
}
