function Clear-CIBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        $BuildFolder,

        [switch]$IsCore,

        [switch]$NuGetOnly
    )

    if(!$NuGetOnly)
    {
        if($IsCore)
        {
            Invoke-Process { dotnet clean (Join-Path $BuildFolder PrtgAPIv17.sln) }
        }
        else
        {
            Invoke-Process { & (Get-MSBuild) /t:clean (Join-Path $BuildFolder PrtgAPI.sln) }
        }
    }

    $nupkgs = gci $BuildFolder -Recurse -Filter *.*nupkg | where {
        !$_.FullName.StartsWith((Join-Path $BuildFolder "packages"))
    }
    
    $nupkgs | foreach {
        Write-LogError "`tRemoving $($_.FullName)"

        $_ | Remove-Item -Force
    }
}