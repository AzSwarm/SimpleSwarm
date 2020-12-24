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
using SimpleSwarm.Tools;
using System.Collections.Generic;
using Microsoft.Azure.Management.Compute.Fluent.Models;

namespace SimpleSwarm.Management.DevOps
{
    [Cmdlet("Add", "SimpleSwarmDevOpsAgent")]
    public class SimpleSwarmDevOpsAgent : PSCmdlet
    {
        // Parameters
        [Parameter(Mandatory = true)]
        public string ResourceGroupName
        {
            get { return resourceGroupName; }
            set { resourceGroupName = value; }
        }
        private string resourceGroupName;

        [Parameter(Mandatory = true)]
        public string DevOpsOrganization
        {
            get { return devOpsOrganization; }
            set { devOpsOrganization = value; }
        }
        private string devOpsOrganization;

        [Parameter(Mandatory = true)]
        public string DevOpsPool
        {
            get { return devOpsPool; }
            set { devOpsPool = value; }
        }
        private string devOpsPool;

        [Parameter(Mandatory = true)]
        public string DevOpsPat
        {
            get { return devOpsPat; }
            set { devOpsPat = value; }
        }
        private string devOpsPat;

        [Parameter(Mandatory = true)]
        public string DevOpsReplicas
        {
            get { return devOpsReplicas; }
            set { devOpsReplicas = value; }
        }
        private string devOpsReplicas;

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

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Searching SimpleSwarm Information...");
            WriteProgress(progress);
            IAvailabilitySet availabilitySet = azure.AvailabilitySets.GetByResourceGroup(resourceGroupName, "azswarm-manager-avset");
            var vmIds = availabilitySet.VirtualMachineIds;
            String vmId = "";
            foreach (var id in vmIds)
            {
                vmId = id;
            }
            var vm = azure.VirtualMachines.GetById(vmId);


            vm.RunShellScript(
                new List<string>() {
                    "sudo docker service create -d -e AZP_URL=$azpUrl -e AZP_TOKEN=$azpToken -e AZP_POOL=$azpPool --replicas $replicas --name dockeragent alfespa17/devopsagent:v1"
                },
                new List<RunCommandInputParameter>()
                {
                        new RunCommandInputParameter("azpUrl", devOpsOrganization),
                        new RunCommandInputParameter("azpToken", devOpsPat),
                        new RunCommandInputParameter("azpPool", devOpsPool),
                        new RunCommandInputParameter("replicas", devOpsReplicas)
                }); 
        }


        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
