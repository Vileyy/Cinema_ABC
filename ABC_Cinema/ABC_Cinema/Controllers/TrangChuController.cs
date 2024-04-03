using ABC_Cinema.Models;
using ABC_Cinema.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using PagedList;

namespace ABC_Cinema.Controllers
{
    public class TrangChuController : Controller
    {
        private dbDataContext db = new dbDataContext();

        /*private List<PhimViewModel> getPhimThinhHanh()
        {
            var today = DateTime.Now.Date;
            var phim = db.Phims.Join(db.LoaiPhims, p => p.Id_LoaiPhim, lp => lp.Id_LoaiPhim, (p, lp) => new { p, lp })
                .Join(db.LichChieus, plp => plp.p.Id_Phim, lc => lc.Id_Phim, (plp, lc) => new { plp, lc })
                .Join(db.Ves, plc => plc.lc.Id_LichChieu, v => v.Id_LichChieu, (plc, v) => new { plc, v })
                .GroupBy(pxn => new
                {
                    pxn.plc.plp.p.Id_Phim,
                    pxn.plc.plp.p.TenPhim,
                    pxn.plc.plp.p.AnhPhim,
                    pxn.plc.plp.p.ThoiLuong,
                    pxn.plc.plp.p.NgayCongChieu,
                    pxn.plc.plp.p.NgayKetThuc,
                    pxn.plc.plp.lp.TenLoai
                })
                .Select(g => new PhimViewModel
                {
                    Id = g.Key.Id_Phim,
                    TenPhim = g.Key.TenPhim,
                    AnhPhim = g.Key.AnhPhim,
                    ThoiLuong = g.Key.ThoiLuong,
                    NgayCongChieu = g.Key.NgayCongChieu,
                    NgayKetThuc = g.Key.NgayKetThuc,
                    TenLoai = g.Key.TenLoai,
                    Sum = g.Sum(pxn => pxn.v.SoluongGhe)
                }).Where(pxn => pxn.NgayCongChieu.Date <= today && pxn.NgayKetThuc.Date >= today).OrderByDescending(pxn => pxn.Sum);
            return phim.ToList();
        }*/

        private List<Phim> getPhimDeXuat(int count)
        {
            var today = DateTime.Now.Date;
            int seed = Guid.NewGuid().GetHashCode();
            Random random = new Random(seed);

            var allPhims = db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today || p.NgayCongChieu.Date > today).ToList();
            var shuffledPhims = allPhims.OrderBy(x => random.Next()).ToList();

            return shuffledPhims.Take(count).ToList();
        }

        public ActionResult Index()
        {
            var today = DateTime.Now.Date;
            List<Phim> listPhimSapChieu = db.Phims.Where(p => p.NgayCongChieu.Date > today).OrderByDescending(p => p.NgayCapNhat).Take(4).ToList();
            List<Phim> listPhimDangChieu = db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today).OrderByDescending(p => p.NgayCapNhat).Take(4).ToList();
            ViewData["listPhimDangChieu"] = listPhimDangChieu;
            ViewData["listPhimSapChieu"] = listPhimSapChieu;
            return View();
        }

        public ActionResult PhimDangChieu(int? page)
        {
            ViewBag.Title_Header = "PHIM ĐANG CHIẾU";
            var today = DateTime.Now.Date;
            if (page == null) page = 1;
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var listPhimDangChieu = db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today).OrderByDescending(p => p.NgayCapNhat).ToList();
            return View(listPhimDangChieu.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult PhimSapChieu(int? page)
        {
            ViewBag.Title_Header = "PHIM SẮP CHIẾU";
            var today = DateTime.Now.Date;
            if (page == null) page = 1;
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            var listPhimSapChieu = db.Phims.Where(p => p.NgayCongChieu.Date > today).OrderByDescending(p => p.NgayCapNhat).ToList();
            return View(listPhimSapChieu.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult PhimThinhHanh(int? page)
        {
            ViewBag.Title_Header = "PHIM THỊNH HÀNH";
            var today = DateTime.Now.Date;
            if (page == null) page = 1;
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var result = db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today);
            var listPhimThinHanh = result
                .Where(p => p.LichChieus.Any(lc => lc.Ves.Any()))
                .OrderByDescending(p => p.LichChieus.Sum(lc => lc.Ves.Count()));
            return View(listPhimThinHanh.ToPagedList(pageNumber, pageSize));
        }

        [ChildActionOnly]
        public ActionResult Header_Partial()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult Nav_Partial()
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == null).OrderBy(m => m.OrderNumber).ToList();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                var l = db.MENUs.Where(m => m.ParentId == lst[i].Id);
                a[i] = l.Count();
            }

            ViewBag.lst = a;
            return PartialView(lst);
        }

        [ChildActionOnly]
        public ActionResult LoadChildMenu(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                var l = db.MENUs.Where(m => m.ParentId == lst[i].Id);
                a[i] = l.Count();
            }

            ViewBag.lst = a;
            return PartialView("LoadChildMenu", lst);
        }

        [ChildActionOnly]
        public ActionResult Slider_Partial()
        {
            List<Slider> listSlider = db.Sliders.Where(sl => sl.TrangThai == true).ToList();
            return PartialView(listSlider);
        }

        [ChildActionOnly]
        public ActionResult PhimDeXuat_Partial()
        {
            var pdx = getPhimDeXuat(6);
            return PartialView(pdx);
        }

        [ChildActionOnly]
        public ActionResult Footer_Partial()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult PhimDangChieu_Partial()
        {
            var today = DateTime.Now.Date;
            return PartialView(db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today).OrderByDescending(p => p.NgayCapNhat).Take(4).ToList());
        }

        public ActionResult detailsPhim(int Id_Phim)
        {
            ViewBag.Title_Header = "CHI TIẾT PHIM";
            var phim = db.Phims.SingleOrDefault(p => p.Id_Phim == Id_Phim);
            ViewData["phim_LichChieu"] = db.LichChieus.Where(lc => lc.Id_Phim == Id_Phim && lc.NgayChieu.Date >= DateTime.Now.Date).ToList();
            ViewData["DanhGia"] = db.NoiDungs.Where(nd => nd.BinhLuan.Id_Phim == Id_Phim).ToList();
            return View(phim);
        }

        public ActionResult TrangTin(string metatitle)
        {
            var tt = db.TRANGTINs.Where(m => m.MetaTitle == metatitle).Single();
            return View(tt);
        }

        private int SaveBL(BinhLuan bl)
        {
            db.BinhLuans.InsertOnSubmit(bl);
            db.SubmitChanges();
            return bl.Id_BinhLuan;
        }

        public void updateRating(int id_Phim)
        {
            var phim = db.Phims.SingleOrDefault(p => p.Id_Phim == id_Phim);
            var avgRating = db.BinhLuans.Where(r => r.Id_Phim == phim.Id_Phim)
                .Average(r => (float?)r.DanhGia) ?? 0;
            phim.DanhGia = avgRating;
            db.SubmitChanges();
        }

        public ActionResult DanhGia(int rating, string comment, int id_Phim)
        {
            int id_TaiKhoan = Convert.ToInt32(Session["Id_TaiKhoan"]);
            bool userHasComments = db.NoiDungs.Any(nd => nd.Id_TaiKhoan == id_TaiKhoan && nd.BinhLuan.Id_Phim == id_Phim);

            if (!userHasComments)
            {
                BinhLuan bl = new BinhLuan();
                NoiDung nd = new NoiDung();
                bl.Id_Phim = id_Phim;
                bl.NoiDung = comment;
                bl.NgayDang = DateTime.Now;
                bl.TinhTrang = true;
                bl.DanhGia = rating;

                var id_BinhLuan = SaveBL(bl);

                nd.Id_BinhLuan = id_BinhLuan;
                nd.Id_TaiKhoan = id_TaiKhoan;
                db.NoiDungs.InsertOnSubmit(nd);
                db.SubmitChanges();

                updateRating(id_Phim);

                return RedirectToAction("detailsPhim", "TrangChu", new { Id_Phim = id_Phim });
            }
            else
            {
                var existNd = db.NoiDungs.SingleOrDefault(n => n.Id_TaiKhoan == id_TaiKhoan && n.BinhLuan.Id_Phim == id_Phim);
                var existComment = db.BinhLuans.SingleOrDefault(b => b.Id_BinhLuan == existNd.Id_BinhLuan);
                existComment.NoiDung = comment;
                existComment.DanhGia = rating;
                db.SubmitChanges();

                updateRating(id_Phim);
                return RedirectToAction("detailsPhim", "TrangChu", new { Id_Phim = id_Phim });
            }
        }
    }
}