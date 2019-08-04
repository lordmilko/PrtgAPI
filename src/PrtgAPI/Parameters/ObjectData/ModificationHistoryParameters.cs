using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    class ModificationHistoryParameters : ContentParameters<ModificationEvent>
    {
        public ModificationHistoryParameters(Either<IPrtgObject, int> objectOrId) : base(Content.History)
        {
            this[Parameter.Id] = objectOrId.GetId();
        }
    }
}
