using System;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.IO;
using NetworkOperator.OperandSelectors;

namespace NetworkOperator.Operations
{
    public class KeyboardSharing : Operation
    {
        private class KeyboardActionRequestCreator : ActionRequestCreator
        {
            private new KeyboardSharing parent;

            public KeyboardActionRequestCreator(Core.NetworkOperator networkOperator, KeyboardSharing parent)
                : base(networkOperator, parent)
            {
                this.parent = parent;
            }
            public override void StartAction()
            {
                parent.Keyboard.OnKeyStateChanged += Keyboard_OnKeyStateChanged;
                parent.ScreenEdgeSelector.OnOperandChanged += ScreenEdgeSelector_OnOperandChanged;
            }
            public override void PauseAction() => StopAction();
            public override void ResumeAction() => StartAction();
            public override void StopAction()
            {
                parent.Keyboard.OnKeyStateChanged -= Keyboard_OnKeyStateChanged;
                parent.ScreenEdgeSelector.OnOperandChanged -= ScreenEdgeSelector_OnOperandChanged;
            }
            private void Keyboard_OnKeyStateChanged(KeyboardEventArgs eventArgs)
            {
                Console.WriteLine(eventArgs);
                if (!parent.ScreenEdgeSelector.IsLocalMachineSelected)
                {
                    Send(new ActionRequest() { Content = eventArgs.Serialize() });
                }
            }
            private void ScreenEdgeSelector_OnOperandChanged(string operand)
            {
                parent.Keyboard.Blocked = !parent.ScreenEdgeSelector.IsLocalMachineSelected;
            }
        }

        private class KeyboardActionRequestProcessor : ActionRequestProcessor
        {
            private new KeyboardSharing parent;
            private bool isSimulationAllowed = false;

            public KeyboardActionRequestProcessor(Core.NetworkOperator networkOperator, KeyboardSharing parent)
                : base(networkOperator, parent)
            {
                this.parent = parent;
            }
            public override void StartAction() => isSimulationAllowed = true;
            public override void PauseAction() => StopAction();
            public override void ResumeAction() => StartAction();
            public override void StopAction() => isSimulationAllowed = false;
            public override void ProcessActionRequest(ActionRequest actionRequest)
            {
                if (!isSimulationAllowed)
                {
                    return;
                }
                parent.Keyboard.Simulate(KeyboardEventArgs.Deserialize(actionRequest.Content));
            }
        }

        public override ActionRequestCreator ActionRequestCreator { get; }
        public override ActionRequestProcessor ActionRequestProcessor { get; }
        public override Type OperandSelectorType => typeof(ScreenEdgeSelector);
        public override string OperandSelectorFieldName => nameof(ScreenEdgeSelector);
        public ScreenEdgeSelector ScreenEdgeSelector;
        public Keyboard Keyboard { get; private set; } = new Keyboard();

        private OperationInfo info;
        public override OperationInfo Info => info;
        public override Configuration Configuration => ScreenEdgeSelector.Configuration;

        public KeyboardSharing(Core.NetworkOperator networkOperator) : base(networkOperator)
        {
            ActionRequestCreator = new KeyboardActionRequestCreator(NetworkOperator, this);
            ActionRequestProcessor = new KeyboardActionRequestProcessor(NetworkOperator, this);
            info = new OperationInfo(this)
            {
                Name = "Keyboard sharing",
                Description = "Share your keyboard in easy way with other computers!"
            };
        }
        public override void Dispose()
        {
            base.Dispose();
            Keyboard.Dispose();
        }
    }
}
