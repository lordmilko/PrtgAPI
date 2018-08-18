using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class RenameParameters : BaseMultiActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.Rename;

        public RenameParameters(int[] objectIds, string name) : base(objectIds)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be null or empty", nameof(name));

            Name = name;
        }

        public string Name
        {
            get { return (string)this[Parameter.Value]; }
            set { this[Parameter.Value] = value; }
        }
    }
}
