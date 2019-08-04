. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTestSafe.ps1

Describe "Backup-PrtgConfig_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "can execute" {
        $originalFiles = [PrtgAPI.Tests.IntegrationTests.ObjectManipulation.AdminToolTests]::GetBackupFiles() | select -ExpandProperty FullName

        $originalFiles.Count | Should BeGreaterThan 0

        Backup-PrtgConfig

        LogTest "Pausing for 10 seconds while backup is created"
        Sleep 10

        $newFiles = [PrtgAPI.Tests.IntegrationTests.ObjectManipulation.AdminToolTests]::GetBackupFiles() | select -ExpandProperty FullName

        $newFiles.Count | Should Be ($originalFiles.Count + 1)

        $diff = @($newFiles | where { $originalFiles -notcontains $_ })

        $diff.Count | Should Be 1

        $firstFile = $diff | select -First 1

        [PrtgAPI.Tests.IntegrationTests.ObjectManipulation.AdminToolTests]::RemoveBackupFile($firstFile)
    }
}