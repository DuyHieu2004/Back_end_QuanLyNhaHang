using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class MonAnsController : Controller
    {
        private readonly QLNhaHangContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

  
        public MonAnsController(QLNhaHangContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var monAns = _context.MonAns
                .Include(m => m.MaDanhMucNavigation) 
                .Include(m => m.HinhAnhMonAns);      
            return View(await monAns.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.MaDanhMucNavigation)
                .Include(m => m.HinhAnhMonAns)
                .FirstOrDefaultAsync(m => m.MaMonAn == id);

            if (monAn == null)
            {
                return NotFound();
            }

            return View(monAn);
        }


        public IActionResult Create()
        {
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMonAns, "MaDanhMuc", "TenDanhMuc");
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaMonAn,TenMonAn,Gia,TrangThai,MaDanhMuc,SoLuong")] MonAn monAn, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                _context.Add(monAn);
                await _context.SaveChangesAsync(); 

                if (images != null && images.Count > 0)
                {
              
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string monAnFolderPath = Path.Combine(webRootPath, "images", "monans", monAn.MaMonAn);
                    Directory.CreateDirectory(monAnFolderPath);

                    int imageOrder = 1;
                    foreach (var image in images)
                    {
                        
                        string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                        string extension = Path.GetExtension(image.FileName);
                        string uniqueFileName = $"{fileName}_{imageOrder++}{extension}";

                        string filePath = Path.Combine(monAnFolderPath, uniqueFileName);

                       
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var hinhAnhMonAn = new HinhAnhMonAn
                        {
                            MaMonAn = monAn.MaMonAn,
                            URLHinhAnh = $"images/monans/{monAn.MaMonAn}/{uniqueFileName}"
                        };
                        _context.Add(hinhAnhMonAn);
                    }
                    await _context.SaveChangesAsync(); 
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMonAns, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns.Include(m => m.HinhAnhMonAns).FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (monAn == null)
            {
                return NotFound();
            }
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMonAns, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaMonAn,TenMonAn,Gia,TrangThai,MaDanhMuc,SoLuong")] MonAn monAn, List<IFormFile> newImages, List<int> imagesToDelete)
        {
            if (id != monAn.MaMonAn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                  
                    if (imagesToDelete != null && imagesToDelete.Count > 0)
                    {
                        foreach (var imageId in imagesToDelete)
                        {
                            var imageRecord = await _context.HinhAnhMonAns.FindAsync(imageId);
                            if (imageRecord != null)
                            {
                             
                                string webRootPath = _webHostEnvironment.WebRootPath;
                                var oldImagePath = Path.Combine(webRootPath, imageRecord.URLHinhAnh.Replace("/", "\\"));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                             
                                _context.HinhAnhMonAns.Remove(imageRecord);
                            }
                        }
                    }

                  
                    if (newImages != null && newImages.Count > 0)
                    {
                        string webRootPath = _webHostEnvironment.WebRootPath;
                        string monAnFolderPath = Path.Combine(webRootPath, "images", "monans", monAn.MaMonAn);
                        Directory.CreateDirectory(monAnFolderPath);

                        int imageOrder = (await _context.HinhAnhMonAns.Where(h => h.MaMonAn == monAn.MaMonAn).CountAsync()) + 1;
                        foreach (var image in newImages)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(image.FileName);
                            string extension = Path.GetExtension(image.FileName);
                            string uniqueFileName = $"{fileName}_{imageOrder++}{extension}";
                            string filePath = Path.Combine(monAnFolderPath, uniqueFileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(fileStream);
                            }
                            var hinhAnhMonAn = new HinhAnhMonAn
                            {
                                MaMonAn = monAn.MaMonAn,
                                URLHinhAnh = $"images/monans/{monAn.MaMonAn}/{uniqueFileName}"
                            };
                            _context.Add(hinhAnhMonAn);
                        }
                    }

                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonAnExists(monAn.MaMonAn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDanhMuc"] = new SelectList(_context.DanhMucMonAns, "MaDanhMuc", "TenDanhMuc", monAn.MaDanhMuc);
            return View(monAn);
        }

        
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monAn = await _context.MonAns
                .Include(m => m.MaDanhMucNavigation)
                .FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (monAn == null)
            {
                return NotFound();
            }

            return View(monAn);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var monAn = await _context.MonAns.Include(m => m.HinhAnhMonAns).FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (monAn != null)
            {
                
                string webRootPath = _webHostEnvironment.WebRootPath;
                string monAnFolderPath = Path.Combine(webRootPath, "images", "monans", monAn.MaMonAn);
                if (Directory.Exists(monAnFolderPath))
                {
                    Directory.Delete(monAnFolderPath, true); 
                }

                
                _context.MonAns.Remove(monAn);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonAnExists(string id)
        {
            return _context.MonAns.Any(e => e.MaMonAn == id);
        }
    }
}