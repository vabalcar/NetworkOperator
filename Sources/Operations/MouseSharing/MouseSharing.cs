using NetworkOperator.OperandSelectors;
using NetworkOperator.Core.OperationDescription;
using System;
using NetworkOperator.IO;
using System.Diagnostics;
using NetworkOperator.Core.CommunicationInterfaces;

namespace NetworkOperator.Operations
{
    public class MouseSharing : Operation
    {
        private class MouseActionRequestCreator : ActionRequestCreator
        {
            public const int INITIAL_MOVEMENT_LENGHT = 5;
            private new MouseSharing parent;
            private bool blockMouse = false;

            public MouseActionRequestCreator(Core.NetworkOperator networkOperator, MouseSharing parent)
                : base(networkOperator, parent)
            {
                this.parent = parent;
            }
            public override void StartAction()
            {
                parent.Mouse.OnMoved += Mouse_OnMoved;
                parent.Mouse.OnButtonStateChanged += Mouse_OnButtonStateChanged;
                parent.Mouse.OnWheelRotated += Mouse_OnWheelRotated;
                parent.Mouse.OnScreenEdgeHit += Mouse_OnScreenEdgeHit;
                parent.ScreenEdgeSelector.OnOperandChanged += ScreenEdgeSelector_OnOperandChanged;
            }
            public override void PauseAction() => StopAction();
            public override void ResumeAction() => StartAction();
            public override void StopAction()
            {
                parent.Mouse.OnMoved -= Mouse_OnMoved;
                parent.Mouse.OnButtonStateChanged -= Mouse_OnButtonStateChanged;
                parent.Mouse.OnWheelRotated -= Mouse_OnWheelRotated;
                parent.Mouse.OnScreenEdgeHit -= Mouse_OnScreenEdgeHit;
                parent.ScreenEdgeSelector.OnOperandChanged -= ScreenEdgeSelector_OnOperandChanged;
            }
            private void Mouse_OnMoved(Movement movement)
            {
                parent.Write(movement);
                if (blockMouse)
                {
                    parent.Mouse.Blocked = true;
                }
                SendActionRequest(new ActionRequest() { Content = movement.Serialize() });
            }
            private void Mouse_OnWheelRotated(MouseEventArgs eventArgs) => SendMouseEvent(eventArgs);
            private void Mouse_OnButtonStateChanged(MouseEventArgs eventArgs) => SendMouseEvent(eventArgs);
            private void Mouse_OnScreenEdgeHit(ScreenEdge screenEdge)
            {
                Console.WriteLine(screenEdge);
                parent.ScreenEdgeSelector.SelectOperand(screenEdge, operand =>
                {
                    Console.WriteLine($"Operand {operand} selected");
                });
            }
            private void ScreenEdgeSelector_OnOperandChanged(string operand)
            {
                blockMouse = !parent.ScreenEdgeSelector.IsLocalMachineSelected;
                if (!blockMouse)
                {
                    parent.Mouse.Blocked = false;
                    Movement movement = new Movement();
                    switch (parent.Mouse.NearestEdge)
                    {
                        case ScreenEdge.Top:
                            movement.Y += INITIAL_MOVEMENT_LENGHT;
                            break;
                        case ScreenEdge.Bottom:
                            movement.Y -= INITIAL_MOVEMENT_LENGHT;
                            break;
                        case ScreenEdge.Left:
                            movement.X += INITIAL_MOVEMENT_LENGHT;
                            break;
                        case ScreenEdge.Right:
                            movement.X -= INITIAL_MOVEMENT_LENGHT;
                            break;
                        default:
                            return;
                    }
                    parent.Mouse.Simulate(movement);
                }
            }
            private void SendMouseEvent(MouseEventArgs eventArgs)
            {
                parent.Write(eventArgs);
                SendActionRequest(new ActionRequest() { Content = eventArgs.Serialize() });
            }
            private void SendActionRequest(ActionRequest actionRequest)
            {
                if (!parent.ScreenEdgeSelector.IsLocalMachineSelected)
                {
                    Send(actionRequest);
                }
            }
        }

        private class MouseActionRequestProcessor : ActionRequestProcessor
        {
            private new MouseSharing parent;
            private bool isSimulationAllowed = false;
            private static int lengthOfSerializedMovement = new Movement().Serialize().Length;
            private static int lengthOfSerializedMouseEventArgs = new MouseEventArgs().Serialize().Length;
            public MouseActionRequestProcessor(Core.NetworkOperator networkOperator, MouseSharing parent) 
                : base(networkOperator, parent)
            {
                this.parent = parent;
            }
            public override void StartAction() => isSimulationAllowed = true;
            public override void PauseAction() => StartAction();
            public override void ResumeAction() => StartAction();
            public override void StopAction() => isSimulationAllowed = false;
            public override void ProcessActionRequest(ActionRequest actionRequest)
            {
                if (!isSimulationAllowed)
                {
                    return;
                }
                if (actionRequest.Content.Length == lengthOfSerializedMovement)
                {
                    Movement deserializedMovement = Movement.Deserialize(actionRequest.Content);
                    parent.Mouse.Simulate(deserializedMovement);
                }
                else if (actionRequest.Content.Length == lengthOfSerializedMouseEventArgs)
                {
                    MouseEventArgs deserializedMouseEventArgs = MouseEventArgs.Deserialize(actionRequest.Content);
                    parent.Mouse.Simulate(deserializedMouseEventArgs);
                }
            }
        }

        public override ActionRequestCreator ActionRequestCreator { get; }
        public override ActionRequestProcessor ActionRequestProcessor { get; }
        public override Type OperandSelectorType => typeof(ScreenEdgeSelector);
        public override string OperandSelectorFieldName => nameof(ScreenEdgeSelector);
        public ScreenEdgeSelector ScreenEdgeSelector;
        public Mouse Mouse { get; private set; } = new Mouse();

        private OperationInfo info;
        public override OperationInfo Info => info;
        public override Configuration Configuration => ScreenEdgeSelector.Configuration;

        private Core.NetworkOperator networkOperator;
        public MouseSharing(Core.NetworkOperator networkOperator) : base(networkOperator)
        {
            this.networkOperator = networkOperator;
            ActionRequestCreator = new MouseActionRequestCreator(NetworkOperator, this);
            ActionRequestProcessor = new MouseActionRequestProcessor(NetworkOperator, this);
            info = new OperationInfo(this)
            {
                Name = "Mouse sharing",
                Description = "Share your mouse in easy way with other computers!"
            };
        }
        private void Write(MouseEventArgs mouseEventArgs)
        {
            ConsoleColor color;
            if (mouseEventArgs.Cancel)
            {
                color = ConsoleColor.DarkRed;
            }
            else if (mouseEventArgs.IsSimulated)
            {
                color = ConsoleColor.DarkBlue;
            }
            else
            {
                color = ConsoleColor.DarkGreen;
            }
            Write(color, mouseEventArgs.ToString(), true);
        }
        private void Write(Movement movement) => Write(ConsoleColor.Gray, movement.ToString(), true);
        private void Write(ConsoleColor color, string s, bool terminateLine)
        {
            switch (networkOperator.SessionMode)
            {
                case SessionMode.Normal:
                    if (terminateLine)
                    {
                        Debug.WriteLine(s);
                    }
                    else
                    {
                        Debug.Write(s);
                    }
                    break;
                case SessionMode.Debug:
                    ConsoleColor previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    if (terminateLine)
                    {
                        Console.WriteLine(s);
                    }
                    else
                    {
                        Console.Write(s);
                    }
                    Console.ForegroundColor = previousColor;
                    break;
                default:
                    break;
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            Mouse.Dispose();
        }
    }
}
