using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core.UIMessanging;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.Core.PluginLoading;
using NetworkOperator.Core.UIMessanging.UIMessages;
using System;
using System.Collections.Generic;
using System.Collections;

namespace NetworkOperator.Core
{
    public class NetworkOperator
    {
        private class UINetworkOperatorController : IUserInterfaceController
        {
            private NetworkOperator parent;
            private bool ended = false;

            public SessionMode SessionMode
            {
                get => parent.SessionMode;
                set => parent.SessionMode = value;
            }
            public IReadOnlyRegisterAccessor<Operation> Operations { get; private set; }

            private Loader loader;

            public UINetworkOperatorController(NetworkOperator parent)
            {
                this.parent = parent;
                Operations = parent.Registers.CreateReadOnlyAccessor<Operation>();
                loader = new Loader(parent);
            }
            private void ForEach<T>(Action<T> action) where T : IRegistrable => parent.Registers.ForEach(action);
            public void Load()
            {
                UIUpdater.ChangeStatus("Loading");
                loader.OnProgressChanged += percentage => UIUpdater.ChangeProgress(percentage);
                if (!loader.LoadAdditionalDlls() || ended)
                {
                    return;
                }

                ForEach<ICommunicationInterface>(ci => parent.InitializeCommunicationInterface(ci));
                UIUpdater.ChangeStatus("Connecting");
                UIUpdater.ChangeSubstatus("This can take a while");
                UIUpdater.ChangeProgress(ProgressChangeType.ProcessIsIndeterminate);
                ForEach<ICommunicationInterface>(ci => ci.Connect());
                
                UIUpdater.ChangeProgress(ProgressChangeType.ProcessCompleted);
            }
            public void End()
            {
                ended = true;
                ForEach<Operation>(op => op.Dispose());
                ForEach<ICommunicationInterface>(ci => ci.Disconnect());
            }
        }
        private IUserInterfaceController UIController { get; set; }
        internal class RegistersManager<T> where T : IRegistrable
        {
            private class RegisterAccessorConvertor<R> : IReadOnlyRegisterAccessor<R> where R : T
            {
                private IReadOnlyRegisterAccessor<T> internalRegisterAccessor;
                public R this[Type type] => (R)internalRegisterAccessor[type];
                public R this[string name] => (R)internalRegisterAccessor[name];
                public R this[short id] => (R)internalRegisterAccessor[id];
                public int Count => internalRegisterAccessor.Count;

                public RegisterAccessorConvertor(IReadOnlyRegisterAccessor<T> internalRegisterAccessor)
                {
                    this.internalRegisterAccessor = internalRegisterAccessor;
                }
                public IEnumerator<R> GetEnumerator()
                {
                    foreach (R item in internalRegisterAccessor)
                    {
                        yield return item;
                    }
                }
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            private Dictionary<Type, IRegisterAccessor<T>> registers = new Dictionary<Type, IRegisterAccessor<T>>();
            
            public void CreateRegister<R>() where R : T => registers.Add(typeof(R), new Register<T>());
            public IReadOnlyRegisterAccessor<R> CreateReadOnlyAccessor<R>() where R : T
                => new RegisterAccessorConvertor<R>(registers[typeof(R)]);
            public IRegisterAccessor<T> AccessRegister<R>() where R : T => registers[typeof(R)];
            public void Add<R>(R item) where R : T => AccessRegister<R>().Add(item);
            public R Get<R, S>() where R : T where S : R => (R)AccessRegister<R>()[typeof(S)];
            public R Get<R>(string name) where R : T => (R)AccessRegister<R>()[name];
            public R Get<R>(short id) where R : T => (R)AccessRegister<R>()[id];
            public short GetId<R, S>() where R : T where S : R => Get<R, S>().ID;
            public short GetId<R>(Type type) where R : T => AccessRegister<R>()[type].ID;
            public void ForEach<R>(Action<R> action) where R : T
            {
                foreach (R item in AccessRegister<R>())
                {
                    action(item);
                }
            }
        }
        internal RegistersManager<IRegistrable> Registers = new RegistersManager<IRegistrable>();
        public SessionMode SessionMode { get; set; } = SessionMode.Normal;
        internal UIMessenger Messenger { get; private set; }
        public NetworkOperator()
        {
            Registers.CreateRegister<IUserInterface>();
            Registers.CreateRegister<OperandSelector>();
            Registers.CreateRegister<Operation>();
            Registers.CreateRegister<ICommunicationInterface>();

            Messenger = new UIMessenger(this);
            UIUpdater.Messenger = Messenger;
            UIController = new UINetworkOperatorController(this);

            OperandSelectorMessageProcessor.Current.NetworkOperator = this;
            OperandSelectorMessageFactory.Current.NetworkOperator = this;
        }
        public IUserInterfaceController RegisterUI(IUserInterface ui)
        {
            Registers.Add(ui);
            return UIController;
        }
        private void InitializeCommunicationInterface(ICommunicationInterface communicationInterface)
        {
            communicationInterface.OnOperandRemoved
                += operand => Registers.ForEach<OperandSelector>(operandSelector => operandSelector.UnregisterOperand(operand));
            communicationInterface.OnActionRequestReceived 
                += actionRequest => Registers.Get<Operation>(actionRequest.ParentOperation)
                    .ActionRequestProcessor.ProcessActionRequest(actionRequest);
            communicationInterface.OnOperandSelectorMessageReceived
                += message => OperandSelectorMessageProcessor.Current.ProcessUnparsedMessage(message);
            communicationInterface.OnCommunicationInterfaceDown
                += () => Registers.ForEach<OperandSelector>(operandSelector => operandSelector.UnregisterAllOperands());

            Registers.ForEach<OperandSelector>(selector => selector.OnOperandChanged += operand => 
            {
                if (!selector.IsLocalMachineSelected)
                {
                    communicationInterface.ConnectWith(operand, selector.IsReliableTransferRequired);
                    if (selector.AutoInformOthersAboutNewOperand)
                    {
                        Registers.ForEach<ICommunicationInterface>(ci =>
                            ci.Send(OperandSelectorMessageFactory.Current.Create(selector.GetType(), OperandSelectorMessageType.SetOperand, operand)));
                    }
                }
            });
        }
    }
}
