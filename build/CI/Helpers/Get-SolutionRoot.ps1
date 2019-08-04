function Get-SolutionRoot
{
    return (Resolve-Path "$PSScriptRoot\..\..\..\").Path
}