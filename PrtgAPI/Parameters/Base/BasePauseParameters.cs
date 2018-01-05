using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class PauseParametersBase : BaseMultiActionParameters
    {
        protected PauseParametersBase(int[] objectIds) : base(objectIds)
        {
        }

        public string PauseMessage
        {
            get { return (string) this[Parameter.PauseMessage]; }
            set { this[Parameter.PauseMessage] = value; }
        }
    }
}
