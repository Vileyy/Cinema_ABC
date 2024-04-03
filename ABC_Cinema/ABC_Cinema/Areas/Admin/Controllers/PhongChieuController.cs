using ABC_Cinema.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class PhongChieuController : Controller
    {
        private dbDataContext db = new dbDataContext();

        // GET: Admin/PhongChieu
        public ActionResult Index(int? page, int? size, string sortProperty, string sortOrder = "", string searchString = null)
        {
            ViewBag.Search = searchString;

            var result = db.Phongs.Select(p => p);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p =>
                    p.RapPhim.TenRapChieu.Contains(searchString) || p.LoaiPhong.TenLoaiPhong.Contains(searchString) ||
                    p.TenPhong.Contains(searchString));
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
                sortProperty = "TenPhong";
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

        public ActionResult Index_LP(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 7;

            return View(db.LoaiPhongs.ToList().ToPagedList(iPageNum, iPageSize));
        }

        public ActionResult Index_G(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 10;

            return View(db.Ghes.ToList().ToPagedList(iPageNum, iPageSize));
        }

        public ActionResult Index_LG(int? page)
        {
            int iPageNum = (page ?? 1);
            int iPageSize = 7;

            return View(db.LoaiGhes.ToList().ToPagedList(iPageNum, iPageSize));
        }

        private int SavePhong(Phong phongchieu)
        {
            db.Phongs.InsertOnSubmit(phongchieu);
            db.SubmitChanges();
            return phongchieu.Id_Phong;
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaLP = new SelectList(db.LoaiPhongs.ToList().OrderBy(n => n.TenLoaiPhong), "Id_LoaiPhong", "TenLoaiPhong");
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Phong phongchieu, FormCollection f)
        {
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaLP = new SelectList(db.LoaiPhongs.ToList().OrderBy(n => n.TenLoaiPhong), "Id_LoaiPhong", "TenLoaiPhong");
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe");
            var soluongghe = int.Parse(f["iSoLuong"]);
            var idloaighe = int.Parse(f["MaLG"]);
            var pg = db.Ghes.Where(n => n.Id_LoaiGhe == idloaighe).OrderBy(n => n.TenGhe).Take(soluongghe).ToList();
            if (pg.Count() < soluongghe)
            {
                ViewBag.ThongBao = "Số lượng ghế loại này không có đủ số lượng <br>" +
                                   " Nếu muốn dùng loại ghế này thì phải bổ sung thêm loại ghế này ở bảng Ghế!";
                return View(phongchieu);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    phongchieu.TenPhong = f["sTenPhong"];
                    phongchieu.SoLuong = soluongghe;
                    phongchieu.TinhTrang = true;
                    phongchieu.Id_RapChieu = int.Parse(f["MaRap"]);
                    phongchieu.Id_LoaiPhong = int.Parse(f["MaLP"]);
                    phongchieu.Id_LoaiGhe = idloaighe;
                    int id_phong = SavePhong(phongchieu);

                    List<Phong_Ghe> phongghes = new List<Phong_Ghe>();

                    foreach (var ghe in pg)
                    {
                        phongghes.Add(new Phong_Ghe { Id_Phong = id_phong, Id_Ghe = ghe.Id_Ghe, TinhTrang = true});
                    }

                    db.Phong_Ghes.InsertAllOnSubmit(phongghes);
                    db.SubmitChanges();

                    var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == phongchieu.Id_RapChieu);
                    rapphim.TongSoPhong = rapphim.TongSoPhong + 1;
                    db.SubmitChanges();

                    return RedirectToAction("Index");
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var phongchieu = db.Phongs.SingleOrDefault(n => n.Id_Phong == id);
            if (phongchieu == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu", phongchieu.Id_RapChieu);
            ViewBag.MaLP = new SelectList(db.LoaiPhongs.ToList().OrderBy(n => n.TenLoaiPhong), "Id_LoaiPhong", "TenLoaiPhong", phongchieu.Id_LoaiPhong);
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe", phongchieu.Id_LoaiGhe);
            return View(phongchieu);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(FormCollection f)
        {
            var id_phong = int.Parse(f["iMaPhong"]);
            var phongchieu = db.Phongs.SingleOrDefault(n => n.Id_Phong == id_phong);
            ViewBag.MaRap = new SelectList(db.RapPhims.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu", phongchieu.Id_RapChieu);
            ViewBag.MaLP = new SelectList(db.LoaiPhongs.ToList().OrderBy(n => n.TenLoaiPhong), "Id_LoaiPhong", "TenLoaiPhong", phongchieu.Id_LoaiPhong);
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe", phongchieu.Id_LoaiGhe);
            var soluongold = phongchieu.SoLuong;
            var soluongghe = int.Parse(f["iSoLuong"]);
            var idloaighe = int.Parse(f["MaLG"]);
            var pg = db.Ghes.Where(n => n.Id_LoaiGhe == idloaighe).OrderBy(n => n.TenGhe).Take(soluongghe).ToList();
            if (pg.Count() < soluongghe)
            {
                ViewBag.ThongBao = "Số lượng ghế loại này không có đủ số lượng <br>" +
                                   " Nếu muốn dùng loại ghế này thì phải bổ sung thêm loại ghế này ở bảng Ghế!";
                return View(phongchieu);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    phongchieu.TenPhong = f["sTenPhong"];
                    phongchieu.SoLuong = soluongghe;
                    phongchieu.TinhTrang = true;
                    phongchieu.Id_RapChieu = int.Parse(f["MaRap"]);
                    phongchieu.Id_LoaiPhong = int.Parse(f["MaLP"]);
                    phongchieu.Id_LoaiGhe = idloaighe;

                    db.SubmitChanges();

                    
                    if (soluongold != soluongghe)
                    {
                        db.Phong_Ghes.DeleteAllOnSubmit(db.Phong_Ghes.Where(n => n.Id_Phong == id_phong));

                        List<Phong_Ghe> phongghes = new List<Phong_Ghe>();
                        foreach (var ghe in pg)
                        {
                            phongghes.Add(new Phong_Ghe { Id_Phong = id_phong, Id_Ghe = ghe.Id_Ghe, TinhTrang = true});
                        }
                        db.Phong_Ghes.InsertAllOnSubmit(phongghes);

                        db.SubmitChanges();
                    }

                    return RedirectToAction("Index");
                }
            }

            return View(phongchieu);
        }

        public ActionResult Details(int id)
        {
            var phongchieu = db.Phongs.SingleOrDefault(n => n.Id_Phong == id);
            if (phongchieu == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(phongchieu);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var phongchieu = db.Phongs.SingleOrDefault(n => n.Id_Phong == id);
            if (phongchieu == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(phongchieu);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id, FormCollection f)
        {
            var phongchieu = db.Phongs.SingleOrDefault(n => n.Id_Phong == id);
            if (phongchieu == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var lc = db.LichChieus.Where(n => n.Id_Phong == id);
            if (lc.Count() > 0)
            {
                ViewBag.ThongBao = "Phòng này đang có Lịch chiếu <br>" +
                                   " Nếu muốn xóa thì phải hủy Lịch chiếu của phòng này!";
                return View(phongchieu);
            }

            var phong_ghe = db.Phong_Ghes.Where(pg => pg.Id_Phong == id).ToList();
            if (phong_ghe != null)
            {
                db.Phong_Ghes.DeleteAllOnSubmit(phong_ghe);
                db.SubmitChanges();
            }

            db.Phongs.DeleteOnSubmit(phongchieu);
            db.SubmitChanges();

            var rapphim = db.RapPhims.SingleOrDefault(n => n.Id_RapChieu == phongchieu.Id_RapChieu);
            rapphim.TongSoPhong = rapphim.TongSoPhong - 1;
            db.SubmitChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create_LP()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create_LP(LoaiPhong lp, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                lp.TenLoaiPhong = f["sTenLP"];
                lp.MoTa = f["sMoTa"];

                db.LoaiPhongs.InsertOnSubmit(lp);
                db.SubmitChanges();

                return RedirectToAction("Index_LP");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit_LP(int id)
        {
            var lp = db.LoaiPhongs.SingleOrDefault(n => n.Id_LoaiPhong == id);
            if (lp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(lp);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit_LP(FormCollection f)
        {
            var lp = db.LoaiPhongs.SingleOrDefault(n => n.Id_LoaiPhong == int.Parse(f["iMaLP"]));
            if (ModelState.IsValid)
            {
                lp.TenLoaiPhong = f["sTenLP"];
                lp.MoTa = f["sMoTa"];

                db.SubmitChanges();
                return RedirectToAction("Index_LP");
            }

            return View(lp);
        }

        [HttpGet]
        public ActionResult Delete_LP(int id)
        {
            var lp = db.LoaiPhongs.SingleOrDefault(n => n.Id_LoaiPhong == id);
            if (lp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(lp);
        }

        [HttpPost, ActionName("Delete_LP")]
        public ActionResult Delete_LPConfirmed(int id, FormCollection f)
        {
            var lp = db.LoaiPhongs.SingleOrDefault(n => n.Id_LoaiPhong == id);
            if (lp == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var phong = db.Phongs.Where(n => n.Id_LoaiPhong == id);
            if (phong.Count() > 0)
            {
                ViewBag.ThongBao = "Loại phòng này đang có Phòng chiếu sử dụng <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Phòng chiếu có dùng Loại phòng này!";
                return View(lp);
            }

            db.LoaiPhongs.DeleteOnSubmit(lp);
            db.SubmitChanges();

            return RedirectToAction("Index_LP");
        }

        [HttpGet]
        public ActionResult Create_LG()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create_LG(LoaiGhe lg, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                lg.TenLoaiGhe = f["sTenLG"];
                lg.GiaGhe = int.Parse(f["iGiaGhe"]);

                db.LoaiGhes.InsertOnSubmit(lg);
                db.SubmitChanges();

                return RedirectToAction("Index_LG");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit_LG(int id)
        {
            var lg = db.LoaiGhes.SingleOrDefault(n => n.Id_LoaiGhe == id);
            if (lg == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(lg);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit_LG(FormCollection f)
        {
            var lg = db.LoaiGhes.SingleOrDefault(n => n.Id_LoaiGhe == int.Parse(f["iMaLG"]));
            if (ModelState.IsValid)
            {
                lg.TenLoaiGhe = f["sTenLG"];
                lg.GiaGhe = int.Parse(f["iGiaGhe"]);

                db.SubmitChanges();
                return RedirectToAction("Index_LG");
            }

            return View(lg);
        }

        [HttpGet]
        public ActionResult Delete_LG(int id)
        {
            var lg = db.LoaiGhes.SingleOrDefault(n => n.Id_LoaiGhe == id);
            if (lg == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(lg);
        }

        [HttpPost, ActionName("Delete_LG")]
        public ActionResult Delete_LGConfirmed(int id, FormCollection f)
        {
            var lg = db.LoaiGhes.SingleOrDefault(n => n.Id_LoaiGhe == id);
            if (lg == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var phongchieu = db.Phongs.Where(n => n.Id_LoaiGhe == id);
            if (phongchieu.Count() > 0)
            {
                ViewBag.ThongBao = "Một số Phòng chiếu đang dùng Loại ghế này <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Phòng chiếu đang dùng Loại ghế này!";
                return View(lg);
            }

            var ghe = db.Ghes.Where(n => n.Id_LoaiGhe == id);
            if (ghe.Count() > 0)
            {
                ViewBag.ThongBao = "Một số ghế đang tồn tại là Loại ghế này <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Ghế đang là Loại ghế này!";
                return View(lg);
            }

            db.LoaiGhes.DeleteOnSubmit(lg);
            db.SubmitChanges();

            return RedirectToAction("Index_LG");
        }

        [HttpGet]
        public ActionResult Create_G()
        {
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create_G(Ghe g, FormCollection f)
        {
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe");
            if (ModelState.IsValid)
            {
                g.TenGhe = f["sTenGhe"];
                g.Id_LoaiGhe = int.Parse(f["MaLG"]);

                db.Ghes.InsertOnSubmit(g);
                db.SubmitChanges();

                return RedirectToAction("Index_G");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit_G(int id)
        {
            var g = db.Ghes.SingleOrDefault(n => n.Id_Ghe == id);
            if (g == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe", g.Id_LoaiGhe);
            return View(g);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit_G(FormCollection f)
        {
            var g = db.Ghes.SingleOrDefault(n => n.Id_Ghe == int.Parse(f["iMaG"]));
            ViewBag.MaLG = new SelectList(db.LoaiGhes.ToList().OrderBy(n => n.TenLoaiGhe), "Id_LoaiGhe", "TenLoaiGhe", g.Id_LoaiGhe);
            if (ModelState.IsValid)
            {
                g.TenGhe = f["sTenGhe"];
                g.Id_LoaiGhe = int.Parse(f["MaLG"]);

                db.SubmitChanges();
                return RedirectToAction("Index_G");
            }

            return View(g);
        }

        [HttpGet]
        public ActionResult Delete_G(int id)
        {
            var g = db.Ghes.SingleOrDefault(n => n.Id_Ghe == id);
            if (g == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            return View(g);
        }

        [HttpPost, ActionName("Delete_G")]
        public ActionResult Delete_GConfirmed(int id, FormCollection f)
        {
            var g = db.Ghes.SingleOrDefault(n => n.Id_Ghe == id);
            if (g == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var ghe = db.Phong_Ghes.Where(n => n.Id_Ghe == id);
            if (ghe.Count() > 0)
            {
                ViewBag.ThongBao = "Ghế này đang có Phòng chiếu sử dụng <br>" +
                                   " Nếu muốn xóa thì phải thay đổi hoặc hủy Phòng chiếu có dùng Ghế này!";
                return View(g);
            }

            db.Ghes.DeleteOnSubmit(g);
            db.SubmitChanges();

            return RedirectToAction("Index_G");
        }
    }
}