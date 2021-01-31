using Microsoft.Azure.Cosmos.Table;

namespace SimpleSwarm.Tools
{
    class SimpleSwarmVM : TableEntity
    {
        public SimpleSwarmVM() { }

        public SimpleSwarmVM(string vmName, string role, string nicId, string diskId, string ipId, string vmId)
        {
            PartitionKey = role;
            RowKey = vmName;
            this.nicId = nicId;
            this.diskId = diskId;
            this.ipId = ipId;
            this.vmId = vmId;
        }

        public string vmId { get; set; }
        public string nicId { get; set; }
        public string diskId { get; set; }
        public string ipId { get; set; }
    }
}
