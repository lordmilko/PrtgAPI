. $PSScriptRoot\..\Support\ObjectPropertyMultiType.ps1

Describe "Set-ObjectProperty_Containers_IT" {

    $testCases = @(
        @{name = "Devices"; obj = { Get-Device -Id (Settings Device) }}
        @{name = "Groups";  obj = { Get-Group  -Id (Settings Group)  }}
        @{name = "Probes";  obj = { Get-Probe  -Id (Settings Probe)  }}
    )

    It "Basic Settings" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetValue "Name"       "TestName"
        #SetValue "Tags" "TestTag"
        SetValue "Active"     $false
        SetValue "Priority"   5
    }

    It "Location" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        { SetChild "Location"        "23 Fleet Street" "InheritLocation" $false } | Should Throw "23 Fleet St, Boston, MA 02113, USA"

        $object | Set-ObjectProperty InheritLocation $true # InheritLocation doesn't get reverted because SetChild throws

        SetValue "InheritLocation" $false
    }

    It "Credentials for Windows Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "WindowsDomain"             "banana"      "InheritWindowsCredentials" $false
        SetChild "WindowsUserName"           "newUsername" "InheritWindowsCredentials" $false
        #SetChild "WindowsPassword"           "newPassword" "InheritWindowsCredentials" $false
        SetValue "InheritWindowsCredentials" $false        "InheritWindowsCredentials" $false
    }

    It "Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "LinuxUserName" "newUsername" "InheritLinuxCredentials" $false
        #need to set linux password
        SetChild "LinuxLoginMode" "PrivateKey" "InheritLinuxCredentials" $false
        #SetGrandChild "LinuxPrivateKey" BLAH
        SetChild "WbemProtocolMode" "HTTP"     "InheritLinuxCredentials" $false # this uses the xmlenumattribute. we need to be able to signify that we want to use xmlenumalternatename

        { SetChild "WbemPortMode" Manual "InheritLinuxCredentials" $false } | Should Throw "Required field, not defined"
        SetGrandChild "WbemPort"  6000         "WbemPortMode"            "Manual" "InheritLinuxCredentials" $false
        SetChild "SSHPort" 23 "InheritLinuxCredentials" $false
        SetChild "SSHElevationMode" RunAsAnotherWithPasswordViaSudo "InheritLinuxCredentials" $false
        #SetChild "SSHElevationUser" BLAH #this value binds to multiple sshelevationmodes. if you set this value without setting the parent,
        #does it do anything
        #SetChild SSHElevationPassword BLAH # this value binds to multiple SSHElevationMode values
        SetChild "SSHEngine" CompatibilityMode "InheritLinuxCredentials" $false
        SetValue "InheritLinuxCredentials" $false

        #what do we do where we need to fill in 2 fields, e.g. for the custom user for su we need a user and a pw? maybe you can leave them blank
    }

    It "Credentials for VMware/XenServer" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "VMwareUserName"           "newUsername"      "InheritVMwareCredentials" $false
        #SetChild "VMwarePassword"
        SetChild "VMwareProtocol"           "HTTP"             "InheritVMwareCredentials" $false
        SetChild "VMwareSessionMode"        "CreateNewSession" "InheritVMwareCredentials" $false
        SetValue "InheritVMwareCredentials" $false
    }

    It "Credentials for SNMP Devices" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "SNMPVersion"           "v1"          "InheritSNMPCredentials" $false
        SetChild "SNMPCommunityStringV1" "testString1" "InheritSNMPCredentials" $false
        SetChild "SNMPCommunityStringV2" "testString2" "InheritSNMPCredentials" $false

        SetChild "SNMPv3AuthType"        "SHA"         "InheritSNMPCredentials" $false
        SetChild "SNMPv3UserName"        "newUsername" "InheritSNMPCredentials" $false
        #SetChild "SNMPv3Password"
        SetChild "SNMPv3EncryptionType"  "AES"         "InheritSNMPCredentials" $false
        #SetChild "SNMPv3EncryptionKey
        SetChild "SNMPv3Context"        "newContext"   "InheritSNMPCredentials" $false

        SetChild "SNMPPort"             "200"          "InheritSNMPCredentials" $false
        SetChild "SNMPTimeout"          "10"           "InheritSNMPCredentials" $false

        #todo: how are we supposed to get the right description for community string for the different versions?
        #maybe we should make it use [description] as a preference, or fail when there are multiple xmlelements
        
        SetValue "InheritSNMPCredentials" $false
    }

    It "Credentials for Database Management Systems" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        { SetChild "DBPortMode" Manual "InheritDBCredentials" $false } | Should Throw "Required field, not defined"
        SetGrandChild "DBPort" "8080" "DBPortMode" "Manual" "InheritDBCredentials" $false

        { SetChild "DBAuthMode" SQL "InheritDBCredentials" $false } | Should Throw "Required field, not defined"
        SetGrandChild "DBUserName" "newUsername"  "DBAuthMode" "SQL" "InheritDBCredentials" $false
        #SetChild "DBPassword" "newPassword"

        SetChild "DBTimeout" "70" "InheritDBCredentials" $false
        SetValue "InheritDBCredentials" $false
    }

    It "Credentials for Amazon Cloudwatch" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "AmazonAccessKey" "newKey" "InheritAmazonCredentials" $false
        #SetChild "AmazonSecretKey" BLAH
        SetValue "InheritAmazonCredentials" $false
    }

    It "Windows Compatibility Options" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "WmiDataSource"               WMI    "InheritWindowsCompatibility" $false
        { SetChild "WmiTimeoutMethod"            Manual "InheritWindowsCompatibility" $false } | Should Throw "Required field, not defined"
        SetGrandChild "WmiTimeout"             50     "WmiTimeoutMethod"           "Manual" "InheritWindowsCompatibility" $false
        SetValue "InheritWindowsCompatibility" $false "InheritWindowsCompatibility" $false
    }

    It "SNMP Compatibility Options" -TestCases $testCases {
        param($name, $obj)

        $object = (& $obj)

        SetChild "SNMPDelay"                20             "InheritSNMPCompatibility" $false
        SetChild "SNMPRetryMode"            "DoNotRetry"   "InheritSNMPCompatibility" $false
        SetChild "SNMPOverflowMode"         "Ignore"       "InheritSNMPCompatibility" $false
        SetChild "SNMPZeroValueMode"        "Handle"       "InheritSNMPCompatibility" $false
        SetChild "SNMPCounterMode"          "Use32BitOnly" "InheritSNMPCompatibility" $false
        SetChild "SNMPRequestMode"          "SingleGet"    "InheritSNMPCompatibility" $false
        SetChild "SNMPPortNameTemplate"     "[ifsensor]"   "InheritSNMPCompatibility" $false
        SetChild "SNMPPortNameUpdateMode"   "Automatic"    "InheritSNMPCompatibility" $false
        SetChild "SNMPPortIdMode"           "UseIfName"    "InheritSNMPCompatibility" $false
        SetChild "SNMPInterfaceStartIndex"  1              "InheritSNMPCompatibility" $false
        SetChild "SNMPInterfaceEndIndex"    2              "InheritSNMPCompatibility" $false

        SetValue "InheritSNMPCompatibility" $false
    }
}