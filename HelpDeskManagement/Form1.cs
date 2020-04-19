using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelpDeskManagement.Libs;

namespace HelpDeskManagement
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(tbUsername.Text.Trim()!="" && tbPassword.Text.Trim()!="")
            {
                //MessageBox.Show("Đăng nhập thành công", "Thông báo");
                //Cần kết nối với CSDL để kiểm tra
                // Cách sử dụng các Parameters & Stored Procedure trong SQL 

                // Bước 1. Gán giá trị cho các Parameters 
                //Cách1: tạo các Parameters đầy đủ
                SqlParameter[] sqlParams = {
                new SqlParameter("@username",SqlDbType.NVarChar,30),
                new SqlParameter("@password",SqlDbType.NVarChar,40)
            };
                sqlParams[0].Value = tbUsername.Text.Trim();
                sqlParams[1].Value = Libs.Database.Data.HashBytesSHA1(tbPassword.Text.Trim()); //Mã hóa mật khẩu sang chuẩn HASH SH-A1 có chiều dài là 40 ký tự --> Mã hóa trực tuyến tại http://www.sha1-online.com/


                // Cách2: tạo các Parameters ngắn gọn
                SqlParameter[] sqlParams2 = {
                new SqlParameter("@username",tbUsername.Text),
                new SqlParameter("@password", Libs.Database.Data.HashBytesSHA1(tbPassword.Text.Trim()))
            };

                // Bước 3: Tạo ra một DataTable rỗng để chứa dữ liệu khi kết quả trả về
                DataTable table = new DataTable();

                //Bước 4: thực hiện kết nối 1 cấu SP cơ bản
                try
                {
                   table= Libs.Database.Data.ExcuteToDataTable("USERNAME_SELECTCHECKLOGIN", CommandType.StoredProcedure,
                        sqlParams);
                    if(table.Rows.Count>0)
                    {
                        MessageBox.Show("Đăng nhập thành công", "Thông báo");
                        String Username = table.Rows[0][0].ToString();
                        String Department = table.Rows[0][3].ToString();
                        MessageBox.Show("Chào bạn " + Username + " !\n bạn thuộc phòng "+Department, "Thông báo");
                    }
                    else
                    {
                        MessageBox.Show("Đăng nhập thất bại", "Thông báo");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Kết nối bị lỗi", "THÔNG BÁO!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng nhập liệu", "THÔNG BÁO!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
