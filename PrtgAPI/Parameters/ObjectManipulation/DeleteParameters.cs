namespace PrtgAPI.Parameters
{
    class DeleteParameters : BaseActionParameters
    {
        public DeleteParameters(int objectId) : base(objectId)
        {
            this[Parameter.Approve] = 1;
        }
    }
}
