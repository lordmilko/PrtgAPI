function New-CSharpPackage
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$BuildFolder,

        [Parameter(Mandatory = $true)]
        [string]$OutputFolder,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [string]$Configuration,

        [Parameter(Mandatory = $true)]
        [switch]$IsCore
    )

    Write-LogInfo "`t`tBuilding package"

    $nuget = $null
    $nugetArgs = $null

    if($IsCore)
    {
        $nugetArgs = @(
            "pack"
            Join-Path $BuildFolder "PrtgAPI\PrtgAPIv17.csproj"
            "--include-symbols"
            "--no-restore"
            "--no-build"
            "-c"
            $Configuration
            "--output"
            "$OutputFolder"
            "/nologo"
            "-p:EnableSourceLink=true;SymbolPackageFormat=snupkg"
        )

        #todo: dotnet pack exclusions?

        $nuget = "dotnet"
    }
    else
    {
        $nugetArgs = @(
            "pack"
            Join-Path $BuildFolder "PrtgAPI\PrtgAPI.csproj"
            "-Exclude"
            "**/*.tt;**/Resources/*.txt;*PrtgClient.Methods.xml;**/*.json"
            "-outputdirectory"
            "$OutputFolder"
            "-NoPackageAnalysis"
            "-symbols"
            "-version"
            $Version
            "-properties"
            "Configuration=$Configuration"
        )

        $nuget = "nuget"
    }

    Write-Verbose "Executing command '$nuget $nugetArgs'"

    $result = Invoke-Process { & $nuget @nugetArgs }

    if($result)
    {
        Write-LogInfo $result
    }
}