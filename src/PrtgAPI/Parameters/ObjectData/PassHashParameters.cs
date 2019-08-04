namespace PrtgAPI.Parameters
{
    class PassHashParameters : BaseParameters, IJsonParameters
    {
        JsonFunction IJsonParameters.Function => JsonFunction.GetPassHash;

        public PassHashParameters(string password)
        {
            this[Parameter.Password] = password;
        }
    }
}
