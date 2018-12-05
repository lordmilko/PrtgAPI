using System.Management.Automation;
using PrtgAPI.Parameters;
using IDynamicParameters = PrtgAPI.Parameters.IDynamicParameters;

namespace PrtgAPI.PowerShell
{
    class PSObjectParameterContainer : IParameterContainer
    {
        //We create a PSObject that wraps ourselves and append our PSVariableProperty values to it.
        //When we write to the pipeline, PowerShell will wrap us in a PSObject and add our previous
        //members from the PSObject._instanceMembersResurrectionTable
        private PSObject psObject;
        private IDynamicParameters parameters;

        public void Initialize(IParameters parameters)
        {
            this.parameters = (IDynamicParameters)parameters;
            psObject = new PSObject(parameters);
        }

        public void Add(object value)
        {
            //Add will replace the existing property value if a member with the same name already exists
            psObject.Properties.Add(new PSVariableProperty((PSVariableEx)value));
        }

        public IParameterContainerValue CreateValue(string name, object value, bool trimName)
        {
            var var = new PSVariableEx(
                name,
                value,
                v => parameters[name] = v,
                trimName
            );

            return var;
        }
    }
}
