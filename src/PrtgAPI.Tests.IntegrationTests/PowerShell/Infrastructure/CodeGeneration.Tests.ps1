. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Add Missing Sensor Types" -Tag @("PowerShell", "IntegrationTest") {
    It "generates missing sensor types" {

        $types = [PrtgAPI.Tests.IntegrationTests.Support.SensorTypeManager]::GetMissingSensorTypes()

        $missingTypes = ($types|where missing -EQ $true).Count

        if(($types|where missing -EQ $true).Count -gt 0)
        {
            LogTestDetail "Adding $missingTypes missing types"

            $projectName = "PrtgAPI.Tests.IntegrationTests".ToLower()

            $path = (Get-Module $projectName).Path.ToLower()

            $index = $path.IndexOf($projectName)
            $rootDir = $path.Substring(0, $index) # PrtgAPI root solution directory

            $sensorTypeFile = $rootDir + "PrtgAPI\Enums\Serialization\SensorTypeInternal.cs"

            if(!(Test-Path $sensorTypeFile))
            {
                throw "File '$sensorTypeFile' cannot be found"
            }

            $text = [PrtgAPI.Tests.IntegrationTests.Support.SensorTypeManager]::GetSensorTypesInternalText($types)

            $text | Out-File $sensorTypeFile
        }
        else
        {
            LogTestDetail "All types are up to date; nothing to do"
        }
    }
}