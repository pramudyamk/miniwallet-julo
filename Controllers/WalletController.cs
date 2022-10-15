using JwtApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace JwtApp.Controllers
{ 
    public class WalletController : ControllerBase
    {
        private IConfiguration _config;

        public WalletController(IConfiguration config)
        {
            _config = config;
        }

        public WalletModel GetWallet(Guid customer_xid)
        {
            WalletModel wallet = new WalletModel();
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./SqliteDB.db";

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var selectCmd = connection.CreateCommand();
                    selectCmd.CommandText = "SELECT * FROM Wallets where owned_by = '" + customer_xid + "'";
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetGuid(0);
                            var owned_by = reader.GetGuid(1);
                            var status = reader.GetString(2);
                            DateTime? enabled_at = null;
                            DateTime? disabled_at = null;
                            if (!reader.IsDBNull(3))
                            { 
                                enabled_at = reader.GetDateTime(3);
                            }
                            if (!reader.IsDBNull(4) && reader.GetString(4) != "")
                            {
                                disabled_at = reader.GetDateTime(4);
                            }
                            var balance = reader.GetDouble(5);

                            wallet.id = id;
                            wallet.owned_by = owned_by;
                            wallet.status = status;
                            wallet.enabled_at = enabled_at;
                            wallet.disabled_at = disabled_at;
                            wallet.balance = balance;
                        }
                    }
                }
            }

            return wallet;
        }
         
        [Authorize]
        [HttpGet("api/wallet")]
        public IActionResult GetWallet()
        {
            var currentUser = GetCurrentUser();

            var wallet = GetWallet(currentUser.customer_xid); 
            var result = new { wallet = wallet };

            return new JsonResult(new { status = "success", data = result });
        }


        public static void InsertWallet(Guid customer_xid)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./SqliteDB.db";

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCmd = connection.CreateCommand();
                    //id VARCHAR(100), owned_by VARCHAR(100), status VARCHAR(20), enabled_at DATETIME, disabled_at DATETIME, balance BIGINT
                    var guid = Guid.NewGuid();
                    insertCmd.CommandText = String.Format("INSERT INTO Wallets VALUES('{0}','{1}', 'disabled', null, null, 0 )", guid, customer_xid);
                    insertCmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public static void UpdateWallet(Guid customer_xid, WalletModel wallet)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./SqliteDB.db";

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var cmd = connection.CreateCommand();
                    //id VARCHAR(100), owned_by VARCHAR(100), status VARCHAR(20), enabled_at DATETIME, balance BIGINT
                    cmd.CommandText = String.Format("UPDATE Wallets SET status = '{0}', enabled_at='{1}', disabled_at='{2}', balance='{3}' " +
                        "where owned_by = '{4}'", wallet.status, wallet.enabled_at, wallet.disabled_at, wallet.balance, customer_xid);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        [Authorize]
        [HttpPost("api/wallet")]
        public IActionResult EnableWallet()
        {
            var currentUser = GetCurrentUser();

            var wallet = GetWallet(currentUser.customer_xid);
            wallet.status = "enabled";
            wallet.enabled_at = DateTime.Now;

            UpdateWallet(currentUser.customer_xid, wallet);

            var result = new { wallet = GetWallet(currentUser.customer_xid) };

            return new JsonResult(new { status = "success", data = result });
        }

        [Authorize]
        [HttpPost("api/wallet/deposits")]
        public IActionResult Deposits(TransactionModel transaction)
        {
            var currentUser = GetCurrentUser();
             
            var deposit = InsertDeposit(currentUser.customer_xid, transaction);
             
            var result = new { deposit = deposit };

            return new JsonResult(new { status = "success", data = result });
        }

        [Authorize]
        [HttpPost("api/wallet/withdrawals")]
        public IActionResult Withdrawals(TransactionModel transaction)
        {
            var currentUser = GetCurrentUser();
             
            var withdrawal = InsertWithdrawal(currentUser.customer_xid, transaction);
             
            var result = new { withdrawal = withdrawal };

            return new JsonResult(new { status = "success", data = result });
        }


        [Authorize]
        [HttpPatch("api/wallet")]
        public IActionResult DisableWallet()
        {
            var currentUser = GetCurrentUser();

            var wallet = GetWallet(currentUser.customer_xid);
            wallet.status = "disabled";
            wallet.disabled_at = DateTime.Now;

            UpdateWallet(currentUser.customer_xid, wallet);

            var result = new { wallet = GetWallet(currentUser.customer_xid) };

            return new JsonResult(new { status = "success", data = result });
        }


        public static dynamic InsertDeposit(Guid customer_xid, TransactionModel transmodel)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./SqliteDB.db";
            dynamic transModel = new System.Dynamic.ExpandoObject();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCmd = connection.CreateCommand();
                    var guid = Guid.NewGuid();
                    var refID = transmodel.reference_id;
                    var date = DateTime.Now;
                    insertCmd.CommandText = String.Format("INSERT INTO Transactions(id, deposited_by, status, deposited_at, amount, reference_id) " +
                        "VALUES('{0}','{1}', 'success', '{2}', '{3}', '{4}' )", guid, customer_xid, date, transmodel.amount, refID);
                    insertCmd.ExecuteNonQuery(); 

                    var cmd = connection.CreateCommand(); 
                    cmd.CommandText = String.Format("UPDATE Wallets SET balance= balance + {0} " +
                        "where owned_by = '{1}'", transmodel.amount, customer_xid);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();

                    transModel.id = guid;
                    transModel.deposited_by = customer_xid;
                    transModel.status = "success";
                    transModel.deposited_at = date;
                    transModel.amount = transmodel.amount;
                    transModel.reference_id = refID; 
                }
            }

            return transModel;
        }

        public static dynamic InsertWithdrawal(Guid customer_xid, TransactionModel transmodel)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = "./SqliteDB.db";
            dynamic transModel = new System.Dynamic.ExpandoObject();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCmd = connection.CreateCommand();
                    var guid = Guid.NewGuid();
                    var refID = transmodel.reference_id;
                    var date = DateTime.Now;
                    insertCmd.CommandText = String.Format("INSERT INTO Transactions(id, withdrawn_by, status, withdrawn_at, amount, reference_id) " +
                        "VALUES('{0}','{1}', 'success', '{2}', '{3}', '{4}' )", guid, customer_xid, date, transmodel.amount, refID);
                    insertCmd.ExecuteNonQuery(); 

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("UPDATE Wallets SET balance= balance - {0} " +
                        "where owned_by = '{1}'", transmodel.amount, customer_xid);
                    cmd.ExecuteNonQuery();
                    transaction.Commit();

                    transModel.id = guid;
                    transModel.withdrawn_by = customer_xid;
                    transModel.status = "success";
                    transModel.withdrawn_at = date;
                    transModel.amount = transmodel.amount;
                    transModel.reference_id = refID;
                }
            }

            return transModel;
        }

        public UserLogin GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;

                return new UserLogin
                {
                    customer_xid = Guid.Parse(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value)
                };
            }
            return null;
        }
    }
}
