using System;
using System.Management.Automation;

namespace SimpleSwarm
{
    [Cmdlet("Add", "SimpleSwarmWorker")]
    public class AddSimpleClusterWorkerCmdletCommand : PSCmdlet
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
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
