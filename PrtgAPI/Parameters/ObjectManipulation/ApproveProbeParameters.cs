namespace PrtgAPI.Parameters
{
    class ApproveProbeParameters : BaseActionParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function => CommandFunction.ProbeState;

        public ApproveProbeParameters(Either<Probe, int> probeOrId, ProbeApproval action) : base(probeOrId.ToPrtgObject())
        {
            this[Parameter.Action] = action;
        }
    }
}
