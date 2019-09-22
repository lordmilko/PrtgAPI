. $PSScriptRoot\..\..\Support\PowerShell\Progress.ps1

function TestMessage
{
    param(
        $Expression,
        $Title,
        $Message,
        $QueueFormat,
        $WhatIfMessage,
        $Progress = $true
    )

    $result = (Invoke-Expression "$Expression -Verbose 4>&1" | where { $_ -like "Performing the operation*" }) -join ", "
    $result | Should Be $WhatIfMessage

    if(!$Progress)
    {
        Assert-NoProgress

        return
    }

    if($QueueFormat -eq $null)
    {
        $length = @($Message).Count

        if($length -eq 1)
        {
            Validate(@(
                (Gen $Title "$Message (1/1)" 100)
                (Gen "$Title (Completed)" "$Message (1/1)" 100)
            ))
        }
        else
        {
            Validate(@(
                $Message
            ))
        }
        
    }
    else
    {
        $first = [string]::Format($QueueFormat, "0")
        $second = [string]::Format($QueueFormat, "1")

        Validate(@(
            (Gen $Title "$first (1/2)" 50)
            (Gen $Title "$second (2/2)" 100)
            (Gen $Title "$Message (2/2)" 100)
            (Gen "$Title (Completed)" "$Message (2/2)" 100)
        ))
    }
}

function TestMessageNoMulti
{
    param(
        $Expression,
        $Title,
        $Message,
        $QueueFormat,
        $WhatIfMessage,
        $Progress = $true
    )

    $newP = @{
        Expression = $Expression
        Title = $Title
        Message = @(
            (Gen $Title "$([string]::Format($Message, "0")) (1/2)" 50)
            (Gen $Title "$([string]::Format($Message, "1")) (2/2)" 100)
            (Gen "$Title (Completed)" "$([string]::Format($Message, "1")) (2/2)" 100)
        )
        QueueFormat = $QueueFormat
        WhatIfMessage = $WhatIfMessage
        Progress = $Progress
    }

    TestMessage @newP
}

Describe "Test-ProgressMessage" -Tag @("PowerShell", "UnitTest") {
        
    $filter = "*"
    $ignoreNotImplemented = $false

    SetMultiTypeResponse

    $sensor = Get-Sensor -Count 1
    $sensors = Get-Sensor -Count 2
    $device = Get-Device -Count 1
    $devices = Get-Device -Count 2
    
    #region 1: Acknowledge-Sensor

    $testCases = @(
        @{name = "Duration"; description = "for 10 minutes";   params = "-Duration 10"}
        @{name = "Until";    description = "for 1440 minutes"; params = "-Until (Get-Date).AddDays(1)"}
        @{name = "Forever";  description = "forever";          params = "-Forever"}
    )

    It "1a: processes a single object using -<name>" -TestCases $testCases {

        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "`$sensor | Acknowledge-Sensor $params"
            Title = "Acknowledging PRTG Sensors"
            Message = "Acknowledging sensor 'Volume IO _Total0' $description"
            WhatIfMessage = "Performing the operation `"Confirm-Sensor`" on target `"'Volume IO _Total0' (ID: 4000, Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "1b: processes multiple objects" -TestCases $testCases {

        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "`$sensors | Acknowledge-Sensor $params"
            Title = "Acknowledging PRTG Sensors"
            Message = "Acknowledging sensors 'Volume IO _Total0' and 'Volume IO _Total1' $description"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Confirm-Sensor`" on target `"'Volume IO _Total0' (ID: 4000, Duration: $whatIf)`"., Performing the operation `"Confirm-Sensor`" on target `"'Volume IO _Total1' (ID: 4001, Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "1c: processes a single ID" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "Acknowledge-Sensor -Id 4000 $params"
            Progress = $false
            WhatIfMessage = "Performing the operation `"Confirm-Sensor`" on target `"ID 4000 (Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "1d: processes multiple IDs" -TestCases $testCases {

        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "Acknowledge-Sensor -Id 4000,4001 $params"
            Progress = $false
            WhatIfMessage = "Performing the operation `"Confirm-Sensor`" on target `"IDs 4000, 4001 (Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    #endregion
    #region 2: Pause-Object

    It "2a: processes a single object" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "`$sensor | Pause-Object $params"
            Title = "Pausing PRTG Objects"
            Message = "Pausing sensor 'Volume IO _Total0' $description"
            WhatIfMessage = "Performing the operation `"Suspend-Object`" on target `"'Volume IO _Total0' (ID: 4000, Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "2b: processes multiple objects" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "`$sensors | Pause-Object $params"
            Title = "Pausing PRTG Objects"
            Message = "Pausing sensors 'Volume IO _Total0' and 'Volume IO _Total1' $description"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Suspend-Object`" on target `"'Volume IO _Total0' (ID: 4000, Duration: $whatIf)`"., Performing the operation `"Suspend-Object`" on target `"'Volume IO _Total1' (ID: 4001, Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "2c: processes a single ID" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "Pause-Object -Id 4000 $params"
            Progress = $false
            WhatIfMessage = "Performing the operation `"Suspend-Object`" on target `"ID 4000 (Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "2d: processes multiple IDs" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $p = @{
            Expression = "Pause-Object -Id 4000,4001 $params"
            Progress = $false
            WhatIfMessage = "Performing the operation `"Suspend-Object`" on target `"IDs 4000, 4001 (Duration: $whatIf)`"."
        }

        TestMessage @p
    }

    It "2e: processes multiple objects of different types" -TestCases $testCases {
        
        param($description, $params)

        $whatIf = $description -replace "for ",""

        $both = $sensor,$device

        $expr = "`$both | Pause-Object $params"

        Invoke-Expression $expr

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' and device 'Probe Device0' $description (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' and device 'Probe Device0' $description (2/2)" 100)
        ))
    }

    #endregion
    #region 3: Move-Object

    It "3a: processes an object" {
        $p = @{
            Expression = "`$devices | Move-Object -DestinationId 1000"
            Title = "Moving PRTG Objects"
            Message = "Moving device 'Probe Device{0}' (ID: 300{0}) to object ID 1000"
            WhatIfMessage = "Performing the operation `"Move-Object`" on target `"'Probe Device0' (ID: 3000, Destination ID: 1000)`"., Performing the operation `"Move-Object`" on target `"'Probe Device1' (ID: 3001, Destination ID: 1000)`"."
        }

        TestMessageNoMulti @p
    }

    It "3b: processes an ID" {
        $p = @{
            Expression = "Move-Object -Id 4000 -DestinationId 1000"
            Title = "Moving PRTG Objects"
            WhatIfMessage = "Performing the operation `"Move-Object`" on target `"ID 4000 (Destination ID: 1000)`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 4: Start-AutoDiscovery

    It "4a: processes an object" {
        $p = @{
            Expression = "`$devices | Start-AutoDiscovery"
            Title = "PRTG Auto-Discovery"
            Message = "Starting Auto-Discovery on device 'Probe Device{0}' (ID: 300{0})"
            WhatIfMessage = "Performing the operation `"Start-AutoDiscovery`" on target `"'Probe Device0' (ID: 3000)`"., Performing the operation `"Start-AutoDiscovery`" on target `"'Probe Device1' (ID: 3001)`"."
        }

        TestMessageNoMulti @p
    }

    It "4b: processes an ID" {
        $p = @{
            Expression = "Start-AutoDiscovery -Id 3000"
            Title = "PRTG Auto-Discovery"
            WhatIfMessage = "Performing the operation `"Start-AutoDiscovery`" on target `"ID 3000`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 5: Sort-PrtgObject

    It "5a: processes an object" {
        $p = @{
            Expression = "`$devices | Sort-PrtgObject"
            Title = "Sorting PRTG Objects"
            Message = "Sorting children of object 'Probe Device{0}' (ID: 300{0})"
            WhatIfMessage = "Performing the operation `"Invoke-SortPrtgObject`" on target `"'Probe Device0' (ID: 3000)`"., Performing the operation `"Invoke-SortPrtgObject`" on target `"'Probe Device1' (ID: 3001)`"."
        }

        TestMessageNoMulti @p
    }

    It "5b: processes an ID"  {
        $p = @{
            Expression = "Sort-PrtgObject -Id 1000"
            Title = "Sorting PRTG Objects"
            WhatIfMessage = "Performing the operation `"Invoke-SortPrtgObject`" on target `"ID 1000`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 6: Refresh-Object

    It "6a: processes an object" {
        $p = @{
            Expression = "`$sensor | Refresh-Object"
            Title = "Refreshing PRTG Objects"
            Message = "Refreshing sensor 'Volume IO _Total0'"
            WhatIfMessage = "Performing the operation `"Update-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "6b: processes multiple objects" {
        $p = @{
            Expression = "`$sensors | Refresh-Object"
            Title = "Refreshing PRTG Objects"
            Message = "Refreshing sensors 'Volume IO _Total0' and 'Volume IO _Total1'"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Update-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"., Performing the operation `"Update-Object`" on target `"'Volume IO _Total1' (ID: 4001)`"."
        }

        TestMessage @p
    }

    It "6c: processes an ID" {
        $p = @{
            Expression = "Refresh-Object -Id 1000"
            Title = "Refreshing PRTG Objects"
            WhatIfMessage = "Performing the operation `"Update-Object`" on target `"ID 1000`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "6d: processes multiple IDs" {
        $p = @{
            Expression = "Refresh-Object -Id 1000,1001"
            Title = "Refreshing PRTG Objects"
            WhatIfMessage = "Performing the operation `"Update-Object`" on target `"IDs 1000, 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 7: Remove-Object

    It "7a: processes an object" {
        $p = @{
            Expression = "`$sensor | Remove-Object -Force"
            Title = "Removing PRTG Objects"
            Message = "Removing sensor 'Volume IO _Total0'"
            WhatIfMessage = "Performing the operation `"Remove-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "7b: processes multiple objects" {
        $p = @{
            Expression = "`$sensors | Remove-Object -Force"
            Title = "Removing PRTG Objects"
            Message = "Removing sensors 'Volume IO _Total0' and 'Volume IO _Total1'"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Remove-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"., Performing the operation `"Remove-Object`" on target `"'Volume IO _Total1' (ID: 4001)`"."
        }

        TestMessage @p
    }

    It "7c: processes an ID" {
        $p = @{
            Expression = "Remove-Object -Id 1000 -Force"
            Title = "Removing PRTG Objects"
            WhatIfMessage = "Performing the operation `"Remove-Object`" on target `"ID 1000`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "7d: processes multiple IDs" {
        $p = @{
            Expression = "Remove-Object -Id 1000,1001 -Force"
            Title = "Removing PRTG Objects"
            WhatIfMessage = "Performing the operation `"Remove-Object`" on target `"IDs 1000, 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 8: Rename-Object

    It "8a: processes an object" {
        $p = @{
            Expression = "`$sensor | Rename-Object `"test`""
            Title = "Rename PRTG Objects"
            Message = "Renaming sensor 'Volume IO _Total0' to 'test'"
            WhatIfMessage = "Performing the operation `"Rename-Object`" on target `"'Volume IO _Total0' (ID: 4000, New Name: 'test')`"."
        }

        TestMessage @p
    }

    It "8b: processes multiple objects" {
        $p = @{
            Expression = "`$sensors | Rename-Object `"test`""
            Title = "Rename PRTG Objects"
            Message = "Renaming sensors 'Volume IO _Total0' and 'Volume IO _Total1' to 'test'"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Rename-Object`" on target `"'Volume IO _Total0' (ID: 4000, New Name: 'test')`"., Performing the operation `"Rename-Object`" on target `"'Volume IO _Total1' (ID: 4001, New Name: 'test')`"."
        }

        TestMessage @p
    }

    It "8c: processes an ID" {
        $p = @{
            Expression = "Rename-Object -Id 1000 `"test`""
            Title = "Rename PRTG Objects"
            WhatIfMessage = "Performing the operation `"Rename-Object`" on target `"ID 1000 (New Name: 'test')`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "8d: processes multiple IDs" {
        $p = @{
            Expression = "Rename-Object -Id 1000,1001 `"test`""
            Title = "Rename PRTG Objects"
            WhatIfMessage = "Performing the operation `"Rename-Object`" on target `"IDs 1000, 1001 (New Name: 'test')`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 9: Simulate-ErrorStatus

    It "9a: processes an object" {
        $p = @{
            Expression = "`$sensor | Simulate-ErrorStatus"
            Title = "Simulating Sensor Errors"
            Message = "Simulating errors on sensor 'Volume IO _Total0'"
            WhatIfMessage = "Performing the operation `"Test-ErrorStatus`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "9b: processes multiple objects" {
        $p = @{
            Expression = "`$sensors | Simulate-ErrorStatus"
            Title = "Simulating Sensor Errors"
            Message = "Simulating errors on sensors 'Volume IO _Total0' and 'Volume IO _Total1'"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Test-ErrorStatus`" on target `"'Volume IO _Total0' (ID: 4000)`"., Performing the operation `"Test-ErrorStatus`" on target `"'Volume IO _Total1' (ID: 4001)`"."
        }

        TestMessage @p
    }

    It "9c: processes an ID" {
        $p = @{
            Expression = "Simulate-ErrorStatus -Id 1000"
            Title = "Simulating Sensor Errors"
            WhatIfMessage = "Performing the operation `"Test-ErrorStatus`" on target `"ID 1000`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "9d: processes multiple IDs" {
        $p = @{
            Expression = "Simulate-ErrorStatus -Id 1000,1001"
            Title = "Simulating Sensor Errors"
            WhatIfMessage = "Performing the operation `"Test-ErrorStatus`" on target `"IDs 1000, 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 10: Resume-Object

    It "10a: processes an object" {
        $p = @{
            Expression = "`$sensor | Resume-Object"
            Title = "Resuming PRTG Objects"
            Message = "Resuming sensor 'Volume IO _Total0'"
            WhatIfMessage = "Performing the operation `"Resume-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "10b: processes multiple objects" {
        $p = @{
            Expression = "`$sensors | Resume-Object"
            Title = "Resuming PRTG Objects"
            Message = "Resuming sensors 'Volume IO _Total0' and 'Volume IO _Total1'"
            QueueFormat = "Queuing sensor 'Volume IO _Total{0}' (ID: 400{0})"
            WhatIfMessage = "Performing the operation `"Resume-Object`" on target `"'Volume IO _Total0' (ID: 4000)`"., Performing the operation `"Resume-Object`" on target `"'Volume IO _Total1' (ID: 4001)`"."
        }

        TestMessage @p
    }

    It "10c: processes an ID" {
        $p = @{
            Expression = "Resume-Object -Id 1000"
            Title = "Resuming PRTG Objects"
            WhatIfMessage = "Performing the operation `"Resume-Object`" on target `"ID 1000`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "10d: processes multiple IDs" {
        $p = @{
            Expression = "Resume-Object -Id 1000,1001"
            Title = "Resuming PRTG Objects"
            WhatIfMessage = "Performing the operation `"Resume-Object`" on target `"IDs 1000, 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 11: Set-ObjectProperty

    It "11a: processes an object with a typed property" {
        $p = @{
            Expression = "`$sensor | Set-ObjectProperty Name 'test'"
            Title = "Modify PRTG Object Settings"
            Message = "Setting sensor 'Volume IO _Total0' setting 'Name' to 'test'"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty Name = 'test'`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "11b: processes an object with a raw property" {
        $p = @{
            Expression = "`$sensor | Set-ObjectProperty -RawProperty 'name_' -RawValue 'test' -Force"
            Title = "Modify PRTG Object Settings"
            Message = "Setting sensor 'Volume IO _Total0' setting 'name_' to 'test'"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty name_ = 'test'`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "11c: processes an object with raw parameters" {
        $p = @{
            Expression = "`$sensor | Set-ObjectProperty -RawParameters @{ name_ = 'test' } -Force"
            Title = "Modify PRTG Object Settings"
            Message = "Setting sensor 'Volume IO _Total0' settings 'name_' = 'test'"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty name_ = 'test'`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "11d: processes an object with dynamic parameters" {
        $p = @{
            Expression = "`$sensor | Set-ObjectProperty -Interval 00:05:00"
            Title = "Modify PRTG Object Settings"
            Message = "Setting sensor 'Volume IO _Total0' setting 'Interval' to '00:05:00'"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty Interval = '00:05:00'`" on target `"'Volume IO _Total0' (ID: 4000)`"."
        }

        TestMessage @p
    }

    It "11e: processes an ID with a typed property" {
        $p = @{
            Expression = "Set-ObjectProperty -Id 1001 Name 'test'"
            Title = "Modify PRTG Object Settings"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty Name = 'test'`" on target `"ID 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "11f: processes an ID with a raw property" {
        $p = @{
            Expression = "Set-ObjectProperty -Id 1001 -RawProperty name_ -RawValue 'test' -Force"
            Title = "Modify PRTG Object Settings"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty name_ = 'test'`" on target `"ID 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "11g: processes an ID with raw parameters" {
        $p = @{
            Expression = "Set-ObjectProperty -Id 1001 -RawParameters @{ name_ = 'test' } -Force"
            Title = "Modify PRTG Object Settings"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty name_ = 'test'`" on target `"ID 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    It "11h: processes an ID with dynamic parameters" {
        $p = @{
            Expression = "Set-ObjectProperty -Id 1001 -Interval 00:05:00"
            Title = "Modify PRTG Object Settings"
            WhatIfMessage = "Performing the operation `"Set-ObjectProperty Interval = '00:05:00'`" on target `"ID 1001`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
    #region 12: Restart-Probe

    # Object -Wait tests are in Progress.Tests.ps1. The -Wait logic is identical with an -Id or a specified Probe

    It "12: processes an ID without waiting" {
        $p = @{
            Expression = "Restart-Probe -Id 1000 -Wait:`$false -Force"
            Title = "Restart PRTG Probes"
            WhatIfMessage = "Performing the operation `"Restart-Probe`" on target `"'127.0.0.10' (ID: 1000)`"."
            Progress = $false
        }

        TestMessage @p
    }

    #endregion
}