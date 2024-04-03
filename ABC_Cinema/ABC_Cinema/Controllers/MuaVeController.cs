using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;

namespace ABC_Cinema.Controllers
{
    public class MuaVeController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: MuaVe
        public ActionResult MuaVe(int Id_Phim = 0)
        {
            ViewBag.Title_Header = "MUA VÉ";
            var lichchieutoday = db.LichChieus.Where(lc => lc.NgayChieu.Date >= DateTime.Now.Date).ToList();
            var phimIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_Phim).Distinct();
            var phimsInLichChieuToday = db.Phims.Where(p => phimIdsInLichChieuToday.Contains(p.Id_Phim)).ToList();

            var rapIdsInLichChieuToday = lichchieutoday.Select(lc => lc.Id_RapChieu).Distinct();
            var rapsInLichChieuToday = db.RapPhims.Where(r => rapIdsInLichChieuToday.Contains(r.Id_RapChieu)).ToList();

            ViewBag.MaPhim = new SelectList(phimsInLichChieuToday, "Id_Phim", "TenPhim", Id_Phim);
            ViewBag.MaRap = new SelectList(rapsInLichChieuToday.ToList().OrderBy(n => n.TenRapChieu), "Id_RapChieu", "TenRapChieu");
            ViewBag.MaLC_NgayChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaLC_SuatChieu = new SelectList(new List<SelectListItem>());
            ViewBag.MaGhe = "";

            return View();
        }

        [HttpPost]
        public JsonResult GetLichChieus(int phimId, int rapId)
        {
            var lichChieus = db.LichChieus.Where(lc => lc.Id_Phim == phimId && lc.Id_RapChieu == rapId && lc.NgayChieu.Date >= DateTime.Now.Date).OrderBy(lc => lc.NgayChieu).ToList();
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
        public JsonResult GetMovieDetails(int phimId = 0, int lichchieuId = 0)
        {
            if (phimId > 0 && lichchieuId == 0)
            {
                var movieDetails_p = db.Phims.FirstOrDefault(p => p.Id_Phim == phimId);
                return Json(new
                {
                    AnhPhim = movieDetails_p.AnhPhim,
                    TenPhim = movieDetails_p.TenPhim,
                    ChatLuong = movieDetails_p.ChatLuong,
                    DoTuoi = movieDetails_p.DanhMuc.TenDanhMuc,
                });
            }

            if (lichchieuId > 0 && phimId > 0)
            {
                var movieDetails_lc = db.LichChieus.FirstOrDefault(lc => lc.Id_LichChieu == lichchieuId);
                return Json(new
                {
                    AnhPhim = movieDetails_lc.Phim.AnhPhim,
                    TenPhim = movieDetails_lc.Phim.TenPhim,
                    ChatLuong = movieDetails_lc.Phim.ChatLuong,
                    DoTuoi = movieDetails_lc.Phim.DanhMuc.TenDanhMuc,
                    TenRap = movieDetails_lc.RapPhim.TenRapChieu,
                    NgayChieu = movieDetails_lc.NgayChieu.ToString("dd/MM/yyyy"),
                    GioChieu = movieDetails_lc.GioBatDau.ToString(),
                    GiaGhe = movieDetails_lc.Phong.LoaiGhe.GiaGhe
                });
            }

            return Json(new { });
        }

        private int SaveVe(Ve v)
        {
            db.Ves.InsertOnSubmit(v);
            db.SubmitChanges();
            return v.Id_Ve;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MuaVe(Ve v, FormCollection f)
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

            var tenTK = Session["username"];

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

                /*var url = "/Payment/SendOrder?order=" + v.Id_Ve;*/
                return RedirectToAction("Payment","Payment", new {Id_Ve = v.Id_Ve});
            }

            return View();
        }
    }
}