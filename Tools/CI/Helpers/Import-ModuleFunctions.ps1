function Import-ModuleFunctions
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Folder,

        [string]$Exclude
    )

    $functions = Get-ChildItem $folder

    foreach($function in $functions)
    {
        if($Exclude -ne $null -and $function.Name -like $Exclude)
        {
            continue
        }

        . $function.FullName

        Export-ModuleMember $function.Name.Substring(0, $function.Name.Length - $function.Extension.Length)
    }
}