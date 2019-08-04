using System;
using System.Collections.Generic;
using System.Linq;
using PrtgAPI.Html;
using PrtgAPI.Request;

namespace PrtgAPI.Targets
{
    /// <summary>
    /// Represents a generic sensor target that can be used for creating a new sensor.
    /// </summary>
    public class GenericSensorTarget : SensorTarget<GenericSensorTarget>
    {
        /// <summary>
        /// Gets or sets the internal value or unique identifier of the target.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the individual raw properties of this object.
        /// </summary>
        public string[] Properties
        {
            get
            {
                if (components.Last() == string.Empty)
                    return components.Take(components.Length - 1).ToArray();

                return components;
            }
        }

        private GenericSensorTarget(string raw) : base(raw)
        {
            Value = components[0];
            Name = components[1];
        }

        internal static List<GenericSensorTarget> GetTargets(string response, string tableName)
        {
            if (tableName != null)
                return GetTargetFromName(response, tableName);

            return GetTargetFromUnknown(response);
        }

        internal static Dictionary<string, List<GenericSensorTarget>> GetAllTargets(string response)
        {
            var checkboxes = HtmlParser.Default.GetInput(response).Where(i => i.Type == InputType.Checkbox).ToList();
            var dropdowns = HtmlParser.Default.GetDropDownList(response).ToList();

            var validCheckboxes = GetValidCheckboxes(checkboxes, false, false);
            var validDropdowns = GetValidDropdowns(dropdowns, false, false);

            var checkboxDict = validCheckboxes.ToDictionary(c => c, c => CreateFromCheckbox(response, c, o => new GenericSensorTarget(o)));
            var boxDict = validDropdowns.ToDictionary(c => c, d => CreateFromDropDownOptions(response, d, o => new GenericSensorTarget(o)));

            foreach (var d in boxDict)
                checkboxDict.Add(d.Key, d.Value);

            return checkboxDict;
        }

        private static List<GenericSensorTarget> GetTargetFromName(string response, string tableName)
        {
            var checkboxes = CreateFromCheckbox(response, tableName, o => new GenericSensorTarget(o));

            if (checkboxes.Count > 0)
                return checkboxes;

            var options = CreateFromDropDownOptions(response, tableName, o => new GenericSensorTarget(o));

            if (options.Count > 0)
                return options;

            //todo: we should filter out the common items, and then dont display that item type description entirely if there are none
            //if there are none of EITHER, fall back and just list all of them (including hidden)

            var allCheckboxes = HtmlParser.Default.GetInput(response).Where(i => i.Type == InputType.Checkbox).ToList();
            var allDropdowns = HtmlParser.Default.GetDropDownList(response).ToList();

            if (allCheckboxes.FirstOrDefault(c => c.Name == tableName) == null &&
                allDropdowns.FirstOrDefault(d => d.Name == tableName) == null)
            {
                throw GetInvalidNameException(allCheckboxes, allDropdowns, tableName);
            }

            return new List<GenericSensorTarget>();
        }

        private static List<GenericSensorTarget> GetTargetFromUnknown(string response)
        {
            var checkboxes = HtmlParser.Default.GetInput(response).Where(i => i.Type == InputType.Checkbox).ToList();
            var dropdowns = HtmlParser.Default.GetDropDownList(response).ToList();

            var validCheckboxes = GetValidCheckboxes(checkboxes, false, false);
            var validDropdowns = GetValidDropdowns(dropdowns, false, false);

            var all = new List<string>();
            all.AddRange(validCheckboxes);
            all.AddRange(validDropdowns);

            if (all.Count == 0)
                throw new ArgumentException("Cannot guess sensor target table. Please specify tableName.");

            if (all.Count > 2)
                throw new ArgumentException($"Cannot guess sensor target table: multiple tables found. Available tables: {string.Join(", ", all)}.");

            var tableName = all.First();

            if (validCheckboxes.Count == 1)
            {
                return CreateFromCheckbox(response, tableName, o => new GenericSensorTarget(o));
            }
            else if (validDropdowns.Count == 1)
            {
                return CreateFromDropDownOptions(response, tableName, o => new GenericSensorTarget(o));
            }
            else
            {
                throw new NotImplementedException("Don't know what element type to create sensor targets from.");
            }
        }

        private static ArgumentException GetInvalidNameException(List<Input> checkboxes, List<DropDownList> dropdowns, string tableName)
        {
            var tables = new List<string>();

            tables.AddRange(GetValidCheckboxes(checkboxes, false));
            tables.AddRange(GetValidDropdowns(dropdowns, false));

            if (tables.Count == 0)
            {
                tables.AddRange(GetValidDropdowns(dropdowns, true));
                tables.AddRange(GetValidCheckboxes(checkboxes, true));
            }

            return new ArgumentException($"Cannot find any tables named '{tableName}'. Available tables: {string.Join(", ", tables)}.");
        }

        private static List<string> GetValidDropdowns(List<DropDownList> dropdowns, bool all, bool display = true)
        {
            if (!all)
                dropdowns = dropdowns.Where(d => d.Name != "priority" && d.Name != "interval" && d.Name != "errorintervalsdown" && d.Options.All(o => o.Value.Contains("|"))).ToList();

            List<string> lists;

            if (display)
                lists = dropdowns.Select(d => $"'{d.Name}'").ToList();
            else
                lists = dropdowns.Select(d => d.Name).ToList();

            return lists;
        }

        private static List<string> GetValidCheckboxes(List<Input> checkboxes, bool all, bool display = true)
        {
            if (!all)
                checkboxes = checkboxes.Where(c => !c.Hidden && c.Name != "intervalgroup" && c.Value.Contains("|")).ToList();

            List<string> boxes;

            if (display)
                boxes = checkboxes.Select(c => $"'{c.Name}'").Distinct().ToList();
            else
                boxes = checkboxes.Select(c => c.Name).Distinct().ToList();

            return boxes;
        }
    }
}
