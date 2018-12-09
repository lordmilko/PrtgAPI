namespace PrtgAPI.Parameters
{
    class ApproveProbeParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.ProbeState;

        public ApproveProbeParameters(int probeId, ProbeApproval action) : base(probeId)
        {
            this[Parameter.Action] = action;
        }
    }
}
