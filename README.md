# SimpleSwarm
PowerShell Cmdlet to quickly setup a Docker Swarm Cluster in Microsoft Azure and use it to run Azure DevOps Agents.

## How to compile the module
In order to compile the SimpleSwarm PowerShell Module follow this steps:

* Install .Net SDK
* Clone the Github repository.
* Run the command.

```properties
dotnet build
```

* Open PowerShell and execute the command:

```powershell
Import-Module <ProjectLocation>\bin\Debug\netstandard2.0\SimpleSwarm.dll
```
* Now that you have imported the module the different PowerShell Cmdlet will be available in the terminal.

## Requirements to run the module
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

* Create an environment variable AZURE_AUTH_LOCATION with the location of azureauth.json. 

## PowerShell Cmdlet Available

The following Cmdlet are available for the SimpleSwarm Module.

| Cmdlet Name                |
|----------------------------|
| New-SimpleSwarmCluster     |    
| Add-SimpleSwarmManager     |
| Add-SimpleSwarmWorker      |
| Add-SimpleSwarmDevOpsAgent |

### New-SimpleSwarmCluster
This Cmdlet will create the following resources inside Azure:

* Resource group.
* Swarm Manager User-assigned Manged Identity.
* Swarm Worker User-assigned Manged Identity.
* Key Vault.
* Virtual Network.
* Storage Account.
* Docker Swarm Managers Availability Set. 
* Docker Swarm Workers Availability Set.

The Cmdlet has the following parameters:

| Parameter                 | Description                                              |  
|---------------------------|----------------------------------------------------------|
| ResourceGroupName         | Azure resource group to create the resources             |    
| Location                  | Azure regiont to create the resources (Example: eastus2) |

* Example:
```powershell
New-SimpleSwarmCluster -ResourceGroupName myNewResourceGroup -Location eastus2
```

### Add-SimpleSwarmManager
This Cmdlet will do the following configuration:

* Create a new Azure Virtual Machine that will be the Docker Swarm Manager.
* Add the virtual machine to the Docker Swarm Manager Availability Set.
* Add the virtual machine information to the storage account table SimpleSwarmSetup.
* Initialize the Docker Swarm Manager 
* Save the Docker Swarm tokens inside the key vault using User-assigned Identity


The Cmdlet has the following parameters:

| Parameter                 | Description                                              |  
|---------------------------|----------------------------------------------------------|
| ResourceGroupName         | Azure resource group to create the resources             |    
| Location                  | Azure regiont to create the resources (Example: eastus2) |
| AdminUsername             | Azure VM user name to connect using ssh                  |    
| AdminPassword             | Azure VM password to connect using ssh                   |

* Example:
```powershell
Add-SimpleSwarmManager -ResourceGroupName XXX -Location XXX -AdminUsername XXX -AdminPassword XXX
```

### Add-SimpleSwarmWorker
This Cmdlet will do the following configuration:

* Create a new Azure Virtual Machine that will be Docker Swarm Worker.
* Add the virtual machine to the Docker Swarm Worker Availability Set.
* Add the virtual machine information to the storage account table SimpleSwarmSetup.
* Read Docker Swarm tokens inside the key vault using User-assigned Identity.
* Initialize the Docker Swarm Worker.

The Cmdlet has the following parameters:

| Parameter                 | Description                                              |  
|---------------------------|----------------------------------------------------------|
| ResourceGroupName         | Azure resource group to create the resources             |    
| Location                  | Azure regiont to create the resources (Example: eastus2) |
| AdminUsername             | Azure VM user name to connect using ssh                  |    
| AdminPassword             | Azure VM password to connect using ssh                   |

* Example:
```powershell
Add-SimpleSwarmManager -ResourceGroupName XXX -Location XXX -AdminUsername XXX -AdminPassword XXX
```

### Add-SimpleSwarmDevOpsAgent
This Cmdlet will do the following configuration:

* Run Azure DevOps Agent inside the Docker Swarm Cluster.
* [Docker image](https://hub.docker.com/repository/docker/alfespa17/devopsagent).
* [Dockerfile](https://github.com/AzSwarm/DevOpsAgent)

The Cmdlet has the following parameters:

| Parameter                 | Description                                              |  
|---------------------------|----------------------------------------------------------|
| ResourceGroupName         | Azure resource group to create the resources             |    
| DevOpsOrganization        | AzureDevOps Organization URL                             |
| DevOpsPool                | AzureDevOps Pool name                                    |    
| DevOpsPat                 | AzureDevops Personal Access Token                        |
| DevOpsReplicas            | Azure DevOps Agent replicas to run                       |

* Example:
```powershell
 Add-SimpleSwarmDevOpsAgent -ResourceGroupName XXX -DevOpsOrganization XXX -DevOpsPool XXX -DevOpsPat XXX -DevOpsReplicas XXX
```