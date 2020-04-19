using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

namespace HelpDeskManagement.Libs
{
    class Database
    {

        private Database() { }//Private constructor: Cấu trúc thuộc tính riêng của Class Database này khi được khởi tạo - Hàm dựng
        // Design theo Singleton Pattern: 1 Class chỉ có duy nhất 1 Instance được khởi tạo
        private static Database data; //CTR + R + E để tự sinh code phần dưới mà không cần gõ
        public static Database Data //Instance duy nhất của Class được tạo ra
        {
            get
            {
                if (data == null)
                {
                    data = new Database(); // Quá trình khởi tạo lần đầu Class Database
                }
                return Database.data;
            }

            private set // Bên ngoài không được thiết lập lớp này, có thể không có phần này
            {
                Database.data = value;
            }
        }




        #region Chuỗi kết nối đến SQL Server

        //Chuỗi kết nối dạng Windows Authentication
        //Data Source=.;Initial Catalog=HELPDESK;Integrated Security=True      
        //Chuối kết nối dành cho SQL Server Authentication 
        //Data Source =.; Initial Catalog = HELPDESK; Persist Security Info=True;User ID = helpdesk; Password=123456

        //Khai báo cấu hình kết nối cố định - lưu vào code
        public SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-C9BEO2P\SQLEXPRESS;Initial Catalog=HELPDESK;Integrated Security=True");

        //Khai báo cấu hình kết nối từ file cấu hình Setting, dễ dàng khi đi mang đi cài đặt
        //public SqlConnection con = new SqlConnection(Properties.Settings.Default.ConnectionString);

        #endregion

        #region Thư viện thực thi SQL Command có Parameter
        SqlCommand command = new SqlCommand();

        public DataTable ExcuteToDataTable(string cmdText, CommandType type = CommandType.Text, SqlParameter[] prms=null)
        {
            command.CommandText = cmdText;
            command.CommandType = type;
            command.Parameters.Clear();
            if (prms != null)
            {
                foreach (SqlParameter p in prms)
                {
                    if (p != null)
                    {
                        command.Parameters.Add(p);
                    }
                }
            }
            command.Connection = con;
            DataTable dttb = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            try
            {
                adapter.Fill(dttb);
                adapter.Dispose();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Không thể thực thi SQL!", ex);
            }
            return dttb;
        }

        public void ExecuteNonQuery(string cmdText, CommandType type = CommandType.Text, SqlParameter[] prms=null)
        {
            command.CommandText = cmdText;
            command.CommandType = type;
            command.Parameters.Clear();
            if (prms != null)
            {
                foreach (SqlParameter p in prms)
                {
                    if (p != null) {
                        command.Parameters.Add(p);
                    }                
                }
            }          
            command.Connection = con;
            con.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
               throw new ApplicationException("Không thể thực thi SQL!", ex);
            }
            con.Close();
        }

        /*
        ExecuteScalar thường được sử dụng khi truy vấn của bạn trả về một giá trị duy nhất. Nếu nó trả về nhiều hơn, thì kết quả là cột đầu tiên của hàng đầu tiên. Một ví dụ có thể là SELECT @@IDENTITY AS 'Identity'.
        ExecuteReader được sử dụng cho bất kỳ tập hợp kết quả nào có nhiều hàng / cột (ví dụ: SELECT col1, col2 from sometable).
        ExecuteNonQuery thường được sử dụng cho các câu lệnh SQL không có kết quả (ví dụ: UPDATE, INSERT, v.v.). 
        */
        #endregion

        #region Hàm mã hóa mật khẩu dạng SHA1
        public string HashBytesSHA1(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);
        }

        public string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        #endregion

        #region  Hàm mã hóa mật khẩu có thể giải mã được dạng Hash Base64
        //Mã hóa 1 chuỗi theo Base64 
        public string EncodeStringToBase64(string src)
        {
            try
            {
                // Chuyển chuỗi thành mảng kiểu byte.
                byte[] b = Encoding.Unicode.GetBytes(src);
                // Trả về chuỗi được mã hóa theo Base64.
                return Convert.ToBase64String(b);
            }
            catch
            {
                return null;
            }
        }
        // Giải mã một chuỗi Unicode được mã hóa theo Base64.
        public string DecodeBase64ToString(string src)
        {
            
            try
            {
                // Giải mã vào mảng kiểu byte.
                byte[] b = Convert.FromBase64String(src);
                // Trả về chuỗi Unicode.
                return Encoding.Unicode.GetString(b);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Một số thư viện Function cần thiết khác

        //Kiểm tra ứng dụng đang mở
        public bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;
            bool bCreatedNew;

            Mutex mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
            if (bCreatedNew)
                mutex.ReleaseMutex();

            return !bCreatedNew;
        }


        //Kiểm tra tình trạng PING tín hiệu
        public bool pingIP(string IP)
        {
            bool pinging = false;

            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(IP, 1000);
                pinging = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                pinging = false;
            }
            return pinging;
        }

        //Lấy thông tin IP máy tính
        public string GetLanIP()
        {
            string strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            foreach (IPAddress ipAddress in ipEntry.AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    return ipAddress.ToString();
                }
            }
            return "-";
        }

        //Lấy dữ liệu HTML từ URL
        public string getHTML(string url)
        {
            try
            {
                string myPageSource = "";
                HttpWebRequest myWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                myWebRequest.Method = "GET";
                HttpWebResponse myWebResponse = (HttpWebResponse)myWebRequest.GetResponse();
                StreamReader myWebSource = new StreamReader(myWebResponse.GetResponseStream());
                myPageSource = myWebSource.ReadToEnd();
                myWebResponse.Close();
                string AllHTMLCode = myPageSource;
                return myPageSource;
            }
            catch
            {
                return "";
            }
        }

        //Bóc tách HTML từ vị trí mong muốn
        public string splitHTML(string begin, string end, int addbegin, int addend, string content)
        {
            try
            {
                string AllHTMLCode = content;
                string chuoibatdau = begin;
                int vitria = AllHTMLCode.IndexOf(chuoibatdau);
                string xoadoandau = AllHTMLCode.Substring(vitria + addbegin);

                string chuoiketthuc = end;
                int vitrib = AllHTMLCode.IndexOf(chuoiketthuc);
                string laydoancuoi = AllHTMLCode.Substring(vitrib + addend);

                string noidungchinh = xoadoandau.Replace(laydoancuoi, "");
                return noidungchinh;
            }
            catch
            {
                string ch = "";
                return ch;
            }
        }

        public string ConvertUTF8toUnicode(string str)
        {
            byte[] aBytes = Encoding.Default.GetBytes(str);
            return Encoding.UTF8.GetString(aBytes);
        }

        static string DecodeEncodedNonAsciiCharacters2(string value)
        {
            return Regex.Replace(
            value,
            @"\\u(?<Value>[a-zA-Z0-9]{4})",
            m =>
            {
                return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
            });
        }

        private string[] VietNamChar = new string[]
        {
           "aAeEoOuUiIdDyY",
           "áàạảãâấầậẩẫăắằặẳẵ",
           "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
           "éèẹẻẽêếềệểễ",
           "ÉÈẸẺẼÊẾỀỆỂỄ",
           "óòọỏõôốồộổỗơớờợởỡ",
           "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
           "úùụủũưứừựửữ",
           "ÚÙỤỦŨƯỨỪỰỬỮ",
           "íìịỉĩ",
           "ÍÌỊỈĨ",
           "đ",
           "Đ",
           "ýỳỵỷỹ",
           "ÝỲỴỶỸ"
        };

        //Chuyển chuỗi tiếng việt có dấu sang không dấu
        public string ChuyenKhongDau(string strInput)
        {
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    strInput = strInput.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            return strInput;
        }

        #endregion



    }
}