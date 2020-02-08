using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Parameters;

namespace PrtgAPI.Tree
{
    [ExcludeFromCodeCoverage]
    internal class GroupProxy : GroupOrProbeProxy<Group>, IGroup
    {
        public string Probe => Resolved.Probe;

        public string Condition => Resolved.Condition;

        public GroupProxy(Func<Group> valueResolver) : base(valueResolver)
        {
        }

        public GroupProxy(NewGroupParameters parameters, PrtgClient client) : base(parentId => client.AddGroup(parentId, parameters))
        {
        }
    }
}
