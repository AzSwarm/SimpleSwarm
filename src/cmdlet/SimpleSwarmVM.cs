using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace SimpleSwarm
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
