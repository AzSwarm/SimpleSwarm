using System;
using System.Collections.Generic;
using SimpleSwarm.Tools;
using System.Management.Automation;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Cosmos.Table;

namespace SimpleSwarm.SimpleSwarm.Management.Manager
{
    [Cmdlet("Remove", "SimpleSwarmManager")]
    public class RemoveSimpleSwarmManager : PSCmdlet
    {
        // Parameters
        [Parameter(Mandatory = true)]
        public string ResourceGroupName
        {
            get { return resourceGroupName; }
            set { resourceGroupName = value; }
        }
        private string resourceGroupName;

        // Parameters
        [Parameter(Mandatory = false)]
        public string ManagerName
        {
            get { return managerName; }
            set { managerName = value; }
        }
        private string managerName;

        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            ProgressRecord progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Connecting to Azure...");
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            var storageAccount = new List<IStorageAccount>(azure.StorageAccounts.ListByResourceGroup(resourceGroupName))[0];

            string storageConnectionString = "DefaultEndpointsProtocol=https"
                + ";AccountName=" + storageAccount.Name
                + ";AccountKey=" + storageAccount.GetKeys()[0].Value
                + ";EndpointSuffix=core.windows.net";

            var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference("SimpleSwarmSetup");

            TableQuery<SimpleSwarmVM> query = new TableQuery<SimpleSwarmVM>()
                .Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.GreaterThanOrEqual,
                        "Manager"
                    )
                );
            SimpleSwarmVM deleteManager = null;

            foreach (SimpleSwarmVM worker in table.ExecuteQuery(query))
            {
                if (ManagerName != null && ManagerName.Equals(worker.RowKey))
                {
                    deleteManager = worker;
                    break;
                }
                else
                {
                    deleteManager = worker;
                }
            }

            azure.VirtualMachines.DeleteById(deleteManager.vmId);
            azure.NetworkInterfaces.DeleteById(deleteManager.nicId);
            azure.Disks.DeleteById(deleteManager.diskId);

            if(deleteManager.ipId != null)
            {
                azure.PublicIPAddresses.DeleteById(deleteManager.ipId);
            }

            TableOperation deleteOperation = TableOperation.Delete(deleteManager);
            TableResult result = table.Execute(deleteOperation);

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
