using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ModificationHistoryParameters : ContentParameters<ModificationEvent>
    {
        public ModificationHistoryParameters(int objectId) : base(Content.History)
        {
            this[Parameter.Id] = objectId;
        }
    }
}
