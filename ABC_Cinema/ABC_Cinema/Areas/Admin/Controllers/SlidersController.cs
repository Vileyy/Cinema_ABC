using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using ABC_Cinema.Models;
using ImageResizer;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class SlidersController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Admin/Sliders
        public ActionResult Index(int? page)
        {
            page = page ?? 1;
            int pageSize = 7;
            int pageNumber = (page ?? 1);

            return View(db.Sliders.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ChangeStatus(FormCollection f)
        {
            int Id_Slider = int.Parse(f["Id_Slider"]);
            int tinhTrang = int.Parse(f["tinhtrang"]);
            bool tt = false;
            if (tinhTrang == 1)
            {
                tt = true;
            }
            else
            {
                tt = false;
            }

            if (ModelState.IsValid)
            {
                var sl = db.Sliders.SingleOrDefault(s => s.Id_Slider == Id_Slider);
                sl.TrangThai = tt;
                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Slider sl, FormCollection f, HttpPostedFileBase fFileUpLoad)
        {
            ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");
            if (fFileUpLoad == null)
            {
                ViewBag.ThongBao = "Hãy chọn ảnh Silder.";
                ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var sFileName = Path.GetFileName(fFileUpLoad.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Slider"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        // Resize ảnh trước khi lưu
                        using (var image = Image.FromStream(fFileUpLoad.InputStream, true, true))
                        {
                            var instructions = new Instructions("width=1920;height=560;format=jpg;mode=stretch");
                            var resizedImage = new ImageJob(image, path, instructions);
                            resizedImage.Build();
                        }
                    }
                    sl.Id_Phim = int.Parse(f["MaPhim"]);
                    sl.AnhSlider = sFileName;
                    sl.TrangThai = true;
                    db.Sliders.InsertOnSubmit(sl);
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }

                ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim");
                return View();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int Id_Slider)
        {
            var sl = db.Sliders.SingleOrDefault(n => n.Id_Slider == Id_Slider);
            if (sl == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim", sl.Id_Phim);
            return View(sl);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            var sl = db.Sliders.SingleOrDefault(n => n.Id_Slider == int.Parse(f["iMaSlider"]));
            ViewBag.MaPhim = new SelectList(db.Phims.ToList().OrderBy(n => n.TenPhim), "Id_Phim", "TenPhim", sl.Id_Phim);

            if (ModelState.IsValid)
            {
                if (fFileUpload != null)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/Slider"), sFileName);

                    if (!System.IO.File.Exists(path))
                    {
                        // Resize ảnh trước khi lưu
                        using (var image = Image.FromStream(fFileUpload.InputStream, true, true))
                        {
                            var instructions = new Instructions("width=1920;height=560;format=jpg;mode=stretch");
                            var resizedImage = new ImageJob(image, path, instructions);
                            resizedImage.Build();
                        }
                    }

                    sl.AnhSlider = sFileName;
                }

                sl.Id_Phim = int.Parse(f["MaPhim"]);

                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return View(sl);
        }

        [HttpGet]
        public ActionResult Delete(int Id_Slider)
        {
            var sl = db.Sliders.SingleOrDefault(n => n.Id_Slider == Id_Slider);
            if (sl == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(sl);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int Id_Slider, FormCollection f)
        {
            var sl = db.Sliders.SingleOrDefault(n => n.Id_Slider == Id_Slider);
            if (sl == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            db.Sliders.DeleteOnSubmit(sl);
            db.SubmitChanges();

            return RedirectToAction("Index");
        }
    }
}