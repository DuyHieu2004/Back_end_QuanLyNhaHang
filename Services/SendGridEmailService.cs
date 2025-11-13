using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QuanLyNhaHang.Services; // Namespace của bạn

// Giữ nguyên tên class cũ để đỡ phải sửa code ở Controller và Program.cs
public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration config, ILogger<SendGridEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ========================================================================
    // HÀM GỬI MAIL CHÍNH (DÙNG MAILKIT - SMTP GMAIL)
    // ========================================================================
    private async Task SendEmailViaGmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var email = new MimeMessage();

        // Người gửi
        email.Sender = MailboxAddress.Parse(_config["MailSettings:Mail"]);
        email.From.Add(new MailboxAddress("Nhà Hàng DoAn", _config["MailSettings:Mail"]));

        // Người nhận
        email.To.Add(MailboxAddress.Parse(toEmail));

        // Tiêu đề & Nội dung
        email.Subject = subject;
        var builder = new BodyBuilder();
        builder.HtmlBody = htmlMessage;
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            // Kết nối tới Server Gmail
            await smtp.ConnectAsync(_config["MailSettings:Host"], int.Parse(_config["MailSettings:Port"]), SecureSocketOptions.StartTls);

            // Đăng nhập bằng App Password
            await smtp.AuthenticateAsync(_config["MailSettings:Mail"], _config["MailSettings:Password"]);

            // Gửi
            await smtp.SendAsync(email);

            _logger.LogInformation($"✅ Gửi mail thành công tới {toEmail}");
        }
        catch (Exception ex)
        {
            // In lỗi đỏ lòm ra console để bạn biết nếu sai mật khẩu
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ LỖI GỬI MAIL: {ex.Message}");
            Console.ResetColor();
            _logger.LogError(ex.Message);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }

    // ========================================================================
    // CÁC HÀM NGHIỆP VỤ (GỌI LẠI HÀM Ở TRÊN)
    // ========================================================================

    // 1. Gửi xác nhận đặt bàn
    public async Task SendBookingConfirmationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianDat, int soNguoi, string? ghiChu)
    {
        string content = $@"
            <h3>Xác nhận đặt bàn thành công</h3>
            <p>Xin chào <b>{hoTen}</b>,</p>
            <p>Cảm ơn bạn đã đặt bàn. Mã đơn: <b>{maDonHang}</b></p>
            <p>Bàn: {tenBan} - {soNguoi} người.</p>
            <p>Thời gian: {thoiGianDat:HH:mm dd/MM/yyyy}</p>
            <p>Ghi chú: {ghiChu ?? "Không"}</p>
            <p>Hẹn gặp lại quý khách!</p>";

        await SendEmailViaGmailAsync(toEmail, $"[Xác nhận] Đơn đặt bàn #{maDonHang}", content);
    }

    // 2. Gửi thông báo Hủy
    public async Task SendCancellationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianAn, decimal tienCoc, bool duocHoanTien)
    {
        string noteTien = "";
        if (tienCoc > 0)
        {
            noteTien = duocHoanTien
                ? $"<p style='color:green'>Bạn được hoàn lại cọc: {tienCoc:N0}đ</p>"
                : $"<p style='color:red'>Bạn KHÔNG được hoàn cọc: {tienCoc:N0}đ (Do hủy sát giờ)</p>";
        }

        string content = $@"
            <h3>Thông báo Hủy Đặt Bàn</h3>
            <p>Đơn hàng <b>{maDonHang}</b> đã được hủy thành công.</p>
            {noteTien}
            <p>Cảm ơn bạn.</p>";

        await SendEmailViaGmailAsync(toEmail, $"[Đã Hủy] Đơn bàn #{maDonHang}", content);
    }

    // 3. Gửi nhắc nhở
    public async Task SendReminderEmailAsync(string toEmail, string hoTen, DateTime thoiGianAn, string linkXacNhan, string linkHuy)
    {
        string content = $@"
            <h3>Nhắc nhở lịch hẹn</h3>
            <p>Bạn có lịch ăn lúc <b>{thoiGianAn:HH:mm}</b> hôm nay.</p>
            <p>Vui lòng đến đúng giờ.</p>
            <br/>
            <a href='{linkHuy}' style='color:red; font-weight:bold;'>BẤM VÀO ĐÂY ĐỂ HỦY NẾU BẬN</a>";

        await SendEmailViaGmailAsync(toEmail, $"⏰ Nhắc lịch hẹn lúc {thoiGianAn:HH:mm}", content);
    }

    // 4. GỬI MÃ OTP (QUAN TRỌNG)
    public async Task SendOtpEmailAsync(string toEmail, string hoTen, string otpCode)
    {
        string content = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; max-width: 500px;'>
                <h2 style='color: #007bff;'>Mã xác thực OTP</h2>
                <p>Xin chào <b>{hoTen}</b>,</p>
                <p>Mã OTP của bạn là:</p>
                <h1 style='background-color: #f8f9fa; padding: 10px; text-align: center; letter-spacing: 5px; border-radius: 5px;'>{otpCode}</h1>
                <p style='color: red; font-size: 12px;'>Mã này có hiệu lực trong 5 phút. Tuyệt đối không chia sẻ cho ai.</p>
                <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
            </div>";

        await SendEmailViaGmailAsync(toEmail, "Mã OTP Xác Thực", content);
    }


    public string GetHtml_XacNhanHuy(string maDonHang, string linkXacNhan)
    {
        return $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding-top: 50px; background-color: #f9f9f9; }}
                        .container {{ background: white; max-width: 500px; margin: 0 auto; padding: 30px; border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }}
                        h2 {{ color: #d32f2f; margin-bottom: 20px; }}
                        .btn {{ background-color: #d32f2f; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block; margin-top: 20px; }}
                        .btn:hover {{ background-color: #b71c1c; }}
                        .link-secondary {{ display: block; margin-top: 20px; color: #666; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>⚠ XÁC NHẬN HỦY ĐẶT BÀN</h2>
                        <p>Bạn đang yêu cầu hủy đơn đặt bàn mã: <strong>#{maDonHang}</strong></p>
                        <p>Hành động này sẽ không thể hoàn tác. Bạn có chắc chắn không?</p>

                        <a href='{linkXacNhan}' class='btn'>TÔI CHẮC CHẮN MUỐN HỦY</a>

                        <a href='javascript:window.close()' class='link-secondary'>Không, tôi bấm nhầm</a>
                    </div>
                </body>
                </html>";
    }

    public string GetHtml_HuyThanhCong()
    {
        return @"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <style>
                        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding-top: 50px; background-color: #f9f9f9; }
                        .container { background: white; max-width: 500px; margin: 0 auto; padding: 30px; border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
                        h1 { color: #2e7d32; margin-bottom: 10px; }
                        p { font-size: 18px; color: #333; }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>✅ Đã hủy thành công!</h1>
                        <p>Đơn đặt bàn của bạn đã được hủy.</p>
                        <p>Cảm ơn bạn đã thông báo sớm cho nhà hàng.</p>
                    </div>
                </body>
                </html>";
    }

    public string GetHtml_ThongBaoLoi(string noiDungLoi)
    {
        return $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; text-align: center; padding-top: 50px; background-color: #f9f9f9; }}
                        .container {{ background: white; max-width: 500px; margin: 0 auto; padding: 30px; border-radius: 10px; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }}
                        h1 {{ color: #c62828; margin-bottom: 10px; }}
                        p {{ font-size: 16px; color: #555; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>🚫 Có lỗi xảy ra</h1>
                        <p>{noiDungLoi}</p>
                        <p>Vui lòng liên hệ hotline để được hỗ trợ.</p>
                    </div>
                </body>
                </html>";
    }
}











//using QuanLyNhaHang.Services;
//using SendGrid;
//using SendGrid.Helpers.Mail;
//using System.Threading.Tasks;

//public class SendGridEmailService : IEmailService
//{
//    private readonly IConfiguration _config;
//    private readonly ILogger<SendGridEmailService> _logger;

//    public SendGridEmailService(IConfiguration config, ILogger<SendGridEmailService> logger)
//    {
//        _config = config;
//        _logger = logger;
//    }

//    public async Task SendOtpEmailAsync(string toEmail, string hoTen, string otp)
//    {

//        var apiKey = _config["SendGrid:ApiKey"];
//        var fromEmail = _config["SendGrid:FromEmail"];
//        var fromName = _config["SendGrid:FromName"];

//        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
//        {
//            _logger.LogError("SendGrid API Key hoặc FromEmail chưa được cấu hình.");
//            return;
//        }

//        var client = new SendGridClient(apiKey);
//        var from = new EmailAddress(fromEmail, fromName);
//        var subject = $"Mã xác thực OTP của bạn là {otp}";
//        var to = new EmailAddress(toEmail, hoTen);

//        var htmlContent = $@"
//            <html>
//            <body>
//                <div style='font-family: Arial, sans-serif; font-size: 16px; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
//                    <h2 style='color: #333;'>Xin chào {hoTen},</h2>
//                    <p>Cảm ơn bạn đã sử dụng dịch vụ của {fromName}.</p>
//                    <p>Mã OTP của bạn là:</p>
//                    <h1 style='color: #d9534f; letter-spacing: 2px;'>{otp}</h1>
//                    <p>Mã này sẽ hết hạn trong 5 phút.</p>
//                    <p>Trân trọng,<br/>Đội ngũ {fromName}</p>
//                </div>
//            </body>
//            </html>
//        ";
//        var plainTextContent = $"Mã OTP của bạn là {otp}"; 

//        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

//        try
//        {
//            var response = await client.SendEmailAsync(msg);
//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError($"Gửi email thất bại: {response.StatusCode} - {await response.Body.ReadAsStringAsync()}");
//            }
//            else
//            {
//                _logger.LogInformation($"Đã gửi OTP đến {toEmail} thành công!");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"Lỗi nghiêm trọng khi gửi email đến {toEmail}");
//        }
//    }

//    public async Task SendBookingConfirmationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianDat, int soNguoi, string? ghiChu)
//    {
//        var apiKey = _config["SendGrid:ApiKey"];
//        var fromEmail = _config["SendGrid:FromEmail"];
//        var fromName = _config["SendGrid:FromName"];

//        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
//        {
//            _logger.LogError("SendGrid API Key hoặc FromEmail chưa được cấu hình.");
//            return;
//        }

//        var client = new SendGridClient(apiKey);
//        var from = new EmailAddress(fromEmail, fromName);
//        var subject = $"Xác nhận đặt bàn - Mã đơn: {maDonHang}";
//        var to = new EmailAddress(toEmail, hoTen);

//        var htmlContent = $@"
//            <html>
//            <body>
//                <div style='font-family: Arial, sans-serif; font-size: 16px; padding: 20px; border: 1px solid #ddd; border-radius: 5px; max-width: 600px;'>
//                    <h2 style='color: #333;'>Xin chào {hoTen},</h2>
//                    <p>Cảm ơn bạn đã đặt bàn tại nhà hàng của chúng tôi.</p>
//                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
//                        <h3 style='color: #d9534f; margin-top: 0;'>Thông tin đặt bàn:</h3>
//                        <p><strong>Mã đơn:</strong> {maDonHang}</p>
//                        <p><strong>Bàn:</strong> {tenBan}</p>
//                        <p><strong>Thời gian:</strong> {thoiGianDat:dd/MM/yyyy HH:mm}</p>
//                        <p><strong>Số lượng người:</strong> {soNguoi}</p>
//                        {(string.IsNullOrEmpty(ghiChu) ? "" : $"<p><strong>Ghi chú:</strong> {ghiChu}</p>")}
//                    </div>
//                    <p>Chúng tôi rất mong được phục vụ bạn!</p>
//                    <p>Trân trọng,<br/>Đội ngũ {fromName}</p>
//                </div>
//            </body>
//            </html>
//        ";
//        var plainTextContent = $"Xin chào {hoTen},\n\nCảm ơn bạn đã đặt bàn tại nhà hàng của chúng tôi.\n\nThông tin đặt bàn:\n- Mã đơn: {maDonHang}\n- Bàn: {tenBan}\n- Thời gian: {thoiGianDat:dd/MM/yyyy HH:mm}\n- Số lượng người: {soNguoi}\n{(string.IsNullOrEmpty(ghiChu) ? "" : $"- Ghi chú: {ghiChu}\n")}\nTrân trọng,\nĐội ngũ {fromName}";

//        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

//        try
//        {
//            var response = await client.SendEmailAsync(msg);
//            if (!response.IsSuccessStatusCode)
//            {
//                _logger.LogError($"Gửi email xác nhận đặt bàn thất bại: {response.StatusCode} - {await response.Body.ReadAsStringAsync()}");
//            }
//            else
//            {
//                _logger.LogInformation($"Đã gửi email xác nhận đặt bàn đến {toEmail} thành công!");
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"Lỗi nghiêm trọng khi gửi email xác nhận đặt bàn đến {toEmail}");
//        }
//    }

//    public async Task SendCancellationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianAn, decimal tienCoc, bool duocHoanTien)
//    {
//        var apiKey = _config["SendGrid:ApiKey"];
//        var fromEmail = _config["SendGrid:FromEmail"];
//        var fromName = _config["SendGrid:FromName"];

//        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail)) return;

//        var client = new SendGridClient(apiKey);
//        var from = new EmailAddress(fromEmail, fromName);
//        var subject = $"[Nhà Hàng] Xác nhận HỦY đặt bàn - Mã đơn: {maDonHang}";
//        var to = new EmailAddress(toEmail, hoTen);

//        // Xử lý nội dung hoàn tiền (Màu xanh/đỏ)
//        string noiDungHoanTien = "";
//        if (tienCoc > 0)
//        {
//            if (duocHoanTien)
//            {
//                noiDungHoanTien = $@"
//                    <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; border-left: 5px solid #2e7d32; color: #1b5e20; margin: 15px 0;'>
//                        <h4 style='margin-top:0;'>THÔNG TIN HOÀN TIỀN</h4>
//                        <p>Bạn đã hủy đơn đủ sớm (hoặc trong thời gian ân hạn).</p>
//                        <p>Nhà hàng sẽ hoàn lại số tiền cọc: <strong>{tienCoc:N0} VNĐ</strong>.</p>
//                        <p>Vui lòng liên hệ Hotline/Zalo OA để cung cấp số tài khoản nhận tiền.</p>
//                    </div>";
//            }
//            else
//            {
//                noiDungHoanTien = $@"
//                    <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; border-left: 5px solid #c62828; color: #b71c1c; margin: 15px 0;'>
//                        <h4 style='margin-top:0;'>THÔNG TIN HOÀN TIỀN</h4>
//                        <p>Rất tiếc, do bạn hủy đơn quá sát giờ ăn (dưới 12 tiếng).</p>
//                        <p>Số tiền cọc <strong>{tienCoc:N0} VNĐ</strong> sẽ KHÔNG được hoàn lại theo quy định giữ chỗ.</p>
//                    </div>";
//            }
//        }

//        var htmlContent = $@"
//            <html>
//            <body style='font-family: Arial, sans-serif; color: #333;'>
//                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
//                    <h2 style='color: #d32f2f; text-align: center;'>XÁC NHẬN HỦY ĐẶT BÀN</h2>
//                    <p>Chào <strong>{hoTen}</strong>,</p>
//                    <p>Yêu cầu hủy đơn đặt bàn của bạn đã được thực hiện thành công.</p>

//                    <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
//                        <tr style='border-bottom: 1px solid #eee;'><td style='padding: 8px;'>Mã đơn:</td><td style='padding: 8px; font-weight: bold;'>{maDonHang}</td></tr>
//                        <tr style='border-bottom: 1px solid #eee;'><td style='padding: 8px;'>Bàn:</td><td style='padding: 8px; font-weight: bold;'>{tenBan}</td></tr>
//                        <tr style='border-bottom: 1px solid #eee;'><td style='padding: 8px;'>Thời gian dự kiến:</td><td style='padding: 8px; font-weight: bold;'>{thoiGianAn:HH:mm dd/MM/yyyy}</td></tr>
//                    </table>

//                    {noiDungHoanTien}

//                    <p style='text-align: center; margin-top: 30px;'>Hẹn gặp lại bạn trong dịp khác!</p>
//                    <p style='text-align: center; color: #777; font-size: 12px;'>Trân trọng,<br/>{fromName}</p>
//                </div>
//            </body>
//            </html>";

//        var msg = MailHelper.CreateSingleEmail(from, to, subject, "Hủy đặt bàn thành công.", htmlContent);
//        await SendWrapperAsync(client, msg, toEmail, "Hủy Đặt Bàn");
//    }

//    // =========================================================================
//    // 3. PHƯƠNG THỨC MỚI: GỬI MAIL NHẮC NHỞ (CÓ LINK HỦY NHANH)
//    // =========================================================================
//    public async Task SendReminderEmailAsync(string toEmail, string hoTen, DateTime thoiGianAn, string linkXacNhan, string linkHuy)
//    {
//        var apiKey = _config["SendGrid:ApiKey"];
//        var fromEmail = _config["SendGrid:FromEmail"];
//        var fromName = _config["SendGrid:FromName"];

//        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail)) return;

//        var client = new SendGridClient(apiKey);
//        var from = new EmailAddress(fromEmail, fromName);
//        var subject = $"⏰ Nhắc nhở lịch hẹn lúc {thoiGianAn:HH:mm} hôm nay";
//        var to = new EmailAddress(toEmail, hoTen);

//        var htmlContent = $@"
//            <html>
//            <body style='font-family: Arial, sans-serif; color: #333;'>
//                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
//                    <h2 style='color: #1976d2; text-align: center;'>LỜI NHẮC LỊCH HẸN</h2>
//                    <p>Chào <strong>{hoTen}</strong>,</p>
//                    <p>Bạn có một lịch hẹn ăn uống sắp tới tại nhà hàng chúng tôi:</p>

//                    <div style='background-color: #e3f2fd; padding: 20px; text-align: center; border-radius: 8px; margin: 20px 0;'>
//                        <h1 style='margin: 0; color: #0d47a1;'>{thoiGianAn:HH:mm}</h1>
//                        <p style='margin: 5px 0 0 0; font-weight: bold;'>Ngày {thoiGianAn:dd/MM/yyyy}</p>
//                    </div>

//                    <p>Vui lòng đến đúng giờ để chúng tôi phục vụ bạn tốt nhất.</p>

//                    <div style='margin-top: 30px; padding-top: 20px; border-top: 1px dashed #ccc;'>
//                        <p>Nếu kế hoạch thay đổi, vui lòng thông báo cho chúng tôi:</p>
//                        <table style='width: 100%; text-align: center;'>
//                            <tr>
//                                <td>
//                                    <a href='{linkHuy}' style='background-color: #d32f2f; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>HỦY ĐẶT BÀN</a>
//                                </td>
//                            </tr>
//                        </table>
//                        <p style='font-size: 12px; color: #666; text-align: center; margin-top: 10px;'>
//                            *Lưu ý: Hủy trước 12 tiếng sẽ được hoàn tiền cọc (nếu có).
//                        </p>
//                    </div>
//                </div>
//            </body>
//            </html>";

//        var msg = MailHelper.CreateSingleEmail(from, to, subject, "Nhắc nhở lịch hẹn.", htmlContent);
//        await SendWrapperAsync(client, msg, toEmail, "Nhắc Nhở");
//    }

//    // Hàm phụ để bọc try-catch cho gọn code
//    private async Task SendWrapperAsync(SendGridClient client, SendGridMessage msg, string toEmail, string actionName)
//    {
//        try
//        {
//            var response = await client.SendEmailAsync(msg);
//            if (response.IsSuccessStatusCode)
//                _logger.LogInformation($"Gửi email [{actionName}] đến {toEmail} thành công.");
//            else
//                _logger.LogError($"Gửi email [{actionName}] thất bại: {response.StatusCode}");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"Lỗi gửi email [{actionName}] đến {toEmail}");
//        }
//    }




//}