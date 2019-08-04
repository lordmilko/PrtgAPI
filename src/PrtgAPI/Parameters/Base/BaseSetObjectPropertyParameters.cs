using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Parameters.Helpers;

namespace PrtgAPI.Parameters
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal abstract class BaseSetObjectPropertyParameters<TObjectProperty> :
        BaseParameters,
        IMultiTargetParameters, IHtmlParameters, ICustomParameterContainer, IPropertyCacheResolver
    {
        HtmlFunction IHtmlParameters.Function => HtmlFunction.EditSettings;

        private string DebuggerDisplay => string.Join(", ", CustomParameters);

        public List<CustomParameter> CustomParameters
        {
            get { return (List<CustomParameter>)this[Parameter.Custom]; }
            set { this[Parameter.Custom] = value; }
        }

        protected ObjectPropertyParser parser;

        protected BaseSetObjectPropertyParameters(int[] ids)
        {
            if (ids != null && ids.Length > 0)
                ObjectIdsInternal = ids.Distinct().ToArray();

            CustomParameters = new List<CustomParameter>();
            parser = new ObjectPropertyParser(this, this, GetParameterName);
        }

        protected void AddTypeSafeValue(Enum property, object value, bool disableDependentsOnNotReqiuiredValue)
        {
            parser.ParseValueAndDependents<TObjectProperty>(property, value, disableDependentsOnNotReqiuiredValue);
        }

        public abstract PropertyCache GetPropertyCache(Enum property);

        protected virtual string GetParameterName(Enum property, PropertyCache cache)
        {
            return ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);
        }

        void ICustomParameterContainer.AddParameter(CustomParameter parameter)
        {
            CustomParameters.Add(parameter);
        }

        ICollection<CustomParameter> ICustomParameterContainer.GetParameters() => CustomParameters;

        bool ICustomParameterContainer.AllowDuplicateParameters => false;

        /// <summary>
        /// Searches this object's <see cref="CustomParameters"/> for any objects with a duplicate <see cref="CustomParameter.Name"/> and removes all but the last one.
        /// </summary>
        internal void RemoveDuplicateParameters()
        {
            var groups = CustomParameters.GroupBy(p => p.Name).Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
                var list = group.ToList();

                for (int i = 0; i < list.Count - 1; i++)
                {
                    CustomParameters.Remove(list[i]);
                }
            }
        }

        int[] IMultiTargetParameters.ObjectIds
        {
            get { return ObjectIdsInternal; }
            set { ObjectIdsInternal = value; }
        }

        protected abstract int[] ObjectIdsInternal { get; set; }
    }
}
