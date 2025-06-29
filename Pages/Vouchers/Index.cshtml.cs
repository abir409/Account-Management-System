using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;

namespace MiniAccountSystem.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;

        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        public List<VoucherViewModel> Vouchers { get; set; }

        public void OnGet()
        {
            LoadVouchers();
        }

        public IActionResult OnGetExport()
        {
            LoadVouchers();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Vouchers");

            worksheet.Cell(1, 1).Value = "Voucher ID";
            worksheet.Cell(1, 2).Value = "Type";
            worksheet.Cell(1, 3).Value = "Date";
            worksheet.Cell(1, 4).Value = "Reference No";

            int row = 2;
            foreach (var v in Vouchers)
            {
                worksheet.Cell(row, 1).Value = v.VoucherId;
                worksheet.Cell(row, 2).Value = v.VoucherType;
                worksheet.Cell(row, 3).Value = v.VoucherDate.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 4).Value = v.ReferenceNo;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Vouchers.xlsx");
        }

        private void LoadVouchers()
        {
            Vouchers = new List<VoucherViewModel>();

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT VoucherId, VoucherType, VoucherDate, ReferenceNo FROM Vouchers", con);

            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Vouchers.Add(new VoucherViewModel
                {
                    VoucherId = (int)reader["VoucherId"],
                    VoucherType = reader["VoucherType"].ToString(),
                    VoucherDate = (DateTime)reader["VoucherDate"],
                    ReferenceNo = reader["ReferenceNo"].ToString()
                });
            }
        }

        public class VoucherViewModel
        {
            public int VoucherId { get; set; }
            public string VoucherType { get; set; }
            public DateTime VoucherDate { get; set; }
            public string ReferenceNo { get; set; }
        }
    }
}
