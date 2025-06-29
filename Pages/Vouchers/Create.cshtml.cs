using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;

namespace MiniAccountSystem.Pages.Vouchers
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _config;

        public CreateModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public VoucherInput Voucher { get; set; } = new();

        public List<Account> Accounts { get; set; }

        public void OnGet()
        {
            LoadAccounts();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadAccounts();
                return Page();
            }

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_SaveVoucher", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@VoucherType", Voucher.VoucherType);
            cmd.Parameters.AddWithValue("@VoucherDate", Voucher.VoucherDate);
            cmd.Parameters.AddWithValue("@ReferenceNo", Voucher.ReferenceNo);

            var tvp = new DataTable();
            tvp.Columns.Add("AccountId", typeof(int));
            tvp.Columns.Add("DebitAmount", typeof(decimal));
            tvp.Columns.Add("CreditAmount", typeof(decimal));

            foreach (var entry in Voucher.Entries)
            {
                if (entry.AccountId > 0)
                {
                    tvp.Rows.Add(entry.AccountId, entry.DebitAmount, entry.CreditAmount);
                }
            }

            var tvpParam = new SqlParameter("@Entries", SqlDbType.Structured)
            {
                TypeName = "TVP_VoucherEntry",
                Value = tvp
            };

            cmd.Parameters.Add(tvpParam);

            con.Open();
            cmd.ExecuteNonQuery();

            return RedirectToPage("/Index");
        }

        private void LoadAccounts()
        {
            Accounts = new List<Account>();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT AccountId, AccountName FROM ChartOfAccounts", con);
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Accounts.Add(new Account
                {
                    AccountId = (int)reader["AccountId"],
                    AccountName = reader["AccountName"].ToString()
                });
            }
        }

        public class VoucherInput
        {
            public string VoucherType { get; set; }
            public DateTime VoucherDate { get; set; }
            public string ReferenceNo { get; set; }
            public List<EntryInput> Entries { get; set; } = new();
        }

        public class EntryInput
        {
            public int AccountId { get; set; }
            public decimal DebitAmount { get; set; }
            public decimal CreditAmount { get; set; }
        }

        public class Account
        {
            public int AccountId { get; set; }
            public string AccountName { get; set; }
        }
    }
}
