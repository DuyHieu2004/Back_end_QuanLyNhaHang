namespace QuanLyNhaHang.Services
{
    public interface IOtpService
    {
        string GenerateAndStoreOtp(string identifier);
        bool ValidateOtp(string identifier, string otp);
    }
}
