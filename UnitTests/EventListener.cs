using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace UnitTests
{
    internal class EventListener
    {
        #region static helpers fields
        private static readonly ConstructorInfo _dictionaryCto =
            typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes);

        private static readonly MethodInfo _dictionarySetItemMethod =
            typeof(Dictionary<string, object>).GetProperty("Item").GetSetMethod();

        private static readonly MethodInfo _listAddMethod = typeof(ObservableCollection<Dictionary<string, object>>).GetMethod("Add"); 
        #endregion

        private readonly ObservableCollection<Dictionary<string, object>> _savedArgs = new ObservableCollection<Dictionary<string, object>>();

        private List<dynamic> _dynamicSavedArgs;

        public EventListener(object raiser, string eventName)
        {
            EventInfo eventInfo = raiser.GetType().GetEvent(eventName);

            var handler = CreateCompatibleListener(eventInfo);
            eventInfo.AddEventHandler(raiser, handler);
        }

        private Delegate CreateCompatibleListener(EventInfo eventInfo)
        {
            string methodName = eventInfo.Name + "_handler_";

            var delegateType = eventInfo.EventHandlerType;

            var invokeMethod = delegateType.GetMethod("Invoke");

            if (invokeMethod.ReturnType != typeof(void))
                throw new NotSupportedException();

            var delegateParameters = invokeMethod.GetParameters();

            var args = new List<Type> {typeof (EventListener)};
            args.AddRange(delegateParameters.Select(p=>p.ParameterType));

            var dm = new DynamicMethod(methodName,
                typeof (void),
                args.ToArray(),
                typeof(EventListener));

            var generator = dm.GetILGenerator(256);
            generator.DeclareLocal(typeof (Dictionary<string, object>));
            
            generator.Emit(OpCodes.Newobj, _dictionaryCto);
            generator.Emit(OpCodes.Stloc_0);

            var savedArgsField = GetType().GetField("_savedArgs", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(savedArgsField != null);

            // add the arguments received to the dictionary
            for (var idx = 0; idx < delegateParameters.Length; idx++)
            {
                var parameter = delegateParameters[idx];

                dm.DefineParameter(idx + 2, ParameterAttributes.In, parameter.Name);

                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ldstr, parameter.Name);
                generator.Emit(OpCodes.Ldarg, idx + 1);

                if (parameter.ParameterType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, parameter.ParameterType);
                }
                generator.Emit(OpCodes.Callvirt, _dictionarySetItemMethod);
            }

            // add the dictionary to the list
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, savedArgsField);
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Callvirt, _listAddMethod);
            
            generator.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType, this);
        }

        public ObservableCollection<Dictionary<string, object>> SavedArgs
        {
            get
            {
                return _savedArgs;
            }
        }

        public IList<dynamic> DynamicSavedArgs
        {
            get
            {
                return _dynamicSavedArgs ??
                       (_dynamicSavedArgs = _savedArgs.Select(dict => (dynamic) dict.ToExpando()).ToList());
            }
        }
    }
}
