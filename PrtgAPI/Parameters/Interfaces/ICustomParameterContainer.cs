using System.Collections.Generic;

namespace PrtgAPI.Parameters
{
    interface ICustomParameterContainer
    {
        void AddParameter(CustomParameter parameter);

        ICollection<CustomParameter> GetParameters();

        bool AllowDuplicateParameters { get; }
    }
}
