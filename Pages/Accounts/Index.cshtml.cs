using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MiniAccountSystem.Pages.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;

        public List<Account> AccountTree { get; set; } = new();

        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        public void OnGet()
        {
            var allAccounts = new List<Account>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts", con);
            con.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                allAccounts.Add(new Account
                {
                    AccountId = (int)reader["AccountId"],
                    AccountName = reader["AccountName"].ToString(),
                    AccountType = reader["AccountType"].ToString(),
                    ParentId = reader["ParentId"] == DBNull.Value ? null : (int?)reader["ParentId"]
                });
            }

            // Build tree: top-level accounts
            AccountTree = allAccounts
                .Where(a => a.ParentId == null)
                .ToList();

            foreach (var root in AccountTree)
            {
                AddChildren(root, allAccounts);
            }
        }

        private void AddChildren(Account parent, List<Account> all)
        {
            parent.Children = all
                .Where(a => a.ParentId == parent.AccountId)
                .ToList();

            foreach (var child in parent.Children)
            {
                AddChildren(child, all); // recursive
            }
        }

        public class Account
        {
            public int AccountId { get; set; }
            public string AccountName { get; set; }
            public string AccountType { get; set; }
            public int? ParentId { get; set; }

            public List<Account> Children { get; set; } = new();
        }
    }
}
