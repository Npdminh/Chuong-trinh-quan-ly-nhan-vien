using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing; 
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
//using System.Xml;//
namespace QuanLyNhanVien
{
    public class NhanVien
    {
        public string Ma { get; set; }
        public string Ten { get; set; }
        public string Phong { get; set; }
        public string CV { get; set; }
        // ===== THÔNG TIN CÁ NHÂN =====
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string CCCD { get; set; }
        public string DiaChi { get; set; }
        // ===== LIÊN HỆ =====
        public string DienThoai { get; set; }
        public string Email { get; set; }
        public string NguoiThan { get; set; }
        // ===== CÔNG VIỆC =====
        public DateTime NgayVaoLam { get; set; }
        public string LoaiHopDong { get; set; } // Giá trị: "Thử việc", "1 năm", "Không thời hạn", "Đã hủy"

        public string TrangThai { get; set; }

        public string LoaiNV
        {
            get
            {
                if (LoaiHopDong == "Đã hủy") return "Đã nghỉ"; // Logic mới
                return (TrangThai == "Thực tập") ? "Part-time" : "Full-time";
            }
        }
        // ===== FILE =====
        public string FileHopDong { get; set; } // lưu path
        // ===== CHẤM CÔNG =====
        public string Vao { get; set; }
        public string Ra { get; set; }
        public string Tong { get; set; }
        public string TT { get; set; }
        public string Anh { get; set; } // đường dẫn ảnh
        public string DanToc { get; set; }
        public string TonGiao { get; set; }
        public DateTime BatDauHD { get; set; }
        public DateTime HetHanHD { get; set; }
        public int TongPhepNam { get; set; } = 12;
        public int DaDungPhep { get; set; } = 0;
        public Dictionary<int, int> NgayPhep { get; set; } = new Dictionary<int, int>();
        public BindingList<string> TangCa { get; set; } = new BindingList<string>();
        public BindingList<string> LichSuCongTac { get; set; } = new BindingList<string>();
        public BindingList<string> LichSuChamCong { get; set; } = new BindingList<string>();
        public Dictionary<int, string> LichThang { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> CustomStatus = new Dictionary<int, string>();
        public List<DonNghiPhep> DsDonNghi { get; set; } = new List<DonNghiPhep>();
        public List<DonTangCa> DsDonTangCa { get; set; } = new List<DonTangCa>();
        public Dictionary<int, double> MonthlyBonus { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, double> MonthlyPenalty { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, bool> IsSalaryLocked { get; set; } = new Dictionary<int, bool>();
        
        
        public virtual double TinhLuong()
        {
            return 0;
        }
    }
    public class UserAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // Admin / Staff

        public string MaNhanVien { get; set; } // 👈 liên kết NV
    }
    // --- LỚP ĐƠN NGHỈ PHÉP ---
    public class DonNghiPhep
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int SoNgay { get; set; }
        public string LyDo { get; set; }
        public string ChuThich { get; set; }
        public string TrangThai { get; set; } = "Chờ duyệt";
    }

    // --- LỚP ĐƠN TĂNG CA ---
    public class DonTangCa
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public double Hours { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; } = "Chờ duyệt";
    }
    public class ThongBao
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
}


    public static class SalarySettings
    {
        // Cấu hình lương theo Chức Vụ (CV)
        public static Dictionary<string, double> RoleSalaries = new Dictionary<string, double>
    {
        { "Giám đốc", 25000000 },
         { "Kỹ sư", 20000000 },
        { "Trưởng phòng", 15000000 },
        { "Nhân viên", 8000000 },
        { "", 5000000 }
    };
    }
    // ===== NHÂN VIÊN FULLTIME =====
    public class NVFullTime : NhanVien
    {
        // Bạn vẫn giữ LuongCoBan để làm giá trị mặc định phòng hờ
        public double LuongCoBan { get; set; } = 8000000;

        public override double TinhLuong()
        {
            // Ensure that CV is not null or empty
            if (string.IsNullOrEmpty(this.CV) || !SalarySettings.RoleSalaries.ContainsKey(this.CV))
            {
                // Return a default salary or handle the error accordingly
                return LuongCoBan; // You can also set a default value or handle it as needed
            }

            // Return the salary for the position
            return SalarySettings.RoleSalaries[this.CV];
        }
    }
    // ===== NHÂN VIÊN PARTTIME =====
    public class NVPartTime : NhanVien
    {
        public double LuongTheoGio { get; set; } = 30000;
        public int SoGio { get; set; } = 0;
        public override double TinhLuong()
        {
            return LuongTheoGio * SoGio;
        }

    }

    public partial class Form1 : Form
    {
        // Tab và TabControl toàn cục
        TabControl tabControl = new TabControl();
        TabPage tabAccount = new TabPage("Tài khoản");
        private bool isAdmin;
        private NhanVien currentUser = new NhanVien { Ma = "NV001" };
        Panel sidebar, header, content;
        DataGridView dgv;
        Button btnCSV;
        Panel homePanel;
        Panel employeePanel;
        Panel attendancePanel;
        Panel lichPanel;
        Panel accountPanel;
        Button btnTongQuan, btnNhanVien;
        BindingList<NhanVien> dsNhanVien = new BindingList<NhanVien>();
        Label lbTongNV, lbDiLam, lbNghi, lbTangCa;
        Random rd = new Random();
        Timer clockTimer;
        Label clockLabel;
        string dataFile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "QuanLyNhanVien",
    "data_nhanvien.json"
);
        
        TextBox txtUsername = new TextBox();
        TextBox txtPassword = new TextBox();
        BindingList<UserAccount> dsAccount = new BindingList<UserAccount>();
        public BindingList<UserAccount> allAccounts = new BindingList<UserAccount>();
        public UserAccount CurrentAccount;
        NhanVien currentNhanVien;
        string accountFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyNhanVien",
            "data_account.json"
        );
        
        string notiFile = "thongbao.json";
        public static BindingList<ThongBao> dsThongBao = new BindingList<ThongBao>();
        private Panel pThongBaoRight;

        private DateTime lastResetDate = DateTime.MinValue; // ngày đã reset chấm công lần cuối
        public Form1(UserAccount acc)
        {
            InitializeComponent();
            CurrentAccount = acc;
            BuildUI();
            CurrentAccount = acc;

            LoadData();
            CheckResetNgayMoi();
            UpdateTrangThai();
            UpdateThongKe();
            MessageBox.Show(CurrentAccount == null ? "NULL" : CurrentAccount.Username);
            if (CurrentAccount == null)
            {
                MessageBox.Show("Lỗi đăng nhập!");
                this.Close();
                return;
            }
            /*MessageBox.Show("NV count = " + dsNhanVien.Count);
            MessageBox.Show("Form hash: " + this.GetHashCode());Check khi cần*/
            MapUser();
           
            LoadAccounts();
            LoadThongBaoDashboard(pThongBaoRight);
            Application.ApplicationExit += (s, e) => SaveData();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveData();
            base.OnFormClosing(e);
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveData();
        }
        private bool isDarkMode = false;
        void BuildUI()
        {
            this.Text = "Quản Lý Ngày Công Nhân Viên";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(245, 246, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            BuildSidebar();
            // ================= HEADER =================
            header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.White
            };
            this.Controls.Add(header);

            // 👉 LẤY TÊN HIỂN THỊ
            string displayName = isAdmin
                ? "Quản trị viên"
                : currentNhanVien?.Ten ?? CurrentAccount.Username;

            // 👉 HELLO
            Label hello = new Label
            {
                Text = "Xin chào, " + displayName + " 👋",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                Location = new Point(270, 18),
                AutoSize = true
            };
            header.Controls.Add(hello);

            // 👉 DATE
            Label date = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm"),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(272, 55),
                AutoSize = true
            };
            header.Controls.Add(date);

            // ================= CLOCK =================
            clockLabel = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(1100, 20),
                AutoSize = true
            };
            header.Controls.Add(clockLabel);

            // ❗ tránh lỗi trùng biến s,e
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (senderClock, evClock) =>
            {
                clockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            clockTimer.Start();

            // ================= BUTTON CSV =================
            btnCSV = new Button
            {
                Text = "📁 Xuất CSV",
                Size = new Size(130, 40),
                Location = new Point(1380, 22),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnCSV.FlatAppearance.BorderSize = 0;
            btnCSV.Click += BtnCSV_Click;
            header.Controls.Add(btnCSV);

            // ================= CONTENT =================
            content = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(content);
            homePanel = content;

            // ================= TABLE PANEL =================
            Panel tablePanel = new Panel
            {
                Location = new Point(270, 250),
                Size = new Size(980, 520),
                BackColor = Color.White
            };
            content.Controls.Add(tablePanel);

            // TITLE
            Label title = new Label
            {
                Text = "Danh sách nhân viên",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };
            tablePanel.Controls.Add(title);

            // ================= GRID =================
            dgv = new DataGridView
            {

                Location = new Point(15, 55),
                Size = new Size(950, 440),
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            UpdateTrangThai();

            // 1. Tắt tự động tạo cột
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear(); // Xóa sạch các cột cũ

            // 2. Thêm các cột muốn hiển thị (Mapping với thuộc tính của NhanVien)
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Ma", HeaderText = "Mã NV", Name = "Ma" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Ten", HeaderText = "Tên", Name = "Ten" });

            // 3. Thêm cột tính toán (Không có DataPropertyName vì không lấy trực tiếp từ class)
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TT",HeaderText = "Trạng thái", Name = "TT" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Vao", HeaderText = "Giờ vào", Name = "" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Ra", HeaderText = "Giờ ra", Name = "" });
            // 4. Bind dữ liệu
            dgv.DataSource = dsNhanVien;


            // 👉 STYLE HEADER
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.RoyalBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.EnableHeadersVisualStyles = false;

            // 👉 COLOR STATUS
            dgv.CellFormatting += (senderGrid, eGrid) =>
            {
                if (dgv.Columns[eGrid.ColumnIndex].Name == "TrangThai" && eGrid.Value != null)
                {
                    string val = eGrid.Value.ToString();

                    if (val == "Nghỉ")
                        eGrid.CellStyle.ForeColor = Color.Gray;
                    else if (val == "Đang làm")
                        eGrid.CellStyle.ForeColor = Color.SeaGreen;
                    else if (val == "Hoàn tất")
                        eGrid.CellStyle.ForeColor = Color.RoyalBlue;
                }
            };

            tablePanel.Controls.Add(dgv);

            // 👉 đảm bảo không bị panel khác đè
            content.BringToFront();
            tablePanel.BringToFront();
            // ================= BÊN PHẢI =================
            Panel right = new Panel();
            right.Location = new Point(1280, 250);
            right.Size = new Size(280, 520);
            right.BackColor = Color.White;
            content.Controls.Add(right);
            Label lbCal = new Label();
            lbCal.Text = "Lịch";
            lbCal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lbCal.Location = new Point(15, 15);
            lbCal.AutoSize = true;
            right.Controls.Add(lbCal);
            MonthCalendar mc = new MonthCalendar();
            mc.Location = new Point(25, 50);
            right.Controls.Add(mc);

            pThongBaoRight = new Panel
            {
                Location = new Point(15, 250),
                Size = new Size(250, 220),
                BackColor = Color.White
            };

            right.Controls.Add(pThongBaoRight);

            // 🔥 gọi SAU khi tạo xong
            LoadThongBaoDashboard(pThongBaoRight);

            right.Controls.Add(pThongBaoRight);
            // ================= BÊN TRÊN =================
            
            lbTongNV = AddCard("Tổng nhân viên", 270, 110, Color.RoyalBlue);
            lbDiLam = AddCard("Đi làm hôm nay", 520, 110, Color.SeaGreen);
            lbNghi = AddCard("Nghỉ hôm nay", 770, 110, Color.Goldenrod);
            lbTangCa = AddCard("Tăng ca", 1020, 110, Color.MediumPurple);

            // ================= BIỂU ĐỒ CỘT =================
            /*Chart chart1 = new Chart();
            chart1.Location = new Point(270, 800);
            chart1.Size = new Size(520, 260);
            chart1.BackColor = Color.White;
            chart1.Titles.Add("Biểu đồ ngày công 6 tháng");
            ChartArea area1 = new ChartArea();
            chart1.ChartAreas.Add(area1);
            Series s1 = new Series();
            s1.ChartType = SeriesChartType.Column;
            s1.Color = Color.RoyalBlue;
            s1.Points.AddXY("T1", 24);
            s1.Points.AddXY("T2", 26);
            s1.Points.AddXY("T3", 25);
            s1.Points.AddXY("T4", 27);
            s1.Points.AddXY("T5", 23);
            s1.Points.AddXY("T6", 26);
            chart1.Series.Add(s1);
            content.Controls.Add(chart1);
            // ================= BIỂU ĐỒ TRÒN =================
            Chart chart2 = new Chart();
            chart2.Location = new Point(820, 800);
            chart2.Size = new Size(350, 260);
            chart2.BackColor = Color.White;
            chart2.Titles.Add("Tỷ lệ phòng ban");
            ChartArea area2 = new ChartArea();
            chart2.ChartAreas.Add(area2);
            Series s2 = new Series();
            s2.ChartType = SeriesChartType.Pie;
            s2.Points.AddXY("Nhân sự", 10);
            s2.Points.AddXY("Kế toán", 8);
            s2.Points.AddXY("CNTT", 15);
            s2.Points.AddXY("Kinh doanh", 23);
            chart2.Series.Add(s2);
            chart2.Legends.Add(new Legend());
            content.Controls.Add(chart2);
            // ================= BIỂU ĐỒ ĐƯỜNG =================
            Chart chart3 = new Chart();
            chart3.Location = new Point(1200, 800);
            chart3.Size = new Size(350, 260);
            chart3.BackColor = Color.White;
            chart3.Titles.Add("Giờ tăng ca theo tuần");
            ChartArea area3 = new ChartArea();
            chart3.ChartAreas.Add(area3);
            Series s3 = new Series();
            s3.ChartType = SeriesChartType.Line;
            s3.BorderWidth = 3;
            s3.Color = Color.MediumPurple;
            s3.Points.AddXY("Tuần 1", 12);
            s3.Points.AddXY("Tuần 2", 18);
            s3.Points.AddXY("Tuần 3", 9);
            s3.Points.AddXY("Tuần 4", 20);
            chart3.Series.Add(s3);
            content.Controls.Add(chart3);*/
            // ================= VẼ 3 BIỂU ĐỒ TRỰC TIẾP LÊN DASHBOARD =================
            LoadData();
            int y = 800; // vị trí Y trên dashboard

            // ----- BIỂU ĐỒ CỘT (Ngày công 6 tháng) -----
            // ----- BIỂU ĐỒ CỘT (Ngày công 6 tháng) -----
            Chart dashboardColumn = new Chart
            {
                Size = new Size(520, 260),
                BackColor = Color.White,
                Location = new Point(270, 800) // vị trí trên dashboard
            };

            ChartArea areaCol = new ChartArea { BackColor = Color.Transparent };
            dashboardColumn.ChartAreas.Add(areaCol);

            Title titleCol = new Title
            {
                Text = "Ngày công 6 tháng gần nhất",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Docking = Docking.Top
            };
            dashboardColumn.Titles.Add(titleCol);

            Series sCol = new Series { ChartType = SeriesChartType.Column, Color = Color.RoyalBlue };

            // Loop 6 tháng gần nhất
            // ----- BIỂU ĐỒ CỘT: Ngày công 6 tháng gần nhất -----
            var columnChart = new Chart
            {
                Size = new Size(520, 260),
                BackColor = Color.White,
                Location = new Point(270, 800) // vị trí trên dashboard
            };

            // Tạo ChartArea
            var columnArea = new ChartArea { BackColor = Color.Transparent };
            columnChart.ChartAreas.Add(columnArea);

            // Tạo Title
            var columnTitle = new Title
            {
                Text = "Ngày công 6 tháng gần nhất",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Docking = Docking.Top
            };
            columnChart.Titles.Add(columnTitle);

            // Series cột
            var columnSeries = new Series
            {
                ChartType = SeriesChartType.Column,
                Color = Color.RoyalBlue
            };

            // Vẽ dữ liệu thật
            for (int i = 5; i >= 0; i--)
            {
                DateTime targetMonth = DateTime.Now.AddMonths(-i);
                int count = 0;

                foreach (var nv in dsNhanVien)
                {
                    foreach (var s in nv.LichSuChamCong)
                    {
                        // Tách ngày
                        string[] parts = s.Split('|');
                        if (parts.Length < 3) continue;

                        string ngayStr = parts[0].Trim();
                        DateTime dt;

                        // Parse chuẩn dd/MM/yyyy
                        if (DateTime.TryParseExact(ngayStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                        {
                            if (dt.Month == targetMonth.Month && dt.Year == targetMonth.Year)
                            {
                                count++;
                            }
                        }
                    }
                }

                // Thêm vào Series
                columnSeries.Points.AddXY(targetMonth.ToString("MM/yyyy"), count);
            }

            // Thêm series vào chart
            columnChart.Series.Add(columnSeries);

            // Thêm chart vào dashboard
            content.Controls.Add(columnChart);

            // ----- BIỂU ĐỒ TRÒN (Phòng ban) -----
            Chart dashboardPie = new Chart { Size = new Size(350, 260), BackColor = Color.White, Location = new Point(820, y) };
            ChartArea areaPie = new ChartArea { BackColor = Color.Transparent };
            dashboardPie.ChartAreas.Add(areaPie);

            Title titlePie = new Title
            {
                Text = "Tỷ lệ phòng ban",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Docking = Docking.Top
            };
            dashboardPie.Titles.Add(titlePie);

            Series sPie = new Series { ChartType = SeriesChartType.Pie, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            sPie.Label = "#AXISLABEL";
            sPie["PieLabelStyle"] = "Inside";

            // Dữ liệu thật từ dsNhanVien
            var dataStructure = dsNhanVien.GroupBy(n => n.Phong).Select(g => new { Key = g.Key, Value = g.Count() });
            foreach (var d in dataStructure)
            {
                int i = sPie.Points.AddXY(d.Key, d.Value);
                switch (d.Key)
                {
                    case "Kinh doanh": sPie.Points[i].Color = Color.FromArgb(0, 89, 137); break;
                    case "CNTT": sPie.Points[i].Color = Color.FromArgb(211, 47, 47); break;
                    case "Kế toán": sPie.Points[i].Color = Color.FromArgb(255, 179, 71); break;
                    case "Nhân sự": sPie.Points[i].Color = Color.FromArgb(66, 133, 244); break;
                    default: sPie.Points[i].Color = Color.Gray; break;
                }
            }

            dashboardPie.Series.Add(sPie);

            Legend legendPie = new Legend { Docking = Docking.Bottom, Alignment = StringAlignment.Center, Font = new Font("Segoe UI", 9) };
            dashboardPie.Legends.Add(legendPie);

            content.Controls.Add(dashboardPie);

            // ----- BIỂU ĐỒ ĐƯỜNG (Giờ tăng ca tuần) -----
            Chart dashboardLine = new Chart { Size = new Size(350, 260), BackColor = Color.White, Location = new Point(1200, y) };
            ChartArea areaLine = new ChartArea { BackColor = Color.Transparent };
            dashboardLine.ChartAreas.Add(areaLine);

            Title titleLine = new Title
            {
                Text = "Giờ tăng ca theo tuần",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Docking = Docking.Top
            };
            dashboardLine.Titles.Add(titleLine);

            Series sLine = new Series { ChartType = SeriesChartType.Line, BorderWidth = 3, Color = Color.MediumPurple };

            // Dữ liệu thật từ dsNhanVien
            for (int i = 6; i >= 0; i--)
            {
                DateTime targetDate = DateTime.Now.Date.AddDays(-i);
                double sumOtDay = dsNhanVien.Sum(nv => nv.DsDonTangCa
                                    .Where(d => d.Day == targetDate.Day && d.Month == targetDate.Month)
                                    .Sum(d => d.Hours));
                sLine.Points.AddXY(targetDate.ToString("dd/MM"), sumOtDay);
            }

            dashboardLine.Series.Add(sLine);
            content.Controls.Add(dashboardLine);
        }
        Label AddCard(string title, int x, int y, Color color)
        {
            Panel p = new Panel();
            p.Location = new Point(x, y);
            p.Size = new Size(220, 110);
            p.BackColor = Color.White;
            content.Controls.Add(p);

            Label t = new Label();
            t.Text = title;
            t.ForeColor = Color.Gray;
            t.Location = new Point(15, 18);
            t.AutoSize = true;
            p.Controls.Add(t);

            Label v = new Label();
            v.Text = "0";
            v.ForeColor = color;
            v.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            v.Location = new Point(15, 45);
            v.AutoSize = true;
            p.Controls.Add(v);

            return v; // 👈 quan trọng
        }
        // Cập nhật các Label khi có thay đổi dữ liệu
        // Hàm tính toán tổng số trạng thái cho mỗi loại (Nghỉ, Nghỉ phép, Tăng ca, Đi làm)



        void LoadData()
        {
            try
            {
                // ===== NHÂN VIÊN =====
                if (!File.Exists(dataFile))
                {
                    dsNhanVien.Clear();
                    lastResetDate = DateTime.MinValue;
                }
                else
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };

                    var json = File.ReadAllText(dataFile);

                    // Deserialize như cũ
                    var list = JsonConvert.DeserializeObject<List<NhanVien>>(json, settings);

                    dsNhanVien.Clear();
                    foreach (var nv in list ?? new List<NhanVien>())
                        dsNhanVien.Add(nv);

                    // Kiểm tra xem trong dsNhanVien có metadata lastResetDate không
                    // Nếu bạn lưu lastResetDate trước đây, parse từ 1 file riêng hoặc từ 1 field trong JSON
                    // Ví dụ bạn có 1 file lastResetDate.txt
                    string dateFile = Path.Combine(Path.GetDirectoryName(dataFile), "lastResetDate.txt");
                    if (File.Exists(dateFile))
                    {
                        DateTime tmp;
                        if (DateTime.TryParse(File.ReadAllText(dateFile), out tmp))
                            lastResetDate = tmp;
                    }
                }

                // ===== THÔNG BÁO =====
                if (File.Exists(notiFile))
                {
                    var jsonNoti = File.ReadAllText(notiFile);
                    var listNoti = JsonConvert.DeserializeObject<BindingList<ThongBao>>(jsonNoti);

                    dsThongBao.Clear();
                    if (listNoti != null)
                        foreach (var tb in listNoti)
                            dsThongBao.Add(tb);
                }
                else
                {
                    dsThongBao.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load dữ liệu: " + ex.Message);
            }
        }

        // ===================== Save Data =====================
        void SaveData()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dataFile));

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                var json = JsonConvert.SerializeObject(dsNhanVien.ToList(), settings);
                File.WriteAllText(dataFile, json);

                // ===== Lưu ngày reset =====
                string dateFile = Path.Combine(Path.GetDirectoryName(dataFile), "lastResetDate.txt");
                File.WriteAllText(dateFile, lastResetDate.ToString("yyyy-MM-dd"));

                // ===== THÔNG BÁO =====
                File.WriteAllText(notiFile,
                    JsonConvert.SerializeObject(dsThongBao.ToList(), Newtonsoft.Json.Formatting.Indented));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi save: " + ex.Message);
            }
        }

        void BtnCSV_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "CSV file (*.csv)|*.csv";
            save.FileName = "DanhSachNhanVien.csv";

            if (save.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(save.FileName, false, Encoding.UTF8))
                {
                    // HEADER CHUẨN (phải match import)
                    sw.WriteLine("Ma,Ten,Phong,CV,NgaySinh,GioiTinh,CCCD,DiaChi,DienThoai,Email,DanToc,TonGiao,LoaiHopDong");

                    foreach (NhanVien nv in dsNhanVien)
                    {
                        sw.WriteLine(string.Join(",",
                            nv.Ma,
                            nv.Ten,
                            nv.Phong,
                            nv.CV,
                            nv.NgaySinh.ToString("yyyy-MM-dd"),
                            nv.GioiTinh,
                            $"=\"{nv.CCCD}\"",
                            $"\"{nv.DiaChi}\"",
                            $"=\"{nv.DienThoai}\"",
                            nv.Email,
                            nv.DanToc,
                            nv.TonGiao,
                            nv.LoaiHopDong
                            
                        ));
                    }
                }

                MessageBox.Show("Xuất CSV OK!");
            }
        }

        private Label AddCard(string title, string value, int x, int y, Color color)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(230, 110),
                BackColor = Color.White
            };

            Panel line = new Panel
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = color
            };

            Label lbTitle = new Label
            {
                Text = title,
                Location = new Point(15, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            Label lbValue = new Label
            {
                Text = value,
                Location = new Point(15, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = color
            };

            card.Controls.Add(line);
            card.Controls.Add(lbTitle);
            card.Controls.Add(lbValue);

            content.Controls.Add(card);

            return lbValue;
        }
        
        void BtnTongQuan_Click(object sender, EventArgs e)
        {
            if (employeePanel != null)
                employeePanel.Hide();
            UpdateThongKe();
            content.Show();
            content.BringToFront();
        }
        
        private BindingSource nvBindingSource = new BindingSource();
        void BtnNhanVien_Click(object sender, EventArgs e)
        {
            
            if (employeePanel == null)
            {
                
                employeePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 243, 247) };

                // --- TIÊU ĐỀ ---
                Label title = new Label { Text = "Hồ Sơ Nhân Sự", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(30, 20), AutoSize = true };
                employeePanel.Controls.Add(title);

                // --- THANH CÔNG CỤ ---
                Panel toolBar = new Panel { Location = new Point(30, 85), Size = new Size(1250, 70), BackColor = Color.White };
                toolBar.Paint += (s, ev) => ControlPaint.DrawBorder(ev.Graphics, toolBar.ClientRectangle, Color.FromArgb(220, 220, 220), ButtonBorderStyle.Solid);

                TextBox txtSearch = new TextBox { Location = new Point(20, 22), Width = 300, Font = new Font("Segoe UI", 11), Text = "🔍 Tìm kiếm...", ForeColor = Color.Gray };
                txtSearch.Enter += (s, ev) => { if (txtSearch.Text == "🔍 Tìm kiếm...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; } };
                txtSearch.Leave += (s, ev) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "🔍 Tìm kiếm..."; txtSearch.ForeColor = Color.Gray; } };

                ComboBox cbFilter = new ComboBox { Location = new Point(330, 22), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
                cbFilter.Items.AddRange(new string[] { "Tất cả phòng ban", "Nhân sự", "Kế toán", "CNTT", "Kinh doanh" });
                cbFilter.SelectedIndex = 0;

                Button btnAdd = CreateModernButton("➕ Thêm mới", 750, 15, Color.FromArgb(52, 152, 219));
                Button btnDelete = CreateModernButton("🗑 Xóa ", 910, 15, Color.FromArgb(231, 76, 60));
                Button btnImport = CreateModernButton("📂 Nhập CSV", 1070, 15, Color.FromArgb(46, 204, 113));

                toolBar.Controls.AddRange(new Control[] { txtSearch, cbFilter, btnAdd, btnDelete, btnImport });
                employeePanel.Controls.Add(toolBar);

                // --- BẢNG DỮ LIỆU ---

                Panel gridCard = new Panel { Location = new Point(30, 170), Size = new Size(1250, 580), BackColor = Color.White };
                DataGridView grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowHeadersVisible = false, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowTemplate = { Height = 40 }, Font = new Font("Segoe UI", 10), ReadOnly = true };

                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.EnableHeadersVisualStyles = false;
                grid.AutoGenerateColumns = false;

                grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Ma", HeaderText = "MÃ NV", FillWeight = 60 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Ten", HeaderText = "HỌ VÀ TÊN", FillWeight = 160 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phong", HeaderText = "PHÒNG BAN", FillWeight = 100 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CV", HeaderText = "CHỨC VỤ", FillWeight = 100 });
                grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TrangThai", HeaderText = "TRẠNG THÁI", FillWeight = 100 });

                // --- HÀM TÔ MÀU TRẠNG THÁI (Sửa lỗi 'e' bằng cách đổi tên tham số thành 'args') ---
                grid.CellFormatting += (s, args) => {
                    if (grid.Columns[args.ColumnIndex].HeaderText == "TRẠNG THÁI" && args.Value != null)
                    {
                        string status = args.Value.ToString();
                        if (status == "Thực tập") { args.CellStyle.ForeColor = Color.Orange; args.CellStyle.Font = new Font(grid.Font, FontStyle.Bold); }
                        else if (status == "Chính thức") { args.CellStyle.ForeColor = Color.Green; args.CellStyle.Font = new Font(grid.Font, FontStyle.Bold); }
                        else if (status == "Thôi việc") { args.CellStyle.ForeColor = Color.Red; args.CellStyle.Font = new Font(grid.Font, FontStyle.Bold); }
                    }
                };

                grid.DataSource = dsNhanVien;
                gridCard.Controls.Add(grid);
                employeePanel.Controls.Add(gridCard);

                // --- LOGIC LỌC ---
                void RunFilter()
                {
                    string key = (txtSearch.Text == "🔍 Tìm kiếm...") ? "" : txtSearch.Text.ToLower();
                    string phong = cbFilter.Text;
                    var filtered = dsNhanVien.Where(nv =>
                        (nv.Ten.ToLower().Contains(key) || nv.Ma.ToLower().Contains(key)) &&
                        (phong == "Tất cả phòng ban" || nv.Phong == phong)).ToList();
                    grid.DataSource = new BindingList<NhanVien>(filtered);
                }

                txtSearch.TextChanged += (s, ev) => RunFilter();
                cbFilter.SelectedIndexChanged += (s, ev) => RunFilter();

                // --- DOUBLE CLICK SỬA ---
                grid.CellDoubleClick += (s, ev) =>
                {
                    if (ev.RowIndex >= 0)
                    {
                        var selectedNV = grid.Rows[ev.RowIndex].DataBoundItem as NhanVien;
                        if (selectedNV != null && new FormNhanVien(selectedNV).ShowDialog() == DialogResult.OK)
                        {
                            RunFilter();
                            SaveData();
                        }
                    }
                };

                // --- NÚT XÓA ---
                btnDelete.Click += (s, ev) =>
                {
                    if (grid.SelectedRows.Count > 0)
                    {
                        var item = grid.SelectedRows[0].DataBoundItem as NhanVien;
                        if (item != null && MessageBox.Show("Xác nhận xóa " + item.Ten + "?", "Xóa", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            dsNhanVien.Remove(item);
                            RunFilter();
                            SaveData();
                        }
                    }
                    else { MessageBox.Show("Vui lòng chọn dòng cần xóa!"); }
                };
                UpdateThongKe();

                // --- NÚT THÊM ---
                btnAdd.Click += (s, ev) =>
                {
                    Form f = new Form { Text = "Chọn loại HĐ", Size = new Size(300, 180), StartPosition = FormStartPosition.CenterParent };
                    ComboBox cb = new ComboBox { Items = { "Thử việc", "1 năm", "Không thời hạn" }, Location = new Point(50, 30), Width = 180 };
                    Button btnOk = new Button { Text = "Tiếp tục", Location = new Point(50, 80) };
                    btnOk.Click += (s2, ev2) => { f.DialogResult = DialogResult.OK; f.Close(); };
                    f.Controls.AddRange(new Control[] { cb, btnOk });

                    if (f.ShowDialog() == DialogResult.OK && cb.SelectedItem != null)
                    {
                        string loaiHD = cb.SelectedItem.ToString();
                        NhanVien moi = (loaiHD == "Thử việc") ? (NhanVien)new NVPartTime() : (NhanVien)new NVFullTime();
                        moi.Ma = "NV" + (dsNhanVien.Count + 1).ToString("000");
                        moi.Ten = "Nhân viên mới";
                        moi.LoaiHopDong = loaiHD;

                        // Set the status based on the contract type
                        if (loaiHD == "Thử việc")
                        {
                            moi.TrangThai = "Thực tập";  // For "Thử việc", set status to "Thực tập"
                        }
                        else if (loaiHD == "1 năm" || loaiHD == "Không thời hạn")
                        {
                            moi.TrangThai = "Chính thức";  // For "1 năm" and "Không thời hạn", set status to "Chính thức"
                        }

                        if (new FormNhanVien(moi).ShowDialog() == DialogResult.OK)
                        {
                            dsNhanVien.Add(moi);
                            RunFilter();
                            SaveData();
                        }
                    }
                };
                UpdateThongKe();

                // --- NÚT IMPORT ---
                btnImport.Click += (s, ev) =>
                {
                    OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV|*.csv" };
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        var lines = File.ReadAllLines(ofd.FileName);
                        for (int i = 1; i < lines.Length; i++)
                        {
                            var c = lines[i].Split(',');
                            if (c.Length < 4) continue;
                            NhanVien nv = (c[4].Trim() == "Thử việc") ? (NhanVien)new NVPartTime() : (NhanVien)new NVFullTime();
                            nv.Ma = c[0].Trim(); nv.Ten = c[1].Trim(); nv.Phong = c[2].Trim(); nv.CV = c[3].Trim(); nv.LoaiHopDong = c[4].Trim();
                            dsNhanVien.Add(nv);
                        }
                        RunFilter(); SaveData(); UpdateThongKe(); MessageBox.Show("Đã nhập thành công!");
                    }
                };
                

                this.Controls.Add(employeePanel);
            }
            employeePanel.Show();
            employeePanel.BringToFront();
        }
        // Hàm hỗ trợ tạo Button Modern
        private Button CreateModernButton(string text, int x, int y, Color themeColor)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(150, 40),
                BackColor = themeColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;

            // Hiệu ứng hover làm đậm màu hơn
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(
                Math.Max(0, themeColor.R - 25),
                Math.Max(0, themeColor.G - 25),
                Math.Max(0, themeColor.B - 25));
            btn.MouseLeave += (s, e) => btn.BackColor = themeColor;

            return btn;
        }

        // =======================================================
        // NÂNG CẤP TAB CHẤM CÔNG
        // ✔ Lịch sử đẹp hơn
        // ✔ Tổng giờ làm hiển thị ngay card nhân viên
        // =======================================================
        void BtnChamCong_Click(object sender, EventArgs e)
        {
            CheckResetNgayMoi();
            homePanel?.Hide();
            employeePanel?.Hide();
            attendancePanel?.Hide();
            lichPanel?.Hide();

            if (attendancePanel == null)
            {
                attendancePanel = new Panel();
                attendancePanel.Dock = DockStyle.Fill;
                attendancePanel.BackColor = Color.FromArgb(245, 247, 250);
                this.Controls.Add(attendancePanel);
            }
            else
            {
                attendancePanel.Controls.Clear();
            }

            attendancePanel.Show();
            attendancePanel.BringToFront();

            int W = this.ClientSize.Width - 240;
            int H = this.ClientSize.Height - 85;

            // ================= LEFT =================
            Panel left = new Panel();
            left.Location = new Point(15, 15);
            left.Size = new Size(W - 370, H - 30);
            left.BackColor = Color.White;
            attendancePanel.Controls.Add(left);

            Label lbTitle = new Label();
            lbTitle.Text = "Bảng chấm công hôm nay";
            lbTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lbTitle.Location = new Point(20, 15);
            lbTitle.AutoSize = true;
            left.Controls.Add(lbTitle);

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Location = new Point(20, 55);
            flow.Size = new Size(left.Width - 40, left.Height - 75);
            flow.AutoScroll = true;
            flow.WrapContents = true;
            left.Controls.Add(flow);

            // ================= RIGHT =================
            Panel right = new Panel();
            right.Location = new Point(W - 340, 15);
            right.Size = new Size(320, H - 30);
            right.BackColor = Color.White;
            attendancePanel.Controls.Add(right);

            Label clock = new Label();
            clock.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            clock.ForeColor = Color.SeaGreen;
            clock.Location = new Point(20, 20);
            clock.AutoSize = true;
            right.Controls.Add(clock);

            if (clockTimer == null)
            {
                clockTimer = new Timer();
                clockTimer.Interval = 1000;
                clockTimer.Tick += (a, b) =>
                {
                    clock.Text = DateTime.Now.ToString("HH:mm:ss");
                };
                clockTimer.Start();
            }

            Label lbHistoryTitle = new Label();
            lbHistoryTitle.Text = "Lịch sử chấm công";
            lbHistoryTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lbHistoryTitle.ForeColor = Color.Gray;
            lbHistoryTitle.Location = new Point(15, 65);
            lbHistoryTitle.AutoSize = true;
            right.Controls.Add(lbHistoryTitle);

            Label line = new Label();
            line.BackColor = Color.Silver;
            line.Size = new Size(290, 1);
            line.Location = new Point(15, 90);
            right.Controls.Add(line);

            FlowLayoutPanel history = new FlowLayoutPanel();
            history.Location = new Point(15, 90);
            history.Size = new Size(290, H - 120);
            history.FlowDirection = FlowDirection.TopDown;
            history.WrapContents = false;
            history.AutoScroll = true;
            right.Controls.Add(history);

            // ================= NHÂN VIÊN =================
            flow.SuspendLayout();

            foreach (NhanVien nv in dsNhanVien)
            {
                // ✅ PHÂN QUYỀN CHUẨN
                if (!isAdmin)
                {
                    if (currentUser == null || nv.Ma != currentUser.Ma)
                        continue;
                }
                

                Panel card = new Panel();
                card.Size = new Size(300, 165);
                card.BackColor = Color.WhiteSmoke;
                card.Margin = new Padding(8);

                Label ten = new Label();
                ten.Text = nv.Ten;
                ten.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                ten.Location = new Point(15, 10);
                ten.AutoSize = true;
                card.Controls.Add(ten);

                Label phong = new Label();
                phong.Text = nv.Phong;
                phong.Location = new Point(15, 35);
                phong.AutoSize = true;
                card.Controls.Add(phong);

                Label tt = new Label();
                tt.Location = new Point(15, 60);
                tt.AutoSize = true;
                card.Controls.Add(tt);

                Label tong = new Label();
                tong.Text = nv.Tong ?? "0h";
                tong.Location = new Point(15, 85);
                tong.AutoSize = true;
                tong.ForeColor = Color.RoyalBlue;
                card.Controls.Add(tong);

                Button btn = new Button();
                btn.Size = new Size(260, 38);
                btn.Location = new Point(15, 115);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.White;
                UpdateTrangThai();
                // ===== LOAD STATE =====
                if (string.IsNullOrEmpty(nv.Vao))
                {
                    btn.Text = "CHECK IN";
                    btn.BackColor = Color.SeaGreen;

                    tt.Text = "Nghỉ";
                    tt.ForeColor = Color.Gray;
                    nv.TT = "Nghỉ";
                    
                }
                else if (string.IsNullOrEmpty(nv.Ra))
                {
                    btn.Text = "CHECK OUT";
                    btn.BackColor = Color.IndianRed;

                    tt.Text = "Đang làm (" + nv.Vao + ")";
                    tt.ForeColor = Color.SeaGreen;
                }
                else
                {
                    btn.Text = "Hoàn tất";
                    btn.BackColor = Color.Gray;
                    btn.Enabled = false;

                    tt.Text = "Vào: " + nv.Vao + " - Ra: " + nv.Ra;
                    tt.ForeColor = Color.RoyalBlue;
                }

                card.Controls.Add(btn);

                // HISTORY
                if (!string.IsNullOrEmpty(nv.Vao))
                    AddHistory(history, nv.Ten + " check in " + nv.Vao, Color.SeaGreen);

                if (!string.IsNullOrEmpty(nv.Ra))
                    AddHistory(history, nv.Ten + " check out " + nv.Ra, Color.RoyalBlue);

                // DOUBLE CLICK
                card.DoubleClick += (x1, y1) =>
                {
                    new FormLichSuChamCong(nv).ShowDialog();
                };

                foreach (Control c in card.Controls)
                {
                    c.DoubleClick += (x2, y2) =>
                    {
                        new FormLichSuChamCong(nv).ShowDialog();
                    };
                }

                // CLICK CHECK
                btn.Click += (x3, y3) =>
                {
                    if (string.IsNullOrEmpty(nv.Vao))
                    {
                        nv.Vao = DateTime.Now.ToString("HH:mm");

                        btn.Text = "CHECK OUT";
                        btn.BackColor = Color.IndianRed;

                        tt.Text = "Đang làm (" + nv.Vao + ")";
                        tt.ForeColor = Color.SeaGreen;

                        AddHistory(history, nv.Ten + " check in", Color.SeaGreen);
                    }
                    else if (string.IsNullOrEmpty(nv.Ra))
                    {
                        nv.Ra = DateTime.Now.ToString("HH:mm");

                        TimeSpan vao = TimeSpan.Parse(nv.Vao);
                        TimeSpan ra = TimeSpan.Parse(nv.Ra);

                        double gio = Math.Max(0, (ra - vao).TotalHours);
                        nv.Tong = gio.ToString("0.0") + "h";

                        tong.Text = nv.Tong;

                        btn.Text = "Hoàn tất";
                        btn.Enabled = false;
                        btn.BackColor = Color.Gray;

                        tt.Text = "Vào: " + nv.Vao + " - Ra: " + nv.Ra;
                        tt.ForeColor = Color.RoyalBlue;

                        nv.LichSuChamCong.Insert(0,
                            DateTime.Now.ToString("dd/MM/yyyy") + " | " +
                            nv.Vao + " - " + nv.Ra + " | " + nv.Tong);

                        AddHistory(history, nv.Ten + " check out", Color.RoyalBlue);
                    }

                    SaveData();
                    UpdateTrangThai(); // Cập nhật trạng thái cho tất cả nhân viên
                    UpdateThongKe();   // Cập nhật thống kê
                };

                flow.Controls.Add(card);
            }

            flow.ResumeLayout();
        }
        void AddHistory(FlowLayoutPanel panel, string text, Color color)
        {
            Panel item = new Panel();
            item.Size = new Size(255, 55);
            item.BackColor = Color.FromArgb(248, 249, 252);
            item.Margin = new Padding(3);

            Panel bar = new Panel();
            bar.BackColor = color;
            bar.Size = new Size(5, 55);
            item.Controls.Add(bar);

            Label lb = new Label();
            lb.Text = text;
            lb.Location = new Point(12, 8);
            lb.Size = new Size(235, 38);
            lb.Font = new Font("Segoe UI", 9);
            item.Controls.Add(lb);

            panel.Controls.Add(item);
            panel.Controls.SetChildIndex(item, 0);
        }
        // Modified method to update the statuses of all employees in dsNhanVien
        private void UpdateTrangThai()
        {
            foreach (var nv in dsNhanVien)
            {
                if (string.IsNullOrEmpty(nv.Vao))
                {
                    nv.TT = "Nghỉ"; // If 'Vao' is null or empty, set status to "Nghỉ"
                }
                else if (string.IsNullOrEmpty(nv.Ra))
                {
                    nv.TT = "Đang làm"; // If 'Ra' is null or empty, set status to "Đang làm"
                }
                else
                {
                    nv.TT = "Hoàn tất"; // If both 'Vao' and 'Ra' are filled, set status to "Hoàn tất"
                }
            }
        }
        // ===================================================
        // PANEL DÙNG CHUNG CHO 3 TAB
        // ===================================================
        void EnsureLichPanel()
        {
            if (lichPanel == null)
            {
                lichPanel = new Panel();
                lichPanel.Dock = DockStyle.Fill;
                lichPanel.BackColor = Color.FromArgb(245, 247, 250);
                this.Controls.Add(lichPanel);
            }

            homePanel?.Hide();
            employeePanel?.Hide();
            attendancePanel?.Hide();

            lichPanel.Controls.Clear();
            lichPanel.Show();
            lichPanel.BringToFront();
        }

        // ===================================================
        // TAB 1: NGÀY CÔNG
        // ===================================================

        // 1. HÀM HIỂN THỊ TAB CHẤM CÔNG
        void ShowTabNgayCong()
        {
            EnsureLichPanel();
            lichPanel.Controls.Clear();
            lichPanel.AutoScroll = true;

            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            // Header
            Label title = new Label { Text = $"📅 Bảng chấm công tháng {currentMonth}/{currentYear}", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            lichPanel.Controls.Add(title);

            // Table
            TableLayoutPanel table = new TableLayoutPanel { Location = new Point(20, 70), AutoSize = true, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single, Padding = new Padding(5) };
            table.ColumnCount = daysInMonth + 1;
            table.RowCount = dsNhanVien.Count + 2;

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            for (int i = 0; i < daysInMonth; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));

            // Vẽ Header Thứ & Ngày
            table.Controls.Add(new Label { Text = "Nhân viên", Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter }, 0, 0);

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(currentYear, currentMonth, day);
                bool isSun = (date.DayOfWeek == DayOfWeek.Sunday);

                // Hàng 0: Thứ
                table.Controls.Add(new Label { Text = date.ToString("ddd"), Font = new Font("Segoe UI", 7), TextAlign = ContentAlignment.MiddleCenter, BackColor = isSun ? Color.LightGray : Color.White }, day, 0);
                // Hàng 1: Ngày
                table.Controls.Add(new Label { Text = day.ToString(), Font = new Font("Segoe UI", 8, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter, BackColor = isSun ? Color.LightGray : Color.White }, day, 1);
            }

            // Vẽ nội dung nhân viên
            ToolTip toolTip = new ToolTip();
            int rowIdx = 2;
            foreach (var nv in dsNhanVien)
            {
                table.Controls.Add(new Label { Text = nv.Ten, TextAlign = ContentAlignment.MiddleLeft, AutoSize = true }, 0, rowIdx);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    string status = GetStatus(nv, day);  // Lấy trạng thái cho từng ngày
                    Button btnDay = new Button { Size = new Size(30, 30), FlatStyle = FlatStyle.Flat, BackColor = GetColorByStatus(status), Margin = new Padding(1) };
                    btnDay.FlatAppearance.BorderSize = 0;
                    toolTip.SetToolTip(btnDay, $"Ngày {day}: {status}");
                    btnDay.ContextMenuStrip = CreateStatusMenu(nv, day, btnDay, toolTip);
                    table.Controls.Add(btnDay, day, rowIdx);
                }
                rowIdx++;
            }
            lichPanel.Controls.Add(table);

            // Chú thích
            FlowLayoutPanel pnlLegend = new FlowLayoutPanel { Location = new Point(20, 480 + (dsNhanVien.Count * 35)), Size = new Size(800, 50), FlowDirection = FlowDirection.LeftToRight };
            string[] legends = { "Đi làm", "Đi trễ", "Nghỉ", "Nghỉ phép", "Lễ", "Thai sản", "Tăng ca (OT)" };
            foreach (var l in legends)
            {
                Panel colorBox = new Panel { Size = new Size(15, 15), BackColor = GetColorByStatus(l) };
                Label lbl = new Label { Text = l, AutoSize = true, Margin = new Padding(0, 0, 15, 0) };
                pnlLegend.Controls.Add(colorBox);
                pnlLegend.Controls.Add(lbl);
            }
            lichPanel.Controls.Add(pnlLegend);
        }

        // 2. TẠO MENU CHUỘT PHẢI
        ContextMenuStrip CreateStatusMenu(NhanVien nv, int day, Button btn, ToolTip tt)
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            string[] options = { "Đi làm", "Đi trễ", "Nghỉ phép", "Thai sản", "Nghỉ ", "Lễ", "Tăng ca (OT)", "Hủy ghi đè" };

            foreach (string opt in options)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(opt);
                item.Click += (s, e) => {
                    if (opt == "Hủy ghi đè")
                    {
                        if (nv.CustomStatus.ContainsKey(day)) nv.CustomStatus.Remove(day);
                    }
                    else
                    {
                        nv.CustomStatus[day] = opt;
                    }
                    string newStatus = GetStatus(nv, day);
                    btn.BackColor = GetColorByStatus(newStatus);
                    tt.SetToolTip(btn, "Ngày " + day + ": " + newStatus);
                };
                cms.Items.Add(item);
            }
            return cms;
        }

        // 3. LOGIC TRẠNG THÁI
        // Sửa lại hàm GetStatus để kiểm tra Tăng ca chính xác
        string GetStatus(NhanVien nv, int day)
        {
            DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day);

            // 1️⃣ Kiểm tra CustomStatus trước
            if (nv.CustomStatus.ContainsKey(day))
            {
                string status = nv.CustomStatus[day];
                if (status.Contains("Nghỉ phép")) return "Nghỉ phép";
                if (status.Contains("OT")) return "Tăng ca (OT)";
                return status; // trả về ghi đè
            }

            // 2️⃣ Ngày chưa tới
            if (day > DateTime.Now.Day) return "Chưa tới";

            // 3️⃣ Kiểm tra lễ
            if (DateTime.Now.Month == 5 && day == 1) return "Lễ";

            // 4️⃣ Kiểm tra nghỉ phép
            if (nv.NgayPhep.ContainsKey(day) && nv.NgayPhep[day] == 1) return "Nghỉ phép";

            // 5️⃣ Kiểm tra tăng ca
            var ot = nv.DsDonTangCa.FirstOrDefault(d => d.Day == day && d.Month == DateTime.Now.Month && d.TrangThai == "Đã duyệt");
            if (ot != null) return "Tăng ca (OT)";

            // 6️⃣ Kiểm tra giờ vào/ra hiện tại (chỉ áp dụng cho ngày hôm nay)
            if (date.Date == DateTime.Now.Date)
            {
                if (!string.IsNullOrEmpty(nv.Vao) || !string.IsNullOrEmpty(nv.Ra))
                {
                    TimeSpan gioVao;
                    if (TimeSpan.TryParse(nv.Vao, out gioVao))
                    {
                        if (gioVao > new TimeSpan(8, 0, 0))
                            return "Đi trễ";
                        else
                            return "Đi làm";
                    }
                    else
                        return "Đi làm";
                }
            }

            // 7️⃣ Ngày chủ nhật mà không đi làm
            if (date.DayOfWeek == DayOfWeek.Sunday) return "Nghỉ";

            // 8️⃣ Mặc định nghỉ
            return "Nghỉ";
        }

        // Cập nhật logic màu sắc cho các trạng thái
        Color GetColorByStatus(string status)
        {
            switch (status)
            {
                case "Đi làm": return Color.FromArgb(46, 204, 113);  // Xanh khi đi làm
                case "Đi trễ": return Color.FromArgb(231, 76, 60);   // Đỏ khi đi trễ
                case "Nghỉ phép": return Color.FromArgb(241, 196, 15);  // Vàng khi nghỉ phép
                case "Lễ": return Color.FromArgb(155, 89, 182);   // Tím khi là lễ
                case "Nghỉ": return Color.FromArgb(230, 126, 34);   // Cam khi nghỉ
                case "Thai sản": return Color.FromArgb(231, 84, 128);  // Màu cho thai sản
                case "Tăng ca (OT)": return Color.FromArgb(52, 152, 219);  // Xanh dương cho tăng ca
                default: return Color.FromArgb(236, 240, 241);   // Màu mặc định
            }
        }

        // ===================================================
        // TAB 2: TĂNG CA
        // ===================================================
        void ShowTabTangCa()
        {
            EnsureLichPanel();
            lichPanel.Controls.Clear();
            lichPanel.AutoScroll = true;

            Label title = new Label { Text = isAdmin ? "🛡 Quản lý Tăng ca (Admin)" : "⏱ Đăng ký tăng ca của tôi", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(30, 20), AutoSize = true };
            lichPanel.Controls.Add(title);

            FlowLayoutPanel flow = new FlowLayoutPanel { Location = new Point(30, 70), Size = new Size(1200, 650), AutoScroll = true };
            lichPanel.Controls.Add(flow);

            foreach (var nv in dsNhanVien)
            {
                // LỌC: Nếu là nhân viên, chỉ hiển thị card của chính họ
                if (!isAdmin)
                {
                    if (currentUser == null || nv.Ma != currentUser.Ma)
                        continue;
                }

                GroupBox group = new GroupBox { Text = nv.Ten, Size = new Size(500, 300), Margin = new Padding(10) };

                // DataGridView (Tự động hiển thị danh sách đơn)
                DataGridView dgv = new DataGridView { Location = new Point(10, 25), Size = new Size(480, 160), AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
                dgv.DataSource = new BindingSource { DataSource = nv.DsDonTangCa };

                // Buttons
                Button btnDuyet = new Button { Text = "Duyệt đơn", Size = new Size(90, 35), Location = new Point(10, 240), BackColor = Color.SeaGreen, ForeColor = Color.White, Visible = isAdmin };
                Button btnTuChoi = new Button { Text = "Từ chối", Size = new Size(90, 35), Location = new Point(110, 240), BackColor = Color.Firebrick, ForeColor = Color.White, Visible = isAdmin };
                Button btnGui = new Button { Text = "Gửi đơn mới", Size = new Size(120, 35), Location = new Point(10, 240), BackColor = Color.Goldenrod, ForeColor = Color.White, Visible = !isAdmin };

                // --- XỬ LÝ LOGIC ---

                // 1. DUYỆT (Admin)
                btnDuyet.Click += (s, e) => {
                    if (dgv.SelectedRows.Count == 0) return;
                    var don = (DonTangCa)dgv.SelectedRows[0].DataBoundItem;

                    if (don.TrangThai == "Đã duyệt")
                    {
                        MessageBox.Show("Đơn này đã được duyệt!");
                        return;
                    }

                    // Cập nhật trạng thái và màu sắc
                    don.TrangThai = "Đã duyệt";
                    nv.CustomStatus[don.Day] = $"OT: {don.Hours}h"; // Cập nhật màu trên lịch

                    dgv.Refresh();
                    SaveData(); // Lưu lại dữ liệu
                    UpdateThongKe();
                    MessageBox.Show($"Đơn tăng ca đã được duyệt: {don.Hours}h");
                };

                // 2. TỪ CHỐI (Admin)
                btnTuChoi.Click += (s, e) => {
                    if (dgv.SelectedRows.Count == 0) return;
                    var don = (DonTangCa)dgv.SelectedRows[0].DataBoundItem;

                    don.TrangThai = "Đã từ chối";
                    if (nv.CustomStatus.ContainsKey(don.Day)) nv.CustomStatus.Remove(don.Day); // Xóa khỏi lịch

                    dgv.Refresh(); SaveData();
                };

                // 3. GỬI ĐƠN (Nhân viên)
                btnGui.Click += (s, e) => {
                    // Tạo Form mới
                    Form frm = new Form
                    {
                        Text = "Đăng ký tăng ca",
                        Size = new Size(350, 350),
                        StartPosition = FormStartPosition.CenterParent
                    };

                    // Tạo các control với tên biến riêng biệt
                    NumericUpDown ot_numDay = new NumericUpDown { Location = new Point(120, 30), Minimum = 1, Maximum = 31, Value = DateTime.Now.Day };
                    NumericUpDown ot_numMonth = new NumericUpDown { Location = new Point(120, 70), Minimum = 1, Maximum = 12, Value = DateTime.Now.Month }; // Thêm trường chọn tháng
                    NumericUpDown ot_numHour = new NumericUpDown { Location = new Point(120, 110), Minimum = 1, Maximum = 24, Value = 1 };
                    TextBox ot_txtLyDo = new TextBox { Location = new Point(120, 150), Width = 180 };
                    Button ot_btnSubmit = new Button { Text = "Gửi đơn", Location = new Point(120, 200), BackColor = Color.SeaGreen, ForeColor = Color.White };

                    // Sự kiện Gửi
                    ot_btnSubmit.Click += (s2, e2) => {
                        // Gán tháng chọn từ form
                        int selectedMonth = (int)ot_numMonth.Value;

                        nv.DsDonTangCa.Add(new DonTangCa
                        {
                            Day = (int)ot_numDay.Value,
                            Month = selectedMonth, // Gán tháng chọn từ form
                            Hours = (double)ot_numHour.Value,
                            LyDo = ot_txtLyDo.Text,
                            TrangThai = "Chờ duyệt"
                        });

                        // Refresh lại DataGridView
                        dgv.DataSource = null;
                        dgv.DataSource = new BindingSource { DataSource = nv.DsDonTangCa };

                        SaveData();
                        frm.Close();
                    };

                    // Thêm control vào form (Đảm bảo add đầy đủ)
                    frm.Controls.Add(new Label { Text = "Ngày:", Location = new Point(20, 32) });
                    frm.Controls.Add(ot_numDay);
                    frm.Controls.Add(new Label { Text = "Tháng:", Location = new Point(20, 72) }); // Thêm label tháng
                    frm.Controls.Add(ot_numMonth);
                    frm.Controls.Add(new Label { Text = "Số giờ:", Location = new Point(20, 112) });
                    frm.Controls.Add(ot_numHour);
                    frm.Controls.Add(new Label { Text = "Lý do:", Location = new Point(20, 152) });
                    frm.Controls.Add(ot_txtLyDo);
                    frm.Controls.Add(ot_btnSubmit);

                    frm.ShowDialog();
                };

                group.Controls.AddRange(new Control[] { dgv, btnDuyet, btnTuChoi, btnGui });
                flow.Controls.Add(group);
            }
        }
        void ShowHistory(NhanVien nv)
        {
            Form frm = new Form { Text = "Lịch sử tăng ca - " + nv.Ten, Size = new Size(400, 300) };
            ListBox lb = new ListBox { Dock = DockStyle.Fill };

            // Đổ dữ liệu từ BindingList vào ListBox
            foreach (var item in nv.TangCa)
            {
                lb.Items.Add(item);
            }

            frm.Controls.Add(lb);
            frm.ShowDialog();
        }

        // ===================================================
        // TAB 3: NGHỈ PHÉP
        // ===================================================
        void ShowTabNghiPhep()
        {
            EnsureLichPanel();
            lichPanel.Controls.Clear();
            lichPanel.AutoScroll = true;

            Label title = new Label { Text = isAdmin ? "🛡 Quản lý đơn nghỉ (Admin)" : "💤 Đơn xin nghỉ phép", Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(30, 20), AutoSize = true };
            lichPanel.Controls.Add(title);

            FlowLayoutPanel flow = new FlowLayoutPanel { Location = new Point(30, 80), Size = new Size(1200, 650), AutoScroll = true };
            lichPanel.Controls.Add(flow);

            foreach (var nv in dsNhanVien)
            {
                if (nv == null) continue;
                if (!isAdmin)
                {
                    if (currentUser == null || nv.Ma != currentUser.Ma)
                        continue;
                }

                if (nv.DsDonNghi == null) nv.DsDonNghi = new List<DonNghiPhep>();

                GroupBox group = new GroupBox { Text = nv.Ten, Size = new Size(500, 300), Margin = new Padding(10) };
                Label lblPhep = new Label
                {
                    Text = $"Phép: {nv.DaDungPhep}/{nv.TongPhepNam}",
                    Location = new Point(350, 10),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = (nv.TongPhepNam - nv.DaDungPhep <= 0) ? Color.Red : Color.Black
                };
                group.Controls.Add(lblPhep);

                DataGridView dgv = new DataGridView { Location = new Point(10, 25), Size = new Size(480, 160), AutoGenerateColumns = true, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
                dgv.DataSource = new BindingSource { DataSource = nv.DsDonNghi };

                Button btnDuyet = new Button { Text = "Duyệt", Size = new Size(80, 35), Location = new Point(10, 240), BackColor = Color.SeaGreen, ForeColor = Color.White, Visible = isAdmin };
                Button btnTuChoi = new Button { Text = "Từ chối", Size = new Size(80, 35), Location = new Point(100, 240), BackColor = Color.Firebrick, ForeColor = Color.White, Visible = isAdmin };
                Button btnGui = new Button { Text = "Gửi đơn", Size = new Size(120, 35), Location = new Point(10, 240), BackColor = Color.Goldenrod, ForeColor = Color.White, Visible = !isAdmin };

                Action RefreshGrid = () => {
                    dgv.DataSource = null;
                    dgv.DataSource = new BindingSource { DataSource = nv.DsDonNghi };
                    lblPhep.Text = $"Phép đã dùng: {nv.DaDungPhep}/{nv.TongPhepNam}";
                };

                // Gửi đơn nghỉ phép
                btnGui.Click += (s, e) => {
                    Form frm = new Form { Text = "Gửi đơn nghỉ phép", Size = new Size(350, 300), StartPosition = FormStartPosition.CenterParent };
                    NumericUpDown num = new NumericUpDown { Location = new Point(120, 30), Minimum = 1, Maximum = 31 };
                    NumericUpDown monthNum = new NumericUpDown { Location = new Point(120, 70), Minimum = 1, Maximum = 12, Value = DateTime.Now.Month };
                    TextBox txtLyDo = new TextBox { Location = new Point(120, 110), Width = 180 };
                    TextBox txtGhiChu = new TextBox { Location = new Point(120, 150), Width = 180, Multiline = true, Height = 60 };
                    Button btnSubmit = new Button { Text = "Gửi đi", Location = new Point(120, 200) };

                    btnSubmit.Click += (s2, e2) => {
                        int selectedMonth = (int)monthNum.Value;

                        if (nv.DsDonNghi == null) nv.DsDonNghi = new List<DonNghiPhep>();

                        nv.DsDonNghi.Add(new DonNghiPhep
                        {
                            Day = (int)num.Value,
                            Month = selectedMonth,
                            LyDo = txtLyDo.Text,
                            ChuThich = txtGhiChu.Text,
                            TrangThai = "Chờ duyệt"
                        });

                        RefreshGrid();
                        SaveData();
                        frm.Close();
                    };

                    frm.Controls.AddRange(new Control[] { new Label { Text = "Ngày:" }, num, new Label { Text = "Tháng:", Location = new Point(20, 72) }, monthNum, new Label { Text = "Lý do:", Location = new Point(20, 112) }, txtLyDo, new Label { Text = "Ghi chú:", Location = new Point(20, 152) }, txtGhiChu, btnSubmit });
                    frm.ShowDialog();
                };

                // Duyệt đơn nghỉ phép
                btnDuyet.Click += (s, e) => {
                    if (dgv.SelectedRows.Count > 0)
                    {
                        var don = (DonNghiPhep)dgv.SelectedRows[0].DataBoundItem;

                        if (don.TrangThai == "Đã duyệt")
                        {
                            MessageBox.Show("Đơn này đã được duyệt!");
                            return;
                        }

                        // Kiểm tra số ngày phép còn lại
                        int conLai = nv.TongPhepNam - nv.DaDungPhep;
                        if (conLai < don.SoNgay)
                        {
                            MessageBox.Show($"Không đủ ngày phép! (Còn lại: {conLai} ngày)");
                            return;
                        }

                        // Cập nhật trạng thái và trừ ngày phép
                        don.TrangThai = "Đã duyệt";
                        nv.DaDungPhep += 1; // Tăng số phép đã dùng

                        // Cập nhật lại thông tin về số phép đã dùng
                        lblPhep.Text = $"Phép đã dùng: {nv.DaDungPhep}/{nv.TongPhepNam}";

                        // Cập nhật thông tin ngày phép
                        nv.CustomStatus[don.Day] = $"Nghỉ phép: {don.LyDo}";

                        // Lưu lại dữ liệu
                        RefreshGrid();
                        SaveData();
                        UpdateThongKe();
                        MessageBox.Show($"Đã duyệt. Đã dùng: {nv.DaDungPhep}/{nv.TongPhepNam} phép");
                    }
                };

                // Từ chối đơn nghỉ phép
                btnTuChoi.Click += (s, e) => {
                    if (dgv.SelectedRows.Count > 0)
                    {
                        var don = (DonNghiPhep)dgv.SelectedRows[0].DataBoundItem;

                        // Nếu trước đó đã duyệt thì hoàn lại phép
                        if (don.TrangThai == "Đã duyệt")
                        {
                            nv.DaDungPhep -= don.SoNgay;
                            if (nv.DaDungPhep < 0) nv.DaDungPhep = 0;
                            if (nv.CustomStatus.ContainsKey(don.Day))
                                nv.CustomStatus.Remove(don.Day);
                        }

                        don.TrangThai = "Đã từ chối";
                        MessageBox.Show("Đơn này đã bị từ chối!");
                        RefreshGrid();
                        SaveData();
                    }
                };

                group.Controls.AddRange(new Control[] { dgv, btnDuyet, btnTuChoi, btnGui });
                flow.Controls.Add(group);
            }
        }
        void ShowHistoryForm(NhanVien nv)
        {
            Form frm = new Form { Text = "Lịch sử nghỉ phép - " + nv.Ten, Size = new Size(300, 400), StartPosition = FormStartPosition.CenterParent };
            ListBox lb = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

            // Lọc các ngày có giá trị = 1 trong Dictionary NgayPhep
            if (nv.NgayPhep.Count == 0)
            {
                lb.Items.Add("Chưa có ngày nghỉ nào.");
            }
            else
            {
                foreach (var item in nv.NgayPhep)
                {
                    if (item.Value == 1)
                    {
                        lb.Items.Add("Ngày " + item.Key + "/" + DateTime.Now.Month);
                    }
                }
            }

            frm.Controls.Add(lb);
            frm.ShowDialog();
        }

        // --- GỌI HÀM NÀY ĐỂ HIỂN THỊ TAB TÍNH LƯƠNG ---
        // --- HÀM CHÍNH: HIỂN THỊ TAB TÍNH LƯƠNG ---

        void ShowTabTinhLuong()
        {
            EnsureLichPanel();
            lichPanel.Controls.Clear();
            lichPanel.AutoScroll = true;

            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };
            lichPanel.Controls.Add(flow);

            foreach (NhanVien nv in dsNhanVien)
            {
                // ✅ FIX PHÂN QUYỀN
                if (!isAdmin)
                {
                    if (currentUser == null || nv.Ma != currentUser.Ma)
                        continue;
                }

                int thang = DateTime.Now.Month;
                double thucLinh = TinhToanNhanh(nv, thang);

                Panel card = new Panel
                {
                    Size = new Size(300, 170),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(10)
                };

                // ===== TÊN =====
                card.Controls.Add(new Label
                {
                    Text = nv.Ten,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Location = new Point(15, 15),
                    AutoSize = true
                });

                // ===== LƯƠNG =====
                card.Controls.Add(new Label
                {
                    Text = $"Thực lĩnh: {thucLinh:N0} đ",
                    Location = new Point(15, 50),
                    ForeColor = Color.DarkGreen,
                    AutoSize = true
                });

                // ===== NÚT XEM =====
                Button btnView = new Button
                {
                    Text = "XEM PHIẾU LƯƠNG",
                    Location = new Point(15, 90),
                    Size = new Size(270, 35),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                btnView.Click += (s, e) => {
                    FormPhieuLuong f = new FormPhieuLuong(nv, thang, isAdmin);
                    f.ShowDialog();
                    ShowTabTinhLuong();
                };

                card.Controls.Add(btnView);

                // ================= ADMIN ONLY =================
                if (isAdmin)
                {
                    Button btnLock = new Button
                    {
                        Text = "CHỐT LƯƠNG",
                        Location = new Point(15, 130),
                        Size = new Size(270, 30),
                        BackColor = Color.IndianRed,
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };

                    btnLock.Click += (s, e) =>
                    {
                        if (!nv.IsSalaryLocked.ContainsKey(thang))
                            nv.IsSalaryLocked[thang] = false;

                        nv.IsSalaryLocked[thang] = true;

                        SaveData();

                        MessageBox.Show($"Đã chốt lương {nv.Ten} tháng {thang}!");
                        ShowTabTinhLuong();
                    };

                    card.Controls.Add(btnLock);
                }

                flow.Controls.Add(card);
            }
        }

        // Hàm phụ để tính nhanh số tiền hiển thị trên card
        // Hàm phụ để tính nhanh số tiền hiển thị trên card
        double TinhToanNhanh(NhanVien nv, int thang)
        {
            // Số ngày công thực tế: ngày công thực tế = ngày công (làm việc) + số ngày phép đã dùng
            int cong = SalaryUtils.GetCongThucTe(nv, thang) + nv.DaDungPhep;

            double ot = SalaryUtils.TinhTongOT(nv, thang) * 30000; // Tính tổng giờ tăng ca
            double phat = SalaryUtils.TinhPhatTre(nv, thang); // Tính phạt nếu có
            double luong = (nv.TinhLuong() / 26) * cong;  // Lương tính theo công chuẩn cộng với phép, có thể chỉnh theo bảng công tùy từng tháng
            double bonus = nv.MonthlyBonus.ContainsKey(thang) ? nv.MonthlyBonus[thang] : 0; // Tiền thưởng
            double penalty = nv.MonthlyPenalty.ContainsKey(thang) ? nv.MonthlyPenalty[thang] : 0; // Tiền phạt

            return luong + ot + bonus - phat - penalty;
        }
        // --- HÀM TÍNH CÔNG THỰC TẾ (Dùng cho bảng lương) ---
        private int GetCongThucTe(NhanVien nv, int thang)
        {
            int count = 0;
            foreach (var ls in nv.LichSuChamCong)
            {
                // Format: "dd/MM/yyyy | Vao - Ra | Tong"
                string[] parts = ls.Split('|');
                if (parts.Length < 2) continue;

                string datePart = parts[0].Trim();
                string timePart = parts[1].Trim(); // "Vao - Ra"

                if (DateTime.TryParse(datePart, out DateTime ngay) && ngay.Month == thang)
                {
                    string[] times = timePart.Split('-');
                    if (times.Length >= 2)
                    {
                        string vao = times[0].Trim();
                        string ra = times[1].Trim();

                        // Chỉ tính công khi có cả giờ vào và giờ ra
                        if (!string.IsNullOrEmpty(vao) && !string.IsNullOrEmpty(ra))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private double TinhPhatTre(NhanVien nv, int thang)
        {
            double tongPhat = 0;
            TimeSpan gioVaoQuyDinh = new TimeSpan(8, 0, 0); // 8:00 sáng

            foreach (string entry in nv.LichSuChamCong)
            {
                // Format lưu: "dd/MM/yyyy | 08:30 - 17:00 | 8.5h"
                string[] parts = entry.Split('|');
                if (parts.Length < 2) continue;

                string dateStr = parts[0].Trim();
                string timeStr = parts[1].Trim();

                // 1. Kiểm tra ngày tháng (đảm bảo đọc đúng format dd/MM/yyyy)
                if (DateTime.TryParseExact(dateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime ngay))
                {
                    if (ngay.Month == thang)
                    {
                        // 2. Lấy giờ vào (trước dấu gạch ngang)
                        string[] times = timeStr.Split('-');
                        string vaoStr = times[0].Trim();

                        if (TimeSpan.TryParse(vaoStr, out TimeSpan vao))
                        {
                            // 3. Nếu giờ vào > 8:00 thì tính phạt
                            if (vao > gioVaoQuyDinh)
                            {
                                double phutTre = (vao - gioVaoQuyDinh).TotalMinutes;
                                tongPhat += phutTre * 1000; // Phạt 1.000đ mỗi phút
                            }
                        }
                    }
                }
            }
            return tongPhat;
        }


        Panel reportPanel;

        private void BtnBaoCao_Click(object sender, EventArgs e)
        {
            // Tạo Panel nếu chưa có
            if (reportPanel == null)
            {
                reportPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 243, 247) };
                this.Controls.Add(reportPanel);
            }

            reportPanel.Controls.Clear(); // Xóa dữ liệu cũ để vẽ lại
            LoadData();
            CreateReportContent();
            reportPanel.BringToFront();
            reportPanel.Show();

        }

        // --- GỌI HÀM NÀY TỪ BtnBaoCao_Click ---
        private void CreateReportContent()
        {
            // Xóa dữ liệu cũ
            reportPanel.Controls.Clear();

            int currentMonth = DateTime.Now.Month;

            // ===== 1. THẺ THỐNG KÊ =====
            double totalOT = dsNhanVien.Sum(nv => nv.DsDonTangCa.Sum(d => d.Hours));
            int totalLeaves = dsNhanVien.Sum(nv => nv.DaDungPhep);
            double totalSalary = dsNhanVien.Sum(nv => TinhToanNhanh(nv, currentMonth));

            reportPanel.Controls.Add(CreateStatCard("TỔNG NHÂN VIÊN", dsNhanVien.Count.ToString(), 30, 20, Color.Blue));
            reportPanel.Controls.Add(CreateStatCard("TỔNG GIỜ TĂNG CA", totalOT.ToString("F1") + "h", 340, 20, Color.Orange));
            reportPanel.Controls.Add(CreateStatCard("TỔNG NGÀY NGHỈ PHÉP", totalLeaves.ToString(), 650, 20, Color.Red));
            reportPanel.Controls.Add(CreateStatCard("TỔNG LƯƠNG", totalSalary.ToString("N0") + "đ", 960, 20, Color.Green));

            // ===== 2. BIỂU ĐỒ TRÒN (Phòng ban) =====
            Panel piePanel = CreateChartPanel("Tỷ lệ phòng ban", 30, 150, 400, 400);
            Chart pieChart = new Chart { Dock = DockStyle.Fill, BackColor = Color.White };
            ChartArea pieArea = new ChartArea { BackColor = Color.Transparent };
            pieChart.ChartAreas.Add(pieArea);

            Legend legendPie = new Legend { Docking = Docking.Bottom, Alignment = StringAlignment.Center, Font = new Font("Segoe UI", 9) };
            pieChart.Legends.Add(legendPie);

            Series sPie = new Series { ChartType = SeriesChartType.Pie, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            sPie.Label = "#AXISLABEL";
            sPie["PieLabelStyle"] = "Inside";
            sPie.BorderColor = Color.White;

            // Dữ liệu phòng ban
            var dataStructure = dsNhanVien.GroupBy(n => n.Phong).Select(g => new { Key = g.Key, Value = g.Count() });
            foreach (var d in dataStructure)
            {
                int i = sPie.Points.AddXY(d.Key, d.Value);
                switch (d.Key)
                {
                    case "Kinh doanh": sPie.Points[i].Color = Color.FromArgb(0, 89, 137); break;
                    case "CNTT": sPie.Points[i].Color = Color.FromArgb(211, 47, 47); break;
                    case "Kế toán": sPie.Points[i].Color = Color.FromArgb(255, 179, 71); break;
                    case "Nhân sự": sPie.Points[i].Color = Color.FromArgb(66, 133, 244); break;
                    default: sPie.Points[i].Color = Color.Gray; break;
                }
            }
            pieChart.Series.Add(sPie);
            piePanel.Controls.Add(pieChart);

            // ===== 3. BIỂU ĐỒ ĐƯỜNG (Tăng ca tuần) =====
            Panel linePanel = CreateChartPanel("Tăng ca trong tuần (Giờ)", 440, 150, 400, 400);
            Chart lineChart = CreateBaseChart(linePanel, SeriesChartType.Line);

            for (int i = 6; i >= 0; i--)
            {
                DateTime targetDate = DateTime.Now.Date.AddDays(-i);
                double sumOtDay = dsNhanVien.Sum(nv => nv.DsDonTangCa
                    .Where(d => d.Day == targetDate.Day && d.Month == targetDate.Month)
                    .Sum(d => d.Hours));
                lineChart.Series[0].Points.AddXY(targetDate.ToString("dd/MM"), sumOtDay);
            }
            lineChart.Series[0].BorderWidth = 3;

            // ===== 4. BIỂU ĐỒ CỘT (Ngày công 6 tháng gần nhất) =====
            Panel columnPanel = CreateChartPanel("Ngày công 6 tháng gần nhất", 850, 150, 400, 400);
            Chart columnChart = CreateBaseChart(columnPanel, SeriesChartType.Column);

            for (int i = 5; i >= 0; i--)
            {
                DateTime targetMonth = DateTime.Now.AddMonths(-i);
                int count = 0;

                foreach (var nv in dsNhanVien)
                {
                    foreach (var s in nv.LichSuChamCong)
                    {
                        string[] parts = s.Split('|'); // tách ngày | vào-ra | tổng giờ
                        if (parts.Length < 3) continue;

                        string ngayStr = parts[0].Trim();
                        DateTime dt;
                        if (DateTime.TryParseExact(ngayStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                        {
                            if (dt.Month == targetMonth.Month && dt.Year == targetMonth.Year)
                                count++;
                        }
                    }
                }

                columnChart.Series[0].Points.AddXY(targetMonth.ToString("MM/yyyy"), count);
            }
        }

        // --- CÁC HÀM HỖ TRỢ CHUNG ---
        private Chart CreateBaseChart(Panel parent, SeriesChartType type)
        {
            Chart chart = new Chart { Dock = DockStyle.Fill };
            ChartArea area = new ChartArea(); chart.ChartAreas.Add(area);
            Series series = new Series { ChartType = type }; chart.Series.Add(series);
            parent.Controls.Add(chart);
            return chart;
        }

        private Panel CreateChartPanel(string title, int x, int y, int w, int h)
        {
            Panel p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = Color.White };
            Label lblTitle = new Label { Text = title, Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            p.Controls.Add(lblTitle);
            reportPanel.Controls.Add(p);
            return p;
        }

        private Panel CreateStatCard(string title, string value, int x, int y, Color color)
        {
            Panel p = new Panel { Location = new Point(x, y), Size = new Size(280, 110), BackColor = Color.White };
            Panel line = new Panel { Dock = DockStyle.Left, Width = 8, BackColor = color };
            p.Controls.Add(line);
            p.Controls.Add(new Label { Text = title, Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Gray });
            p.Controls.Add(new Label { Text = value, Location = new Point(20, 50), AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = color });
            return p;
        }
        private void CreateNotificationTab()
        {
            reportPanel.Controls.Clear();

            // 1. Panel danh sách thông báo
            Panel pList = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1100, 400),
                BackColor = Color.White
            };

            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = dsThongBao,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect // 🔥 QUAN TRỌNG
            };

            pList.Controls.Add(dgv);
            reportPanel.Controls.Add(pList);

            // 2. ADMIN PANEL
            if (isAdmin)
            {
                Panel pAdmin = new Panel
                {
                    Location = new Point(20, 440),
                    Size = new Size(1100, 150),
                    BackColor = Color.White
                };

                TextBox txtTitle = new TextBox
                {
                    Location = new Point(20, 20),
                    Width = 300,
                    Text = "Tiêu đề...",
                    ForeColor = Color.Gray
                };

                TextBox txtContent = new TextBox
                {
                    Location = new Point(20, 50),
                    Width = 300,
                    Height = 60,
                    Multiline = true,
                    Text = "Nội dung...",
                    ForeColor = Color.Gray
                };

                Button btnSend = new Button
                {
                    Location = new Point(340, 50),
                    Text = "GỬI",
                    Size = new Size(80, 60),
                    BackColor = Color.DeepSkyBlue,
                    ForeColor = Color.White
                };

                // 🔥 NÚT XÓA
                Button btnDelete = new Button
                {
                    Location = new Point(440, 50),
                    Text = "XÓA",
                    Size = new Size(80, 60),
                    BackColor = Color.IndianRed,
                    ForeColor = Color.White
                };

                // ===== Placeholder =====
                txtTitle.Enter += (s, e) => {
                    if (txtTitle.Text == "Tiêu đề...")
                    {
                        txtTitle.Text = "";
                        txtTitle.ForeColor = Color.Black;
                    }
                };

                txtTitle.Leave += (s, e) => {
                    if (txtTitle.Text == "")
                    {
                        txtTitle.Text = "Tiêu đề...";
                        txtTitle.ForeColor = Color.Gray;
                    }
                };

                txtContent.Enter += (s, e) => {
                    if (txtContent.Text == "Nội dung...")
                    {
                        txtContent.Text = "";
                        txtContent.ForeColor = Color.Black;
                    }
                };

                txtContent.Leave += (s, e) => {
                    if (txtContent.Text == "")
                    {
                        txtContent.Text = "Nội dung...";
                        txtContent.ForeColor = Color.Gray;
                    }
                };

                // ===== GỬI =====
                btnSend.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtTitle.Text) || txtTitle.Text == "Tiêu đề...")
                        return;

                    dsThongBao.Add(new ThongBao
                    {
                        TieuDe = txtTitle.Text,
                        NoiDung = txtContent.Text,
                        ThoiGian = DateTime.Now
                    });

                    SaveData();
                    LoadThongBaoDashboard(pThongBaoRight);
                    dgv.DataSource = null;
                    dgv.DataSource = dsThongBao;

                    txtTitle.Text = "Tiêu đề...";
                    txtTitle.ForeColor = Color.Gray;

                    txtContent.Text = "Nội dung...";
                    txtContent.ForeColor = Color.Gray;

                    MessageBox.Show("Thông báo đã được gửi!");
                };

                // ===== XÓA =====
                btnDelete.Click += (s, e) =>
                {
                    if (dgv.SelectedRows.Count == 0)
                    {
                        MessageBox.Show("Vui lòng chọn thông báo để xóa!");
                        return;
                    }

                    var tb = dgv.SelectedRows[0].DataBoundItem as ThongBao;

                    if (tb != null)
                    {
                        var confirm = MessageBox.Show(
                            "Xóa thông báo này?",
                            "Xác nhận",
                            MessageBoxButtons.YesNo
                        );

                        if (confirm == DialogResult.Yes)
                        {
                            dsThongBao.Remove(tb);

                            SaveData();
                            LoadThongBaoDashboard(pThongBaoRight);
                            dgv.DataSource = null;
                            dgv.DataSource = dsThongBao;

                            MessageBox.Show("Đã xóa thông báo!");
                        }
                    }
                };

                pAdmin.Controls.AddRange(new Control[]
                {
            txtTitle, txtContent, btnSend, btnDelete
                });

                reportPanel.Controls.Add(pAdmin);
            }
        }

        private void BtnThongBao_Click(object sender, EventArgs e)
        {
            if (reportPanel == null)
            {
                reportPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 243, 247) };
                this.Controls.Add(reportPanel);
            }

            CreateNotificationTab();
            reportPanel.BringToFront();
            reportPanel.Show();
        }

        private void CreateSettingsTab()
        {
            EnsureLichPanel();

            if (reportPanel == null)
            {
                reportPanel = new Panel
                {
                    Dock = DockStyle.Fill
                };
                this.Controls.Add(reportPanel);
            }

            reportPanel.Controls.Clear();

            // ===== LẤY ACCOUNT HIỆN TẠI =====
            var acc = allAccounts.FirstOrDefault(a => a.Username == CurrentAccount.Username);

            // ===== TITLE =====
            Label title = new Label
            {
                Text = "⚙ CÀI ĐẶT HỆ THỐNG",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            reportPanel.Controls.Add(title);

            // ================= PROFILE =================
            Panel pProfile = CreateGroup("👤 Thông tin cá nhân", 20, 70, 500, 220);

            TextBox txtName = CreateInput(pProfile, "Họ tên:", currentUser?.Ten ?? "", 50);
            TextBox txtUsername = CreateInput(pProfile, "Tài khoản:", CurrentAccount.Username, 100);
            TextBox txtRole = CreateInput(pProfile, "Quyền:", CurrentAccount.Role, 150);

            txtUsername.Enabled = false;
            txtRole.Enabled = false;

            // ================= SECURITY =================
            Panel pSecurity = CreateGroup("🔐 Đổi mật khẩu", 540, 70, 500, 220);

            TextBox txtPass = CreateInput(pSecurity, "Mật khẩu mới:", "", 50);
            TextBox txtRePass = CreateInput(pSecurity, "Nhập lại:", "", 100);

            txtPass.PasswordChar = '*';
            txtRePass.PasswordChar = '*';

            Button btnChangePass = new Button
            {
                Text = "Đổi mật khẩu",
                Location = new Point(20, 150),
                Width = 150,
                BackColor = Color.Orange,
                ForeColor = Color.White
            };

            pSecurity.Controls.Add(btnChangePass);

            // ===== LOGIC ĐỔI PASSWORD =====
            btnChangePass.Click += (s, e) =>
            {
                string newPass = txtPass.Text.Trim();
                string rePass = txtRePass.Text.Trim();

                if (string.IsNullOrEmpty(newPass))
                {
                    MessageBox.Show("Nhập mật khẩu mới!");
                    return;
                }

                if (newPass.Length < 3)
                {
                    MessageBox.Show("Mật khẩu phải >= 3 ký tự!");
                    return;
                }

                if (newPass != rePass)
                {
                    MessageBox.Show("Mật khẩu không khớp!");
                    return;
                }

                if (acc == null)
                {
                    MessageBox.Show("Không tìm thấy tài khoản!");
                    return;
                }

                acc.Password = newPass;

                SaveAccounts();

                MessageBox.Show("Đổi mật khẩu thành công!");

                txtPass.Clear();
                txtRePass.Clear();
            };

            // ================= THEME =================
            Panel pTheme = CreateGroup("🎨 Giao diện", 20, 310, 500, 220);

            ComboBox cbTheme = new ComboBox
            {
                Location = new Point(120, 50),
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cbTheme.Items.AddRange(new string[] { "Light", "Dark" });
            cbTheme.SelectedIndex = isDarkMode ? 1 : 0;

            cbTheme.SelectedIndexChanged += (s, e) =>
            {
                isDarkMode = cbTheme.SelectedIndex == 1;
                ApplyTheme(reportPanel);
            };

            pTheme.Controls.Add(new Label { Text = "Chế độ:", Location = new Point(20, 55) });
            pTheme.Controls.Add(cbTheme);

            // ================= SAVE =================
            Button btnSave = new Button
            {
                Text = "LƯU",
                Location = new Point(20, 560),
                Size = new Size(200, 45),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnSave.Click += (s, e) =>
            {
                if (currentUser != null)
                {
                    currentUser.Ten = txtName.Text.Trim();
                }

                SaveData();
                SaveAccounts();

                MessageBox.Show("Đã lưu thông tin!");
            };

            // ===== ADD =====
            reportPanel.Controls.Add(pProfile);
            reportPanel.Controls.Add(pSecurity);
            reportPanel.Controls.Add(pTheme);
            reportPanel.Controls.Add(btnSave);
        }
        private Panel CreateGroup(string title, int x, int y, int w, int h)
        {
            Panel p = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            p.Controls.Add(lbl);
            return p;
        }

        private TextBox CreateInput(Panel p, string label, string value, int y)
        {
            Label lb = new Label
            {
                Text = label,
                Location = new Point(20, y),
                AutoSize = true
            };

            TextBox txt = new TextBox
            {
                Location = new Point(150, y - 3),
                Width = 250,
                Text = value
            };

            p.Controls.Add(lb);
            p.Controls.Add(txt);

            return txt;
        }
        private void ApplyTheme(Control parent)
        {
            Color back = isDarkMode ? Color.FromArgb(28, 28, 30) : Color.White;
            Color card = isDarkMode ? Color.FromArgb(40, 40, 45) : Color.White;
            Color fore = isDarkMode ? Color.White : Color.Black;
            Color accent = Color.FromArgb(0, 122, 204);

            this.BackColor = back;

            foreach (Control c in parent.Controls)
            {
                if (c is Panel)
                {
                    c.BackColor = card;
                    c.ForeColor = fore;
                }
                else if (c is Label)
                {
                    c.ForeColor = fore;
                }
                else if (c is TextBox)
                {
                    c.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
                    c.ForeColor = fore;
                }
                else if (c is ComboBox)
                {
                    c.BackColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
                    c.ForeColor = fore;
                }
                else if (c is Button btn)
                {
                    btn.BackColor = accent;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }

                if (c.HasChildren)
                    ApplyTheme(c);
            }
        }
        private void BtnCaiDat_Click(object sender, EventArgs e)
        {
            CreateSettingsTab();

            if (reportPanel != null)
            {
                reportPanel.Show();
                reportPanel.BringToFront();
            }
        }
        private void CheckAndCreateAdmin()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyNhanVien");
            string file = Path.Combine(folder, "data_account.json");

            if (!File.Exists(file))
            {
                Directory.CreateDirectory(folder);
                var admin = new List<UserAccount> {
            new UserAccount { Username = "admin", Password = "123", Role = "Admin", MaNhanVien = "NV001" }
        };
                string json = JsonConvert.SerializeObject(admin, Formatting.Indented);
                File.WriteAllText(file, json);
            }
        }
        private void SetupAccountTab(TabPage page)
        {
            // Bảng danh sách tài khoản
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 250,
                DataSource = allAccounts, // Gán list vào đây
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Button btnAdd = new Button { Text = "Thêm TK", Top = 270, Left = 20 };
            Button btnDel = new Button { Text = "Xóa TK", Top = 270, Left = 120 };

            // Nút Thêm: Mở form tạo
            btnAdd.Click += (s, e) => {
                FormCreateUser f = new FormCreateUser(allAccounts, dsNhanVien);
                if (f.ShowDialog() == DialogResult.OK)
                {
                    SaveAccounts(); // Lưu lại sau khi thêm
                }
            };

            // Nút Xóa: Xóa dòng được chọn
            btnDel.Click += (s, e) => {
                if (dgv.SelectedRows.Count > 0)
                {
                    allAccounts.RemoveAt(dgv.SelectedRows[0].Index);
                    SaveAccounts(); // Lưu lại sau khi xóa
                    MessageBox.Show("Đã xóa tài khoản!");
                }
            };

            page.Controls.AddRange(new Control[] { dgv, btnAdd, btnDel });
        }

        private void SaveAccounts()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyNhanVien", "data_account.json");
            string json = JsonConvert.SerializeObject(allAccounts, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, json);
        }
        // Hàm này gán cho sự kiện Click của nút "Tài khoản"
        private void BtnTaiKhoan_Click(object sender, EventArgs e)
        {
            if (accountPanel == null)
            {
                accountPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(240, 243, 247)
                };

                // ===== TITLE =====
                Label title = new Label
                {
                    Text = "Quản lý tài khoản",
                    Font = new Font("Segoe UI", 24, FontStyle.Bold),
                    Location = new Point(30, 20),
                    AutoSize = true
                };
                accountPanel.Controls.Add(title);

                // ===== TOOLBAR =====
                Panel toolBar = new Panel
                {
                    Location = new Point(30, 80),
                    Size = new Size(1100, 60),
                    BackColor = Color.White
                };

                Button btnAdd = new Button
                {
                    Text = "➕ Thêm",
                    Location = new Point(20, 15),
                    Width = 120
                };

                Button btnDelete = new Button
                {
                    Text = "🗑 Xóa",
                    Location = new Point(160, 15),
                    Width = 120
                };

                toolBar.Controls.AddRange(new Control[] { btnAdd, btnDelete });
                accountPanel.Controls.Add(toolBar);

                // ===== GRID =====
                DataGridView dgv = new DataGridView
                {
                    Location = new Point(30, 160),
                    Size = new Size(1100, 500),
                    DataSource = allAccounts,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    ReadOnly = true,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false
                };

                accountPanel.Controls.Add(dgv);

                // ===== EVENTS =====

                // ADD
                btnAdd.Click += (s, ev) =>
                {
                    FormCreateUser f = new FormCreateUser(allAccounts, dsNhanVien);
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        SaveAccounts();
                        dgv.Refresh();
                    }
                };

                // DELETE
                btnDelete.Click += (s, ev) =>
                {
                    if (dgv.CurrentRow == null)
                    {
                        MessageBox.Show("Vui lòng chọn tài khoản!");
                        return;
                    }

                    var acc = dgv.CurrentRow.DataBoundItem as UserAccount;

                    if (acc == null) return;

                    if (MessageBox.Show("Xóa tài khoản " + acc.Username + " ?",
                        "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        allAccounts.Remove(acc);
                        SaveAccounts();

                        dgv.DataSource = null;
                        dgv.DataSource = allAccounts;

                        MessageBox.Show("Đã xóa!");
                    }
                };

                this.Controls.Add(accountPanel);
            }

            accountPanel.BringToFront();
            accountPanel.Show();
        }
        void LoadAccounts()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "QuanLyNhanVien",
                "data_account.json"
            );

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<UserAccount>>(json);

                allAccounts.Clear();

                foreach (var acc in list)
                    allAccounts.Add(acc);
            }
        }
        void MapUser()
        {
            if (CurrentAccount.Role == "Admin")
            {
                isAdmin = true;
                currentUser = null;
                return;
            }

            isAdmin = false;

            string target = CurrentAccount.MaNhanVien?.Trim().ToLower();

            currentUser = dsNhanVien.FirstOrDefault(nv =>
                nv.Ma != null &&
                nv.Ma.Trim().ToLower() == target
            );

            if (currentUser == null)
            {
                MessageBox.Show(
                    "❌ Không tìm thấy nhân viên!\n\n" +
                    "Account: [" + CurrentAccount.MaNhanVien + "]\n" +
                    "Danh sách:\n" +
                    string.Join("\n", dsNhanVien.Select(x => "[" + x.Ma + "]"))
                );
            }
        }
        private void LoadThongBaoDashboard(Panel container)
        {
            if (container == null) return;

            container.Controls.Clear();

            // ===== TITLE =====
            Label title = new Label
            {
                Text = "📢 Thông báo",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            container.Controls.Add(title);

            // ===== LẤY 5 THÔNG BÁO MỚI NHẤT =====
            var list = dsThongBao
                .OrderByDescending(x => x.ThoiGian)
                .Take(5)
                .ToList();

            int y = 40;

            foreach (var tb in list)
            {
                Panel card = new Panel
                {
                    Size = new Size(container.Width - 20, 70),
                    Location = new Point(10, y),
                    BackColor = Color.FromArgb(245, 247, 250)
                };

                // ===== TIÊU ĐỀ =====
                Label lbTitle = new Label
                {
                    Text = tb.TieuDe,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(10, 8),
                    AutoSize = true
                };

                // ===== NỘI DUNG =====
                Label lbContent = new Label
                {
                    Text = tb.NoiDung,
                    Font = new Font("Segoe UI", 8),
                    Location = new Point(10, 28),
                    Size = new Size(card.Width - 20, 35)
                };

                // ===== THỜI GIAN (nhỏ góc phải) =====
                Label lbTime = new Label
                {
                    Text = tb.ThoiGian.ToString("HH:mm dd/MM"),
                    Font = new Font("Segoe UI", 7),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(card.Width - 90, 8)
                };

                card.Controls.Add(lbTitle);
                card.Controls.Add(lbContent);
                card.Controls.Add(lbTime);

                container.Controls.Add(card);

                y += 80;
            }
        }
        private void CheckResetNgayMoi()
        {
            DateTime today = DateTime.Now.Date;

            // Nếu ngày hôm nay khác ngày đã reset lần trước
            if (lastResetDate != today)
            {
                foreach (var nv in dsNhanVien)
                {
                    nv.Vao = "";
                    nv.Ra = "";
                    nv.Tong = "0h";
                    nv.TT = "Nghỉ";
                }

                // Cập nhật dashboard
                UpdateThongKe();

                // Lưu ngày hôm nay đã reset
                lastResetDate = today;
            }
        }
        private void BtnDangXuat_Click(object sender, EventArgs e)
        {
            CurrentAccount = null;
            currentUser = null;
            isAdmin = false;

            this.Hide();

            LoginForm loginForm = new LoginForm();
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInAccount != null)
            {
                CurrentAccount = loginForm.LoggedInAccount;
                currentUser = dsNhanVien.FirstOrDefault(nv => nv.Ma == CurrentAccount.MaNhanVien);
                isAdmin = CurrentAccount.Role == "Admin";

                // 🔥 Xóa toàn bộ panel cũ trước khi rebuild UI
                if (sidebar != null) { this.Controls.Remove(sidebar); sidebar.Dispose(); sidebar = null; }
                if (header != null) { this.Controls.Remove(header); header.Dispose(); header = null; }
                if (content != null) { this.Controls.Remove(content); content.Dispose(); content = null; }

                // Rebuild UI
                BuildUI();
                UpdateThongKe();

                this.Show();
            }
            else
            {
                Application.Exit();
            }
        }

        private void UpdateThongKe()
        {
            int ngayHienTai = DateTime.Now.Day;
            int thangHienTai = DateTime.Now.Month;

            // Đếm nhân viên đi làm: dựa trên việc có giờ vào hôm nay hay không
            int diLam = dsNhanVien.Count(nv => !string.IsNullOrEmpty(nv.Vao));

            // Đếm tăng ca: dựa trên danh sách đơn đã duyệt (giống hàm Report)
            int tangCa = dsNhanVien.Count(nv => nv.DsDonTangCa.Any(d => d.Day == ngayHienTai && d.Month == thangHienTai && d.TrangThai == "Đã duyệt"));

            // Đếm nghỉ phép: dựa trên Dictionary NgayPhep
            int nghiPhep = dsNhanVien.Count(nv => nv.NgayPhep.ContainsKey(ngayHienTai) && nv.NgayPhep[ngayHienTai] == 1);

            // Tính số người nghỉ (Tổng - Đi làm - Nghỉ phép)
            int nghi = dsNhanVien.Count - diLam - nghiPhep;

            // Cập nhật giao diện
            lbTongNV.Text = dsNhanVien.Count.ToString();
            lbDiLam.Text = diLam.ToString();
            lbNghi.Text = Math.Max(0, nghi).ToString();
            lbTangCa.Text = tangCa.ToString();
            lbTongNV.Refresh();
            lbDiLam.Refresh();
            lbNghi.Refresh();
            lbTangCa.Refresh();
        }
        // ========================= Build Sidebar =========================
        private void BuildSidebar()
        {
            sidebar = new Panel();
            sidebar.Dock = DockStyle.Left;
            sidebar.Width = 240;
            sidebar.BackColor = Color.FromArgb(41, 128, 185);
            this.Controls.Add(sidebar);

            Label logo = new Label();
            logo.Text = "HỆ THỐNG HRM";
            logo.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            logo.ForeColor = Color.White;
            logo.Location = new Point(25, 30);
            logo.AutoSize = true;
            sidebar.Controls.Add(logo);

            string[] menu =
            {
        "🏠 Tổng quan",
        "👨‍💼 Nhân viên",
        "🕒 Chấm công",
        "📅 Ngày công",
        "⏱ Tăng ca",
        "💤 Nghỉ phép",
        "💰 Tính lương",
        "📊 Báo cáo",
        "🔔 Thông báo",
        "⚙ Cài đặt",
        "🚪 Đăng xuất",
        "👤 Tài khoản"
    };

            int y = 100;
            Button activeBtn = null;
            ToolTip tt = new ToolTip();

            foreach (string item in menu)
            {
                if (CurrentAccount.Role == "Staff" &&
                    (item.Contains("Nhân viên") ||
                     item.Contains("Ngày công") ||
                     item.Contains("Báo cáo") ||
                     item.Contains("Thông báo") ||
                     item.Contains("Tài khoản")))
                {
                    continue;
                }

                Button btn = new Button();
                btn.Text = item;
                btn.Size = new Size(210, 42);
                btn.Location = new Point(15, y);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.White;
                btn.BackColor = Color.FromArgb(41, 128, 185);
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Tag = item;

                tt.SetToolTip(btn, item);

                // Hover effect
                btn.MouseEnter += (s, e) => { if (activeBtn != btn) btn.BackColor = Color.FromArgb(52, 152, 219); };
                btn.MouseLeave += (s, e) => { if (activeBtn != btn) btn.BackColor = Color.FromArgb(41, 128, 185); };

                // Gradient effect khi active
                btn.Paint += (s, e) =>
                {
                    if (btn == activeBtn)
                    {
                        using (System.Drawing.Drawing2D.LinearGradientBrush brush =
                               new System.Drawing.Drawing2D.LinearGradientBrush(
                                   btn.ClientRectangle, Color.FromArgb(30, 97, 155), Color.FromArgb(52, 152, 219), 90F))
                        {
                            e.Graphics.FillRectangle(brush, btn.ClientRectangle);
                        }
                        TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                    }
                };

                // Click: highlight active + gọi event
                btn.Click += (s, e) =>
                {
                    if (activeBtn != null) activeBtn.Invalidate(); // vẽ lại nút cũ
                    activeBtn = btn;
                    btn.Invalidate(); // vẽ gradient

                    string text = btn.Tag.ToString();
                    switch (text)
                    {
                        case "🏠 Tổng quan": BtnTongQuan_Click(s, e); break;
                        case "👨‍💼 Nhân viên": BtnNhanVien_Click(s, e); break;
                        case "🕒 Chấm công": BtnChamCong_Click(s, e); break;
                        case "📅 Ngày công": ShowTabNgayCong(); break;
                        case "⏱ Tăng ca": ShowTabTangCa(); break;
                        case "💤 Nghỉ phép": ShowTabNghiPhep(); break;
                        case "💰 Tính lương": ShowTabTinhLuong(); break;
                        case "📊 Báo cáo": BtnBaoCao_Click(s, e); break;
                        case "🔔 Thông báo": BtnThongBao_Click(s, e); break;
                        case "⚙ Cài đặt": BtnCaiDat_Click(s, e); break;
                        case "🚪 Đăng xuất": BtnDangXuat_Click(s, e); break;
                        case "👤 Tài khoản": if (CurrentAccount.Role == "Admin") BtnTaiKhoan_Click(s, e); break;
                    }
                };

                sidebar.Controls.Add(btn);
                y += 50;
            }
        }
        


    }


}


