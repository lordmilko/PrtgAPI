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
            if (name == null)
                throw new ArgumentNullException(nameof(name), $"Name cannot be null.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

            Name = name;
        }

        public string Name
        {
            get { return (string)this[Parameter.Value]; }
            set { this[Parameter.Value] = value; }
        }
    }
}
