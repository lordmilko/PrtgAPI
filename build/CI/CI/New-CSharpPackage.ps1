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
        # Unlike nuget.exe, dotnet pack only includes "normal" files in the output directory
        # and so we don't need to manually specify a list of exclusions
        $nugetArgs = @(
            "pack"
            Join-Path $BuildFolder "src\PrtgAPI\PrtgAPIv17.csproj"
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

        Install-CIDependency dotnet

        $nuget = "dotnet"
    }
    else
    {
        $nugetArgs = @(
            "pack"
            Join-Path $BuildFolder "src\PrtgAPI\PrtgAPI.csproj"
            "-Exclude"
            "**/*.tt;**/Resources/*.txt;PublicAPI.txt;*PrtgClient.Methods.xml;**/*.json"
            "-outputdirectory"
            "$OutputFolder"
            "-NoPackageAnalysis"
            "-symbols"
            "-SymbolPackageFormat"
            "snupkg"
            "-version"
            $Version
            "-properties"
            "Configuration=$Configuration"
        )

        Install-CIDependency nuget

        $nuget = "nuget"
    }

    Write-Verbose "Executing command '$nuget $nugetArgs'"

    Invoke-Process { & $nuget @nugetArgs }
}