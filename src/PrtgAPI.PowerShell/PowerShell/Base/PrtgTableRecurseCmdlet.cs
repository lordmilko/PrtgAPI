using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Base
{
    /// <summary>
    /// Base class for all table cmdlets that require recursion to retrieve all records from a parent object.
    /// </summary>
    /// <typeparam name="TObject">The type of objects that will be retrieved.</typeparam>
    /// <typeparam name="TParam">The type of parameters to use to retrieve objects</typeparam>
    public abstract class PrtgTableRecurseCmdlet<TObject, TParam> : PrtgTableStatusCmdlet<TObject, TParam> where TParam : TableParameters<TObject> where TObject : SensorOrDeviceOrGroupOrProbe
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
        /// <param name="shouldStream">Whether this cmdlet should have streaming enabled.</param>
        public PrtgTableRecurseCmdlet(Content content, bool? shouldStream) : base(content, shouldStream)
        {
        }

        internal IEnumerable<TObject> GetAdditionalGroupRecords(NameOrObject<Group> group, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            if (group != null && group.IsObject)
                return GetAdditionalGroupRecords(group.Object, objsOfTypeInGroup, parameters);

            return new List<TObject>();
        }

        /// <summary>
        /// Traverses the child groups of a specified parent group retrieving additional objects of a specified type until all objects of that type have been found.
        /// </summary>
        /// <param name="group">The parent group to traverse.</param>
        /// <param name="objsOfTypeInGroup">A function used to retrieve the total number of objects a group and its children contain.</param>
        /// <param name="parameters">The parameters that were used to execute the initial request on the parent.</param>
        protected IEnumerable<TObject> GetAdditionalGroupRecords(Group group, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            if (!Recurse)
                return new List<TObject>();

            //If we have any child groups we need to analyze
            if (group != null && group.TotalGroups > 0)
            {
                client.Log($"Processing {group.TotalGroups} child {"group".Plural(group.TotalGroups)} of parent group {group}", LogLevel.Trace);
                return GetAdditionalGroupRecordsInternal(group, objsOfTypeInGroup, parameters);
            }

            return new List<TObject>();
        }

        private IEnumerable<TObject> GetAdditionalGroupRecordsInternal(Group parentGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            /*
             * Consider the following hierarchy
             * 
             * Servers
             * -Windows1
             *     -2012
             *         -Q1
             *            -12-dc-1
             *            -12-dc-2
             *         -Q2
             *     -2016
             *         -16-dc-1
             *         -16-dc-2
             * -Windows2
             * -linux-1
             * 
             */

            //Get groups Windows1, Windows2
            var childGroups = GetChildGroups(parentGroup);

            foreach (var childGroup in childGroups)
            {
                //Since Servers is a group, we'd like a copy of Windows1 and Windows2. If this cmdlet were asking for sensors or devices,
                //the retrieved child groups are merely a means to an end.
                if (typeof(TObject) == typeof(Group))
                    yield return (TObject) (object) childGroup;

                //Does this Windows1 contain any additional records of the desired type?
                if (objsOfTypeInGroup(childGroup) > 0)
                {
                    //Get all child objects of this group, e.g. if we're asking for groups, we just got groups 2012 and 2016
                    var objectsFromChildGroup = GetObjectsFromChildGroup(childGroup, parameters);

                    var processedChildren = ProcessChilrenAndMaybeRecurseGroupRecords(childGroup, objsOfTypeInGroup, parameters, objectsFromChildGroup);

                    foreach (var obj in processedChildren)
                        yield return obj;
                }
            }
        }

        private IEnumerable<Group> GetChildGroups(Group parentGroup)
        {
            client.Log($"Retrieving all child groups of group {parentGroup}", LogLevel.Trace);

            var childGroups = client.GetGroups(Property.ParentId, parentGroup.Id);

            client.Log($"    Found {childGroups.Count} {"group".Plural(childGroups)}: {string.Join(", ", childGroups)}", LogLevel.Trace);

            return childGroups;
        }

        private IEnumerable<TObject> GetObjectsFromChildGroup(Group childGroup, TParam parameters)
        {
            List<TObject> objectsFromChildGroup;

            //Get the filter that refers to the ID of the parent group we were originally after
            var filter = parameters.SearchFilters.FirstOrDefault(f => f.Property == Property.ParentId);

            if (filter == null) //We're sensors filtering on the Group Name
            {
                //We don't know whether our group name is unique
                objectsFromChildGroup = GetSensorsFromGroupNameFilter(childGroup, false, parameters);
            }
            else
            {
                //If we're a group: we're storing a copy of the original filters so we can restore it
                //once we completely replace the search filters
                //If we're a device: it's ok to have other filters
                //If we're a sensor: we would have filtered by group name, and gone down GetSensorsFromGroupNameFilter
                var originalParentId = filter.Value;
                var originalFilters = parameters.SearchFilters;

                //Replace the reference to the parent's group ID with this child group's ID
                filter.Value = childGroup.Id;

                var objsStr = typeof(TObject).Name.ToLower();

                client.Log($"Retrieving all child {objsStr}s of group {childGroup}", LogLevel.Trace);

                try
                {
                    if (typeof(TObject) == typeof(Group))
                        parameters.SearchFilters = new List<SearchFilter> { new SearchFilter(Property.ParentId, childGroup.Id) };

                    objectsFromChildGroup = client.ObjectEngine.GetObjects<TObject>(parameters);
                }
                finally
                {
                    filter.Value = originalParentId;
                    parameters.SearchFilters = originalFilters;
                }

                if (objectsFromChildGroup.Count > 0)
                    client.Log($"    Found {objectsFromChildGroup.Count} {objsStr.Plural(objectsFromChildGroup)}: {string.Join(", ", objectsFromChildGroup)}", LogLevel.Trace);
                else
                    client.Log($"    Found {objectsFromChildGroup.Count} {objsStr}s", LogLevel.Trace);
            }

            return objectsFromChildGroup;
        }

        private IEnumerable<TObject> ProcessChilrenAndMaybeRecurseGroupRecords(Group childGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters, IEnumerable<TObject> objectsFromChildGroup)
        {
            foreach (var childObj in objectsFromChildGroup)
            {
                //Return group 2012
                yield return childObj;

                if (typeof(TObject) == typeof(Group))
                {
                    //objectsFromChildGroup are in fact GRANDCHILD groups. Therefore, we have to recurse them.
                    //Get all groups under groups 2012 and 2016
                    var greatGrandChildren = MaybeRecurseGroupRecordsInternal((Group) (object) childObj, objsOfTypeInGroup, parameters);

                    foreach (var greatGrandChild in greatGrandChildren)
                        yield return greatGrandChild;
                }
            }

            if (typeof(TObject) != typeof(Group))
            {
                var grandChildObjects = MaybeRecurseGroupRecordsInternal(childGroup, objsOfTypeInGroup, parameters);

                foreach (var obj in grandChildObjects)
                    yield return obj;
            }
        }

        private IEnumerable<TObject> MaybeRecurseGroupRecordsInternal(Group childGroup, Func<Group, int> objsOfTypeInGroup, TParam parameters)
        {
            //If this child group has any children of its own we need to analyze
            if (childGroup.TotalGroups > 0)
            {
                client.Log($"Processing {childGroup.TotalGroups} grandchild {"group".Plural(childGroup.TotalGroups)} of child group '{childGroup}'", LogLevel.Trace);

                var objs = GetAdditionalGroupRecordsInternal(childGroup, objsOfTypeInGroup, parameters);

                foreach (var obj in objs)
                    yield return obj;
            }
        }

        internal List<TObject> GetSensorsFromGroupNameFilter(Group group, bool knownDuplicate, TParam parameters)
        {
            if (knownDuplicate)
                return GetSensorsFromGroupViaDevices(group, parameters);

            var groups = client.GetGroups(Property.Name, @group.Name);

            if (groups.Count == 1)
            {
                client.Log($"Child group name '{group}' is unique; retrieving sensors by group name", LogLevel.Trace);
                return GetSensorsFromSingleGroupNameFilter(@group, parameters);
            }

            client.Log($"Child group name '{group}' is not unique; retrieving sensors by child devices", LogLevel.Trace);
            return GetSensorsFromGroupViaDevices(@group, parameters);
        }

        private List<TObject> GetSensorsFromSingleGroupNameFilter(Group group, TParam parameters)
        {
            //Get the group filter that was used to filter by the paren group's name
            var groupFilter = parameters.SearchFilters.First(f => f.Property == Property.Group);

            //Save the original filter value
            var originalValue = groupFilter.Value;

            try
            {
                groupFilter.Value = group.Name;

                var childObjects = client.ObjectEngine.GetObjects<TObject>(parameters);
                client.Log($"Found {childObjects.Count} {"sensor".Plural(childObjects)} in group {group}", LogLevel.Trace);

                return childObjects;
            }
            finally
            {
                groupFilter.Value = originalValue;
            }
        }

        private List<TObject> GetSensorsFromGroupViaDevices(Group group, TParam parameters)
        {
            var childObjects = new List<TObject>();

            //Get the group filter that was used to filter by the parent group's name
            var groupFilter = parameters.SearchFilters.First(f => f.Property == Property.Group);

            //Save the original filter settings
            var originalProperty = groupFilter.Property;
            var originalValue = groupFilter.Value;

            try
            {
                groupFilter.Property = Property.ParentId;

                //Get all devices that belong to this group
                var devices = client.GetDevices(Property.ParentId, group.Id);
                client.Log($"Found {devices.Count} child {"device".Plural(devices)}", LogLevel.Trace);

                if (devices.Count > 0)
                {
                    groupFilter.Value = devices.Select(d => d.Id).ToList();

                    childObjects = client.ObjectEngine.GetObjects<TObject>(parameters);
                    client.Log($"Found {childObjects.Count} {"sensor".Plural(childObjects)} in all devices of group {group}", LogLevel.Trace);
                }
                else
                {
                    client.Log("Skipping retrieving sensors as sensors must be under child group", LogLevel.Trace);
                }
            }
            finally
            {
                //Restore the original filter settings
                groupFilter.Property = originalProperty;
                groupFilter.Value = originalValue;
            }

            return childObjects;
        }

        /// <summary>
        /// Specifies how the records returned from this cmdlet should be sorted. By default, sensors, devices and groups are sorted by their <see cref="ISensorOrDeviceOrGroup.Probe"/> property.
        /// </summary>
        /// <param name="records">The records to sort.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> representing the sorted collection.</returns>
        protected override IEnumerable<TObject> SortReturnedRecords(IEnumerable<TObject> records)
        {
            if (typeof(ISensorOrDeviceOrGroup).IsAssignableFrom(typeof(TObject)))
                return records.OrderBy(r => ((ISensorOrDeviceOrGroup)r).Probe);

            return records;
        }
    }
}
