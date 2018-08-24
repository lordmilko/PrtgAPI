using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using PrtgAPI.PowerShell.Progress;

namespace PrtgAPI.Tests.UnitTests.Support.Progress
{
    public static class ProgressGrouperLogger
    {
        private static int indentLevel = -1;

        public static void LogStart(ProgressRecord record)
        {
            indentLevel++;

            Log($"## START SEARCHING FOR ALL CHILDREN OF {record.Activity} ({record.CurrentOperation},{record.StatusDescription}) (ID: {record.ActivityId}, {record.ParentActivityId})");

            indentLevel++;
        }

        public static void LogEnd(ProgressRecord record)
        {
            indentLevel--;

            Log($"## END SEARCHING FOR ALL CHILDREN OF {record.Activity} ({record.CurrentOperation},{record.StatusDescription}) (ID: {record.ActivityId}, {record.ParentActivityId})");

            indentLevel--;
        }

        public static void Log(string message)
        {
            Debug.WriteLine($"{GetIndent()}{message}");
        }

        private static string GetIndent()
        {
            var builder = new StringBuilder();

            for (int i = 0; i < indentLevel; i++)
                builder.Append("        ");

            return builder.ToString();
        }
    }

    public static class ProgressGrouper
    {
        public static ProgressHierarchy GetHierarchy(List<ProgressRecordEx> records)
        {
            var parent = new ProgressRecord(0, "Activity", "Description");
            parent.GetType().GetField("id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(parent, -1);
            parent.ParentActivityId = -2;

            var hierarchy = new ProgressHierarchy(parent);

            GetChildren(hierarchy, records);

            return hierarchy;
        }

        static int GetChildren(ProgressHierarchy hierarchy, List<ProgressRecordEx> list, int i = 0)
        {
            /*
             * Given a list of ParentID/ID pairs in the form
             * -1 1
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
             * -1 1    (Grandfather1 [Standalone])
             * 
             * -1 1    (Grandfather2)
             * 
             * -1 1    (Grandfather2 -> Father1)
             *    1 2
             *    
             * -1 1    (Grandfather2 -> Father2 -> Child)
             *    1 2
             *      2 3
             */

            //Our parent will need to know how many records we've processed and needs to skip over
            var returnIndex = i;
            var initial = i;

            ProgressGrouperLogger.LogStart(hierarchy.Record);

            for (; i < list.Count; i++)
            {
                ProgressGrouperLogger.Log($"{i}: Begin loop for item {list[i].Activity} ({list[i].CurrentOperation},{list[i].StatusDescription}) (ID: {list[i].ActivityId}, PID: {list[i].ParentActivityId})");
                ProgressGrouperLogger.Log($"{i}: Parent is {hierarchy.Record.Activity} ({hierarchy.Record.CurrentOperation},{hierarchy.Record.StatusDescription}) (ID: {hierarchy.Record.ActivityId}, {hierarchy.Record.ParentActivityId})");
                //We're on the same level or the next item is below us; no more children to be found for me
                if (hierarchy.Record.ParentActivityId >= list[i].ParentActivityId)
                {
                    ProgressGrouperLogger.Log($"{i}: item {list[i].Activity} ({list[i].CurrentOperation},{list[i].StatusDescription}) is on the same level or is below {hierarchy.Record.Activity}. Returning {returnIndex}");
                    ProgressGrouperLogger.LogEnd(hierarchy.Record);

                    //If the current record isn't the child of the current parent, it could be the child of our grandparent (effectively, our uncle).
                    //So tell our parent we didn't process any records, it's back where it started
                    if (i == initial)
                        return initial - 1;

                    //We processed 1 or more records, and this is the index we got up to.
                    return returnIndex;
                }

                //If this item is the child of the record this method is analyzing
                if (hierarchy.Record.ActivityId == list[i].ParentActivityId)
                {
                    ProgressGrouperLogger.Log($"{i}: item {list[i].Activity} ({list[i].CurrentOperation},{list[i].StatusDescription}) is the child of {hierarchy.Record.Activity}");

                    //Mark the record as a child
                    var nextChild = new ProgressHierarchy(list[i]);
                    hierarchy.Children.Add(nextChild);

                    //Get the children of THAT record
                    var result = GetChildren(nextChild, list, i + 1);

                    //If we skipped over more than 1 record while evaluating our children, we'll need to update our position
                    if (result > i)
                        i = result;

                    //And tell our parent about it too
                    returnIndex = i;
                }
                else
                {
                    ProgressGrouperLogger.Log($"{i}: Record's activity ID {list[i].ParentActivityId} does not match current parent activity ID {hierarchy.Record.ActivityId}. Parent record has already been completed. Creating parent from record history");

                    if (hierarchy.Record.ParentActivityId != -2)
                    {
                        //We need to keep going back to the root loop. Each time we go back, the current loop will +1 us as it begins its
                        //next loop after its own call to GetChildren returns, so we just need to undo that.
                        return returnIndex - 1;
                    }

                    AddLostChildren(hierarchy, list, i);
                }
            }

            ProgressGrouperLogger.LogEnd(hierarchy.Record);

            return returnIndex;
        }

        static void AddLostChildren(ProgressHierarchy hierarchy, List<ProgressRecordEx> list, int i)
        {
            //We will build a collection of progress records we need to append to the root, then apply them

            var theChain = new List<ProgressRecord> { list[i] };
            var startIndex = i;

            var chainLength = theChain.Count;

            //Loop backwards through the list of progress records, trying to find the last parent we would've belonged to,
            //then their last parent, and so on until we reach the record with no parent
            while (theChain.Last().ParentActivityId != -1)
            {
                for (int j = startIndex; j >= 0; j--)
                {
                    if (list[j].ActivityId == theChain.Last().ParentActivityId)
                    {
                        theChain.Add(list[j]);
                        startIndex = j - 1;
                        break;
                    }
                }

                var newLength = theChain.Count;

                if (newLength != chainLength)
                    chainLength = newLength;
                else
                    throw new Exception($"Cannot find parent with activity ID {theChain.Last().ParentActivityId}");
            }

            //Now apply the records against the root record, constructing a parent > child relationship between them
            theChain.Reverse<ProgressRecord>().Aggregate(hierarchy, (h, r) =>
            {
                var newChild = new ProgressHierarchy(r);
                h.Children.Add(newChild);
                return newChild;
            });
        }

        public static IEnumerable<IEnumerable<ProgressRecord>> GetProgressSnapshots(ProgressHierarchy hierarchy, List<ProgressRecord> soFar)
        {
            if (hierarchy.Children.Any())
            {
                foreach (var item in hierarchy.Children)
                {
                    //Add a child to its parent
                    soFar.Add(item.Record);

                    //Retrieve the rest of the hierarchy for this child
                    var result = GetProgressSnapshots(item, soFar);

                    //Return the entire evaluated hierarchy. result will only contain a single IEnumerable<ProgressRecord>,
                    //which we'll transform into a List<List<ProgressRecord>> when we're done.
                    foreach (var r in result)
                        yield return r;

                    //Remove the child from its parent, so we can replace it with the next child if we loop again
                    soFar.Remove(item.Record);
                }
            }
            else
            {
                //It's the end of the line. Return any ProgressRecords we collected up to this point as an
                //IEnumerable(of size 1)<IEnumerable<ProgressRecord>>
                if (soFar.Any())
                    yield return soFar.Select(f => f);
            }
        }
    }
}
