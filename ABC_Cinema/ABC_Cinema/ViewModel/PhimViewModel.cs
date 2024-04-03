using System;

namespace ABC_Cinema.ViewModel
{
    public class PhimViewModel
    {
        public int Id { get; set; }
        public string TenPhim { get; set; }
        public string AnhPhim { get; set; }
        public int ThoiLuong { get; set; }
        public string MoTa { get; set; }
        public bool TinhTrang { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string DienVien { get; set; }
        public string DaoDien { get; set; }
        public DateTime NgayCongChieu { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string NamPhatHanh { get; set; }
        public string ChatLuong { get; set; }
        public int Id_TheLoai { get; set; }
        public string TenLoai { get; set; }
        public int Id_DanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public Nullable<int> Sum { get; set; }

        public Nullable<double> Avg { get; set; }
    }
}