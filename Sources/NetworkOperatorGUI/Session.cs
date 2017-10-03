using NetworkOperator.Core.CommunicationInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NetworkOperator.UserInterfaces
{
    public class Session
    {
        public class ColorTemplate
        {
            public readonly Color Highlight;
            public readonly Color MildHighlight;
            public readonly Color ProgressBar;
            public readonly Color Success;
            public readonly Color MildSuccess;
            public readonly Color Failure;
            public readonly Color MildFailure;

            public ColorTemplate()
            {
                Highlight = Color.FromRgb(0, 255, 255);
                MildHighlight = Color.FromRgb(191, 255, 255);
                ProgressBar = Colors.Lime;
                Success = Colors.Green;
                MildSuccess = Colors.LightGreen;
                Failure = Colors.Red;
                MildFailure = Colors.PaleVioletRed;
            }
        }
        public ColorTemplate SessionColors { get; private set; }

        public string AppName { get; private set; } = "NetworkOperator";
        public bool IsRunning { get; private set; } = true;

        private static Session current;
        public static Session Current
        {
            get
            {
                if (current == null)
                {
                    current = new Session()
                    {
                        SessionColors = new ColorTemplate()
                    };
                }
                return current;
            }
        }

        public Window CurrentWindow { get; private set; }
        private Stack<Window> windowsStack = new Stack<Window>();
        private CancelEventHandler onClosingHandler;
        
        private Session()
        {
            onClosingHandler = (sender, eventArgs) => End();
        }
        public void Init(Window initialWindow)
        {
            string[] args = Environment.GetCommandLineArgs();//returns args just like in C++
            if (args.Length > 1 && args[1] == "-d")
            {
                NetowrkOperatorInterface.Current.SessionMode = SessionMode.Debug;
            }
            else
            {
                ConsoleManager.CloseConsole();
            }
            Console.WriteLine($"{nameof(Session)} started in {NetowrkOperatorInterface.Current.SessionMode} mode.");

            initialWindow.Title = AppName;
            initialWindow.Closing += onClosingHandler;

            new Thread(NetowrkOperatorInterface.Current.Load).Start();
        }
        public void SwitchWindows(Window windowToHide, Window windowToShow)
            => SwitchWindows(windowToHide, windowToShow, true);
        public void SwitchWindows(Window windowToHide, Window windowToShow, bool storeClosedWindow)
        {
            windowToShow.Left = windowToHide.Left;
            windowToShow.Top = windowToHide.Top;
            windowToShow.Title = AppName;

            windowToShow.Closing += onClosingHandler;
            windowToHide.Closing -= onClosingHandler;

            windowToShow.Show();
            CurrentWindow = windowToShow;
            if (storeClosedWindow)
            {
                windowToHide.Hide();
                windowsStack.Push(windowToHide);
            }
            else
            {
                windowToHide.Close();
            }
        }
        public void SwitchToPreviousWindow()
            => SwitchWindows(CurrentWindow, windowsStack.Pop(), false);
        public void End()
        {
            IsRunning = false;
            NetowrkOperatorInterface.Current.End();
            while(windowsStack.Count != 0)
            {
                windowsStack.Pop().Close();
            }
        }
    }
}
