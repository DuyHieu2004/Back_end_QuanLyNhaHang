//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using QuanLyNhaHang.Models;

//namespace QuanLyNhaHang.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class KhachHangController : ControllerBase
//    {
//        private readonly QLNhaHangContext _context;

//        public KhachHangController(QLNhaHangContext context)
//        {
//            _context = context;
//        }

//        [HttpGet("TimKiem/{sdt}")]
//        public IActionResult GetKhachHangBySDT(string sdt)
//        {
//            // 1. Tìm khách hàng trong database
//            var khachHang = _context.KhachHangs
//                                    .FirstOrDefault(k => k.SoDienThoai == sdt);

//            // 2. Xử lý kết quả trả về
//            if (khachHang != null)
//            {
//                // Tìm thấy -> Trả về thông tin
//                return Ok(new
//                {
//                    Success = true,
//                    Message = "Đã tìm thấy khách hàng!",
//                    Data = new
//                    {
//                        MaKhachHang = khachHang.MaKhachHang,
//                        HoTen = khachHang.HoTen, // Hoặc TenKhachHang tùy database bạn
//                        Email = khachHang.Email,
//                        SoLanAn = khachHang.SoLanAnTichLuy // Trả về cái này để hiển thị cho ngầu
//                    }
//                });
//            }
//            else
//            {
//                // Không tìm thấy -> Báo về để Front-end cho nhập mới
//                return Ok(new
//                {
//                    Success = false,
//                    Message = "Khách hàng mới",
//                    Data = (object)null
//                });
//            }
//        }

//    }
//}
