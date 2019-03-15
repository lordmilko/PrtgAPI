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
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName), $"Clone name cannot be null.");

            if (string.IsNullOrWhiteSpace(cloneName))
                throw new ArgumentException("CloneName cannot be empty or whitespace.", nameof(cloneName));

            CloneName = cloneName;
            TargetId = targetId;
        }

        public CloneParameters(int objectId, string cloneName, int targetId, string host) : this(objectId, cloneName, targetId)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host), $"Host cannot be null.");

            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host cannot be empty or whitespace.", nameof(host));

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
