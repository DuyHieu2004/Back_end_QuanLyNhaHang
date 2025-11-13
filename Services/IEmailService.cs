namespace QuanLyNhaHang.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string hoTen, string otp);
        Task SendBookingConfirmationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianDat, int soNguoi, string? ghiChu);

        Task SendCancellationEmailAsync(string toEmail, string hoTen, string maDonHang, string tenBan, DateTime thoiGianAn, decimal tienCoc, bool duocHoanTien);

        Task SendReminderEmailAsync(string toEmail, string hoTen, DateTime thoiGianAn, string linkXacNhan, string linkHuy);

        string GetHtml_XacNhanHuy(string maDonHang, string linkXacNhan);
        string GetHtml_HuyThanhCong();
        string GetHtml_ThongBaoLoi(string noiDungLoi);
    }
}
