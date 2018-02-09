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
        /// <param name="objsOfTypeInGroup">A function used to retrieve the total number of objects a group and its children contain.</param>
        /// <param name="parameters">The parameters that were used to execute the initial request on the parent.</param>
        protected List<TObject> GetAdditionalGroupRecords(Group group, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            if (!Recurse)
                return new List<TObject>();

            //If we have any child groups we need to analyze
            if (group != null && group.TotalGroups > 0)
            {
                client.Log($"Processing {group.TotalGroups} child groups of parent group {group}");
                return GetAdditionalGroupRecordsInternal(group, objsOfTypeInGroup, parameters);
            }

            return new List<TObject>();
        }

        private List<TObject> GetAdditionalGroupRecordsInternal(Group parentGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            var childObjects = new List<TObject>();

            var childGroups = GetChildGroups(parentGroup, parameters, childObjects);

            foreach (var childGroup in childGroups)
            {
                //If this group has any records of the type we're interested in
                if (objsOfTypeInGroup(childGroup) > 0)
                {
                    var objectsFromChildGroup = GetObjectsFromChildGroup(childGroup, parameters);
                    childObjects.AddRange(objectsFromChildGroup);

                    MaybeRecurseGroupRecords(childGroup, objsOfTypeInGroup, parameters, objectsFromChildGroup, childObjects);
                }
            }

            return childObjects;
        }

        private List<Group> GetChildGroups(Group parentGroup, TParam parameters, List<TObject> childObjects)
        {
            List<Group> childGroups;

            if (typeof(TObject) == typeof(Group))
            {
                client.Log($"Retrieveing all child groups of group {parentGroup}");

                var filter = parameters.SearchFilter.FirstOrDefault(f => f.Property == Property.ParentId);
                var originalValue = filter?.Value;

                if (filter != null)
                    filter.Value = parentGroup.Id;

                childGroups = client.GetObjects<Group>(parameters);

                if (filter != null)
                    filter.Value = originalValue;

                client.Log($"    Found {childGroups.Count} groups: {string.Join(", ", childGroups)}");
                childObjects.AddRange(childGroups.Cast<TObject>());
            }
            else
                childGroups = client.GetGroups(Property.ParentId, parentGroup.Id);

            return childGroups;
        }

        private List<TObject> GetObjectsFromChildGroup(Group childGroup, TParam parameters)
        {
            List<TObject> objectsFromChildGroup;

            //Get the filter that refers to the ID of the parent group we were originally after
            var filter = parameters.SearchFilter.FirstOrDefault(f => f.Property == Property.ParentId);

            if (filter == null) //We're sensors filtering on the Group Name
            {
                //We don't know whether our group name is unique
                objectsFromChildGroup = GetSensorsFromGroupNameFilter(childGroup, false, parameters);
            }
            else
            {
                //Replace the reference to the parent's group ID with this child group's ID
                filter.Value = childGroup.Id;

                var objsStr = typeof(TObject).Name.ToLower();

                client.Log($"Retrieving all child {objsStr}s of group {childGroup}");

                objectsFromChildGroup = client.GetObjects<TObject>(parameters);

                client.Log($"    Found {objectsFromChildGroup.Count} {objsStr}: {string.Join(", ", objectsFromChildGroup)}");
            }

            return objectsFromChildGroup;
        }

        private void MaybeRecurseGroupRecords(Group childGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters, List<TObject> objectsFromChildGroup, List<TObject> allChildObjects)
        {
            if (typeof(TObject) == typeof(Group))
            {
                //If any of the children we retrieved have children
                foreach (var obj in objectsFromChildGroup.Cast<Group>())
                {
                    MaybeRecurseGroupRecordsInternal(obj, objsOfTypeInGroup, parameters, allChildObjects);
                }
            }
            else
                MaybeRecurseGroupRecordsInternal(childGroup, objsOfTypeInGroup, parameters, allChildObjects);
        }

        private void MaybeRecurseGroupRecordsInternal(Group childGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters, List<TObject> allChildObjects)
        {
            //If this child group has any children of its own we need to analyze
            if (childGroup.TotalGroups > 0)
            {
                client.Log($"Processing {childGroup.TotalGroups} grandchild groups of child group '{childGroup}'");
                allChildObjects.AddRange(GetAdditionalGroupRecordsInternal(childGroup, objsOfTypeInGroup, parameters));
            }
        }

        internal List<TObject> GetSensorsFromGroupNameFilter(Group group, bool knownDuplicate, TParam parameters)
        {
            if (knownDuplicate)
                return GetSensorsFromGroupViaDevices(group, parameters);

            var groups = client.GetGroups(Property.Name, @group.Name);

            if (groups.Count == 1)
            {
                client.Log($"Child group name '{group}' is unique; retrieving sensors by group name");
                return GetSensorsFromSingleGroupNameFilter(@group, parameters);
            }

            client.Log($"Child group name '{group}' is not unique; retrieving sensors by child devices");
            return GetSensorsFromGroupViaDevices(@group, parameters);
        }

        private List<TObject> GetSensorsFromSingleGroupNameFilter(Group group, TParam parameters)
        {
            //Get the group filter that was used to filter by the paren group's name
            var groupFilter = parameters.SearchFilter.First(f => f.Property == Property.Group);

            //Save the original filter value
            var originalValue = groupFilter.Value;

            groupFilter.Value = group.Name;

            var childObjects = client.GetObjects<TObject>(parameters);

            groupFilter.Value = originalValue;

            return childObjects;
        }

        private List<TObject> GetSensorsFromGroupViaDevices(Group group, TParam parameters)
        {
            //Get the group filter that was used to filter by the paren group's name
            var groupFilter = parameters.SearchFilter.First(f => f.Property == Property.Group);

            //Save the original filter settings
            var originalProperty = groupFilter.Property;
            var originalValue = groupFilter.Value;

            groupFilter.Property = Property.ParentId;

            //Get all devices that belong to this group
            var devices = client.GetDevices(Property.ParentId, group.Id);
            groupFilter.Value = devices.Select(d => d.Id).ToList();

            var childObjects = client.GetObjects<TObject>(parameters);

            //Restore the original filter settings
            groupFilter.Property = originalProperty;
            groupFilter.Value = originalValue;

            return childObjects;
        }
    }
}
