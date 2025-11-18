using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.Dtos;
using System.Data;

namespace QuanLyNhaHang.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class StatisticsController : ControllerBase
	{
		private readonly QLNhaHangContext _context;

		public StatisticsController(QLNhaHangContext context)
		{
			_context = context;
		}

		public class DoanhThuThangDto
		{
			public int Thang { get; set; }
			public decimal DoanhThu { get; set; }
		}

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] string timeRange = "TODAY")
        {
            string safeTimeRange = timeRange?.ToUpper() ?? "TODAY";
            if (safeTimeRange != "TODAY" && safeTimeRange != "WEEK" && safeTimeRange != "MONTH")
            {
                safeTimeRange = "TODAY";
            }

            var result = new DashboardStatDto();

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.GetDashboardStats";
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.CreateParameter();
            p.ParameterName = "@TimeRange";
            p.Value = safeTimeRange;
            cmd.Parameters.Add(p);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
             
                result.TongDoanhThu = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                result.SoDonHoanThanh = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                result.SoBanPhucVu = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                result.TongKhachHang = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
            }

            return Ok(result);
        }

        [HttpGet("doanh-thu-theo-thang")]
		public async Task<IActionResult> GetDoanhThuTheoThang([FromQuery] int nam)
		{
			var list = new List<DoanhThuThangDto>();

			await using var conn = _context.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open)
			{
				await conn.OpenAsync();
			}

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.GetDoanhThuTheoThang";
			cmd.CommandType = CommandType.StoredProcedure;
			var p = cmd.CreateParameter();
			p.ParameterName = "@Nam";
			p.Value = nam;
			cmd.Parameters.Add(p);

			await using var reader = await cmd.ExecuteReaderAsync();
			while (await reader.ReadAsync())
			{
				list.Add(new DoanhThuThangDto
				{
					Thang = reader.GetInt32(0),
					DoanhThu = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
				});
			}

			return Ok(list);
		}
	}
}

