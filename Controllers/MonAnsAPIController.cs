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
    public async Task<ActionResult<IEnumerable<MonAnDetailDTO>>> GetMonAns(
        [FromQuery] string? maDanhMuc,
        [FromQuery] string? searchString)
    {
        // 1. Query cơ bản (Chưa Include vội để lọc cho nhanh)
        var query = _context.MonAns.AsQueryable();

        // 2. Lọc theo Danh Mục (Nếu có)
        if (!string.IsNullOrEmpty(maDanhMuc) && maDanhMuc != "All" && maDanhMuc != "Tất cả")
        {
            // Tìm theo Mã hoặc Tên danh mục (để FE gửi tên cũng được)
            query = query.Where(m => m.MaDanhMuc == maDanhMuc || m.MaDanhMucNavigation.TenDanhMuc == maDanhMuc);
        }

        // 3. Tìm kiếm (Search)
        if (!string.IsNullOrEmpty(searchString))
        {
            string searchLower = searchString.ToLower();
            query = query.Where(m => m.TenMonAn.ToLower().Contains(searchLower));
        }

        // 4. Chỉ lấy món đang hiển thị
        query = query.Where(m => m.IsShow == true);

        // 5. Include dữ liệu liên quan (Sau khi đã lọc xong)
        var monAns = await query
             .Include(m => m.HinhAnhMonAns)
             .Include(m => m.MaDanhMucNavigation)
             .Include(m => m.ChiTietMonAns)
                 .ThenInclude(ct => ct.CongThucNauAns)
                     .ThenInclude(cta => cta.MaPhienBanNavigation) // Để lấy tên phiên bản (Size)
             .AsSplitQuery() // Tăng tốc độ query
             .ToListAsync();

        // 6. Map sang DTO (Logic map của bạn tui giữ lại nhưng làm gọn hơn)
        var dtos = monAns.Select(m => {

            // Gom tất cả công thức (phiên bản) từ các chi tiết món ăn
            var allCongThucs = m.ChiTietMonAns
                .SelectMany(ct => ct.CongThucNauAns)
                .Where(cta => cta.MaPhienBanNavigation != null) // Bỏ qua lỗi null
                .ToList();

            // Group by Phiên Bản để loại bỏ trùng lặp (nếu có)
            var phienBanDTOs = allCongThucs
                .GroupBy(cta => cta.MaPhienBan)
                .Select(g => new PhienBanMonAnDetailDTO
                {
                    MaPhienBan = g.Key,
                    TenPhienBan = g.First().MaPhienBanNavigation.TenPhienBan, // Tên size (Nhỏ, Lớn...)
                    Gia = g.First().Gia, // Giá tiền
                    ThuTu = g.First().MaPhienBanNavigation.ThuTu
                })
                .OrderBy(pb => pb.ThuTu) // Sắp xếp size (Nhỏ -> Lớn)
                .ToList();

            return new MonAnDetailDTO
            {
                MaMonAn = m.MaMonAn,
                TenMonAn = m.TenMonAn,
                MaDanhMuc = m.MaDanhMuc,
                TenDanhMuc = m.MaDanhMucNavigation?.TenDanhMuc,
                IsShow = m.IsShow?? true,

                // Lấy hình ảnh đầu tiên làm đại diện
                HinhAnhMonAns = m.HinhAnhMonAns.Select(h => new HinhAnhDTO
                {
                    Id = h.Id,
                    URLHinhAnh = h.UrlhinhAnh
                }).ToList(),

                PhienBanMonAns = phienBanDTOs
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
           IsShow = monAn.IsShow?? true,
           HinhAnhMonAns = monAn.HinhAnhMonAns.Select(h => new HinhAnhDTO
           {
               Id = h.Id,
               URLHinhAnh = h.UrlhinhAnh
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
                      UrlhinhAnh = finalUrl
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
             string maPhienBan;
             PhienBanMonAn? phienBan = null;

             if (!string.IsNullOrWhiteSpace(pbDto.MaPhienBan))
             {
                 maPhienBan = pbDto.MaPhienBan;
                 phienBan = await _context.PhienBanMonAns
                     .FirstOrDefaultAsync(p => p.MaPhienBan == maPhienBan);

                 if (phienBan == null)
                 {
                     return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn với mã {maPhienBan}." });
                 }
             }
             else
             {
                 maPhienBan = "PB" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + pbIndex.ToString("D3");
                 phienBan = new PhienBanMonAn
                 {
                     MaPhienBan = maPhienBan,
                     TenPhienBan = pbDto.TenPhienBan,
                     MaTrangThai = pbDto.MaTrangThai,
                     ThuTu = pbDto.ThuTu
                 };

                 _context.PhienBanMonAns.Add(phienBan);
             }

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

  [HttpPut("{maMonAn}")]
  public async Task<ActionResult> UpdateMonAn(string maMonAn, [FromBody] CreateMonAnDTO dto)
  {
      if (!ModelState.IsValid)
      {
          return BadRequest(ModelState);
      }

      await using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
          var monAn = await _context.MonAns
              .Include(m => m.HinhAnhMonAns)
              .Include(m => m.ChiTietMonAns)
                  .ThenInclude(ct => ct.CongThucNauAns)
                      .ThenInclude(cta => cta.ChiTietCongThucs)
              .FirstOrDefaultAsync(m => m.MaMonAn == maMonAn);

            if (monAn == null)
            {
                return NotFound();
            }

            monAn.TenMonAn = dto.TenMonAn;
            monAn.MaDanhMuc = dto.MaDanhMuc;
            monAn.IsShow = dto.IsShow;
            _context.MonAns.Update(monAn);

            if (dto.HinhAnhUrls != null)
            {
                var existingImages = monAn.HinhAnhMonAns.ToList();
                _context.HinhAnhMonAns.RemoveRange(existingImages);

                string webRootPath = _webHostEnvironment.WebRootPath;
                int imageOrder = 1;
                foreach (var url in dto.HinhAnhUrls)
                {
                    if (string.IsNullOrWhiteSpace(url)) continue;
                    string finalUrl = url;

                    if (url.StartsWith("images/monans/temp/", StringComparison.OrdinalIgnoreCase))
                    {
                        string relativePath = url.Replace("images/monans/temp/", "");
                        string tempFilePath = Path.Combine(webRootPath, "images", "monans", "temp", relativePath);
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            string fileName = Path.GetFileNameWithoutExtension(relativePath);
                            string extension = Path.GetExtension(relativePath);
                            string newFileName = $"{fileName}_{imageOrder}{extension}";
                            string monAnFolderPath = Path.Combine(webRootPath, "images", "monans", maMonAn);
                            Directory.CreateDirectory(monAnFolderPath);
                            string newFilePath = Path.Combine(monAnFolderPath, newFileName);
                            if (System.IO.File.Exists(newFilePath))
                            {
                                System.IO.File.Delete(newFilePath);
                            }
                            System.IO.File.Move(tempFilePath, newFilePath);
                            finalUrl = $"images/monans/{maMonAn}/{newFileName}";
                        }
                    }
                    else if (finalUrl.StartsWith("/"))
                    {
                        finalUrl = finalUrl.TrimStart('/');
                    }

                    var hinhAnh = new HinhAnhMonAn
                    {
                        MaMonAn = maMonAn,
                        UrlhinhAnh = finalUrl
                    };
                    _context.HinhAnhMonAns.Add(hinhAnh);
                    imageOrder++;
                }
            }

            foreach (var chiTiet in monAn.ChiTietMonAns.ToList())
            {
                foreach (var congThuc in chiTiet.CongThucNauAns.ToList())
                {
                    _context.ChiTietCongThucs.RemoveRange(congThuc.ChiTietCongThucs);
                }
                _context.CongThucNauAns.RemoveRange(chiTiet.CongThucNauAns);
            }
            _context.ChiTietMonAns.RemoveRange(monAn.ChiTietMonAns);

            var maChiTiet = "CT" + DateTime.Now.ToString("yyMMddHHmmssfff");
            var chiTietMonAn = new ChiTietMonAn
            {
                MaCt = maChiTiet,
                TenCt = "Chi tiết 1",
                MaMonAn = maMonAn
            };
            _context.ChiTietMonAns.Add(chiTietMonAn);

            int pbIndex = 0;
            foreach (var pbDto in dto.PhienBanMonAns)
            {
                pbIndex++;
                string maPhienBan;
                PhienBanMonAn? phienBan = null;

                if (!string.IsNullOrWhiteSpace(pbDto.MaPhienBan))
                {
                    maPhienBan = pbDto.MaPhienBan;
                    phienBan = await _context.PhienBanMonAns.FirstOrDefaultAsync(p => p.MaPhienBan == maPhienBan);
                    if (phienBan == null)
                    {
                        return BadRequest(new { message = $"Không tìm thấy phiên bản món ăn với mã {maPhienBan}." });
                    }
                    phienBan.TenPhienBan = pbDto.TenPhienBan;
                    phienBan.MaTrangThai = pbDto.MaTrangThai ?? phienBan.MaTrangThai;
                    phienBan.ThuTu = pbDto.ThuTu;
                    _context.PhienBanMonAns.Update(phienBan);
                }
                else
                {
                    maPhienBan = "PB" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + pbIndex.ToString("D3");
                    phienBan = new PhienBanMonAn
                    {
                        MaPhienBan = maPhienBan,
                        TenPhienBan = pbDto.TenPhienBan,
                        MaTrangThai = pbDto.MaTrangThai ?? "CON_HANG",
                        ThuTu = pbDto.ThuTu
                    };
                    _context.PhienBanMonAns.Add(phienBan);
                }

                int ctIndex = 0;
                foreach (var ctDto in pbDto.CongThucNauAns)
                {
                    ctIndex++;
                    var maCongThuc = "CT" + DateTime.Now.ToString("yyMMddHHmmss") + "_" + pbIndex.ToString("D3") + "_" + ctIndex.ToString("D3");
                    var congThuc = new CongThucNauAn
                    {
                        MaCongThuc = maCongThuc,
                        MaCt = maChiTiet,
                        MaPhienBan = phienBan.MaPhienBan,
                        Gia = pbDto.Gia
                    };
                    _context.CongThucNauAns.Add(congThuc);

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
            await transaction.CommitAsync();

            var updated = await GetMonAn(maMonAn);
            if (updated.Result != null)
            {
                return updated.Result;
            }
            return Ok(updated.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Lỗi khi cập nhật món ăn: " + ex.Message });
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

