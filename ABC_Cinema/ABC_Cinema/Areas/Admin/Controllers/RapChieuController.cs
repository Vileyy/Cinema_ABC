using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class RapChieuController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Admin/RapChieu
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.RapPhims.Select(p => p);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.TenRapChieu.Contains(searchString) || p.ThanhPho.Contains(searchString) ||
                    p.QuanHuyen.Contains(searchString) || p.PhuongXa.Contains(searchString));
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
                sortProperty = "TenRapChieu";
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
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(RapPhim rapphim, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                rapphim.TenRapChieu = f["sTenRap"];
                rapphim.ThanhPho = f["sTenTP"];
                rapphim.QuanHuyen = f["sTenQH"];
                rapphim.PhuongXa = f["sTenPX"];
                rapphim.TongSoPhong = 0;
                rapphim.GioMoCua = TimeSpan.Parse(f["tGioMoCua"]);
                rapphim.GioDongCua = TimeSpan.Parse(f["tGioDongCua"]);

                db.RapPhims.InsertOnSubmit(rapphim);
                db.SubmitChanges();

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == id);
            if (rapphim == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(rapphim);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == int.Parse(f["iMaRap"]));
            if (ModelState.IsValid)
            {
                rapphim.TenRapChieu = f["sTenRap"];
                rapphim.ThanhPho = f["sTenTP"];
                rapphim.QuanHuyen = f["sTenQH"];
                rapphim.PhuongXa = f["sTenPX"];
                rapphim.TongSoPhong = int.Parse(f["iSoLuong"]);
                rapphim.GioMoCua = TimeSpan.Parse(f["tGioMoCua"]);
                rapphim.GioDongCua = TimeSpan.Parse(f["tGioDongCua"]);

                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return View(rapphim);
        }

        public ActionResult Details(int id)
        {
            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == id);
            if (rapphim == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(rapphim);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == id);
            if (rapphim == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(rapphim);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == id);
            if (rapphim == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var lc = db.LichChieus.Where(n => n.Id_RapChieu == id);
            if (lc.Count() > 0)
            {
                ViewBag.ThongBao = "Rạp chiếu này đang có Lịch chiếu <br>" +
                                   " Nếu muốn xóa thì phải hủy Lịch chiếu của rạp này!";
                return View(rapphim);
            }

            var pc = db.Phongs.Where(n => n.Id_RapChieu == id);
            if (pc.Count() > 0)
            {
                ViewBag.ThongBao = "Rạp chiếu này đang có Phòng chiếu <br>" +
                                   " Nếu muốn xóa thì phải hủy Phòng chiếu của rạp này!";
                return View(rapphim);
            }

            db.RapPhims.DeleteOnSubmit(rapphim);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}