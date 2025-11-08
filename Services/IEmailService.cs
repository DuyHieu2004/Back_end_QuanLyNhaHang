namespace QuanLyNhaHang.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string hoTen, string otp);
    }
}
