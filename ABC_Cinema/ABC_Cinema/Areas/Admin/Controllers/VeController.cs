using ABC_Cinema.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class VeController : Controller
    {
        private dbDataContext db = new dbDataContext();

        // GET: Admin/Ve
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.Ves.Select(p => p);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.LichChieu.RapPhim.TenRapChieu.Contains(searchString) || p.LichChieu.Phong.TenPhong.Contains(searchString) ||
                    p.TaiKhoan.TenDangNhap.Contains(searchString) || p.Id_Ve.ToString().Contains(searchString));
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
                sortProperty = "Id_Ve";
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

        private int SaveVe(Ve v)
        {
            db.Ves.InsertOnSubmit(v);
            db.SubmitChanges();
            return v.Id_Ve;
        }

        [HttpGet]
        public ActionResult Create()
        {
            var lichchieutoday = db.LichChieus.Where(lc => lc.NgayChieu.Date >= DateTime.Now.Date).ToList();
            var phimIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_Phim).Distinct();
            var phimsInLichChieuToday = db.Phims.Where(p => phimIdsInLichChieuToday.Contains(p.Id_Phim)).ToList();

            var rapIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_RapChieu).Distinct();
            var rapsInLichChieuToday = db.RapPhims.Where(r => rapIdsInLichChieuToday.Contains(r.Id_RapChieu)).ToList();

            ViewBag.MaPhim = new SelectList(phimsInLichChieuToday, "Id_Phim", "TenPhim");
            ViewBag.MaRap = new SelectList(rapsInLichChieuToday.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaLC_NgayChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaLC_SuatChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaGhe = "";

            return View();
        }

        [HttpPost]
        public JsonResult GetLichChieus(int phimId, int rapId)
        {
            var lichChieus = db.LichChieus.Where(lc => lc.Id_Phim == phimId && lc.Id_RapChieu == rapId).OrderBy(lc => lc.NgayChieu).ToList();
            var lichChieuList = lichChieus.Select(lc => new SelectListItem
            {
                Value = lc.NgayChieu.ToString(),
                Text = lc.NgayChieu.ToString("dd/MM/yyyy")
            });
            return Json(lichChieuList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetLichChieus_SuatChieu(int phimId, int rapId, DateTime ngaychieu)
        {
            /*var nc = Convert.ToDateTime(ngaychieu);*/
            var lichChieus = db.LichChieus.Where(lc => lc.Id_Phim == phimId && lc.Id_RapChieu == rapId && lc.NgayChieu == ngaychieu).OrderBy(lc => lc.GioBatDau).ToList();
            var lichChieuList = lichChieus.Select(lc => new SelectListItem
            {
                Value = lc.Id_LichChieu.ToString(),
                Text = lc.GioBatDau.ToString()
            });
            return Json(lichChieuList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetGhes(int lichId)
        {
            var phongId = db.LichChieus.FirstOrDefault(r => r.Id_LichChieu == lichId)?.Id_Phong;
            var ghes = db.Phong_Ghes.Where(g => g.Id_Phong == phongId).ToList();

            var gheList = ghes.Select(g => new SelectListItem
            {
                Value = g.Id_Ghe.ToString(),
                Text = g.Ghe.TenGhe
            });

            ViewBag.MaGhe = new SelectList(gheList, "Value", "Text");

            return Json(gheList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Ve v, FormCollection f)
        {
            var lichchieutoday = db.LichChieus.Where(lc => lc.NgayChieu.Date >= DateTime.Now.Date).ToList();
            var phimIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_Phim).Distinct();
            var phimsInLichChieuToday = db.Phims.Where(p => phimIdsInLichChieuToday.Contains(p.Id_Phim)).ToList();

            var rapIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_RapChieu).Distinct();
            var rapsInLichChieuToday = db.RapPhims.Where(r => rapIdsInLichChieuToday.Contains(r.Id_RapChieu)).ToList();

            ViewBag.MaPhim = new SelectList(phimsInLichChieuToday, "Id_Phim", "TenPhim");
            ViewBag.MaRap = new SelectList(rapsInLichChieuToday.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaLC_NgayChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaLC_SuatChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaGhe = "";

            var selectedGhes = new HashSet<int>();

            var selectedGhesString = Request.Form["SelectedGhes"];
            var selectedGhesArray = selectedGhesString.Split(',').Select(int.Parse);

            foreach (var gheId in selectedGhesArray)
            {
                selectedGhes.Add(gheId);
            }

            var tenTK = f["sTenTK"];
            var tk = db.TaiKhoans.Any(t => t.TenDangNhap == tenTK && t.PhanQuyen == "USER");

            if (tk == true)
            {
                if (ModelState.IsValid)
                {
                    v.Id_LichChieu = int.Parse(f["MaLC_SuatChieu"]);
                    v.Id_TaiKhoan = db.TaiKhoans.SingleOrDefault(t => t.TenDangNhap == tenTK)?.Id_TaiKhoan;
                    v.NgayDat = DateTime.Now;
                    v.GiaVe = 0;
                    v.SoluongGhe = 0;
                    var idVe = SaveVe(v);

                    List<Ve_Ghe> vg = new List<Ve_Ghe>();
                    foreach (var item in selectedGhes)
                    {
                        vg.Add(new Ve_Ghe { Id_Ve = idVe, Id_Ghe = item, TenGhe = db.Ghes.SingleOrDefault(g => g.Id_Ghe == item)?.TenGhe });
                    }
                    db.Ve_Ghes.InsertAllOnSubmit(vg);
                    db.SubmitChanges();

                    var loaiGhe = db.Ghes.SingleOrDefault(g => g.Id_Ghe == selectedGhes.FirstOrDefault())?.Id_LoaiGhe;
                    var donGiaGhe = db.LoaiGhes.SingleOrDefault(lg => lg.Id_LoaiGhe == loaiGhe)?.GiaGhe;
                    var soLuongGhe = selectedGhes.Count;

                    var update_ve = db.Ves.SingleOrDefault(ve => ve.Id_Ve == idVe);
                    update_ve.SoluongGhe = soLuongGhe;
                    update_ve.GiaVe = (decimal)(soLuongGhe * donGiaGhe);
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.TKError = "Tài khoản khách hàng này không tồn tại trong hệ thống!";
            }

            return View();
        }

        public ActionResult Details(int id)
        {
            var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == id);
            if (ve == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == id).ToList();
            var danhSachTenGhe = string.Join(",", ve_ghe.Select(item => item.TenGhe));

            ViewBag.DSGhe = danhSachTenGhe;

            return View(ve);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == id);
            if (ve == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.MaGhe = "";

            return View(ve);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var id_Ve = int.Parse(f["iMaVe"]);
            var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == id_Ve);

            ViewBag.MaGhe = "";

            var selectedGhes = new HashSet<int>();

            var selectedGhesString = Request.Form["SelectedGhes"];
            var selectedGhesArray = selectedGhesString.Split(',').Select(int.Parse);

            foreach (var gheId in selectedGhesArray)
            {
                selectedGhes.Add(gheId);
            }

            /*var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == id_Ve).ToList();
            var danhSachTenGhe = string.Join(",", ve_ghe.Select(item => item.Id_Ghe));*/

            if (ModelState.IsValid)
            {
                db.Ve_Ghes.DeleteAllOnSubmit(db.Ve_Ghes.Where(n => n.Id_Ve == id_Ve));

                List<Ve_Ghe> vg = new List<Ve_Ghe>();
                foreach (var item in selectedGhes)
                {
                    vg.Add(new Ve_Ghe { Id_Ve = id_Ve, Id_Ghe = item, TenGhe = db.Ghes.SingleOrDefault(g => g.Id_Ghe == item)?.TenGhe });
                }

                db.Ve_Ghes.InsertAllOnSubmit(vg);
                db.SubmitChanges();

                var loaiGhe = db.Ghes.SingleOrDefault(g => g.Id_Ghe == selectedGhes.FirstOrDefault())?.Id_LoaiGhe;
                var donGiaGhe = db.LoaiGhes.SingleOrDefault(lg => lg.Id_LoaiGhe == loaiGhe)?.GiaGhe;
                var soLuongGhe = selectedGhes.Count;

                var update_ve = db.Ves.SingleOrDefault(v => v.Id_Ve == id_Ve);
                update_ve.SoluongGhe = soLuongGhe;
                update_ve.GiaVe = (decimal)(soLuongGhe * donGiaGhe);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }
            return View(ve);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == id);
            if (ve == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == id).ToList();
            var danhSachTenGhe = string.Join(",", ve_ghe.Select(item => item.TenGhe));

            ViewBag.DSGhe = danhSachTenGhe;

            return View(ve);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == id);
            if (ve == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == id).ToList();
            if (ve_ghe != null)
            {
                db.Ve_Ghes.DeleteAllOnSubmit(ve_ghe);
                db.SubmitChanges();
            }

            db.Ves.DeleteOnSubmit(ve);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}