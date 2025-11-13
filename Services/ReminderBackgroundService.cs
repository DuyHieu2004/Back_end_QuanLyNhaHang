using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Nhớ đổi namespace cho đúng với project của bạn
namespace QuanLyNhaHang.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly IConfiguration _configuration; // 1. Khai báo biến đọc cấu hình

        // 2. Tiêm vào Constructor
        public ReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🤖 Robot nhắc nhở đã khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    // --- CẤU HÌNH GIỜ GỬI (Ví dụ: 8 giờ sáng) ---
                    if (now.Hour == 8)
                    {
                        _logger.LogInformation($"⏰ Đã đến 8h sáng ({now})! Bắt đầu quét đơn hàng để nhắc nhở...");

                        // Vì Background Service là Singleton (Sống mãi), còn DbContext là Scoped (Sống theo request)
                        // Nên phải tạo Scope thủ công để lấy DbContext ra dùng.
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<QuanLyNhaHang.Models.QLNhaHangContext>();
                            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                            // 1. Logic tìm đơn hàng (trong vòng 24h tới)
                            var thoiGianQuetDen = now.AddHours(24);

                            var donHangs = await context.DonHangs
                                .Include(dh => dh.MaKhachHangNavigation)
                                .Where(dh =>
                                    (dh.MaTrangThaiDonHang == "CHO_XAC_NHAN" || dh.MaTrangThaiDonHang == "DA_XAC_NHAN") &&
                                    dh.ThoiGianBatDau > now &&
                                    dh.ThoiGianBatDau <= thoiGianQuetDen
                                ).ToListAsync(stoppingToken);

                            int countEmail = 0;
                            int countSMS = 0;

                            // 2. Duyệt danh sách và gửi thông báo
                            foreach (var dh in donHangs)
                            {
                                // Lấy thông tin liên hệ (Ưu tiên người đi ăn thực tế nếu có)
                                var email = dh.MaKhachHangNavigation.Email;
                                var sdt = dh.SDTNguoiDat ?? dh.MaKhachHangNavigation.SoDienThoai;
                                var ten = dh.TenNguoiDat ?? dh.MaKhachHangNavigation.HoTen;

                                // 3. LẤY URL TỪ FILE CẤU HÌNH (Thay vì hardcode)
                                string baseUrl = _configuration["AppBaseUrl"];

                                // Nếu quên cấu hình thì fallback về localhost
                                if (string.IsNullOrEmpty(baseUrl)) baseUrl = "http://localhost:5555";

                                string linkHuy = $"{baseUrl}/api/BookingHistory/quick-cancel/{dh.MaDonHang}";

                                // TRƯỜNG HỢP A: CÓ EMAIL -> GỬI MAIL
                                if (!string.IsNullOrEmpty(email))
                                {
                                    try
                                    {
                                        // Gọi hàm gửi mail nhắc nhở (đã viết trong SendGridEmailService)
                                        await emailService.SendReminderEmailAsync(
                                            email,
                                            ten,
                                            dh.ThoiGianBatDau ?? DateTime.Now,
                                            "", // Link xác nhận (nếu cần thì thêm)
                                            linkHuy
                                        );
                                        countEmail++;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"Lỗi gửi mail cho đơn {dh.MaDonHang}: {ex.Message}");
                                    }
                                }
                                // TRƯỜNG HỢP B: KHÔNG EMAIL -> GIẢ LẬP SMS
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"[SMS MOCK - AUTOMATED] Gửi đến {sdt}: Chào {ten}, nhắc bạn có lịch đặt bàn lúc {dh.ThoiGianBatDau:HH:mm}.");
                                    Console.ResetColor();
                                    countSMS++;
                                }
                            }

                            _logger.LogInformation($"✅ Đã chạy xong tiến trình. Gửi {countEmail} email, Giả lập {countSMS} SMS.");
                        }

                        // Sau khi chạy xong, ngủ 1 tiếng (hoặc lâu hơn) để tránh việc nó quét đi quét lại trong khung giờ 8h
                        // Ví dụ: 8h00 chạy xong, ngủ tới 9h00 dậy check -> lúc đó now.Hour là 9 -> bỏ qua.
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                    else
                    {
                        // Nếu chưa tới 8h sáng, cứ 1 phút mở mắt ra check giờ 1 lần
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔴 Lỗi nghiêm trọng trong Robot nhắc nhở.");
                    // Nếu lỗi, chờ 5 phút rồi thử lại để không spam log
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}