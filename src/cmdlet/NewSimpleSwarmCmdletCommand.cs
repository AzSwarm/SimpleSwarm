using System;
using System.Management.Automation;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Msi.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Management.Compute.Fluent;

namespace SimpleSwarm
{
    [Cmdlet("New", "SimpleSwarmCluster")]
    public class NewSimpleClusterCmdletCommand : PSCmdlet
    {
        // Parameters
        [Parameter(Mandatory = true)]
        public string ResourceGroupName
        {
            get { return resourceGroupName; }
            set { resourceGroupName = value; }
        }
        private string resourceGroupName;

        [Parameter( Mandatory = true)]
        public string Location
        {
            get { return location; }
            set { location = value; }
        }
        private string location;

        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            ProgressRecord progress = new ProgressRecord(1, "SimpleSwarm Setup", "Connecting to Azure...");
            WriteProgress(progress);
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            
            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            String randomSuffix = new Random().Next(1, 10000).ToString();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Resource Group...");
            WriteProgress(progress);
            IResourceGroup resourceGroup = azure.ResourceGroups
                .Define(resourceGroupName)
                .WithRegion(location)
                .Create();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Swarm Manager Identity...");
            WriteProgress(progress);
            IIdentity identityManager = azure.Identities
                .Define("AzSwarmManager")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .Create();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Swarm Worker Identity...");
            WriteProgress(progress);
            IIdentity identityWorker = azure.Identities
                .Define("AzSwarmWorker")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .Create();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Key Vault...");
            WriteProgress(progress);
            IVault keyVault = azure.Vaults
                .Define("azswarmkv-" + randomSuffix)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .WithEmptyAccessPolicy().Create();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Updating Key Vault Swarm Manager Access Policy...");
            WriteProgress(progress);
            keyVault.Update()
                .DefineAccessPolicy()
                .ForObjectId(identityManager.PrincipalId)
                .AllowSecretAllPermissions()
                .Attach()
                .Apply();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Updating Key Vault Swarm Worker Access Policy...");
            WriteProgress(progress);
            keyVault.Update()
                .DefineAccessPolicy()
                .ForObjectId(identityWorker.PrincipalId)
                .AllowSecretPermissions(SecretPermissions.Get)
                .AllowSecretPermissions(SecretPermissions.List)
                .Attach()
                .Apply();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Virtual Network...");
            WriteProgress(progress);
            INetwork network = azure.Networks
                .Define("AzSwarmNetwork" + randomSuffix)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("AzSwarmSubnet", "10.0.0.0/24")
                .Create();


            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Storage Account...");
            WriteProgress(progress);
            IStorageAccount storage = azure.StorageAccounts
                .Define("azswarmst" + randomSuffix)
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .WithAccessFromAllNetworks()
                .WithAccessFromAzureServices()
                .WithGeneralPurposeAccountKindV2()
                .WithOnlyHttpsTraffic()
                .WithSku(StorageAccountSkuType.Standard_LRS)
                .Create ();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Storage Account Table...");
            WriteProgress(progress);
            var storageAccountAccessKeys = storage.GetKeys();

            string storageConnectionString = "DefaultEndpointsProtocol=https" 
                + ";AccountName=" + storage.Name
                + ";AccountKey=" + storageAccountAccessKeys[0].Value
                + ";EndpointSuffix=core.windows.net";

            var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference("SimpleSwarmSetup");
            table.CreateIfNotExists();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Manager Availability Set...");
            WriteProgress(progress);
            IAvailabilitySet availabilitySetManager = azure.AvailabilitySets.Define("azswarm-manager-avset")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .WithFaultDomainCount(3)
                .WithUpdateDomainCount(5)
                .Create();

            progress = new ProgressRecord(1, "SimpleSwarm Setup", "Creating Manager Availability Set...");
            WriteProgress(progress);
            IAvailabilitySet availabilitySetWorker = azure.AvailabilitySets.Define("azswarm-worker-avset")
                .WithRegion(location)
                .WithExistingResourceGroup(resourceGroup)
                .WithFaultDomainCount(3)
                .WithUpdateDomainCount(5)
                .Create();

             WriteVerbose("SimpleSwarm Setup Completed");
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
