using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress
{
    public class ProgressGrouper
    {
        public static ProgressHierarchy GetHierarchy(List<ProgressRecord> records)
        {
            var parent = new ProgressRecord(0, "Activity", "Description");
            parent.GetType().GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(parent, -1);
            parent.ParentActivityId = -2;

            var hierarchy = new ProgressHierarchy(parent);

            GetChildren(hierarchy, records);

            return hierarchy;
        }

        static int GetChildren(ProgressHierarchy hierarchy, List<ProgressRecord> list, int i = 0)
        {
            /*
             * Given a list of ParentID/ID pairs in the form
             * -1 1
             *  1 2
             *  1 2
             *  2 3
             * 
             * We wish to create an association between each level and the children found under them, i.e.
             * -1 1
             * -1 1
             *    1 2
             *    1 2
             *      2 3
             * 
             * This information will be later used to create a unique tree for each branch of the relationship, i.e.
             * 
             * -1 1
             * 
             * -1 1
             *    1 2
             *    
             * -1 1
             *    1 2
             *      2 3
             */

            //Our parent will need to know how many records we've processed and needs to skip over
            var returnIndex = i;

            for (; i < list.Count; i++)
            {
                //We're on the same level or the next item is below us; no more children to be found for me
                if (hierarchy.Record.ParentActivityId >= list[i].ParentActivityId)
                {
                    return returnIndex;
                }

                //If this item is the child of the record this method is analyzing
                if (hierarchy.Record.ActivityId == list[i].ParentActivityId)
                {
                    //Mark the record as a child
                    var nextChild = new ProgressHierarchy(list[i]);
                    hierarchy.Children.Add(nextChild);

                    //Get the children of THAT record
                    var result = GetChildren(nextChild, list, i + 1);

                    //If we skipped over more than 1 record, we'll need to update our position
                    if (result > i + 1)
                        i = result;

                    //And tell our parent about it too
                    returnIndex = i;
                }
            }

            return returnIndex;
        }

        public static IEnumerable<IEnumerable<ProgressRecord>> GetProgressSnapshots(ProgressHierarchy hierarchy, List<ProgressRecord> soFar)
        {
            if (hierarchy.Children.Any())
            {
                foreach (var item in hierarchy.Children)
                {
                    soFar.Add(item.Record);

                    var result = GetProgressSnapshots(item, soFar);

                    foreach (var r in result)
                        yield return r;

                    soFar.Remove(item.Record);
                }
            }
            else
            {
                if (soFar.Any())
                    yield return soFar.Select(f => f);
                //yield return soFar.Select(s => soFar.Select(s2 => s2));
            }
        }
    }
}
