param (
    [string]$ResourceGroupName,
    [string]$WebAppName,
    [string]$SearchServiceName,
    [string]$StorageAccountName
)
Install-Module -Name Az.Search -force
Install-Module -Name Az.Websites -force

$searchServiceKeys = Get-AzSearchAdminKeyPair -ResourceGroupName $ResourceGroupName -ServiceName $SearchServiceName

$newAppSettings = @{"SearchService_ServiceName"=$SearchServiceName;"SearchService_ApiKey"=$searchServiceKeys.Primary;}
Set-AzWebApp -AppSettings $newAppSettings -Name $WebAppName -ResourceGroupName $ResourceGroupName