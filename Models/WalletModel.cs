using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtApp.Models
{
    public class WalletModel
    {
        //id VARCHAR(100), owned_by VARCHAR(100), status VARCHAR(20), enabled_at DATETIME, balance BIGINT
        public Guid id { get; set; }
        public Guid owned_by { get; set; }
        public string status { get; set; }
        public DateTime? enabled_at { get; set; }
        public DateTime? disabled_at { get; set; }
        public double balance { get; set; }
    }
}
