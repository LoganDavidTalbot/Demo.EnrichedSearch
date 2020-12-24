param (
    [string]$ResourceGroupName,
    [string]$WebAppName,
    [string]$SearchServiceName,
    [string]$StorageAccountName
)
Install-Module -Name Az.Search
Install-Module -Name Az.Websites

$keys = Get-AzSearchAdminKeyPair -ResourceGroupName $ResourceGroupName -ServiceName $SearchServiceName

$newAppSettings = @{"SearchService:ApiKey"=$keys.Primary;}
Set-AzureRmWebApp -AppSettings $newAppSettings -Name $WebAppName -ResourceGroupName $ResourceGroupName