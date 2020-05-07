<#
.SYNOPSIS
Compiles PrtgAPI from source

.DESCRIPTION
The Invoke-PrtgBuild cmdlet compiles PrtgAPI from source. By default, all projects in the PrtgAPI solution will uilt using the Debug configuration. A specific project can be built by specifying a wildcard expression to the -Name parameter.

In the event you wish to debug your build, the -Dbg parameter can be specified. This will generate a *.binlog file in the root of the project solution that will be automatically opened in the MSBuild Structured Log Viewer when the build has completed (assuming it is installed).

.PARAMETER Name
Wildcard specifying the name of a single PrtgAPI project to build. If no value is specified, the entire PrtgAPI solution will be built.

.PARAMETER ArgumentList
Additional arguments to pass to the build tool.

.PARAMETER Configuration
Configuration to build. If no value is specified, PrtgAPI will be built for Debug.

.PARAMETER DebugBuild
Specifies whether to generate an msbuild *.binlog file. File will automatically be opened upon completion of the build.

.PARAMETER Legacy
Specifies whether to build the .NET Core version of PrtgAPI or the legacy .NET Framework solution.

.PARAMETER SourceLink
Specifies whether to build the .NET Core version of PrtgAPI with SourceLink debug info. If this value is not specified, on Windows it will be true by default.

.PARAMETER $ViewLog
Specifies whether to open the debug log upon finishing the build when -DebugBuild is specified.

.EXAMPLE
C:\> Invoke-PrtgBuild
Build a Debug version of PrtgAPI

.EXAMPLE
C:\> Invoke-PrtgBuild -c Release
Build a Release version of PrtgAPI

.EXAMPLE
C:\> Invoke-PrtgBuild *powershell*
Build just the PrtgAPI.PowerShell project of PrtgAPI

.EXAMPLE
C:\> Invoke-PrtgBuild -Dbg
Build PrtgAPI and log to a *.binlog file to be opened by the MSBuild Structured Log Viewer upon completion

.LINK
Clear-PrtgBuild
Invoke-PrtgTest
#>
function Invoke-PrtgBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$Name,

        [Alias("Args")]
        [Parameter(Mandatory = $false, Position = 1)]
        [string[]]$ArgumentList,

        [Parameter(Mandatory = $false)]
        [Configuration]$Configuration = "Debug",

        [Alias("Dbg")]
        [Alias("DebugMode")]
        [Parameter(Mandatory = $false)]
        [switch]$DebugBuild,

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy,

        [Parameter(Mandatory = $false)]
        [switch]$SourceLink,

        [Parameter(Mandatory = $false)]
        [switch]$ViewLog = $true
    )

    # On Linux you need to have libcurl and some other stuff for libgit2 to work properly;
    # users don't care about that, and don't need to include SourceLink anyway so just skip it
    if((Test-IsWindows) -and ($SourceLink -or !$PSBoundParameters.ContainsKey("SourceLink")))
    {
        $SourceLink = $true
    }

    $splattedArgs = @{
        Configuration = $Configuration
        IsCore = -not $Legacy
        SourceLink = $SourceLink
    }

    if($Name)
    {
        $candidates = Get-BuildProject (-not $Legacy)

        $projects = $candidates | where { $_.BaseName -like $Name }

        if(!$projects)
        {
            throw "Cannot find any projects that match the wildcard '$Name'. Please specify one of $(($candidates|select -expand BaseName) -join ", ")"
        }

        if($projects.Count -gt 1)
        {
            $str = ($projects|select -ExpandProperty BaseName|Sort-Object) -join ", "
            throw "Can only specify one project at a time, however wildcard '$Name' matched multiple projects: $str"
        }

        $splattedArgs.Target = $projects.FullName
    }

    $root = Get-SolutionRoot

    $additionalArgs = @()

    if($ArgumentList -ne $null)
    {
        $additionalArgs += $ArgumentList
    }

    if($DebugBuild)
    {
        $binLog = Join-Path $root "msbuild.binlog"

        $additionalArgs += "/bl:$binLog"
    }

    $splattedArgs.BuildFolder = $root
    $splattedArgs.AdditionalArgs = $additionalArgs

    if($Legacy)
    {
        Invoke-CIRestoreFull $root -Verbose
    }

    try
    {
        Invoke-CIBuild @splattedArgs -Verbose
    }
    finally
    {
        if($DebugBuild -and $ViewLog)
        {
            if(!(Test-IsWindows))
            {
                Write-Warning "Cannot open $binLog as MSBuld Structured Log Viewer is only compatible with Windows. Please copy binlog to Windows system in order to inspect log"
            }
            else
            {
                Start-Process $binLog
            }
        }
    }
}