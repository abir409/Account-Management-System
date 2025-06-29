using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MiniAccountSystem.Pages.Accounts
{
    public class ManageModel : PageModel
    {
        private readonly IConfiguration _config;

        public ManageModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public AccountInput Account { get; set; } = new();

        public class AccountInput
        {
            public int? AccountId { get; set; }
            public string AccountName { get; set; }
            public string AccountType { get; set; }
            public int? ParentId { get; set; }
        }

        public void OnGet(int? id)
        {
            if (id == null) return;

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT * FROM ChartOfAccounts WHERE AccountId = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Account = new AccountInput
                {
                    AccountId = (int)reader["AccountId"],
                    AccountName = reader["AccountName"].ToString(),
                    AccountType = reader["AccountType"].ToString(),
                    ParentId = reader["ParentId"] == DBNull.Value ? null : (int?)reader["ParentId"]
                };
            }
        }

        public IActionResult OnPost()
        {
            string action = Account.AccountId.HasValue ? "UPDATE" : "INSERT";

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@AccountId", (object?)Account.AccountId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountName", Account.AccountName);
            cmd.Parameters.AddWithValue("@ParentId", (object?)Account.ParentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AccountType", Account.AccountType);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }

        public IActionResult OnGetDelete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Action", "DELETE");
            cmd.Parameters.AddWithValue("@AccountId", id);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToPage("Index");
        }
    }
}
