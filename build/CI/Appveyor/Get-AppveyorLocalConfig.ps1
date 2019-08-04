function Get-AppveyorLocalConfig
{
    $file = Get-AppveyorLocalConfigPath

    if(!(Test-Path $file))
    {
        $config = [PSCustomObject]@{
            "APPVEYOR_NUGET_API_KEY" = ""
        }

        $config|ConvertTo-Json|Set-Content $file

        throw "appveyor.local.json does not exist. Template has been created at '$file'"
    }

    $config = gc $file|ConvertFrom-Json

    if($config.APPVEYOR_NUGET_API_KEY -eq "")
    {
        throw "Property 'APPVEYOR_NUGET_API_KEY' of appveyor.local.json must be specified"
    }

    $env:APPVEYOR_NUGET_API_KEY = $config.APPVEYOR_NUGET_API_KEY

    return $config
}

function Get-AppveyorLocalConfigPath
{
    $folder = Split-Path (gmo Appveyor).Path -Parent
    $file = Join-Path $folder "appveyor.local.json"

    $file
}

Export-ModuleMember Get-AppveyorLocalConfigPath