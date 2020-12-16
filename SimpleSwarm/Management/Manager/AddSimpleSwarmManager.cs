using System.Management.Automation;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.Compute.Fluent;
using System.Text;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Azure.Cosmos.Table;
using SimpleSwarm.Tools;

namespace SimpleSwarm.Management.Worker
{
    [Cmdlet("Add", "SimpleSwarmManager")]
    public class AddSimpleClusterManager : PSCmdlet
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
        public string Location
        {
            get { return location; }
            set { location = value; }
        }
        private string location;

        [Parameter(Mandatory = true)]
        public string AdminUsername
        {
            get { return adminUsername; }
            set { adminUsername = value; }
        }
        private string adminUsername;

        [Parameter(Mandatory = true)]
        public string AdminPassword
        {
            get { return adminPassword; }
            set { adminPassword = value; }
        }
        private string adminPassword;
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            ProgressRecord progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Connecting to Azure...");
            WriteProgress(progress);
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Searching network information...");
            WriteProgress(progress);
            var network = new List<INetwork>(azure.Networks.ListByResourceGroup(resourceGroupName))[0];

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Searching key vault information...");
            WriteProgress(progress);
            var keyVault = new List<IVault>(azure.Vaults.ListByResourceGroup(resourceGroupName))[0];

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Searching identity information...");
            WriteProgress(progress);
            var identity = azure.Identities.GetByResourceGroup(resourceGroupName, "AzSwarmManager");

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Searching availability set...");
            WriteProgress(progress);
            IAvailabilitySet availabilitySet = azure.AvailabilitySets.GetByResourceGroup(resourceGroupName, "azswarm-manager-avset");
            var vmIds = availabilitySet.VirtualMachineIds;

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Generating configuration file...");
            WriteProgress(progress);

            String randomSuffix = "";
            String cloudInit = "";
            String cloudInitBase64 = "";
            INetworkInterface networkInterface = null;
            if (vmIds.Count == 0)
            {
                randomSuffix = new Random().Next(1, 1000).ToString();
                //Will refactor this later. Github Origin https://github.com/AzSwarm/Cloud-Init/blob/main/distribution/ubuntu/18.04/cloud-init-manager.yml
                cloudInitBase64 = "I2Nsb3VkLWNvbmZpZwpwYWNrYWdlX3VwZGF0ZTogdHJ1ZQoKcmVzb2x2X2NvbmY6CiAgICBuYW1lc2VydmVyczoKICAgICAgLSAnOC44LjguOCcKCnBhY2thZ2VzOgogIC0gYXB0LXRyYW5zcG9ydC1odHRwcwogIC0gY2EtY2VydGlmaWNhdGVzCiAgLSBjdXJsCiAgLSBnbnVwZy1hZ2VudAogIC0gc29mdHdhcmUtcHJvcGVydGllcy1jb21tb24KICAtIGxzYi1yZWxlYXNlCiAgLSBnbnVwZwoKcnVuY21kOgogICMgSW5zdGFsbCBEb2NrZXIgQ0UgIAogIC0gY3VybCAtZnNTTCBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1L2dwZyB8IGFwdC1rZXkgYWRkIC0KICAtIGFkZC1hcHQtcmVwb3NpdG9yeSAiZGViIFthcmNoPWFtZDY0XSBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1ICQobHNiX3JlbGVhc2UgLWNzKSBzdGFibGUiCiAgLSBhcHQtZ2V0IHVwZGF0ZSAteQogIC0gYXB0LWdldCBpbnN0YWxsIC15IGRvY2tlci1jZSBkb2NrZXItY2UtY2xpIGNvbnRhaW5lcmQuaW8KICAtIHN5c3RlbWN0bCBzdGFydCBkb2NrZXIKICAtIHN5c3RlbWN0bCBlbmFibGUgZG9ja2VyCiAgIyBJbnN0YWxsIE1pY3Jvc29mdCBSZXBvc2l0b3J5CiAgLSB3Z2V0IC1xIGh0dHBzOi8vcGFja2FnZXMubWljcm9zb2Z0LmNvbS9jb25maWcvdWJ1bnR1LzE4LjA0L3BhY2thZ2VzLW1pY3Jvc29mdC1wcm9kLmRlYgogIC0gZHBrZyAtaSBwYWNrYWdlcy1taWNyb3NvZnQtcHJvZC5kZWIKICAtIGFwdC1nZXQgdXBkYXRlIC15CiAgLSBhZGQtYXB0LXJlcG9zaXRvcnkgdW5pdmVyc2UKICAjIEluc3RhbGwgUG93ZXJzaGVsbAogIC0gYXB0LWdldCBpbnN0YWxsIC15IHBvd2Vyc2hlbGwtbHRzCiAgIyBJbnN0YWxsIEF6dXJlIENsaQogIC0gY3VybCAtc0wgaHR0cHM6Ly9ha2EubXMvSW5zdGFsbEF6dXJlQ0xJRGViIHwgc3VkbyBiYXNoCiAgIyBJbml0aWFsaXplIGEgc3dhcm0gbWFuYWdlcgogIC0gZG9ja2VyIHN3YXJtIGluaXQgLS1hZHZlcnRpc2UtYWRkciBldGgwOjIzNzcKICAjIFNhdmUgRG9ja2VyIFN3YXJtIEtleXMgaW5zaWRlIEF6dXJlIEtleSBWYXVsdAogIC0gYXogbG9naW4gLS1pZGVudGl0eSAtdSAvc3Vic2NyaXB0aW9ucy88c3Vic2NyaXB0aW9uSWQ+L3Jlc291cmNlZ3JvdXBzLzxyZXNvdXJjZUdyb3VwTmFtZT4vcHJvdmlkZXJzL01pY3Jvc29mdC5NYW5hZ2VkSWRlbnRpdHkvdXNlckFzc2lnbmVkSWRlbnRpdGllcy88dXNlckFzc2lnbmVkSWRlbnRpdHlOYW1lPiAtLWFsbG93LW5vLXN1YnNjcmlwdGlvbnMKICAtIGF6IGtleXZhdWx0IHNlY3JldCBzZXQgLS12YXVsdC1uYW1lIDxrZXlWYXVsdE5hbWU+IC0tbmFtZSAibWFuYWdlci1rZXkiIC0tdmFsdWUgIiQoZG9ja2VyIHN3YXJtIGpvaW4tdG9rZW4gbWFuYWdlciAtLXF1aWV0KSIKICAtIGF6IGtleXZhdWx0IHNlY3JldCBzZXQgLS12YXVsdC1uYW1lIDxrZXlWYXVsdE5hbWU+IC0tbmFtZSAid29ya2VyLWtleSIgLS12YWx1ZSAiJChkb2NrZXIgc3dhcm0gam9pbi10b2tlbiB3b3JrZXIgLS1xdWlldCki";
                cloudInit = Encoding.UTF8.GetString(Convert.FromBase64String(cloudInitBase64));
                cloudInit = cloudInit.Replace("<subscriptionId>", credentials.DefaultSubscriptionId);
                cloudInit = cloudInit.Replace("<resourceGroupName>", resourceGroupName);
                cloudInit = cloudInit.Replace("<userAssignedIdentityName>", identity.Name);
                cloudInit = cloudInit.Replace("<keyVaultName>", keyVault.Name);
                cloudInitBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cloudInit));

                networkInterface = azure.NetworkInterfaces.Define("azswarm-nic-" + randomSuffix)
                            .WithRegion(location)
                            .WithNewResourceGroup(resourceGroupName)
                            .WithExistingPrimaryNetwork(network)
                            .WithSubnet("AzSwarmSubnet")
                            .WithPrimaryPrivateIPAddressDynamic()
                            .WithNewPrimaryPublicIPAddress()
                            .Create();
            }
            else
            {
                String swarmManagerIp = "";
                foreach (var id in vmIds)
                {
                    var vm = azure.VirtualMachines.GetById(id);
                    swarmManagerIp = vm.GetPrimaryNetworkInterface().PrimaryPrivateIP;
                }
                
                randomSuffix = new Random().Next(1, 1000).ToString();
                //Will refactor this later. Github Origin https://github.com/AzSwarm/Cloud-Init/blob/main/distribution/ubuntu/18.04/cloud-init-add-manager.yml
                cloudInitBase64 = "I2Nsb3VkLWNvbmZpZwpwYWNrYWdlX3VwZGF0ZTogdHJ1ZQoKcmVzb2x2X2NvbmY6CiAgICBuYW1lc2VydmVyczoKICAgICAgLSAnOC44LjguOCcKCnBhY2thZ2VzOgogIC0gYXB0LXRyYW5zcG9ydC1odHRwcwogIC0gY2EtY2VydGlmaWNhdGVzCiAgLSBjdXJsCiAgLSBnbnVwZy1hZ2VudAogIC0gc29mdHdhcmUtcHJvcGVydGllcy1jb21tb24KICAtIGxzYi1yZWxlYXNlCiAgLSBnbnVwZwoKcnVuY21kOgogICMgSW5zdGFsbCBEb2NrZXIgQ0UgIAogIC0gY3VybCAtZnNTTCBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1L2dwZyB8IGFwdC1rZXkgYWRkIC0KICAtIGFkZC1hcHQtcmVwb3NpdG9yeSAiZGViIFthcmNoPWFtZDY0XSBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1ICQobHNiX3JlbGVhc2UgLWNzKSBzdGFibGUiCiAgLSBhcHQtZ2V0IHVwZGF0ZSAteQogIC0gYXB0LWdldCBpbnN0YWxsIC15IGRvY2tlci1jZSBkb2NrZXItY2UtY2xpIGNvbnRhaW5lcmQuaW8KICAtIHN5c3RlbWN0bCBzdGFydCBkb2NrZXIKICAtIHN5c3RlbWN0bCBlbmFibGUgZG9ja2VyCiAgIyBJbnN0YWxsIE1pY3Jvc29mdCBSZXBvc2l0b3J5CiAgLSB3Z2V0IC1xIGh0dHBzOi8vcGFja2FnZXMubWljcm9zb2Z0LmNvbS9jb25maWcvdWJ1bnR1LzE4LjA0L3BhY2thZ2VzLW1pY3Jvc29mdC1wcm9kLmRlYgogIC0gZHBrZyAtaSBwYWNrYWdlcy1taWNyb3NvZnQtcHJvZC5kZWIKICAtIGFwdC1nZXQgdXBkYXRlIC15CiAgLSBhZGQtYXB0LXJlcG9zaXRvcnkgdW5pdmVyc2UKICAjIEluc3RhbGwgUG93ZXJzaGVsbAogIC0gYXB0LWdldCBpbnN0YWxsIC15IHBvd2Vyc2hlbGwtbHRzCiAgIyBJbnN0YWxsIEF6dXJlIENsaQogIC0gY3VybCAtc0wgaHR0cHM6Ly9ha2EubXMvSW5zdGFsbEF6dXJlQ0xJRGViIHwgc3VkbyBiYXNoCiAgIyBTYXZlIERvY2tlciBTd2FybSBLZXlzIGluc2lkZSBBenVyZSBLZXkgVmF1bHQKICAtIGF6IGxvZ2luIC0taWRlbnRpdHkgLXUgL3N1YnNjcmlwdGlvbnMvPHN1YnNjcmlwdGlvbklkPi9yZXNvdXJjZWdyb3Vwcy88cmVzb3VyY2VHcm91cE5hbWU+L3Byb3ZpZGVycy9NaWNyb3NvZnQuTWFuYWdlZElkZW50aXR5L3VzZXJBc3NpZ25lZElkZW50aXRpZXMvPHVzZXJBc3NpZ25lZElkZW50aXR5TmFtZT4gLS1hbGxvdy1uby1zdWJzY3JpcHRpb25zCiAgIyBJbml0aWFsaXplIGEgU3dhcm0gV29ya2VyCiAgLSBkb2NrZXIgc3dhcm0gam9pbiAtLXRva2VuICIkKGF6IGtleXZhdWx0IHNlY3JldCBzaG93IC0tbmFtZSAibWFuYWdlci1rZXkiIC0tdmF1bHQtbmFtZSA8a2V5VmF1bHROYW1lPiAtLXF1ZXJ5IHZhbHVlIC0tb3V0cHV0IHRzdikiIDxzd2FybU1hbmFnZXJJcD46MjM3Nw==";
                cloudInit = Encoding.UTF8.GetString(Convert.FromBase64String(cloudInitBase64));
                cloudInit = cloudInit.Replace("<subscriptionId>", credentials.DefaultSubscriptionId);
                cloudInit = cloudInit.Replace("<resourceGroupName>", resourceGroupName);
                cloudInit = cloudInit.Replace("<userAssignedIdentityName>", identity.Name);
                cloudInit = cloudInit.Replace("<keyVaultName>", keyVault.Name);
                cloudInit = cloudInit.Replace("<swarmManagerIp>", swarmManagerIp);
                cloudInitBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cloudInit));

                networkInterface = azure.NetworkInterfaces.Define("azswarm-nic-" + randomSuffix)
                            .WithRegion(location)
                            .WithNewResourceGroup(resourceGroupName)
                            .WithExistingPrimaryNetwork(network)
                            .WithSubnet("AzSwarmSubnet")
                            .WithPrimaryPrivateIPAddressDynamic()
                            .Create();
            }


            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Creating virtual machine...");
            WriteProgress(progress);
            IVirtualMachine linuxVM = azure.VirtualMachines.Define("azswarmmanager" + randomSuffix)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithExistingPrimaryNetworkInterface(networkInterface)
                    .WithLatestLinuxImage("Canonical", "UbuntuServer", "18.04-LTS")
                    .WithRootUsername(adminUsername)
                    .WithRootPassword(adminPassword)
                    .WithCustomData(cloudInitBase64)
                    .WithExistingUserAssignedManagedServiceIdentity(identity)
                    .WithSize(Microsoft.Azure.Management.Compute.Fluent.Models.VirtualMachineSizeTypes.StandardB1s)
                    .WithExistingAvailabilitySet(availabilitySet)
                    .Create();

            progress = new ProgressRecord(1, "SimpleSwarm Manager Setup", "Updating SimpleSwarm status...");
            WriteProgress(progress);
            var storageAccount = new List<IStorageAccount>(azure.StorageAccounts.ListByResourceGroup(resourceGroupName))[0];

            string storageConnectionString = "DefaultEndpointsProtocol=https"
                + ";AccountName=" + storageAccount.Name
                + ";AccountKey=" + storageAccount.GetKeys()[0].Value
                + ";EndpointSuffix=core.windows.net";

            var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference("SimpleSwarmSetup");


            // Create the InsertOrReplace table operation
            SimpleSwarmVM simpleSwarmVM = new SimpleSwarmVM(linuxVM.Name, "initializing", "Manager");
            TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(simpleSwarmVM);

            // Execute the operation.
            TableResult result = table.Execute(insertOrMergeOperation);
            SimpleSwarmVM insertedSimpleSwarmVM = result.Result as SimpleSwarmVM;


        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
