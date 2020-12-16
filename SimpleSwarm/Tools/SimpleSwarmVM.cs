using Microsoft.Azure.Cosmos.Table;

namespace SimpleSwarm.Tools
{
    class SimpleSwarmVM : TableEntity
    {
        public SimpleSwarmVM() { }

        public SimpleSwarmVM(string vmName, string status, string role)
        {
            PartitionKey = role;
            RowKey = vmName;
            this.status = status;
        }

        public string status { get; set; }
    }
}
