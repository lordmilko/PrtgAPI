using PrtgAPI.Tree;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Progress;
using PrtgAPI.Tree.Progress;

namespace PrtgAPI.PowerShell.Tree
{
    internal class PowerShellTreeProgressCallback : ITreeProgressCallback
    {
        public DepthManager DepthManager { get; }
        public ProgressManager ProgressManager => ((PowerShellDepthManager) DepthManager).CurrentManager;

        public PowerShellTreeProgressCallback(PrtgCmdlet cmdlet, bool internalProgress = false)
        {
            DepthManager = new PowerShellDepthManager(cmdlet, internalProgress);
        }

        public void OnLevelBegin(ITreeValue value, PrtgNodeType type, int depth)
        {
            var typeDescription = IObjectExtensions.GetTypeDescription(value.GetType());

            var str = typeDescription.ToLower();

            if (value.Id == WellKnownId.Root)
                ProgressManager.InitialDescription = "Processing children of 'Root' Group";
            else
            {
                ProgressManager.InitialDescription = $"Processing children of {str} '{value}'";

                if (value is IPrtgObject)
                    ProgressManager.InitialDescription += $" (ID: {value.Id})";
            }

            var activity = depth == 1 ? "PRTG Tree Search" : $"PRTG {typeDescription} Tree Search";

            ProgressManager.WriteProgress(activity, $"Retrieving children of {str} '{value}'");
        }

        public void OnProcessValue(ITreeValue value)
        {
            ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord, null, true);
        }

        public void OnLevelWidthKnown(ITreeValue parent, PrtgNodeType type, int width)
        {
            ProgressManager.TotalRecords = width;
        }

        public void OnProcessType(PrtgNodeType type, int index, int total)
        {
            ProgressManager.TotalRecords = total;

            ProgressManager.InitialDescription = $"Retrieving all {type.ToString().ToLower()}s";

            ProgressManager.WriteProgress("PRTG Tree Search", ProgressManager.InitialDescription);
            ProgressManager.UpdateRecordsProcessed(ProgressManager.CurrentRecord, null, true);
        }
    }
}
