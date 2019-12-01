. $PSScriptRoot\..\..\Support\PowerShell\Progress.ps1

function ValidateObjectPipeToContainerWithChild
{
    Validate(@(
        (Gen "PRTG Probe Search" "Retrieving all probes")
        (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100)
        (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all groups")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search" "Retrieving children of group 'Windows Infrastructure0'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3000) (1/1)" 100) +
                        (Gen5 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3000) (1/1)" 100) +
                        (Gen5 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3000) (1/1)" 100)

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen3 "PRTG Tree Search (Completed)" "Processing children of group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

        ###################################################################

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search" "Retrieving children of group 'Windows Infrastructure1'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                        (Gen5 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                        (Gen5 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                    (Gen4 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100)

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen3 "PRTG Tree Search (Completed)" "Processing children of group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

        ###################################################################

        (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
            (Gen2 "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

        (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100)
    ))
}

Describe "Test-ProgressTree" -Tag @("PowerShell", "UnitTest") {
        
    $filter = "*"
    $ignoreNotImplemented = $false

    #region 1: Get-PrtgTree
    
    It "1a: Get-PrtgTree (Standalone Container)" {
        SetResponseAndClientWithArguments "TreeRequestResponse" "StandaloneContainer"

        Get-PrtgTree -Id 1001

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving children of probe '127.0.0.1'")
            (Gen "PRTG Tree Search (Completed)" "Retrieving children of probe '127.0.0.1'")
        ))
    }

    It "1b: Get-PrtgTree (Container With Child)" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithChild"

        Get-PrtgTree -Id 1001

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving children of probe '127.0.0.1'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Device Tree Search (Completed)" "Retrieving children of device 'Probe Device'")

            (Gen "PRTG Tree Search (Completed)" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100)
        ))
    }

    It "1c: Get-PrtgTree (Container With Grand Child)" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithGrandChild"

        Get-PrtgTree -Id 1001

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving children of probe '127.0.0.1'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Group Tree Search" "Retrieving children of group 'Windows Infrastructure'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2002) (1/1)" 100) +
                    (Gen3 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2002) (1/1)" 100) +
                    (Gen3 "PRTG Device Tree Search (Completed)" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Group Tree Search (Completed)" "Processing children of group 'Windows Infrastructure' (ID: 2002) (1/1)" 100)

            (Gen "PRTG Tree Search (Completed)" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100)
        ))
    }

    It "1d: Get-PrtgTree (Multi Level Container)" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "MultiLevelContainer"

        Get-PrtgTree -Id 1001

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving children of probe '127.0.0.1'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Retrieving children of group 'Windows Infrastructure'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search" "Retrieving children of group 'VMware'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search" "Processing children of group 'VMware' (ID: 2002) (1/1)" 100) +
                        (Gen4 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search" "Processing children of group 'VMware' (ID: 2002) (1/1)" 100) +
                        (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3003) (1/1)" 100) +
                            (Gen5 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search" "Processing children of group 'VMware' (ID: 2002) (1/1)" 100) +
                        (Gen4 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3003) (1/1)" 100) +
                            (Gen5 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search" "Processing children of group 'VMware' (ID: 2002) (1/1)" 100) +
                        (Gen4 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3003) (1/1)" 100)

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Group Tree Search (Completed)" "Processing children of group 'VMware' (ID: 2002) (1/1)" 100)

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Tree Search (Completed)" "Processing children of group 'Windows Infrastructure' (ID: 2001) (2/2)" 100)

            (Gen "PRTG Tree Search (Completed)" "Processing children of probe '127.0.0.1' (ID: 1001) (2/2)" 100)
        ))
    }

    It "1e: Table -> Table -> Get-PrtgTree" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "ObjectPipeToContainerWithChild"

        Get-Probe -Count 1 | Get-Group | Get-PrtgTree

        ValidateObjectPipeToContainerWithChild
    }

    It "1f: Table -> Action -> Get-PrtgTree" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "ActionPipeToContainerWithChild"

        Get-Group -Count 1 | Clone-Object -DestinationId 1001 | Get-PrtgTree

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving children of group 'Windows Infrastructure'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3001) (1/1)" 100)

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (2/2)" 100) +
                    (Gen3 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (2/2)" 100) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (2/2)" 100) +
                    (Gen3 "PRTG Device Tree Search" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100) +
                        (Gen4 "PRTG Sensor Tree Search (Completed)" "Retrieving children of sensor 'Volume IO _Total'")

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Processing children of group 'Windows Infrastructure' (ID: 2000) (2/2)" 100) +
                    (Gen3 "PRTG Device Tree Search (Completed)" "Processing children of device 'Probe Device' (ID: 3002) (1/1)" 100)

            (Gen1 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100) +
                (Gen2 "PRTG Tree Search (Completed)" "Processing children of group 'Windows Infrastructure' (ID: 2000) (2/2)" 100)

            (Gen "Cloning PRTG Groups (Completed)" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 1001 (1/1)" 100)
        ))
    }
    
    It "1g: Get-PrtgTree (Fast)" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "FastPath"

        Get-PrtgTree

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving all probes")
            (Gen "PRTG Tree Search" "Retrieving all probes (1/4)" 25)
            (Gen "PRTG Tree Search" "Retrieving all groups" 25)
            (Gen "PRTG Tree Search" "Retrieving all groups (2/4)" 50)
            (Gen "PRTG Tree Search" "Retrieving all devices" 50)
            (Gen "PRTG Tree Search" "Retrieving all devices (3/4)" 75)
            (Gen "PRTG Tree Search" "Retrieving all sensors" 75)
            (Gen "PRTG Tree Search" "Retrieving all sensors (4/4)" 100)
            (Gen "PRTG Tree Search (Completed)" "Retrieving all sensors (4/4)" 100)
        ))
    }

    It "1h: Table -> Get-PrtgTree (Fast)" {
        SetResponseAndClientWithArguments "TreeRequestResponse" "ObjectToFastPath"

        Get-Group -Id 0 | Get-PrtgTree

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all probes")

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all probes (1/4)" 25)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all groups" 25)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all groups (2/4)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all devices" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all devices (3/4)" 75)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all sensors" 75)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search" "Retrieving all sensors (4/4)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Root' (ID: 0) (1/1)" 100) +
                (Gen2 "PRTG Tree Search (Completed)" "Retrieving all sensors (4/4)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Root' (ID: 0) (1/1)" 100)
        ))
    }

    #endregion
    #region Show-PrtgTree

    It "2a: Table -> Table -> Show-PrtgTree" {

        SetResponseAndClientWithArguments "TreeRequestResponse" "ObjectPipeToContainerWithChild"

        Get-Probe -Count 1 | Get-Group | Show-PrtgTree

        ValidateObjectPipeToContainerWithChild
    }

    It "2b: Show-PrtgTree -Id" {
        SetResponseAndClientWithArguments "TreeRequestResponse" "ContainerWithChild"

        Show-PrtgTree -Id 1001

        Validate(@(
            (Gen "PRTG Tree Search" "Retrieving children of probe '127.0.0.1'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Device Tree Search" "Retrieving children of device 'Probe Device'")

            (Gen1 "PRTG Tree Search" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100) +
                (Gen2 "PRTG Device Tree Search (Completed)" "Retrieving children of device 'Probe Device'")

            (Gen "PRTG Tree Search (Completed)" "Processing children of probe '127.0.0.1' (ID: 1001) (1/1)" 100)
        ))
    }

    #endregion
}