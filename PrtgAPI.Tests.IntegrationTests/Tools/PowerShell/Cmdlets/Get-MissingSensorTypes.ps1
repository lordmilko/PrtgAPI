function Get-MissingSensorTypes
{
    [CmdletBinding()]
    param ()

    Begin
    {
        if(!(Get-Module PrtgAPI))
        {
            ipmo $PSScriptRoot\PrtgAPI.dll
        }

        if(!(Get-PrtgClient))
        {
            throw "You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer."
        }
    }

    Process {
        $prtgTypes = (Get-PrtgClient).GetSensorTypes(1)

        $uniquePrtgTypes = $prtgTypes|group id|foreach { $_.group | Select -first 1 }

        $knownValues = [Enum]::GetValues([PrtgAPI.SensorTypeInternal])
        $xmlEnumAttributes = $knownValues | foreach {
            $attribs = ($_.GetType().GetMember($_.ToString()) | Select -First 1).GetCustomAttributes([System.Xml.Serialization.XmlEnumAttribute], $false)

            if($attribs)
            {
                return $attribs | Select -First 1
            }

            throw "$_ was missing an XmlEnumAttribute"
        }

        $missingObjs = $uniquePrtgTypes | foreach {
            $missing = $null -eq ($xmlEnumAttributes|where name -eq $_.Id)

            return [MissingSensorType]::new($_.Id, $_.Name, $_.Description, $missing)
        }

        foreach($obj in $missingObjs)
        {
            Write-Output $obj
        }
    }
}