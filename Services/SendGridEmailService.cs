using QuanLyNhaHang.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class SendGridEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(IConfiguration config, ILogger<SendGridEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string hoTen, string otp)
    {
     
        var apiKey = _config["SendGrid:ApiKey"];
        var fromEmail = _config["SendGrid:FromEmail"];
        var fromName = _config["SendGrid:FromName"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogError("SendGrid API Key hoặc FromEmail chưa được cấu hình.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var subject = $"Mã xác thực OTP của bạn là {otp}";
        var to = new EmailAddress(toEmail, hoTen);

        var htmlContent = $@"
            <html>
            <body>
                <div style='font-family: Arial, sans-serif; font-size: 16px; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                    <h2 style='color: #333;'>Xin chào {hoTen},</h2>
                    <p>Cảm ơn bạn đã sử dụng dịch vụ của {fromName}.</p>
                    <p>Mã OTP của bạn là:</p>
                    <h1 style='color: #d9534f; letter-spacing: 2px;'>{otp}</h1>
                    <p>Mã này sẽ hết hạn trong 5 phút.</p>
                    <p>Trân trọng,<br/>Đội ngũ {fromName}</p>
                </div>
            </body>
            </html>
        ";
        var plainTextContent = $"Mã OTP của bạn là {otp}"; 

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        try
        {
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gửi email thất bại: {response.StatusCode} - {await response.Body.ReadAsStringAsync()}");
            }
            else
            {
                _logger.LogInformation($"Đã gửi OTP đến {toEmail} thành công!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi nghiêm trọng khi gửi email đến {toEmail}");
        }
    }

    public async Task SendBookingConfirmationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianDat, int soNguoi, string? ghiChu)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        var fromEmail = _config["SendGrid:FromEmail"];
        var fromName = _config["SendGrid:FromName"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
        {
            _logger.LogError("SendGrid API Key hoặc FromEmail chưa được cấu hình.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var subject = $"Xác nhận đặt bàn - Mã đơn: {maDonHang}";
        var to = new EmailAddress(toEmail, hoTen);

        var htmlContent = $@"
            <html>
            <body>
                <div style='font-family: Arial, sans-serif; font-size: 16px; padding: 20px; border: 1px solid #ddd; border-radius: 5px; max-width: 600px;'>
                    <h2 style='color: #333;'>Xin chào {hoTen},</h2>
                    <p>Cảm ơn bạn đã đặt bàn tại nhà hàng của chúng tôi.</p>
                    <div style='background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <h3 style='color: #d9534f; margin-top: 0;'>Thông tin đặt bàn:</h3>
                        <p><strong>Mã đơn:</strong> {maDonHang}</p>
                        <p><strong>Bàn:</strong> {tenBan}</p>
                        <p><strong>Thời gian:</strong> {thoiGianDat:dd/MM/yyyy HH:mm}</p>
                        <p><strong>Số lượng người:</strong> {soNguoi}</p>
                        {(string.IsNullOrEmpty(ghiChu) ? "" : $"<p><strong>Ghi chú:</strong> {ghiChu}</p>")}
                    </div>
                    <p>Chúng tôi rất mong được phục vụ bạn!</p>
                    <p>Trân trọng,<br/>Đội ngũ {fromName}</p>
                </div>
            </body>
            </html>
        ";
        var plainTextContent = $"Xin chào {hoTen},\n\nCảm ơn bạn đã đặt bàn tại nhà hàng của chúng tôi.\n\nThông tin đặt bàn:\n- Mã đơn: {maDonHang}\n- Bàn: {tenBan}\n- Thời gian: {thoiGianDat:dd/MM/yyyy HH:mm}\n- Số lượng người: {soNguoi}\n{(string.IsNullOrEmpty(ghiChu) ? "" : $"- Ghi chú: {ghiChu}\n")}\nTrân trọng,\nĐội ngũ {fromName}";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        try
        {
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gửi email xác nhận đặt bàn thất bại: {response.StatusCode} - {await response.Body.ReadAsStringAsync()}");
            }
            else
            {
                _logger.LogInformation($"Đã gửi email xác nhận đặt bàn đến {toEmail} thành công!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi nghiêm trọng khi gửi email xác nhận đặt bàn đến {toEmail}");
        }
    }
}