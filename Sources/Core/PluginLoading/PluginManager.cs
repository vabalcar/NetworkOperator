using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using NetworkOperator.Core.DataStructures;
using NetworkOperator.Core.UIMessanging;

namespace NetworkOperator.Core.PluginLoading
{
    internal class PluginManager
    {
        private Action onDllProcessed;
        public event Action OnDllProcessed
        {
            add => onDllProcessed += value;
            remove => onDllProcessed -= value;
        }
        private NetworkOperator networkOperator;
        public PluginManager(NetworkOperator networkOperator)
        {
            this.networkOperator = networkOperator;
        }
        public List<AssemblyName> GetDlls(string folder)
        {
            var directoryInfo = new DirectoryInfo(folder);
            var dlls = new List<AssemblyName>();
            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Extension == ".dll")
                {
                    dlls.Add(AssemblyName.GetAssemblyName(file.FullName));
                }
            }
            return dlls;
        }
        public void RegisterChildren<ParentType>(List<AssemblyName> dlls)
            where ParentType : IRegistrable => RegisterChildren<ParentType>(dlls, null, null);
        public void RegisterChildren<ParentType>(List<AssemblyName> dlls, object[] constructorArgs)
            where ParentType : IRegistrable => RegisterChildren<ParentType>(dlls, constructorArgs, null);
        public void RegisterChildren<ParentType>(List<AssemblyName> dlls, object[] constructorArgs, Action<ParentType> onLoadAction)
            where ParentType : IRegistrable
        {
            foreach (var dll in dlls)
            {
                UIUpdater.ChangeSubstatus($"Loading {dll.Name}");
                var loadedDll = Assembly.Load(dll);

                foreach (var type in loadedDll.ExportedTypes)
                {
                    if (typeof(ParentType).IsAssignableFrom(type))
                    {
                        ParentType loadedTypeInstace;
                        if (constructorArgs == null)
                        {
                            loadedTypeInstace = (ParentType)Activator.CreateInstance(type);
                        }
                        else
                        {
                            loadedTypeInstace = (ParentType)Activator.CreateInstance(type, constructorArgs);
                        }
                        onLoadAction?.Invoke(loadedTypeInstace);
                        networkOperator.Registers.Add(loadedTypeInstace);
                    }
                }
                onDllProcessed?.Invoke();
            }
        }
        public void LoadRegisteredInstanceToField<TargetType, InstanceRegisterType>(TargetType target, string fieldName, Type instanceType) 
            where InstanceRegisterType : IRegistrable => target.GetType().GetField(fieldName)
                .SetValue(target, networkOperator.Registers.Get<InstanceRegisterType>(instanceType.FullName));
        
    }
}
