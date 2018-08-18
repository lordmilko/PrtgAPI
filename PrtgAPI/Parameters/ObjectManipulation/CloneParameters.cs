using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class CloneParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.DuplicateObject;

        public CloneParameters(int objectId, string cloneName, int targetId) : base(objectId)
        {
            if (string.IsNullOrEmpty(cloneName))
                throw new ArgumentException($"{nameof(cloneName)} cannot be null or empty", nameof(cloneName));

            CloneName = cloneName;
            TargetId = targetId;
        }

        public CloneParameters(int objectId, string cloneName, int targetId, string host) : this(objectId, cloneName, targetId)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException($"{nameof(host)} cannot be null or empty", nameof(host));

            Host = host;
        }

        public string CloneName
        {
            get { return (string)this[Parameter.Name]; }
            set { this[Parameter.Name] = value; }
        }

        /// <summary>
        /// The parent to clone the specified object ID to.
        /// </summary>
        public int TargetId
        {
            get { return (int)this[Parameter.TargetId]; }
            set { this[Parameter.TargetId] = value; }
        }

        public string Host
        {
            get { return (string)this[Parameter.Host]; }
            set { this[Parameter.Host] = value; }
        }
    }
}
