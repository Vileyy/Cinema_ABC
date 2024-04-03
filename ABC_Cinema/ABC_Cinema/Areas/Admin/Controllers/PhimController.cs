using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using ABC_Cinema.Models;
using System.Linq.Dynamic;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class PhimController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Admin/Phim
        /*public List<PhimViewModel> getlstPhim()
        {
            var phim = db.Phims.Join(db.LoaiPhims, p => p.Id_LoaiPhim, lp => lp.Id_LoaiPhim, (p, lp) => new { p, lp })
                .Join(db.DanhMucs, plp => plp.p.Id_DanhMuc, dm => dm.Id_DanhMuc, (plp, dm) => new { plp, dm })
                .GroupBy(lstp => new
                {
                    lstp.plp.p.Id_Phim,
                    lstp.plp.p.TenPhim,
                    lstp.plp.p.MoTa,
                    lstp.plp.p.AnhPhim,
                    lstp.plp.lp.Id_LoaiPhim,
                    lstp.plp.lp.TenLoai,
                    lstp.dm.Id_DanhMuc,
                    lstp.dm.TenDanhMuc
                })
                .Select(g => new PhimViewModel
                {
                    Id = g.Key.Id_Phim,
                    TenPhim = g.Key.TenPhim,
                    MoTa = g.Key.MoTa,
                    AnhPhim = g.Key.AnhPhim,
                    Id_TheLoai = g.Key.Id_LoaiPhim,
                    TenLoai = g.Key.TenLoai,
                    Id_DanhMuc = g.Key.Id_DanhMuc,
                    TenDanhMuc = g.Key.TenDanhMuc
                });
            return phim.ToList();
        }*/

        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.Phims.Select(p => p);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.TenPhim.Contains(searchString) || p.DaoDien.Contains(searchString) ||
                    p.DienVien.Contains(searchString));
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
                sortProperty = "TenPhim";
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
            items.Add(new SelectListItem { Text = "5", Value = "5" });
            items.Add(new SelectListItem { Text = "7", Value = "7" });
            items.Add(new SelectListItem { Text = "10", Value = "10" });

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
            int iPageSize = (size ?? 5);

            return View(result.ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaTL = new SelectList(db.LoaiPhims.ToList().OrderBy(n => n.TenLoai), "Id_LoaiPhim", "TenLoai");
            ViewBag.MaDM = new SelectList(db.DanhMucs.ToList().OrderBy(n => n.TenDanhMuc), "Id_DanhMuc", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Phim phim, FormCollection f, HttpPostedFileBase fFileUpLoad)
        {
            ViewBag.MaTL = new SelectList(db.LoaiPhims.ToList().OrderBy(n => n.TenLoai), "Id_LoaiPhim", "TenLoai");
            ViewBag.MaDM = new SelectList(db.DanhMucs.ToList().OrderBy(n => n.TenDanhMuc), "Id_DanhMuc", "TenDanhMuc");
            if (fFileUpLoad == null)
            {
                ViewBag.ThongBao = "Hãy chọn ảnh phim.";
                ViewBag.TenPhim = f["sTenPhim"];
                ViewBag.MoTa = f["sMoTa"];
                ViewBag.ThoiLuong = int.Parse(f["iThoiLuong"]);
                ViewBag.ChatLuong = f["sChatLuong"];
                ViewBag.NamPhatHanh = f["sNamPhatHanh"];
                ViewBag.DienVien = f["sDienVien"];
                ViewBag.DaoDien = f["sDaoDien"];
                ViewBag.NgayCongChieu = Convert.ToDateTime(f["dNgayCongChieu"]);
                ViewBag.NgayKetThuc = Convert.ToDateTime(f["dNgayKetThuc"]);
                ViewBag.MaTL = new SelectList(db.LoaiPhims.ToList().OrderBy(n => n.TenLoai), "Id_LoaiPhim", "TenLoai",
                    int.Parse(f["MaTL"]));
                ViewBag.MaDM = new SelectList(db.DanhMucs.ToList().OrderBy(n => n.TenDanhMuc), "Id_DanhMuc", "TenDanhMuc",
                    int.Parse(f["MaDM"]));
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var sFileName = Path.GetFileName(fFileUpLoad.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Upload"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpLoad.SaveAs(path);
                    }

                    phim.TenPhim = f["sTenPhim"];
                    phim.MoTa = f["sMoTa"];
                    phim.AnhPhim = sFileName;
                    phim.TinhTrang = true;
                    phim.Id_LoaiPhim = int.Parse(f["MaTL"]);
                    phim.Id_DanhMuc = int.Parse(f["MaDM"]);
                    phim.ThoiLuong = int.Parse(f["iThoiLuong"]);
                    phim.ChatLuong = f["sChatLuong"];
                    phim.NamPhatHanh = f["sNamPhatHanh"];
                    phim.DienVien = f["sDienVien"];
                    phim.DaoDien = f["sDaoDien"];
                    phim.NgayCongChieu = Convert.ToDateTime(f["dNgayCongChieu"]);
                    phim.NgayKetThuc = Convert.ToDateTime(f["dNgayKetThuc"]);
                    db.Phims.InsertOnSubmit(phim);
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }

                return View();
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == id);
            if (phim == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            ViewBag.MaTL = new SelectList(db.LoaiPhims.ToList().OrderBy(n => n.TenLoai), "Id_LoaiPhim", "TenLoai", phim.Id_LoaiPhim);
            ViewBag.MaDM = new SelectList(db.DanhMucs.ToList().OrderBy(n => n.TenDanhMuc), "Id_DanhMuc", "TenDanhMuc", phim.Id_DanhMuc);
            return View(phim);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == int.Parse(f["iMaPhim"]));
            ViewBag.MaTL = new SelectList(db.LoaiPhims.ToList().OrderBy(n => n.TenLoai), "Id_LoaiPhim", "TenLoai", phim.Id_LoaiPhim);
            ViewBag.MaDM = new SelectList(db.DanhMucs.ToList().OrderBy(n => n.TenDanhMuc), "Id_DanhMuc", "TenDanhMuc", phim.Id_DanhMuc);
            if (ModelState.IsValid)
            {
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Upload"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }

                    phim.AnhPhim = sFileName;
                }

                phim.TenPhim = f["sTenPhim"];
                phim.MoTa = f["sMoTa"];

                phim.Id_LoaiPhim = int.Parse(f["MaTL"]);
                phim.Id_DanhMuc = int.Parse(f["MaDM"]);
                phim.ThoiLuong = int.Parse(f["iThoiLuong"]);
                phim.ChatLuong = f["sChatLuong"];
                phim.NamPhatHanh = f["sNamPhatHanh"];
                phim.DienVien = f["sDienVien"];
                phim.DaoDien = f["sDaoDien"];
                phim.NgayCongChieu = Convert.ToDateTime(f["dNgayCongChieu"]);
                phim.NgayKetThuc = Convert.ToDateTime(f["dNgayKetThuc"]);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return View(phim);
        }

        public ActionResult Details(int id)
        {
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == id);
            if (phim == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(phim);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == id);
            if (phim == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(phim);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == id);
            if (phim == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var lc = db.LichChieus.Where(n => n.Id_Phim == id);
            if (lc.Count() > 0)
            {
                ViewBag.ThongBao = "Phim này đang có Lịch chiếu <br>" +
                                   " Nếu muốn xóa thì phải hủy Lịch chiếu của phim này!";
                return View(phim);
            }

            var binhluan = db.BinhLuans.Where(bl => bl.Id_Phim == id).ToList();
            if (binhluan != null)
            {
                foreach (var bl in binhluan)
                {
                    var noidung = db.NoiDungs.SingleOrDefault(nd => nd.Id_BinhLuan == bl.Id_BinhLuan);
                    db.NoiDungs.DeleteOnSubmit(noidung);
                    db.SubmitChanges();
                }
                db.BinhLuans.DeleteAllOnSubmit(binhluan);
                db.SubmitChanges();
            }

            db.Phims.DeleteOnSubmit(phim);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Index_TL(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 7;

            return View(db.LoaiPhims.ToList().ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult Create_TL()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create_TL(LoaiPhim tl, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                tl.TenLoai = f["sTenTL"];

                db.LoaiPhims.InsertOnSubmit(tl);
                db.SubmitChanges();

                return RedirectToAction("Index_TL");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit_TL(int id)
        {
            var tl = db.LoaiPhims.SingleOrDefault(n => n.Id_LoaiPhim == id);
            if (tl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(tl);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit_TL(FormCollection f)
        {
            var tl = db.LoaiPhims.SingleOrDefault(n => n.Id_LoaiPhim == int.Parse(f["iMaTL"]));
            if (ModelState.IsValid)
            {
                tl.TenLoai = f["sTenTL"];

                db.SubmitChanges();
                return RedirectToAction("Index_TL");
            }

            return View(tl);
        }

        [HttpGet]
        public ActionResult Delete_TL(int id)
        {
            var tl = db.LoaiPhims.SingleOrDefault(n => n.Id_LoaiPhim == id);
            if (tl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(tl);
        }

        [HttpPost, ActionName("Delete_TL")]
        public ActionResult Delete_TLConfirmed(int id, FormCollection f)
        {
            var tl = db.LoaiPhims.SingleOrDefault(n => n.Id_LoaiPhim == id);
            if (tl == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var phims = db.Phims.Where(p => p.Id_LoaiPhim == tl.Id_LoaiPhim).ToList();
            if (phims.Count > 0)
            {
                ViewBag.ThongBao = "Đang có phim thuộc Thể loại này <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Phim đang dùng Thể loại này!";
                return View(tl);
            }

            db.LoaiPhims.DeleteOnSubmit(tl);
            db.SubmitChanges();

            return RedirectToAction("Index_TL");
        }

        public ActionResult Index_DM(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 7;

            return View(db.DanhMucs.ToList().ToPagedList(iPageNum, iPageSize));
        }

        [HttpGet]
        public ActionResult Create_DM()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create_DM(DanhMuc dm, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                dm.TenDanhMuc = f["sTenDM"];

                db.DanhMucs.InsertOnSubmit(dm);
                db.SubmitChanges();

                return RedirectToAction("Index_DM");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit_DM(int id)
        {
            var dm = db.DanhMucs.SingleOrDefault(n => n.Id_DanhMuc == id);
            if (dm == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(dm);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit_DM(FormCollection f)
        {
            var dm = db.DanhMucs.SingleOrDefault(n => n.Id_DanhMuc == int.Parse(f["iMaDM"]));
            if (ModelState.IsValid)
            {
                dm.TenDanhMuc = f["sTenDM"];

                db.SubmitChanges();
                return RedirectToAction("Index_DM");
            }

            return View(dm);
        }

        [HttpGet]
        public ActionResult Delete_DM(int id)
        {
            var dm = db.DanhMucs.SingleOrDefault(n => n.Id_DanhMuc == id);
            if (dm == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(dm);
        }

        [HttpPost, ActionName("Delete_DM")]
        public ActionResult Delete_DMConfirmed(int id, FormCollection f)
        {
            var dm = db.DanhMucs.SingleOrDefault(n => n.Id_DanhMuc == id);
            if (dm == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var phims = db.Phims.Where(p => p.Id_DanhMuc == dm.Id_DanhMuc).ToList();
            if (phims.Count > 0)
            {
                ViewBag.ThongBao = "Đang có phim thuộc Danh mục này <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Phim đang dùng Danh mục này!";
                return View(dm);
            }

            db.DanhMucs.DeleteOnSubmit(dm);
            db.SubmitChanges();

            return RedirectToAction("Index_DM");
        }
    }
}