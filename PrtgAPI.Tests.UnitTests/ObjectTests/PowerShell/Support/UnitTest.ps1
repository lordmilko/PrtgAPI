#region Initialization

. $PSScriptRoot\Init.ps1

function Describe($name, $script) {

    Pester\Describe $name {
        BeforeAll { Startup $name.Substring($name.indexof("-") + 1) }
        AfterAll { Shutdown }

        & $script
    }
}

function GetItem
{
    return $global:tester.GetItem()
}

function WithItems($items, $assert)
{
    $oldClient = Get-PrtgClient

    $global:tester.SetPrtgSessionState($items)

    try
    {
        $result = & $assert

        if($result)
        {
            return $result
        }
    }
    finally
    {
        $global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
    }
}

function Run($objectType, $script)
{
    $oldClient = Get-PrtgClient
    $oldTester = $global:tester
    $global:tester = (New-Object PrtgAPI.Tests.UnitTests.ObjectTests.$($objectType)Tests)
    $global:tester.SetPrtgSessionState()

    try
    {
        $result = & $script
    }
    finally
    {
        $global:tester.SetPrtgSessionState([PrtgAPI.PrtgClient]$oldClient)
        $global:tester = $oldTester
    }    

    return $result
}

function GetCustomCountDictionary($hashtable)
{
    $dictionary = New-Object "System.Collections.Generic.Dictionary[[PrtgAPI.Content],[int]]"

    foreach($entry in $hashtable.GetEnumerator())
    {
        $newKey = $entry.Key -as "PrtgAPI.Content"

        if($newKey -eq $null)
        {
            throw "$($entry.Key) is not a valid PrtgAPI.Content value"
        }

        $dictionary.Add($newKey, $entry.Value)
    }

    return $dictionary
}

function SetMultiTypeResponse
{
    SetResponseAndClient "MultiTypeResponse"
}

function SetActionResponse
{
    SetResponseAndClientWithArguments "BasicResponse" ""
}

function SetResponseAndClient($responseName)
{
    $response = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.$responseName

    SetResponseAndClientInternal $response
}

function SetResponseAndClientWithArguments($responseName, $arguments)
{
    $response = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.$responseName -ArgumentList $arguments

    SetResponseAndClientInternal $response
}

function SetResponseAndClientInternal($response)
{
    $client = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client($response)

    SetPrtgClient $client
}

function SetPrtgClient($client)
{
    $type = [PrtgAPI.PrtgClient].Assembly.GetType("PrtgAPI.PowerShell.PrtgSessionState")
    $property = $type.GetProperty("Client", [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic)

    $property.SetValue($null, $client)
}