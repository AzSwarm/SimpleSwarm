using System.Management.Automation;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Management.Compute.Fluent;
using System.Text;

namespace SimpleSwarm
{
    [Cmdlet("Add", "SimpleSwarmManager")]
    public class AddSimpleClusterManagerCmdletCommand : PSCmdlet
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
            WriteObject("Processing!");
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            var network = new List<INetwork>(azure.Networks.ListByResourceGroup(resourceGroupName))[0];
            var keyVault = new List<IVault>(azure.Vaults.ListByResourceGroup(resourceGroupName))[0];
            var identity = azure.Identities.GetByResourceGroup(resourceGroupName, "AzSwarmManager");

            String randomSuffix = new Random().Next(1, 1000).ToString();
            //Will refactor this later. Github Origin https://github.com/AzSwarm/Cloud-Init/blob/main/distribution/ubuntu/18.04/cloud-init-manager.yml
            String cloudInitBase64 = "I2Nsb3VkLWNvbmZpZwpwYWNrYWdlX3VwZGF0ZTogdHJ1ZQoKcmVzb2x2X2NvbmY6CiAgICBuYW1lc2VydmVyczoKICAgICAgLSAnOC44LjguOCcKCnBhY2thZ2VzOgogIC0gYXB0LXRyYW5zcG9ydC1odHRwcwogIC0gY2EtY2VydGlmaWNhdGVzCiAgLSBjdXJsCiAgLSBnbnVwZy1hZ2VudAogIC0gc29mdHdhcmUtcHJvcGVydGllcy1jb21tb24KICAtIGxzYi1yZWxlYXNlCiAgLSBnbnVwZwoKcnVuY21kOgogICMgSW5zdGFsbCBEb2NrZXIgQ0UgIAogIC0gY3VybCAtZnNTTCBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1L2dwZyB8IGFwdC1rZXkgYWRkIC0KICAtIGFkZC1hcHQtcmVwb3NpdG9yeSAiZGViIFthcmNoPWFtZDY0XSBodHRwczovL2Rvd25sb2FkLmRvY2tlci5jb20vbGludXgvdWJ1bnR1ICQobHNiX3JlbGVhc2UgLWNzKSBzdGFibGUiCiAgLSBhcHQtZ2V0IHVwZGF0ZSAteQogIC0gYXB0LWdldCBpbnN0YWxsIC15IGRvY2tlci1jZSBkb2NrZXItY2UtY2xpIGNvbnRhaW5lcmQuaW8KICAtIHN5c3RlbWN0bCBzdGFydCBkb2NrZXIKICAtIHN5c3RlbWN0bCBlbmFibGUgZG9ja2VyCiAgIyBJbnN0YWxsIE1pY3Jvc29mdCBSZXBvc2l0b3J5CiAgLSB3Z2V0IC1xIGh0dHBzOi8vcGFja2FnZXMubWljcm9zb2Z0LmNvbS9jb25maWcvdWJ1bnR1LzE4LjA0L3BhY2thZ2VzLW1pY3Jvc29mdC1wcm9kLmRlYgogIC0gZHBrZyAtaSBwYWNrYWdlcy1taWNyb3NvZnQtcHJvZC5kZWIKICAtIGFwdC1nZXQgdXBkYXRlIC15CiAgLSBhZGQtYXB0LXJlcG9zaXRvcnkgdW5pdmVyc2UKICAjIEluc3RhbGwgUG93ZXJzaGVsbAogIC0gYXB0LWdldCBpbnN0YWxsIC15IHBvd2Vyc2hlbGwtbHRzCiAgIyBJbnN0YWxsIEF6dXJlIENsaQogIC0gY3VybCAtc0wgaHR0cHM6Ly9ha2EubXMvSW5zdGFsbEF6dXJlQ0xJRGViIHwgc3VkbyBiYXNoCiAgIyBJbml0aWFsaXplIGEgc3dhcm0gbWFuYWdlcgogIC0gZG9ja2VyIHN3YXJtIGluaXQgLS1hZHZlcnRpc2UtYWRkciBldGgwOjIzNzcKICAjIFNhdmUgRG9ja2VyIFN3YXJtIEtleXMgaW5zaWRlIEF6dXJlIEtleSBWYXVsdAogIC0gYXogbG9naW4gLS1pZGVudGl0eSAtdSAvc3Vic2NyaXB0aW9ucy88c3Vic2NyaXB0aW9uSWQ+L3Jlc291cmNlZ3JvdXBzLzxyZXNvdXJjZUdyb3VwTmFtZT4vcHJvdmlkZXJzL01pY3Jvc29mdC5NYW5hZ2VkSWRlbnRpdHkvdXNlckFzc2lnbmVkSWRlbnRpdGllcy88dXNlckFzc2lnbmVkSWRlbnRpdHlOYW1lPiAtLWFsbG93LW5vLXN1YnNjcmlwdGlvbnMKICAtIGF6IGtleXZhdWx0IHNlY3JldCBzZXQgLS12YXVsdC1uYW1lIDxrZXlWYXVsdE5hbWU+IC0tbmFtZSAibWFuYWdlci1rZXkiIC0tdmFsdWUgIiQoZG9ja2VyIHN3YXJtIGpvaW4tdG9rZW4gbWFuYWdlciAtLXF1aWV0KSIKICAtIGF6IGtleXZhdWx0IHNlY3JldCBzZXQgLS12YXVsdC1uYW1lIDxLZXlWYXVsdE5hbWU+IC0tbmFtZSAid29ya2VyLWtleSIgLS12YWx1ZSAiJChkb2NrZXIgc3dhcm0gam9pbi10b2tlbiB3b3JrZXIgLS1xdWlldCki";
            String cloudInit = Encoding.UTF8.GetString(Convert.FromBase64String(cloudInitBase64));
            cloudInit = cloudInit.Replace("<subscriptionId>", credentials.DefaultSubscriptionId);
            cloudInit = cloudInit.Replace("<resourceGroupName>", resourceGroupName);
            cloudInit = cloudInit.Replace("<userAssignedIdentityName>", identity.Name);
            cloudInit = cloudInit.Replace("<KeyVaultName>", keyVault.Name);
            cloudInitBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(cloudInit));

            IVirtualMachine windowsVM = azure.VirtualMachines.Define("azswarmmanager" + randomSuffix)
                    .WithRegion(location)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithExistingPrimaryNetwork(network)
                    .WithSubnet("AzSwarmSubnet")
                    .WithPrimaryPrivateIPAddressDynamic()
                    .WithNewPrimaryPublicIPAddress("azswarmmanagerip"+randomSuffix)
                    .WithLatestLinuxImage("Canonical", "UbuntuServer", "18.04-LTS")
                    .WithRootUsername(adminUsername)
                    .WithRootPassword(adminPassword)
                    .WithCustomData(cloudInitBase64)
                    .WithExistingUserAssignedManagedServiceIdentity(identity)
                    .WithSize(Microsoft.Azure.Management.Compute.Fluent.Models.VirtualMachineSizeTypes.StandardB1s)
                    .WithNewAvailabilitySet("azswarm-avset" + randomSuffix)
                    .Create();

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
