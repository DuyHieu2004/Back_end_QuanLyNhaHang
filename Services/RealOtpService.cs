using Microsoft.Extensions.Caching.Memory;
using QuanLyNhaHang.Services;

public class RealOtpService : IOtpService
{
    private readonly IMemoryCache _cache;

    public RealOtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateAndStoreOtp(string identifier)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        _cache.Set($"OTP_{identifier}", otp, cacheEntryOptions);

        return otp;
    }

    public bool ValidateOtp(string identifier, string otp)
    {
        if (_cache.TryGetValue($"OTP_{identifier}", out string? storedOtp))
        {
            if (storedOtp == otp)
            {
                _cache.Remove($"OTP_{identifier}");
                return true;
            }
        }
        return false;
    }
}