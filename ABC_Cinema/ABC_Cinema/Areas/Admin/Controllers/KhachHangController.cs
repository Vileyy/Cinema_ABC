using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class KhachHangController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Admin/KhachHang
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.TaiKhoans.Where(p => p.PhanQuyen == "USER");

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.TenDangNhap.Contains(searchString) || p.ThongTin.TenNguoiDung.Contains(searchString) ||
                    p.ThongTin.DiaChi.Contains(searchString) || p.ThongTin.Email.Contains(searchString));
            }

            if (sortOrder == "asc")
            {
                ViewBag.SortOrder = "desc";
            }

            if (sortOrder == "desc")
            {
                ViewBag.SortOrder = "";
            }

            if (sortOrder == "")
            {
                ViewBag.SortOrder = "asc";
            }

            if (String.IsNullOrEmpty(sortProperty))
            {
                sortProperty = "TenDangNhap";
            }

            ViewBag.SortProperty = sortProperty;

            if (sortOrder == "desc")
            {
                result = result.OrderBy(sortProperty + " desc");
            }
            else
            {
                result = result.OrderBy(sortProperty);
            }

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "7", Value = "7" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });
            items.Add(new SelectListItem { Text = "12", Value = "12" });

            foreach (var item in items)
            {
                if (item.Value == size.ToString())
                {
                    item.Selected = true;
                }
            }

            ViewBag.Page = page;
            ViewBag.size = items;
            ViewBag.currentSize = size;

            page = page ?? 1;
            int iPageNum = (page ?? 1);
            int iPageSize = (size ?? 7);

            return View(result.ToPagedList(iPageNum, iPageSize));
        }

        private int SaveTT(ThongTin thongtin)
        {
            db.ThongTins.InsertOnSubmit(thongtin);
            db.SubmitChanges();
            return thongtin.Id_ThongTin;
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TaiKhoan tk, FormCollection f)
        {
            ThongTin tt = new ThongTin();
            bool gioiTinh = true;
            var gt = int.Parse(f["bGioiTinh"]);
            if (gt == 0)
            {
                gioiTinh = false;
            }

            var tenTK = f["sTenTK"];
            var bool_Tk = db.TaiKhoans.Any(t => t.TenDangNhap == tenTK && t.PhanQuyen == "USER");

            var email = f["sEmail"];
            var bool_Email = db.TaiKhoans.Any(t => t.Id_ThongTin != null && t.ThongTin.Email == email && t.PhanQuyen == "USER");

            if (bool_Tk == false)
            {
                if (bool_Email == false)
                {
                    if (ModelState.IsValid)
                    {
                
                        tt.TenNguoiDung = f["sTenKH"];
                        tt.GioiTinh = gioiTinh;
                        tt.NgaySinh = Convert.ToDateTime(f["dNgaySinh"]);
                        tt.DiaChi = f["sDiaChi"];
                        tt.Email = email;
                        tt.NgayCapNhat = DateTime.Now;
                        var id_TT = SaveTT(tt);

                        tk.TenDangNhap = tenTK;
                        tk.MatKhau = f["sMatKhau"];
                        tk.NgayDangKy = DateTime.Now;
                        tk.TinhTrang = true;
                        tk.PhanQuyen = "USER";
                        tk.Id_ThongTin = id_TT;

                        db.TaiKhoans.InsertOnSubmit(tk);
                        db.SubmitChanges();

                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ViewBag.ErrorEmail = "Email này đã được sử dụng!";
                }
            }
            else
            {
                ViewBag.ErrorTK = "Tên tài khoản này đã được sử dụng!";
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == id);
            if (tk == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(tk);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var maKH = int.Parse(f["iMaKH"]);
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == maKH);
            var tt = db.ThongTins.SingleOrDefault(t => t.Id_ThongTin == tk.Id_ThongTin);
            bool gioiTinh = true;
            var gt = int.Parse(f["bGioiTinh"]);
            if (gt == 0)
            {
                gioiTinh = false;
            }

            if (ModelState.IsValid)
            {
                tt.TenNguoiDung = f["sTenKH"];
                tt.GioiTinh = gioiTinh;
                tt.NgaySinh = Convert.ToDateTime(f["dNgaySinh"]);
                tt.DiaChi = f["sDiaChi"];
                tt.NgayCapNhat = DateTime.Now;
                db.SubmitChanges();

                return RedirectToAction("Index");
            }

            return View(tk);
        }

        public ActionResult Details(int id)
        {
            var tk = db.TaiKhoans.SingleOrDefault(n => n.Id_TaiKhoan == id);
            if (tk == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(tk);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var tk = db.TaiKhoans.SingleOrDefault(n => n.Id_TaiKhoan == id);
            if (tk == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(tk);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var tk = db.TaiKhoans.SingleOrDefault(n => n.Id_TaiKhoan == id);
            if (tk == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var id_TT = tk.Id_ThongTin;

            var ves = db.Ves.Where(n => n.Id_TaiKhoan == id).ToList();
            if (ves.Count > 0)
            {
                foreach (var ve in ves)
                {
                    var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == ve.Id_Ve).ToList();
                    if (ve_ghe != null)
                    {
                        db.Ve_Ghes.DeleteAllOnSubmit(ve_ghe);
                        db.SubmitChanges();
                    }
                }

                db.Ves.DeleteAllOnSubmit(ves);
                db.SubmitChanges();
            }

            var nd = db.NoiDungs.Where(n => n.Id_TaiKhoan == tk.Id_TaiKhoan).ToList();
            if (nd.Count > 0)
            {
                List<int> id_BinhLuanList = new List<int>();

                foreach (var n in nd)
                {
                    var bl = db.BinhLuans.SingleOrDefault(b => b.Id_BinhLuan == n.Id_BinhLuan);
                    if (bl != null)
                    {
                        id_BinhLuanList.Add(bl.Id_BinhLuan);
                    }
                }
                db.NoiDungs.DeleteAllOnSubmit(nd);
                db.SubmitChanges();

                foreach (var id_BinhLuan in id_BinhLuanList)
                {
                    var bl = db.BinhLuans.SingleOrDefault(b => b.Id_BinhLuan == id_BinhLuan);
                    db.BinhLuans.DeleteOnSubmit(bl);
                    db.SubmitChanges();
                }
            }

            db.TaiKhoans.DeleteOnSubmit(tk);
            db.SubmitChanges();

            var thongtin = db.ThongTins.SingleOrDefault(tt => tt.Id_ThongTin == id_TT);
            if (thongtin != null)
            {
                db.ThongTins.DeleteOnSubmit(thongtin);
                db.SubmitChanges();
            }

            return RedirectToAction("Index");
        }
    }
}