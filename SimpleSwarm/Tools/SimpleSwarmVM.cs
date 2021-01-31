using Microsoft.Azure.Cosmos.Table;

namespace SimpleSwarm.Tools
{
    class SimpleSwarmVM : TableEntity
    {
        public SimpleSwarmVM() { }

        public SimpleSwarmVM(string vmName, string role, string nic, string disk, string ip)
        {
            PartitionKey = role;
            RowKey = vmName;
            this.nic = nic;
            this.disk = disk;
            this.ip = ip;
        }

        public string nic { get; set; }
        public string disk { get; set; }
        public string ip { get; set; }
    }
}
