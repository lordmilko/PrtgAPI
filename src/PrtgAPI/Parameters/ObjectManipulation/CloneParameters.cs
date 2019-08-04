using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class CloneParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.DuplicateObject;

        public CloneParameters(Either<IPrtgObject, int> sourceObject, string cloneName, Either<IPrtgObject, int> targetParentObject) : base(sourceObject)
        {
            if (cloneName == null)
                throw new ArgumentNullException(nameof(cloneName), $"Clone name cannot be null.");

            if (string.IsNullOrWhiteSpace(cloneName))
                throw new ArgumentException("CloneName cannot be empty or whitespace.", nameof(cloneName));

            CloneName = cloneName;
            TargetId = targetParentObject.GetId();
        }

        public CloneParameters(Either<Device, int> deviceOrId, string cloneName, Either<GroupOrProbe, int> targetParentObject, string host) : this(deviceOrId.ToPrtgObject(), cloneName, targetParentObject.ToPrtgObject())
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
