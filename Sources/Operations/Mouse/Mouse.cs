using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using NetworkOperator.InteropServices;

namespace NetworkOperator.IO
{
    public class Mouse : IDisposable
    {
        [DllImport(WindowsAPILibraries.USER32_DLL,
            CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref POINT point);

        private class MovementEventHandlerWrapper
        {
            private Mouse parent;

            private Action<MouseEventArgs> onPositionChanged;
            public event Action<MouseEventArgs> OnPositionChanged
            {
                add
                {
                    onPositionChanged += value;
                }
                remove
                {
                    onPositionChanged -= value;
                }
            }
            private Action<Movement> onMoved;
            public event Action<Movement> OnMoved
            {
                add
                {
                    onMoved += value;
                }
                remove
                {
                    onMoved -= value;
                }
            }
            private Action<ScreenEdge> onScreenEdgeHit;
            public event Action<ScreenEdge> OnScreenEdgeHit
            {
                add
                {
                    onScreenEdgeHit += value;
                }
                remove
                {
                    onScreenEdgeHit -= value;
                }
            }

            public MovementEventHandlerWrapper(Mouse parent)
            {
                this.parent = parent;
                Interlocked.Increment(ref parent.movementEventHandlerCount);
            }
            public void RaisePositionChangedEvent(MouseEventArgs eventArgs)
            {
                if (!PreprocessMovementEvent(eventArgs, false))
                {
                    return;
                }
                onPositionChanged(eventArgs);
                UpdatePosition(eventArgs);
            }
            public void RaiseMovedEvent(MouseEventArgs eventArgs)
            {
                if (parent.Blocked)
                {
                    parent.currentMovement += eventArgs.Position - parent.Position;
                }
                if (!PreprocessMovementEvent(eventArgs, false))
                {
                    return;
                }
                if (!parent.Blocked)
                {
                    parent.currentMovement = eventArgs.Position - parent.lastNoticedPosition;
                }
                onMoved(parent.currentMovement);
                parent.currentMovement = new Movement();
                UpdatePosition(eventArgs);
            }
            public void RaiseScreenEdgeHitEvent(MouseEventArgs eventArgs)
            {
                if (!PreprocessMovementEvent(eventArgs, true))
                {
                    return;
                }
                if (onScreenEdgeHit != null && !parent.Blocked && !parent.isHidden)
                {
                    bool edgeHit = false;
                    if (parent.lastNoticedPosition.X > 0 && eventArgs.Position.X <= 0)
                    {
                        onScreenEdgeHit(ScreenEdge.Left);
                        edgeHit = true;
                    }
                    else if (parent.lastNoticedPosition.X < parent.screenSize.Width - 1 
                        && eventArgs.Position.X >= parent.screenSize.Width - 1)
                    {
                        onScreenEdgeHit(ScreenEdge.Right);
                        edgeHit = true;
                    }
                    else if (parent.lastNoticedPosition.Y > 0 && eventArgs.Position.Y <= 0)
                    {
                        onScreenEdgeHit(ScreenEdge.Top);
                        edgeHit = true;
                    }
                    else if (parent.lastNoticedPosition.Y < parent.screenSize.Height - 1 
                        && eventArgs.Position.Y >= parent.screenSize.Height - 1)
                    {
                        onScreenEdgeHit(ScreenEdge.Bottom);
                        edgeHit = true;
                    }
                    if (edgeHit)
                    {
                        parent.forcePositionUpdate = true;
                        eventArgs.Cancel = false;
                    }
                }
                UpdatePosition(eventArgs);
            }
            private bool PreprocessMovementEvent(MouseEventArgs eventArgs, bool raiseAll)
            {
                Interlocked.Increment(ref parent.processedHandlers);
                if (eventArgs.EventType == MouseEventType.Movement 
                    && parent.movementEventHandlerCount != 0
                    && parent.processedHandlers % parent.movementEventHandlerCount == 0)
                {
                    eventArgs.Cancel = parent.cancelEventsByDefault;
                }
                if (eventArgs.EventType != MouseEventType.Movement
                    || (!raiseAll && eventArgs.Timestamp - parent.lastPositionChangeTime < parent.movementEventPeriod))
                {
                    if (parent.forcePositionUpdate 
                        || eventArgs.Timestamp - parent.lastPositionChangeTime >= parent.movementEventPeriod)
                    {
                        UpdatePosition(eventArgs);
                    }
                    return false;
                }
                return true;
            }
            private void UpdatePosition(MouseEventArgs eventArgs)
            {
                if (parent.movementEventHandlerCount != 0 
                    && parent.processedHandlers % parent.movementEventHandlerCount == 0)
                {
                    parent.lastNoticedPosition = eventArgs.Position;
                    parent.lastPositionChangeTime = eventArgs.Timestamp;
                    parent.processedHandlers = 0;
                    if (parent.forcePositionUpdate)
                    {
                        parent.forcePositionUpdate = false;
                    }
                }
            }
        }

        public event Action<MouseEventArgs> OnMouseEvent
        {
            add
            {
                OnPositionChanged += value;
                OnButtonStateChanged += e => value(e);
                OnWheelRotated += e => value(e);
            }
            remove
            {
                OnPositionChanged -= value;
                OnButtonStateChanged -= value;
                OnWheelRotated -= value;
            }
        }
        public event Action<MouseEventArgs> OnPositionChanged
        {
            add
            {
                MovementEventHandlerWrapper wrapper = new MovementEventHandlerWrapper(this);
                wrapper.OnPositionChanged += value;
                RegisterEventHandler(wrapper.RaisePositionChangedEvent, value);
            }
            remove => UnregisterMovementEventHandler(value);
        }
        public event Action<Movement> OnMoved
        {
            add
            {
                MovementEventHandlerWrapper wrapper = new MovementEventHandlerWrapper(this);
                wrapper.OnMoved += value;
                RegisterEventHandler(wrapper.RaiseMovedEvent, value);
            }
            remove => UnregisterMovementEventHandler(value);
        }
        public event Action<ScreenEdge> OnScreenEdgeHit
        {
            add
            {
                MovementEventHandlerWrapper wrapper = new MovementEventHandlerWrapper(this);
                wrapper.OnScreenEdgeHit += value;
                RegisterEventHandler(wrapper.RaiseScreenEdgeHitEvent, value);
            }
            remove => UnregisterMovementEventHandler(value);
        }
        private Action<MouseEventArgs> onButtonStateChanged;
        public event Action<MouseEventArgs> OnButtonStateChanged
        {
            add
            {
                onButtonStateChanged += value;
                RegisterEventHandler(eventArgs => 
                {
                    if (eventArgs.EventType == MouseEventType.ButtonStateChanged)
                    {
                        eventArgs.Cancel = cancelEventsByDefault;
                        onButtonStateChanged(eventArgs);
                    }
                }, value);
            }
            remove
            {
                onButtonStateChanged -= value;
                UnregisterEventHandler(value);
            }
        }
        private Action<MouseEventArgs> onWheelRotated;
        public event Action<MouseEventArgs> OnWheelRotated
        {
            add
            {
                onWheelRotated += value;
                RegisterEventHandler(eventArgs => 
                {
                    if (eventArgs.EventType == MouseEventType.WheelRotated)
                    {
                        eventArgs.Cancel = cancelEventsByDefault;
                        onWheelRotated(eventArgs);
                    }
                }, value);
            }
            remove
            {
                onWheelRotated -= value;
                UnregisterEventHandler(value);
            }
        }

        private Movement currentMovement = new Movement();
        private Position lastNoticedPosition;
        public Position Position
        {
            get
            {
                GCPin<POINT> pointPin = new GCPin<POINT>();
                POINT p = new POINT();
                pointPin.Pin(p);
                GetCursorPos(ref p);
                pointPin.Dispose();
                return new Position() { X = p.x, Y = p.y };
            }
            set => Simulate(new MouseEventArgs()
                {
                    EventType = MouseEventType.Movement,
                    Position = value
                });
        }
        private MouseEventHandlerRegister eventHandlerRegister = new MouseEventHandlerRegister();
        private Dictionary<Delegate, IntPtr> eventHandlerHookIds = new Dictionary<Delegate, IntPtr>();
        private Size screenSize = Screen.PrimaryScreen.Bounds.Size;
        private MouseEventSimulator simulator;
        
        private uint movementEventPeriod;
        private uint lastPositionChangeTime = 0;
        private int processedHandlers = 0;
        private int movementEventHandlerCount = 0;
        private bool forcePositionUpdate = false;
        private bool cancelEventsByDefault = false;
        public bool Blocked
        {
            get => cancelEventsByDefault;
            set => cancelEventsByDefault = value;
        }
        private bool isHidden = false;
        public bool Hidden
        {
            get => isHidden;
            set 
            {
                if (value == true && !isHidden)
                {
                    isHidden = true;
                    Blocked = false;
                    Position = new Position() { X = screenSize.Width, Y = screenSize.Height };
                    Blocked = true;
                }
                else if (value == false && isHidden)
                {
                    isHidden = false;
                    Blocked = false;
                    Position = new Position() { X = screenSize.Width / 2, Y = screenSize.Height / 2 };
                }
            }
        }
        public ScreenEdge NearestEdge
        {
            get
            {
                var currentPosition = Position;
                int dTop = currentPosition.Y;
                int dBottom = screenSize.Height - dTop;
                int dLeft = currentPosition.X;
                int dRight = screenSize.Width - dLeft;
                int min = Math.Min(Math.Min(dTop, dBottom), Math.Min(dRight, dLeft));
                if (dTop == min)
                {
                    return ScreenEdge.Top;
                }
                else if (dBottom == min)
                {
                    return ScreenEdge.Bottom;
                }
                else if (dLeft == min)
                {
                    return ScreenEdge.Left;
                }
                else
                {
                    return ScreenEdge.Right;
                }
            }
        }

        public Mouse() : this(60)
        {
        }
        public Mouse(uint movementEventfrequency)//Hz
        {
            movementEventPeriod = 1000 / movementEventfrequency;
            simulator = new MouseEventSimulator(screenSize);
            lastNoticedPosition = Position;
        }
        private void RegisterEventHandler(MouseEventHandler handler, Delegate userHandler)
            => eventHandlerHookIds.Add(userHandler, eventHandlerRegister.RegisterHandler(handler));
        private void UnregisterEventHandler(Delegate handler)
        {
            if (eventHandlerHookIds.TryGetValue(handler, out IntPtr handlerId))
            {
                eventHandlerRegister.UnregisterHandler(handlerId);
                eventHandlerHookIds.Remove(handler);
            }
        }
        private void UnregisterMovementEventHandler(Delegate handler)
        {
            UnregisterEventHandler(handler);
            Interlocked.Decrement(ref movementEventHandlerCount);
        }
        public void Simulate(Movement movement) 
            => Simulate(new MouseEventArgs()
            {
                EventType = MouseEventType.Movement,
                Position = this.Position + movement
            });
        public void Simulate(MouseEventArgs eventArgs)
            => Simulate(new MouseEventArgs[] { eventArgs });
        public void Simulate(MouseEventArgs[] multipleEventArgs) 
            => simulator.Simulate(multipleEventArgs);
        public override string ToString() => $"{nameof(Mouse)} at {Position}";
        public void Dispose()
        {
            if (isHidden)
            {
                Hidden = false;
            }
            eventHandlerRegister.Dispose();
        }
    }

    public enum MouseEventType : byte
    {
        Movement, ButtonStateChanged, WheelRotated
    }
    public enum MouseButton : byte
    {
        Left, Right, Middle, X1, X2, None
    }
    public enum MouseButtonState : byte
    {
        Pressed, Released, None
    }
    public enum MouseWheel : byte
    {
        Vertical, Horizontal, None
    }
    public enum MouseWheelRotationDirection : byte
    {
        Forward, Backward, None
    }
    public enum ScreenEdge : byte
    {
        Top, Bottom, Left, Right
    }
}
