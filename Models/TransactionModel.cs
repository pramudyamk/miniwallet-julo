using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JwtApp.Models
{
    public class TransactionModel
    {
        //id VARCHAR(100), deposited_by VARCHAR(100) null, withdrawn_by VARCHAR(100) null, status VARCHAR(20),
        //deposited_at DATETIME null, withdrawn_at DATETIME null, amount BIGINT, reference_id VARCHAR(100)
        public Guid id { get; set; }
        public Guid deposited_by { get; set; }
        public Guid withdrawn_by { get; set; }
        public string status { get; set; }
        public DateTime? deposited_at { get; set; }
        public DateTime? withdrawn_at { get; set; }
        public double amount { get; set; }
        public Guid reference_id { get; set; }
    }

}
