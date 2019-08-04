using System;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;

namespace PrtgAPI.Utilities
{
    interface IPSObjectUtilities
    {
        object CleanPSObject(object obj);

        object[] CleanPSObject(object[] obj);

        IParameterContainer GetContainer();
    }

    class DefaultPSObjectUtilities : IPSObjectUtilities
    {
        public object[] CleanPSObject(object[] obj)
        {
            return obj;
        }

        public object CleanPSObject(object obj)
        {
            return obj;
        }

        public IParameterContainer GetContainer()
        {
            return new SimpleParameterContainer();
        }
    }

    internal static class PSObjectUtilities
    {
        private static object lockObj = new object();
        private static IPSObjectUtilities instance;

        internal static IPSObjectUtilities Instance
        {
            get
            {
                lock(lockObj)
                {
                    if(instance == null)
                    {
                        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "PrtgAPI.PowerShell.dll");

                        if (assembly != null)
                        {
                            var type = assembly.GetType("PrtgAPI.PowerShell.PSObjectUtilitiesImpl");

                            Debug.Assert(type != null);

                            instance = (IPSObjectUtilities)type.GetInternalStaticField("Instance");
                        }
                        else
                            instance = new DefaultPSObjectUtilities();
                    }
                }

                return instance;
            }
        }

        internal static object CleanPSObject(object obj)
        {
            return Instance.CleanPSObject(obj);
        }

        internal static object[] CleanPSObject(object[] obj)
        {
            return Instance.CleanPSObject(obj);
        }

        internal static IParameterContainer GetContainer()
        {
            return Instance.GetContainer();
        }
    }
}
