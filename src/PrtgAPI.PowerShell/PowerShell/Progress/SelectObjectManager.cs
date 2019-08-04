using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerShell.Commands;

namespace PrtgAPI.PowerShell.Progress
{
    enum Direction
    {
        Upstream,
        Downstream,
    }

    class SelectObjectDescriptor
    {
        public PSCmdlet Command { get; set; }

        public bool HasFirst => HasKey("First");
        public int First => Convert.ToInt32(GetKey("First"));

        public bool HasLast => HasKey("Last");
        public int Last => Convert.ToInt32(GetKey("Last"));

        public bool HasSkip => HasKey("Skip");
        public int Skip => Convert.ToInt32(GetKey("Skip"));

        public bool HasSkipLast => HasKey("SkipLast");
        public int SkipLast => Convert.ToInt32(GetKey("SkipLast"));

        public bool HasIndex => HasKey("Index");
        public int[] Index => ((IEnumerable)GetKey("Index")).Cast<int>().OrderBy(i => i).ToArray();

        public bool Wait => HasKey("Wait") && (SwitchParameter)GetKey("Wait") == SwitchParameter.Present; //todo: test this works

        public bool HasFilters => HasFirst || HasLast || HasSkip || HasSkipLast || HasIndex;

        public SelectObjectDescriptor(PSCmdlet command)
        {
            Command = command;
        }

        private bool HasKey(string key)
        {
            return Command.MyInvocation.BoundParameters.ContainsKey(key);
        }

        private object GetKey(string key)
        {
            return Command.MyInvocation.BoundParameters[key];
        }

        public static bool IsSelectObjectCommand(object obj)
        {
            if (obj == null)
                return false;

            return obj is PSCmdlet && TypeIsSelectObjectCommand(obj.GetType());
        }

        public static bool TypeIsSelectObjectCommand(Type type)
        {
            if (type == null)
                return false;

            return type.Name == "SelectObjectCommand";
        }
    }

    class SelectObjectManager
    {
        public List<SelectObjectDescriptor> Commands = new List<SelectObjectDescriptor>();

        public bool HasFirst => HasParam(c => c.HasFirst);
        public int First => Convert.ToInt32(GetParam(c => c.HasFirst, c => c.First));

        public bool HasLast => HasParam(c => c.HasLast);
        public int Last => Convert.ToInt32(GetParam(c => c.HasLast, c => c.Last));

        public bool HasSkip => HasParam(c => c.HasSkip);
        public int Skip => Convert.ToInt32(GetParam(c => c.HasSkip, c => c.Skip));
        public int TotalSkip => GetTotal(c => c.HasSkip, c => c.Skip);

        public bool HasSkipLast => HasParam(c => c.HasSkipLast);
        public int SkipLast => Convert.ToInt32(GetParam(c => c.HasSkipLast, c => c.SkipLast));
        public int TotalSkipLast => GetTotal(c => c.HasSkipLast, c => c.SkipLast);

        public int TotalAnySkip => TotalSkip + TotalSkipLast;

        public bool HasIndex => HasParam(c => c.HasIndex);
        public int[] Index => (int[])GetParam(c => c.HasIndex, c => c.Index);

        public bool Wait => HasParam(c => c.Wait);

        public SelectObjectManager(PSReflectionCacheManager manager, PSCmdlet cmdlet, Direction direction)
        {
            var cmds = manager.GetPipelineCommands();

            var myIndex = cmds.IndexOf(cmdlet);

            if (direction == Direction.Upstream)
            {
                for (int i = myIndex - 1; i >= 0; i--)
                {
                    if (SelectObjectDescriptor.IsSelectObjectCommand(cmds[i]))
                        Commands.Add(new SelectObjectDescriptor((PSCmdlet) cmds[i]));
                    else
                    {
                        if (i == myIndex - 1 && cmds[i] is WhereObjectCommand)
                            continue;

                        break;
                    }
                }
            }
            else
            {
                for (int i = myIndex + 1; i < cmds.Count; i++)
                {
                    if (SelectObjectDescriptor.IsSelectObjectCommand(cmds[i]))
                        Commands.Add(new SelectObjectDescriptor((PSCmdlet) cmds[i]));
                    else
                    {
                        if (i == myIndex + 1 && cmds[i] is WhereObjectCommand)
                            continue;

                        break;
                    }
                }
            }
        }

        private bool HasParam(Func<SelectObjectDescriptor, bool> predicate)
        {
            return Commands.Any(predicate);
        }

        private object GetParam(Func<SelectObjectDescriptor, bool> predicate, Func<SelectObjectDescriptor, object> selector)
        {
            return selector(Commands.First(predicate));
        }

        private int GetTotal(Func<SelectObjectDescriptor, bool> predicate, Func<SelectObjectDescriptor, object> selector)
        {
            return Commands.Where(predicate).Sum(c => Convert.ToInt32(selector(c)));
        }
    }
}
