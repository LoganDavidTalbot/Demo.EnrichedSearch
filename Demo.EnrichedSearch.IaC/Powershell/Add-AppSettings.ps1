param (
    [string]$ResourceGroupName,
    [string]$WebAppName,
    [string]$SearchServiceName,
    [string]$StorageAccountName,
    [string]$CognitiveServiceName
)
Install-Module -Name Az.Search -force
Install-Module -Name Az.Websites -force

# Get Search Service Key
$searchServiceKeys = Get-AzSearchAdminKeyPair -ResourceGroupName $ResourceGroupName -ServiceName $SearchServiceName

# Get Storage Account String
$sa = Get-AzStorageAccount -StorageAccountName $StorageAccountName -ResourceGroupName $ResourceGroupName
$saKey = (Get-AzStorageAccountKey -ResourceGroupName $ResourceGroupName -Name $StorageAccountName)[0].Value
$saConnectionString = 'DefaultEndpointsProtocol=https;AccountName=' + $StorageAccountName + ';AccountKey=' + $saKey + ';EndpointSuffix=core.windows.net' 

# Get CognitiveService Key
$cognitiveServiceKey = Get-AzCognitiveServicesAccountKey -ResourceGroupName $ResourceGroupName -name $CognitiveServiceName

# Add App Settings to Web App
$newAppSettings = @{"SearchService_ServiceName"=$SearchServiceName;"SearchService_ApiKey"=$searchServiceKeys.Primary;"AzureBlobConnectionString"=$saConnectionString;"CognitiveServiceKey"=$cognitiveServiceKey.Primary;}
Set-AzWebApp -AppSettings $newAppSettings -Name $WebAppName -ResourceGroupName $ResourceGroupName