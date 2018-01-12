using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Parameters
{
    [ExcludeFromCodeCoverage]
    internal class PauseRequestParameters
    {
        internal PauseRequestParameters(int[] objectIds, int? durationMinutes, string pauseMessage)
        {
            if (durationMinutes == null)
            {
                Parameters = new PauseParameters(objectIds);
                Function = CommandFunction.Pause;
            }
            else
            {
                Parameters = new PauseForDurationParameters(objectIds, (int)durationMinutes);
                Function = CommandFunction.PauseObjectFor;
            }

            if (pauseMessage != null)
                Parameters.PauseMessage = pauseMessage;
        }

        internal PauseParametersBase Parameters { get; }

        internal CommandFunction Function { get; }
    }
}
