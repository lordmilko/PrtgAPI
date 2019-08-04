. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

function WaitForAutoDiscovery($device)
{
    for($i = 0; $i -lt 10; $i++)
    {
        LogTestDetail "Auto-Discovery hasn't started yet. Refreshing and sleeping 5 seconds"
        $device | Refresh-Object

        $newDevice = Get-Device -Id $device.Id

        if($newDevice.Condition -like "Auto-Discovery*")
        {
            return $newDevice
        }

        Sleep 5
    }

    return (Get-Device -Id $device.Id)
}

Describe "Start-AutoDiscovery_IT" -Tag @("PowerShell", "IntegrationTest") {
    It "performs an auto-discovery" {

        $probe = Get-Probe -Id (Settings Probe)

        $device = $probe | Add-Device "autoDiscoverTest" "localhost"
        $device.Condition | Should Not BeLike "Auto-Discovery*"

        $device | Start-AutoDiscovery
        
        $newDevice = Get-Device -Id $device.Id

        if($newDevice.Condition -notlike "Auto-Discovery*")
        {
            $newDevice = WaitForAutoDiscovery $newDevice
        }

        $newDevice.Condition | Should BeLike "Auto-Discovery*"

        Unsafe {
            $newDevice | Should Not BeNullOrEmpty
            $newDevice | Remove-Object -Force
        }
    }

    It "performs an auto-discovery with a specified template" {
        $templates = Get-DeviceTemplate *wmi*

        $probe = Get-Probe -Id (Settings Probe)

        $device = $probe | Add-Device "autoDiscoverTemplateTest" "localhost"
        $device.Condition | Should Not BeLike "Auto-Discovery*"

        $device | Start-AutoDiscovery $templates
        $newDevice = Get-Device -Id $device.Id

        if($newDevice.Condition -notlike "Auto-Discovery*")
        {
            $newDevice = WaitForAutoDiscovery $newDevice
        }

        $newDevice.Condition | Should BeLike "Auto-Discovery*"

        Unsafe {
            $newDevice | Should Not BeNullOrEmpty
            $newDevice | Remove-Object -Force
        }
    }

    It "performs an auto-discovery with a specified template name" {

        $probe = Get-Probe -Id (Settings Probe)

        $device = $probe | Add-Device "autoDiscoverTemplateNameTest" "localhost"
        $device.Condition | Should Not BeLike "Auto-Discovery*"

        $device | Start-AutoDiscovery *wmi*

        $newDevice = Get-Device -Id $device.Id

        if($newDevice.Condition -notlike "Auto-Discovery*")
        {
            $newDevice = WaitForAutoDiscovery $newDevice
        }
        
        $newDevice.Condition | Should BeLike "Auto-Discovery*"

        Unsafe {
            $newDevice | Should Not BeNullOrEmpty
            $newDevice | Remove-Object -Force
        }
    }
}