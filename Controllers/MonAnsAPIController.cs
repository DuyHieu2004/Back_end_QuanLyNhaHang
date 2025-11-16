using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using QuanLyNhaHang.Models.DTO;

[Route("api/[controller]")]
[ApiController]
public class MonAnsAPIController : ControllerBase
{
   private readonly QLNhaHangContext _context;
   private readonly IWebHostEnvironment _webHostEnvironment;

   public MonAnsAPIController(QLNhaHangContext context, IWebHostEnvironment webHostEnvironment)
   {
       _context = context;
       _webHostEnvironment = webHostEnvironment;
   }


   [HttpGet]
   public async Task<ActionResult<IEnumerable<MonAnDetailDTO>>> GetMonAns([FromQuery] string? maDanhMuc, [FromQuery] string? searchString)
   {

       var query = _context.MonAns
                          .Include(m => m.HinhAnhMonAns)
                           .Include(m => m.MaDanhMucNavigation)
                           .Include(m => m.ChiTietMonAns)
                               .ThenInclude(ct => ct.CongThucNauAns)
                                   .ThenInclude(cta => cta.MaPhienBanNavigation)
                           .Include(m => m.ChiTietMonAns)
                               .ThenInclude(ct => ct.CongThucNauAns)
                                   .ThenInclude(cta => cta.ChiTietCongThucs)
                                       .ThenInclude(ctct => ctct.MaNguyenLieuNavigation)
                           .Where(m => m.IsShow == true)
                           .AsSplitQuery()
                           .AsQueryable();


       if (!string.IsNullOrEmpty(maDanhMuc))
       {
           query = query.Where(m => m.MaDanhMuc == maDanhMuc);
       }


       if (!string.IsNullOrEmpty(searchString))
       {
           query = query.Where(m => m.TenMonAn.Contains(searchString));
       }


       var monAns = await query.ToListAsync();
        
       // Map sang DTO để tránh circular reference
       var dtos = monAns.Select(m => {
           // Lấy tất cả phiên bản từ các công thức
           var phienBanDict = new Dictionary<string, PhienBanMonAnDetailDTO>();
           
           foreach (var chiTiet in m.ChiTietMonAns)
           {
               foreach (var congThuc in chiTiet.CongThucNauAns)
               {
                   var phienBan = congThuc.MaPhienBanNavigation;
                   if (phienBan == null) continue;
                   
                   if (!phienBanDict.ContainsKey(phienBan.MaPhienBan))
                   {
                       phienBanDict[phienBan.MaPhienBan] = new PhienBanMonAnDetailDTO
                       {
                           MaPhienBan = phienBan.MaPhienBan,
                           TenPhienBan = phienBan.TenPhienBan,
                           Gia = congThuc.Gia, // Lấy giá từ công thức đầu tiên
                           ThuTu = phienBan.ThuTu,
                           CongThucNauAns = new List<CongThucNauAnDetailDTO>()
                       };
                   }
                   
                   // Thêm nguyên liệu từ ChiTietCongThuc
                   foreach (var chiTietCongThuc in congThuc.ChiTietCongThucs)
                   {
                       phienBanDict[phienBan.MaPhienBan].CongThucNauAns.Add(new CongThucNauAnDetailDTO
                       {
                           MaCongThuc = congThuc.MaCongThuc,
                           MaNguyenLieu = chiTietCongThuc.MaNguyenLieu,
                           TenNguyenLieu = chiTietCongThuc.MaNguyenLieuNavigation?.TenNguyenLieu ?? "",
                           DonViTinh = chiTietCongThuc.MaNguyenLieuNavigation?.DonViTinh,
                           SoLuongCanDung = chiTietCongThuc.SoLuongCanDung
                       });
                   }
               }
           }
           
           return new MonAnDetailDTO
           {
               MaMonAn = m.MaMonAn,
               TenMonAn = m.TenMonAn,
               MaDanhMuc = m.MaDanhMuc,
               TenDanhMuc = m.MaDanhMucNavigation?.TenDanhMuc,
               IsShow = m.IsShow,
               HinhAnhMonAns = m.HinhAnhMonAns.Select(h => new HinhAnhDTO
               {
                   Id = h.Id,
                   URLHinhAnh = h.URLHinhAnh
               }).ToList(),
               PhienBanMonAns = phienBanDict.Values.OrderBy(pb => pb.ThuTu ?? 0).ToList()
           };
       }).ToList();
        
       return Ok(dtos);
   }


   [HttpGet("{id}")]
   public async Task<ActionResult<MonAnDetailDTO>> GetMonAn(string id)
   {
       var monAn = await _context.MonAns
                                 .Include(m => m.HinhAnhMonAns)
                                 .Include(m => m.MaDanhMucNavigation)
                                 .Include(m => m.ChiTietMonAns)
                                     .ThenInclude(ct => ct.CongThucNauAns)
                                         .ThenInclude(cta => cta.MaPhienBanNavigation)
                                 .Include(m => m.ChiTietMonAns)
                                     .ThenInclude(ct => ct.CongThucNauAns)
                                         .ThenInclude(cta => cta.ChiTietCongThucs)
                                             .ThenInclude(ctct => ctct.MaNguyenLieuNavigation)
                                 .Where(m => m.IsShow == true)
                                 .AsSplitQuery()
                                 .FirstOrDefaultAsync(m => m.MaMonAn == id);

       if (monAn == null)
       {
           return NotFound();
       }

       // Lấy tất cả phiên bản từ các công thức
       var phienBanDict = new Dictionary<string, PhienBanMonAnDetailDTO>();
       
       foreach (var chiTiet in monAn.ChiTietMonAns)
       {
           foreach (var congThuc in chiTiet.CongThucNauAns)
           {
               var phienBan = congThuc.MaPhienBanNavigation;
               if (phienBan == null) continue;
               
               if (!phienBanDict.ContainsKey(phienBan.MaPhienBan))
               {
                   phienBanDict[phienBan.MaPhienBan] = new PhienBanMonAnDetailDTO
                   {
                       MaPhienBan = phienBan.MaPhienBan,
                       TenPhienBan = phienBan.TenPhienBan,
                       Gia = congThuc.Gia, // Lấy giá từ công thức đầu tiên
                       MaTrangThai = phienBan.MaTrangThai,
                       TenTrangThai = "", // Không có navigation property nên để trống
                       IsShow = true, // Mặc định true vì model không có field này
                       ThuTu = phienBan.ThuTu,
                       CongThucNauAns = new List<CongThucNauAnDetailDTO>()
                   };
               }
               
               // Thêm nguyên liệu từ ChiTietCongThuc
               foreach (var chiTietCongThuc in congThuc.ChiTietCongThucs)
               {
                   phienBanDict[phienBan.MaPhienBan].CongThucNauAns.Add(new CongThucNauAnDetailDTO
                   {
                       MaCongThuc = congThuc.MaCongThuc,
                       MaNguyenLieu = chiTietCongThuc.MaNguyenLieu,
                       TenNguyenLieu = chiTietCongThuc.MaNguyenLieuNavigation?.TenNguyenLieu ?? "",
                       DonViTinh = chiTietCongThuc.MaNguyenLieuNavigation?.DonViTinh,
                       SoLuongCanDung = chiTietCongThuc.SoLuongCanDung
                   });
               }
           }
       }

       // Map sang DTO để tránh circular reference
       var dto = new MonAnDetailDTO
       {
           MaMonAn = monAn.MaMonAn,
           TenMonAn = monAn.TenMonAn,
           MaDanhMuc = monAn.MaDanhMuc,
           TenDanhMuc = monAn.MaDanhMucNavigation?.TenDanhMuc,
           IsShow = monAn.IsShow,
           HinhAnhMonAns = monAn.HinhAnhMonAns.Select(h => new HinhAnhDTO
           {
               Id = h.Id,
               URLHinhAnh = h.URLHinhAnh
           }).ToList(),
           PhienBanMonAns = phienBanDict.Values.OrderBy(pb => pb.ThuTu ?? 0).ToList()
       };

       return Ok(dto);
   }

   [HttpPost]
   public async Task<ActionResult<MonAn>> CreateMonAn([FromBody] CreateMonAnDTO dto)
   {
       if (!ModelState.IsValid)
       {
           return BadRequest(ModelState);
       }

       try
       {
           // Tạo mã món ăn mới
           var maMonAn = "MA" + DateTime.Now.ToString("yyMMddHHmmss");

           // Tạo món ăn
           var monAn = new MonAn
           {
               MaMonAn = maMonAn,
               TenMonAn = dto.TenMonAn,
               MaDanhMuc = dto.MaDanhMuc,
               IsShow = dto.IsShow
           };

           _context.MonAns.Add(monAn);
           await _context.SaveChangesAsync(); // Lưu để có MaMonAn trước khi di chuyển ảnh

           // Tạo thư mục cho món ăn
           string webRootPath = _webHostEnvironment.WebRootPath;
           string monAnFolderPath = Path.Combine(webRootPath, "images", "monans", maMonAn);
           Directory.CreateDirectory(monAnFolderPath);

           // Thêm hình ảnh và di chuyển từ temp nếu cần
           if (dto.HinhAnhUrls != null && dto.HinhAnhUrls.Count > 0)
           {
               int imageOrder = 1;
               foreach (var url in dto.HinhAnhUrls)
               {
                   string finalUrl = url;
                    
                   // Nếu ảnh đang ở thư mục temp, di chuyển sang thư mục món ăn
                   if (url.StartsWith("images/monans/temp/"))
                   {
                       // Xử lý path đúng cách
                       string relativePath = url.Replace("images/monans/temp/", "");
                       string tempFilePath = Path.Combine(webRootPath, "images", "monans", "temp", relativePath);
                        
                       if (System.IO.File.Exists(tempFilePath))
                       {
                           string fileName = Path.GetFileName(tempFilePath);
                           string fileExtension = Path.GetExtension(fileName);
                           string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            
                           // Tạo tên file mới với số thứ tự
                           string newFileName = $"{fileNameWithoutExt}_{imageOrder}{fileExtension}";
                           string newFilePath = Path.Combine(monAnFolderPath, newFileName);
                            
                           // Di chuyển file
                           System.IO.File.Move(tempFilePath, newFilePath);
                            
                           // Cập nhật URL
                           finalUrl = $"images/monans/{maMonAn}/{newFileName}";
                       }
                   }
                   else if (!url.Contains($"/{maMonAn}/"))
                   {
                       // Nếu URL không đúng format, cập nhật lại
                       string fileName = Path.GetFileName(url);
                       string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{imageOrder}{Path.GetExtension(fileName)}";
                       finalUrl = $"images/monans/{maMonAn}/{newFileName}";
                   }

                   var hinhAnh = new HinhAnhMonAn
                   {
                       MaMonAn = maMonAn,
                       URLHinhAnh = finalUrl
                   };
                   _context.HinhAnhMonAns.Add(hinhAnh);
                   imageOrder++;
               }
           }

           // Tạo ChiTietMonAn (ít nhất 1 chi tiết)
           var maChiTiet = "CT" + DateTime.Now.ToString("yyMMddHHmmss") + "_001";
           var chiTietMonAn = new ChiTietMonAn
           {
               MaCt = maChiTiet,
               TenCt = "Chi tiết 1",
               MaMonAn = maMonAn
           };
           _context.ChiTietMonAns.Add(chiTietMonAn);

           // Thêm các phiên bản món ăn
           int pbIndex = 0;
           foreach (var pbDto in dto.PhienBanMonAns)
           {
               pbIndex++;
               var maPhienBan = "PB" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + pbIndex.ToString("D3");
               var phienBan = new PhienBanMonAn
               {
                   MaPhienBan = maPhienBan,
                   TenPhienBan = pbDto.TenPhienBan,
                   MaTrangThai = pbDto.MaTrangThai,
                   ThuTu = pbDto.ThuTu
               };

               _context.PhienBanMonAns.Add(phienBan);

               // Thêm công thức nấu ăn cho mỗi phiên bản
               int ctIndex = 0;
               foreach (var ctDto in pbDto.CongThucNauAns)
               {
                   ctIndex++;
                   var maCongThuc = "CT" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + pbIndex.ToString("D3") + "_" + ctIndex.ToString("D3");
                   var congThuc = new CongThucNauAn
                   {
                       MaCongThuc = maCongThuc,
                       MaCt = maChiTiet, // Liên kết với ChiTietMonAn
                       MaPhienBan = maPhienBan,
                       Gia = pbDto.Gia // Giá nằm trong CongThucNauAn
                   };

                   _context.CongThucNauAns.Add(congThuc);

                   // Thêm chi tiết công thức (nguyên liệu)
                   var chiTietCongThuc = new ChiTietCongThuc
                   {
                       MaCongThuc = maCongThuc,
                       MaNguyenLieu = ctDto.MaNguyenLieu,
                       SoLuongCanDung = ctDto.SoLuongCanDung
                   };

                   _context.ChiTietCongThucs.Add(chiTietCongThuc);
               }
           }

           await _context.SaveChangesAsync();

           // Load lại với đầy đủ thông tin và map sang DTO
           var result = await GetMonAn(maMonAn);
           if (result.Result is OkObjectResult okResult && okResult.Value is MonAnDetailDTO resultDto)
           {
               return CreatedAtAction(nameof(GetMonAn), new { id = maMonAn }, resultDto);
           }
           return CreatedAtAction(nameof(GetMonAn), new { id = maMonAn }, new { MaMonAn = maMonAn });
       }
       catch (Exception ex)
       {
           return StatusCode(500, new { message = "Lỗi khi tạo món ăn: " + ex.Message });
       }
   }

   [HttpPost("upload-image")]
   public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string? maMonAn = null)
   {
       if (file == null || file.Length == 0)
       {
           return BadRequest(new { message = "Không có file được chọn." });
       }

       // Kiểm tra định dạng file
       var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
       var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
       if (!allowedExtensions.Contains(fileExtension))
       {
           return BadRequest(new { message = "Định dạng file không hợp lệ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp" });
       }

       // Kiểm tra kích thước file (tối đa 10MB)
       if (file.Length > 10 * 1024 * 1024)
       {
           return BadRequest(new { message = "File quá lớn. Kích thước tối đa là 10MB." });
       }

       try
       {
           string webRootPath = _webHostEnvironment.WebRootPath;
           string uploadsFolder;
           string imageUrl;

           // Nếu có maMonAn, upload vào thư mục của món ăn
           if (!string.IsNullOrEmpty(maMonAn))
           {
               uploadsFolder = Path.Combine(webRootPath, "images", "monans", maMonAn);
               Directory.CreateDirectory(uploadsFolder);

               // Tạo tên file với số thứ tự
               int imageOrder = Directory.GetFiles(uploadsFolder).Length + 1;
               string fileName = Path.GetFileNameWithoutExtension(file.FileName);
               string uniqueFileName = $"{fileName}_{imageOrder}{fileExtension}";
               string filePath = Path.Combine(uploadsFolder, uniqueFileName);

               // Lưu file
               using (var fileStream = new FileStream(filePath, FileMode.Create))
               {
                   await file.CopyToAsync(fileStream);
               }

               imageUrl = $"images/monans/{maMonAn}/{uniqueFileName}";
           }
           else
           {
               // Upload vào thư mục temp nếu chưa có maMonAn
               uploadsFolder = Path.Combine(webRootPath, "images", "monans", "temp");
               Directory.CreateDirectory(uploadsFolder);

               // Tạo tên file unique
               string fileName = Path.GetFileNameWithoutExtension(file.FileName);
               string uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
               string filePath = Path.Combine(uploadsFolder, uniqueFileName);

               // Lưu file
               using (var fileStream = new FileStream(filePath, FileMode.Create))
               {
                   await file.CopyToAsync(fileStream);
               }

               imageUrl = $"images/monans/temp/{uniqueFileName}";
           }

           return Ok(new { 
               message = "Upload ảnh thành công!",
               url = imageUrl 
           });
       }
       catch (Exception ex)
       {
           return StatusCode(500, new { message = "Lỗi khi upload ảnh: " + ex.Message });
       }
   }
}

