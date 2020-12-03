# SimpleSwarm
PowerShell Cmdlet to quickly setup a Docker Swarm Cluster in Microsoft Azure.

## How to compile the module.
In order to compile the SimpleSwarm PowerShell Module follow this steps:

* Install .Net SDK
* Clone the Github repository.
* Run the command dotnet build.

```properties
dotnet build
```

* Open PowerShell and execute the command:

```powershell
Import-Module <ProjectLocation>\bin\Debug\netstandard2.0\SimpleSwarm.dll
```
* Now that you have imported the module the different PowerShell Cmdlet will be available in the terminal.

## Requirements to run the module.
In order to use the PowerShell module you will need to do the following:

* Create a Service Principal with Contributor access.
  * [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/ad/sp?view=azure-cli-latest#az_ad_sp_create_for_rbac)
  * [Azure PowerShell](https://docs.microsoft.com/en-us/powershell/module/az.resources/new-azadserviceprincipal?view=azps-5.1.0)

* Save the information about the Service Principal inside a new file (Example: azureauth.json).

```json
 {
  "clientId": "XXXXXXXXXXXXXXXXXXXX",
  "clientSecret": "XXXXXXXXXXXXXXXXXXXX",
  "subscriptionId": "XXXXXXXXXXXXXXXXXXXX",
  "tenantId": "XXXXXXXXXXXXXXXXXXXX",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

* Create an environment variable AZURE_AUTH_LOCATION with the location of azureauth.json 

## PowerShell Cmdlet Available.

| Cmdlet Name               | Description                                                                                                                    |  
|---------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| New-SimpleSwarmCluster    | Create the Azure resources requires for the cluster (vnet, storage account, availability set, Key Vaults, Managed Identities)  |    
| Add-SimpleSwarmManager    | Add a new Docker Swarm Manager to the cluster                                                                                  |
| Add-SimpleSwarmWorker     | Work in progress   |
