. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Approve-Probe" -Tag @("PowerShell", "UnitTest") {

    Context "Default" {

        $probe = Run Probe { Get-Probe -Count 1 }
        $probe.Id = 1001

        It "approves a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=allow")
            )

            $probe | Approve-Probe
        }

        It "denies a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=deny")
            )

            $probe | Approve-Probe -Deny
        }

        It "approves and auto-discovers a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=allowanddiscover")
            )

            $probe | Approve-Probe -AutoDiscover
        }
    }

    Context "Manual" {
        It "approves a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=allow")
            )

            Approve-Probe -Id 1001
        }

        It "denies a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=deny")
            )

            Approve-Probe -Id 1001 -Deny
        }

        It "approves and auto-discovers a probe" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "authorized")
                [Request]::Get("api/probestate.htm?id=1001&action=allowanddiscover")
            )

            Approve-Probe -Id 1001 -AutoDiscover
        }

        It "throws specifying an object that is not a probe" {
            
            SetMultiTypeResponse

            { Approve-Probe -Id 9001 } | Should Throw "object does not appear to be a probe"
        }

        It "throws specifying an object that is not a probe non-English" {
            { Approve-Probe -Id 9002 } | Should Throw "object does not appear to be a probe"
        }
    }

    It "skips a probe that is already approved" {
        SetAddressValidatorResponse @(
            [Request]::GetObjectProperty(1002, "authorized")
        )

        $output = [string]::Join("`n",(&{try { Approve-Probe -Id 1002 3>&1 | %{$_.Message} } catch [exception] { }}))

        $output | Should Be "Skipping probe ID '1002' as it is already approved."
    }
}