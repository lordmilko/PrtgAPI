. $PSScriptRoot\..\..\..\Support\PowerShell\ObjectProperty.ps1

Describe "Set-ObjectProperty_Containers_IT" -Tag @("PowerShell", "IntegrationTest") {

    $testCases = @(
        @{name = "Devices"; obj = { Get-Device -Id (Settings Device) }}
        @{name = "Groups";  obj = { Get-Group  -Id (Settings Group)  }}
        @{name = "Probes";  obj = { Get-Probe  -Id (Settings Probe)  }}
    )
    
    It "Basic Settings" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetValue "Name"       "TestName"
        SetValue "Tags" "TestTag1","TestTag2"
        SetValue "Active"     $false
        SetValue "Priority"   5
    }

    It "Location" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        { SetChild "Location"        "410 Terry Ave. North Seattle" "InheritLocation" $false } | Should Throw "410 Terry Ave N, Seattle, WA 98109, United States"

        # Location: Coordinates
        $lat = 51.4521018
        $lon = 13.0766654

        SetValue "Location" "$lat, $lon" $true
        $prop = $object | Get-ObjectProperty
        $prop.Coordinates.Latitude | Should Be $lat
        $prop.Coordinates.Longitude | Should Be $lon

        # Location: Label
        SetLabelLocation "410 Terry Ave. North Seattle" "410 Terry Ave N, Seattle, WA 98109, United States"
        SetLabelLocation "51.4521018, 13.0766654"       "51.4521018, 13.0766654"

        $object | Set-ObjectProperty InheritLocation $true # InheritLocation doesn't get reverted because SetChild throws since the actual value is different after PRTG resolves it

        SetValue "InheritLocation" $false
    }

    It "Credentials for Windows Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild      "WindowsDomain"             "banana"                           "InheritWindowsCredentials" $false
        SetChild      "WindowsUserName"           "newUsername"                      "InheritWindowsCredentials" $false
        SetWriteChild "WindowsPassword"           "newPassword" "HasWindowsPassword" "InheritWindowsCredentials" $false
        SetValue      "InheritWindowsCredentials" $false                             "InheritWindowsCredentials" $false
    }
    
    It "Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild      "LinuxUserName"             "newUsername"                      "InheritLinuxCredentials" $false
        SetChild      "LinuxLoginMode"            "PrivateKey"                       "InheritLinuxCredentials" $false
        SetWriteChild "LinuxPassword"             "newPassword" "HasLinuxPassword"   "InheritLinuxCredentials" $false
        SetWriteChild "LinuxPrivateKey"           "newKey"      "HasLinuxPrivateKey" "InheritLinuxCredentials" $false
        SetChild      "WbemProtocolMode"          "HTTP"                             "InheritLinuxCredentials" $false
      { SetChild      "WbemPortMode"              "Manual"                           "InheritLinuxCredentials" $false } | Should Throw (ForeignMessage "Required field, not defined")
        SetGrandChild "WbemPort"                  6000                               "WbemPortMode"            "Manual"            "InheritLinuxCredentials" $false
        SetChild      "SSHPort"                   23                                 "InheritLinuxCredentials" $false
        SetChild      "SSHElevationMode"          "RunAsAnotherWithPasswordViaSudo"  "InheritLinuxCredentials" $false
        SetGrandChild "SSHElevationSuUser"        "newSuUser"                        "SSHElevationMode"        "RunAsAnotherViaSu" "InheritLinuxCredentials" $false
        SetChild      "SSHElevationSudoUser"      "newSudoUser"                      "InheritLinuxCredentials" $false
        SetWriteChild "SSHElevationPassword"      "newPassword" "HasSSHElevationPassword" "InheritLinuxCredentials" $false
        SetChild      "SSHEngine"                 "CompatibilityMode"                "InheritLinuxCredentials" $false
        SetValue      "InheritLinuxCredentials"   $false
    }
    
    It "Credentials for VMware/XenServer" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild      "VMwareUserName"            "newUsername"                      "InheritVMwareCredentials" $false
        SetWriteChild "VMwarePassword"            "newPassword" "HasVMwarePassword"  "InheritVMwareCredentials" $false
        SetChild      "VMwareProtocol"            "HTTP"                             "InheritVMwareCredentials" $false
        SetChild      "VMwareSessionMode"         "CreateNewSession"                 "InheritVMwareCredentials" $false
        SetValue      "InheritVMwareCredentials"  $false
    }

    It "Credentials for SNMP Devices" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild      "SNMPVersion"              "v1"                                "InheritSNMPCredentials" $false
        SetChild      "SNMPCommunityStringV1"    "testString1"                       "InheritSNMPCredentials" $false
        SetChild      "SNMPCommunityStringV2"    "testString2"                       "InheritSNMPCredentials" $false
        SetChild      "SNMPv3AuthType"           "SHA"                               "InheritSNMPCredentials" $false
        SetChild      "SNMPv3UserName"           "newUsername"                       "InheritSNMPCredentials" $false
        SetWriteChild "SNMPv3Password"           "newPassword" "HasSNMPv3Password"   "InheritSNMPCredentials" $false
        SetChild      "SNMPv3EncryptionType"     "AES"                               "InheritSNMPCredentials" $false
        SetWriteChild "SNMPv3EncryptionKey"      "newKey" "HasSNMPv3EncryptionKey"   "InheritSNMPCredentials" $false
        SetChild      "SNMPv3Context"            "newContext"                        "InheritSNMPCredentials" $false
        SetChild      "SNMPPort"                 "200"                               "InheritSNMPCredentials" $false
        SetChild      "SNMPTimeout"              "10"                                "InheritSNMPCredentials" $false
        SetValue      "InheritSNMPCredentials"   $false
    }

    It "Credentials for Database Management Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

      { SetChild      "DBPortMode"               "Manual"                         "InheritDBCredentials"   $false } | Should Throw (ForeignMessage "Required field, not defined")
        SetGrandChild "DBPort"                   "8080"                           "DBPortMode"             "Manual"            "InheritDBCredentials"    $false
      { SetChild      "DBAuthMode"               "SQL"                            "InheritDBCredentials"   $false } | Should Throw (ForeignMessage "Required field, not defined")
        SetGrandChild "DBUserName"               "newUsername"                    "DBAuthMode"             "SQL"               "InheritDBCredentials"    $false
        SetWriteChild  "DBPassword"              "newPassword" "HasDBPassword"    "InheritDBCredentials"   $false
        SetChild      "DBTimeout"                "70"                             "InheritDBCredentials"   $false
        SetValue      "InheritDBCredentials"     $false
    }

    It "Credentials for Amazon Cloudwatch" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild      "AmazonAccessKey"         "newKey"                          "InheritAmazonCredentials" $false
        SetWriteChild  "AmazonSecretKey"         "newSecretKey" "HasAmazonSecretKey" "InheritAmazonCredentials" $false
        SetValue      "InheritAmazonCredentials" $false
    }

    It "Windows Compatibility Options" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)
        
        SetChild      "WmiDataSource"           "WMI"                             "InheritWindowsCompatibility" $false
      { SetChild      "WmiTimeoutMethod"        "Manual"                          "InheritWindowsCompatibility" $false } | Should Throw (ForeignMessage "Required field, not defined")
        SetGrandChild "WmiTimeout"              50                                "WmiTimeoutMethod"        "Manual"            "InheritWindowsCompatibility" $false
        SetValue      "InheritWindowsCompatibility" $false
    }

    It "SNMP Compatibility Options" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)
        
        SetChild      "SNMPDelay"               20                                "InheritSNMPCompatibility" $false
        SetChild      "SNMPRetryMode"           "DoNotRetry"                      "InheritSNMPCompatibility" $false
        SetChild      "SNMPOverflowMode"        "Ignore"                          "InheritSNMPCompatibility" $false
        SetChild      "SNMPZeroValueMode"       "Handle"                          "InheritSNMPCompatibility" $false
        SetChild      "SNMPCounterMode"         "Use32BitOnly"                    "InheritSNMPCompatibility" $false
        SetChild      "SNMPRequestMode"         "SingleGet"                       "InheritSNMPCompatibility" $false
        SetChild      "SNMPPortNameTemplate"    "[ifsensor]"                      "InheritSNMPCompatibility" $false
        SetChild      "SNMPPortNameUpdateMode"  "Automatic"                       "InheritSNMPCompatibility" $false
        SetChild      "SNMPPortIdMode"          "UseIfName"                       "InheritSNMPCompatibility" $false
        SetChild      "SNMPInterfaceStartIndex" 1                                 "InheritSNMPCompatibility" $false
        SetChild      "SNMPInterfaceEndIndex"   2                                 "InheritSNMPCompatibility" $false
        SetValue      "InheritSNMPCompatibility" $false
    }
}