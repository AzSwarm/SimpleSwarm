using System;
using System.Management.Automation;

namespace SimpleSwarm
{
    [Cmdlet("New", "SimpleSwarm")]
    public class NewSimpleClusterCmdletCommand : PSCmdlet
    {
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
