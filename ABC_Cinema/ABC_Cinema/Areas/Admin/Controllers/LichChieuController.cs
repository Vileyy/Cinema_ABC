using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class LichChieuController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Admin/LichChieu
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.LichChieus.Select(p => p);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.RapPhim.TenRapChieu.Contains(searchString) || p.Phim.TenPhim.Contains(searchString) ||
                    p.Phong.TenPhong.Contains(searchString) || p.Id_LichChieu.ToString().Contains(searchString));
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
                sortProperty = "Id_LichChieu";
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

        [HttpGet]
        public ActionResult Create()
        {
            var today = DateTime.Now;
            var phims = db.Phims.Where(p => p.NgayCongChieu <= today && p.NgayKetThuc >= today).ToList();
            ViewBag.MaPhim = new SelectList(phims.OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");

            // Tạo danh sách rạp và danh sách phòng ban đầu
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaPhong = new SelectList(new List<SelectListItem>(), "Id_Phong", "TenPhong");
            return View();
        }

        // Hành động AJAX để lấy danh sách phòng dựa trên idRap
        [HttpPost]
        public JsonResult GetPhongs(int idRap)
        {
            var phongs = db.Phongs.Where(p => p.Id_RapChieu == idRap).OrderBy(n => n.TenPhong).ToList();
            var phongList = phongs.Select(p => new SelectListItem
            {
                Value = p.Id_Phong.ToString(),
                Text = p.TenPhong
            });
            return Json(phongList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(LichChieu lc, FormCollection f)
        {
            var today = DateTime.Now;
            var phims = db.Phims.Where(p => p.NgayCongChieu <= today && p.NgayKetThuc >= today).ToList();
            ViewBag.MaPhim = new SelectList(phims.OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaPhong = new SelectList(new List<SelectListItem>(), "Id_Phong", "TenPhong");
            var ngaychieu = Convert.ToDateTime(f["dNgayChieu"]);
            var maPhim = int.Parse(f["MaPhim"]);
            var gioBatDau = TimeSpan.Parse(f["tGioBatDau"]);
            var gioKetThuc = TimeSpan.Parse(f["tGioKetThuc"]);
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == maPhim);
            if (phim.NgayCongChieu <= ngaychieu && phim.NgayKetThuc >= ngaychieu)
            {
                if (ModelState.IsValid)
                {
                    lc.Id_Phim = maPhim;
                    lc.Id_Phong = int.Parse(f["MaPhong"]);
                    lc.Id_RapChieu = int.Parse(f["MaRap"]);
                    lc.NgayChieu = ngaychieu;
                    lc.GioBatDau = gioBatDau;
                    lc.GioKetThuc = gioKetThuc;

                    db.LichChieus.InsertOnSubmit(lc);
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.NgayChieu = "Ngày chiếu đã chọn vượt quá hạn chiếu của Phim!";
                ViewBag.GioBatDau = gioBatDau;
                ViewBag.GioKetThuc = gioKetThuc;
            }

            return View();
        }

        public ActionResult Details(int id)
        {
            var lc = db.LichChieus.SingleOrDefault(n => n.Id_LichChieu == id);
            if (lc == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(lc);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var lc = db.LichChieus.SingleOrDefault(n => n.Id_LichChieu == id);
            if (lc == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var today = DateTime.Now;
            var phims = db.Phims.Where(p => p.NgayCongChieu <= today && p.NgayKetThuc >= today).ToList();
            ViewBag.MaPhim = new SelectList(phims.OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim", lc.Id_Phim);

            // Tạo danh sách rạp và danh sách phòng ban đầu
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu", lc.Id_RapChieu);
            ViewBag.MaPhong = new SelectList(new List<SelectListItem>(), "Id_Phong", "TenPhong");
            return View(lc);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var idLichChieu = int.Parse(f["iMaLC"]);
            var lc = db.LichChieus.SingleOrDefault(n => n.Id_LichChieu == idLichChieu);
            var today = DateTime.Now;
            var phims = db.Phims.Where(p => p.NgayCongChieu <= today && p.NgayKetThuc >= today).ToList();
            ViewBag.MaPhim = new SelectList(phims.OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim", lc.Id_Phim);
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu", lc.Id_RapChieu);
            ViewBag.MaPhong = new SelectList(new List<SelectListItem>(), "Id_Phong", "TenPhong");
            var ngaychieu = Convert.ToDateTime(f["dNgayChieu"]);
            var maPhim = int.Parse(f["MaPhim"]);
            var gioBatDau = TimeSpan.Parse(f["tGioBatDau"]);
            var gioKetThuc = TimeSpan.Parse(f["tGioKetThuc"]);
            var phim = db.Phims.SingleOrDefault(n => n.Id_Phim == maPhim);
            if (phim.NgayCongChieu <= ngaychieu && phim.NgayKetThuc >= ngaychieu)
            {
                if (ModelState.IsValid)
                {
                    lc.Id_Phim = maPhim;
                    lc.Id_Phong = int.Parse(f["MaPhong"]);
                    lc.Id_RapChieu = int.Parse(f["MaRap"]);
                    lc.NgayChieu = ngaychieu;
                    lc.GioBatDau = gioBatDau;
                    lc.GioKetThuc = gioKetThuc;

                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.NgayChieu = "Ngày chiếu đã chọn vượt quá hạn chiếu của Phim!";
                ViewBag.GioBatDau = gioBatDau;
                ViewBag.GioKetThuc = gioKetThuc;
            }

            return View(lc);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var lc = db.LichChieus.SingleOrDefault(n => n.Id_LichChieu == id);
            if (lc == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(lc);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var lc = db.LichChieus.SingleOrDefault(n => n.Id_LichChieu == id);
            if (lc== null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ve = db.Ves.Where(bl => bl.Id_LichChieu == id).ToList();
            if (ve != null)
            {
                db.Ves.DeleteAllOnSubmit(ve);
                db.SubmitChanges();
            }

            db.LichChieus.DeleteOnSubmit(lc);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}