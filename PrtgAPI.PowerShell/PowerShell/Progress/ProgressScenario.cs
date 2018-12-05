namespace PrtgAPI.PowerShell.Progress
{
    enum ProgressScenario
    {
        NoProgress,
        StreamProgress,
        MultipleCmdlets,
        VariableToSingleCmdlet,
        VariableToMultipleCmdlets,
        SelectLast,
        SelectSkipLast,
        MultipleCmdletsFromBlockingSelect
    }
}