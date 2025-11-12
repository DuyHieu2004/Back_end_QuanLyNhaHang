namespace QuanLyNhaHang.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string hoTen, string otp);
        Task SendBookingConfirmationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianDat, int soNguoi, string? ghiChu);
    }
}
