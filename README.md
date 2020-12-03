# SimpleSwarm
PowerShell Cmdlet to quickly setup a Docker Swarm Cluster in Microsoft Azure.

## Requirements.
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
