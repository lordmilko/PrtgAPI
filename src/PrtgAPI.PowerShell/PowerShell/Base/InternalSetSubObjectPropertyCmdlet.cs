using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Base
{
    internal class InternalSetSubObjectPropertyCmdlet<TObject, TParameter, TProperty> : IDynamicParameters
        where TObject : ISubObject
        where TParameter : PropertyParameter<TProperty>
        where TProperty : struct
    {
        internal TObject Object { get; set; }

        internal int ObjectId { get; set; }

        internal int SubId { get; set; }

        internal TProperty Property { get; set; }

        internal object Value { get; set; }

        internal object PassThruObject => Object;

        private PrtgOperationCmdlet cmdlet;
        private PrtgMultiOperationCmdlet multiCmdlet => cmdlet as PrtgMultiOperationCmdlet;

        private string objectTypeDescription;
        private string subObjectTypeDescription;
        private Func<TProperty, object, TParameter> createParameter;
        private Action<TObject, TParameter[]> setObjectProperty;
        private Action<List<TObject>, TParameter[]> setObjectPropertyList;
        private Action<int, int, TParameter[]> setObjectPropertyManual;
        private Func<Enum, Type> getPropertyType;

        internal InternalSetSubObjectPropertyCmdlet(
            PrtgOperationCmdlet cmdlet,
            string objectTypeDescription,
            string subObjectTypeDescription,
            Func<TProperty, object, TParameter> createParameter,
            Action<TObject, TParameter[]> setObjectProperty,
            Action<List<TObject>, TParameter[]> setObjectPropertyList,
            Action<int, int, TParameter[]> setObjectPropertyManual,
            Func<Enum, Type> getPropertyType)
        {
            this.cmdlet = cmdlet;
            this.objectTypeDescription = objectTypeDescription;
            this.subObjectTypeDescription = subObjectTypeDescription;
            this.createParameter = createParameter;
            this.setObjectProperty = setObjectProperty;
            this.setObjectPropertyList = setObjectPropertyList;
            this.setObjectPropertyManual = setObjectPropertyManual;
            this.getPropertyType = getPropertyType;
        }

        internal string ProgressActivity => $"Modify PRTG {subObjectTypeDescription} Settings";

        private PropertyDynamicParameterSet<TProperty> dynamicParams;

        private Lazy<List<TParameter>> dynamicParameters;
        
        internal void BeginProcessing()
        {
            Value = PSObjectUtilities.CleanPSObject(Value);

            if (DynamicSet())
            {
                dynamicParameters = new Lazy<List<TParameter>>(() =>
                {
                    var ret = dynamicParams.GetBoundParameters(cmdlet,
                            (p, v) => createParameter(p, PSObjectUtilities.CleanPSObject(v)));

                    if (ret.Count == 0)
                        throw new ParameterBindingException($"At least one dynamic property or -{nameof(Property)} and -{nameof(Value)} must be specified.");

                    return ret;
                });
            }
        }

        internal void ProcessRecord()
        {
            var str = Object != null ?
                $"'{Object.Name}' ({subObjectTypeDescription} ID: {Object.SubId}, {objectTypeDescription} ID: {Object.ObjectId})" :
                $"{subObjectTypeDescription} ID: {SubId} ({objectTypeDescription} ID: {ObjectId})";

            if (!cmdlet.MyInvocation.BoundParameters.ContainsKey(nameof(Value)) && !DynamicSet())
                throw new ParameterBindingException($"{nameof(Value)} parameter is mandatory, however a value was not specified. If {nameof(Value)} should be empty, specify $null.");

            if (Object != null)
            {
                ObjectId = Object.ObjectId;
                SubId = Object.SubId;
            }

            if (cmdlet.ShouldProcess(str, $"{cmdlet.MyInvocation.MyCommand} {GetShouldProcessMessage()}"))
            {
                var desc = Object != null ? Object.Name : $"ID {SubId}";

                //Can't batch something if there's no pipeline input
                if (Object == null && multiCmdlet != null)
                    multiCmdlet.Batch = false;

                if (multiCmdlet != null)
                    multiCmdlet.ExecuteOrQueue(Object, $"Queuing {subObjectTypeDescription.ToLower()} '{desc}'");
                else
                    PerformSingleOperation();
            }
        }

        private string GetShouldProcessMessage()
        {
            if (DynamicSet())
            {
                var strActions = dynamicParameters.Value.Select(p => $"{p.Property} = {p.Value.ToQuotedList()}");
                var str = string.Join(", ", strActions);

                return str;
            }

            return $"{Property} = {Value.ToQuotedList()}";
        }

        internal void PerformSingleOperation()
        {
            string message;

            Action action;

            if (cmdlet.ParameterSetName == ParameterSet.Default)
            {
                message = $"Setting {subObjectTypeDescription.ToLower()} '{Object.Name}' ({objectTypeDescription} ID: {Object.ObjectId}) setting '{Property}' to '{Value}'";

                action = () => setObjectProperty(Object, new[] { createParameter(Property, Value) });
            }
            else if (DynamicSet())
            {
                var strActions = dynamicParameters.Value.Select(p => $"'{p.Property}' to {p.Value.ToQuotedList()}");
                var str = string.Join(", ", strActions);

                var name = Object != null ? $"{subObjectTypeDescription.ToLower()} '{Object.Name}'" : $"ID {SubId}";

                message = $"Setting {subObjectTypeDescription.ToLower()} {name} ({objectTypeDescription} ID: {ObjectId}) setting {str}";

                switch (cmdlet.ParameterSetName)
                {
                    case ParameterSet.Dynamic:
                        action = () => setObjectProperty(Object, dynamicParameters.Value.ToArray());
                        break;
                    case ParameterSet.DynamicManual:
                        action = () => setObjectPropertyManual(ObjectId, SubId, dynamicParameters.Value.ToArray());
                        break;
                    default:
                        throw new UnknownParameterSetException(cmdlet.ParameterSetName);
                }
            }
            else
            {
                message = $"Setting {subObjectTypeDescription.ToLower()} ID {SubId} ({objectTypeDescription} ID: {ObjectId} setting {Property} to '{Value}'";

                action = () => setObjectPropertyManual(ObjectId, SubId, new[] { createParameter(Property, Value) });
            }

            cmdlet.ExecuteOperation(action, message);
        }

        internal void PerformMultiOperation(int[] ids)
        {
            var groups = multiCmdlet.objects.Cast<TObject>().GroupBy(o => o.SubId).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var complete = groups.Count == i + 1;

                var nameGroups = groups[i].GroupBy(g => g.Name).ToList();

                var summary = multiCmdlet.GetListSummary(nameGroups, g =>
                {
                    var t = $"{objectTypeDescription} ID";

                    if (g.Count() > 1)
                        t += "s";

                    var strId = g.Select(a => a.ObjectId.ToString());

                    return $"'{g.Key}' ({t}: {string.Join(", ", strId)})";
                });

                var type = subObjectTypeDescription.ToLower();

                if (nameGroups.Count() > 1)
                    type += "s";

                string message;

                if (DynamicSet())
                {
                    var strActions = dynamicParameters.Value.Select(p => $"'{p.Property}' to {p.Value.ToQuotedList()}");
                    var str = string.Join(", ", strActions);
                    message = $"Setting {type} {summary} setting {str}";
                }
                else
                {
                    message = $"Setting {type} {summary} setting '{Property}' to {Value.ToQuotedList()}";
                    dynamicParameters = new Lazy<List<TParameter>>(() => new[] { createParameter(Property, Value) }.ToList());
                }

                multiCmdlet.ExecuteMultiOperation(
                    () => setObjectPropertyList(groups[i].ToList(), dynamicParameters.Value.ToArray()),
                    message,
                    complete
                );
            }
        }

        public object GetDynamicParameters()
        {
            if (dynamicParams == null)
                dynamicParams = new PropertyDynamicParameterSet<TProperty>(
                    new[] { ParameterSet.Dynamic, ParameterSet.DynamicManual },
                    e => getPropertyType((Enum)(object)e)
                );

            return dynamicParams.Parameters;
        }

        private bool DynamicSet()
        {
            return cmdlet.ParameterSetName == ParameterSet.Dynamic || cmdlet.ParameterSetName == ParameterSet.DynamicManual;
        }
    }
}
