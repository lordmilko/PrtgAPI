function Get-PrtgVersion
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER
    )

    $ErrorActionPreference = "Stop"

    $assemblyFile = "$BuildFolder\PrtgAPI\Properties\AssemblyFileVersion.cs"

    $regex = New-Object System.Text.RegularExpressions.Regex ('(AssemblyFileVersion(Attribute)?\s*\(\s*\")(.*)(\"\s*\))', 
             [System.Text.RegularExpressions.RegexOptions]::MultiLine)

    $content = [IO.File]::ReadAllText($assemblyFile)

    $version = $null
    $match = $regex.Match($content)
    if($match.Success) {
        $version = $match.groups[3].value
    }

    # Extract version
    $ver = New-Object Version $version

    return $ver.ToString(3)
}