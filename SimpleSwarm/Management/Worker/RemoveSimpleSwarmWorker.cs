using System.Management.Automation;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Cosmos.Table;
using SimpleSwarm.Tools;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;


namespace SimpleSwarm.Management.Worker
{
    [Cmdlet("Remove", "SimpleSwarmWorker")]
    public class RemoveSimpleSwarmWorker : PSCmdlet
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
        public string WorkerName
        {
            get { return workerName; }
            set { workerName = value; }
        }
        private string workerName;

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
                        "Worker"
                    )
                );
            SimpleSwarmVM deleteWorker = null;

            foreach (SimpleSwarmVM worker in table.ExecuteQuery(query))
            {
               if(WorkerName != null && WorkerName.Equals(worker.RowKey))
               {
                    deleteWorker = worker;
                    break;
               }
               else
               {
                    deleteWorker = worker;
               }
            }

            //SEARCH MANAGER TO EXECUTE COMMAND
            progress = new ProgressRecord(1, "SimpleSwarm Manager Information", "Searching SimpleSwarm Information...");
            WriteProgress(progress);
            IAvailabilitySet availabilitySet = azure.AvailabilitySets.GetByResourceGroup(resourceGroupName, "azswarm-manager-avset");
            var vmIds = availabilitySet.VirtualMachineIds;
            String vmId = "";
            foreach (var id in vmIds)
            {
                vmId = id;
            }
            var vm = azure.VirtualMachines.GetById(vmId);

            //DRAIN AND REMOVE NODE FROM CLUSTER
            vm.RunShellScript(
                new List<string>() {
                    "sudo docker node update --availability drain $workerDrain; sudo docker node remove $workerRemove"
                },
                new List<RunCommandInputParameter>()
                {
                        new RunCommandInputParameter("workerDrain", deleteWorker.RowKey),
                        new RunCommandInputParameter("workerRemove", deleteWorker.RowKey)
                });

            azure.VirtualMachines.DeleteById(deleteWorker.vmId);
            azure.NetworkInterfaces.DeleteById(deleteWorker.nicId);
            azure.Disks.DeleteById(deleteWorker.diskId);

            TableOperation deleteOperation = TableOperation.Delete(deleteWorker);
            TableResult result = table.Execute(deleteOperation);

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    } 
}
