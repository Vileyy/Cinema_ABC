using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using PagedList;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class TrangTinController : Controller
    {
        dbDataContext db = new dbDataContext();
        // GET: Admin/TrangTin
        public ActionResult Index(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 10;

            return View(db.TRANGTINs.ToPagedList(iPageNum,iPageSize));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TRANGTIN tt, FormCollection f)
        {
            var tenTrang = f["sTenTrang"];
            var noiDung = f["sNoiDung"];
            if (ModelState.IsValid)
            {
                tt.TenTrang = tenTrang;
                tt.MetaTitle = tenTrang.RemoveDiacritics().Replace(" ", "-");
                tt.NoiDung = noiDung;
                tt.NgayTao = DateTime.Now;
                db.TRANGTINs.InsertOnSubmit(tt);
                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tt = db.TRANGTINs.Where(t => t.MaTT == id);
            return View(tt.SingleOrDefault());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var tenTrang = f["sTenTrang"];
            if (ModelState.IsValid)
            {
                var tt = db.TRANGTINs.Where(t => t.MaTT == int.Parse(f["maTT"])).SingleOrDefault();
                tt.TenTrang = tenTrang;
                tt.NoiDung = f["sNoiDung"];
                tt.NgayTao = DateTime.Now;
                tt.MetaTitle = tenTrang.RemoveDiacritics().Replace(" ", "-");
                db.SubmitChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("Edit");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var tt = db.TRANGTINs.Where(t => t.MaTT == id);
            return View(tt.SingleOrDefault());
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirm(int id)
        {
            var tt = db.TRANGTINs.Where(t => t.MaTT == id).SingleOrDefault();
            db.TRANGTINs.DeleteOnSubmit(tt);
            db.SubmitChanges();
            return RedirectToAction("Index");
        }
    }
}