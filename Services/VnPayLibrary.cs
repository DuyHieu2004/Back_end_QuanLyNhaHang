using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyNhaHang.Services
{
    public class VnPayLibrary
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (data.Length > 0)
                {
                    data.Append("&");
                }
                data.Append(kv.Key + "=" + WebUtility.UrlEncode(kv.Value));
            }
            string queryString = data.ToString();
            string baseUrlWithQuery = baseUrl + "?" + queryString;
            string signData = queryString;
            if (signData.Length > 0)
            {
                string vnp_SecureHash = HmacSHA512(vnp_HashSecret, signData);
                baseUrlWithQuery += "&vnp_SecureHash=" + vnp_SecureHash;
            }
            return baseUrlWithQuery;
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }
            return hash.ToString();
        }
    }

    // Class so sánh để sắp xếp tham số theo bảng chữ cái (Bắt buộc của VNPay)
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}