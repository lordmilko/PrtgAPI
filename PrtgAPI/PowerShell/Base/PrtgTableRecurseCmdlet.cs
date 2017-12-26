using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all table cmdlets that require recursion to retrieve all records from a parent object.
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableRecurseCmdlet<TObject, TParam> : PrtgTableCmdlet<TObject, TParam> where TParam : TableParameters<TObject> where TObject : SensorOrDeviceOrGroupOrProbe
    {
        /// <summary>
        /// <para type="description">When piping from a <see cref="Group"/>, specifies that PrtgAPI should also recursively traverse all subgroups until
        /// all objects that should be returned by this cmdlet have been found. By default this value is true. If this value is false,
        /// PrtgAPI will not return objects from under any subgroups of the target group.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Recurse { get; set; } = SwitchParameter.Present;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgTableRecurseCmdlet{TObject,TParam}"/> class. 
        /// </summary>
        /// <param name="content">The type of content this cmdlet will retrieve.</param>
        /// <param name="progressThreshold">The numeric threshold at which this cmdlet should show a progress bar when retrieving results.</param>
        public PrtgTableRecurseCmdlet(Content content, int? progressThreshold) : base(content, progressThreshold)
        {
        }

        /// <summary>
        /// Traverses the child groups of a specified parent group retrieving additional objects of a specified type until all objects of that type have been found.
        /// </summary>
        /// <param name="group">The parent group to traverse.</param>
        /// <param name="groupTotalObjs">A function used to retrieve the total number of objects a group and its children contain.</param>
        /// <param name="objs">The list of objects that have been retrieved so far, including those retrieved from the parent group.</param>
        /// <param name="parameters">The parameters that were used to execute the initial request on the parent.</param>
        protected void GetAdditionalGroupRecords(Group group, Func<Group, int> groupTotalObjs, List<TObject> objs, TParam parameters)
        {
            if (!Recurse)
                return;

            if (group != null && groupTotalObjs(group) > objs.Count)
            {
                GetAdditionalGroupRecordsInternal(group, groupTotalObjs, objs, parameters, groupTotalObjs(group));
            }
        }

        private void GetAdditionalGroupRecordsInternal(Group parentGroup, Func<Group, int> groupTotalObjs, List<TObject> objs, TParam parameters, int maxCount)
        {
            var childGroups = client.GetGroups(Property.ParentId, parentGroup.Id);

            foreach (var group in childGroups)
            {
                if (groupTotalObjs(group) > 0)
                {
                    var filter = parameters.SearchFilter.FirstOrDefault(f => f.Property == Property.ParentId);

                    List<TObject> childObjects = new List<TObject>();

                    if (filter == null) //We're sensors filtering on the Group Name
                    {
                        childObjects = GetObjectsFromGroupNameFilter(group, objs, parameters);
                    }
                    else
                    {
                        filter.Value = group.Id;

                        childObjects = client.GetObjects<TObject>(parameters);

                        objs.AddRange(childObjects);
                    }

                    if (groupTotalObjs(group) > childObjects.Count && objs.Count < maxCount)
                        GetAdditionalGroupRecordsInternal(group, groupTotalObjs, objs, parameters, maxCount);
                }
            }
        }

        internal List<TObject> GetObjectsFromGroupNameFilter(Group group, List<TObject> objs, TParam parameters)
        {
            var groupFilter = parameters.SearchFilter.First(f => f.Property == Property.Group);

            var defaultProperty = groupFilter.Property;
            var defaultValue = groupFilter.Value;

            groupFilter.Property = Property.ParentId;

            var devices = client.GetDevices(Property.ParentId, group.Id);

            List<TObject> childObjects = new List<TObject>();

            foreach (var device in devices)
            {
                groupFilter.Value = device.Id;

                var temp = client.GetObjects<TObject>(parameters);
                childObjects.AddRange(temp);

                objs?.AddRange(childObjects);
            }

            groupFilter.Property = defaultProperty;
            groupFilter.Value = defaultValue;

            return childObjects;
        }
    }
}
