. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "New-PropertyNode" -Tag @("PowerShell", "UnitTest") {
    
    Context "PropertyValuePair" {
        It "creates a new node from a PropetyValuePair" {

            SetMultiTypeResponse

            $pair = (New-PropertyNode -Id 1001 name val).Value

            $node = New-PropertyNode $pair

            $node.Type | Should Be Property
            $pair | Should Be $pair
        }

        It "pipes a PropertyValuePair" {

            SetMultiTypeResponse

            $pair = (New-PropertyNode -Id 1001 name val).Value

            $node = $pair | New-PropertyNode

            $node.Type | Should Be Property
            $pair | Should Be $pair
        }
    }

    Context "PrtgObject" {

        It "pipes a PrtgObject" {
            SetMultiTypeResponse

            $node = Get-Sensor -Count 1 | New-PropertyNode Name potato

            $node.Type | Should Be Property
            $node.Value.ParentId | Should Be 4000
            $node.Value.Property | Should Be "Name"
            $node.Value.Value | Should Be "potato"
        }

        It "specifies an ObjectProperty" {

            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(2203, "name&show=text")
            )

            $sensor = Run Sensor { Get-Sensor }

            $node = $sensor | New-PropertyNode Name
            $node.Value.Property.Left.GetType().FullName | Should Be "PrtgAPI.ObjectProperty"
            $node.Value.Property | Should Be "Name"
        }

        It "specifies a raw property" {

            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(2203, "name")
            )

            $sensor = Run Sensor { Get-Sensor }
            
            $node = $sensor | New-PropertyNode name_

            $node.Value.Property | Should Be "name_"
        }

        It "specifies an ObjectProperty and a value" {

            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1
            $node = $sensor | New-PropertyNode Name potato
            $node.Value.Property | Should Be "Name"
            $node.Value.Value | Should Be "potato"
        }

        It "specifies a raw property and a value" {
            SetMultiTypeResponse

            $sensor = Get-Sensor -Count 1
            $node = $sensor | New-PropertyNode name_ potato
            $node.Value.Property | Should Be "name_"
            $node.Value.Value | Should Be "potato"
        }
    }

    Context "Manual" {
        It "specifies an ObjectProperty" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "name&show=text")
            )

            $node = New-PropertyNode -ID 1001 Name
            $node.Value.Property.Left.GetType().FullName | Should Be "PrtgAPI.ObjectProperty"
            $node.Value.Property | Should Be "Name"
        }

        It "specifies a raw property" {
            SetAddressValidatorResponse @(
                [Request]::GetObjectProperty(1001, "name")
            )
            
            $node = New-PropertyNode -Id 1001 name_

            $node.Value.Property | Should Be "name_"
        }

        It "specifies an ObjectProperty and a value" {
            SetMultiTypeResponse

            $node = New-PropertyNode -Id 1001 Name potato
            $node.Value.Property | Should Be "Name"
            $node.Value.Value | Should Be "potato"
        }

        It "specifies a raw property and a value" {
            SetMultiTypeResponse

            $node = New-PropertyNode -Id 1001 name_ potato
            $node.Value.Property | Should Be "name_"
            $node.Value.Value | Should Be "potato"
        }
    }
}