using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using PagedList;

namespace ABC_Cinema.Controllers
{
    public class SearchController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Search
        public ActionResult Search(int? page, string searchString = null, int Id_TheLoai = 0, string movieStatus = null, string sortMovie = null)
        {
            ViewBag.Title_Header = "TÌM KIẾM";
            ViewBag.Search = searchString;
            ViewBag.MovieStatus = movieStatus;
            ViewBag.SortMovie = sortMovie;
            var today = DateTime.Now.Date;
            var result = db.Phims.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today || p.NgayCongChieu.Date > today);

            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(s => s.TenPhim.Contains(searchString) || s.DaoDien.Contains(searchString) || s.DienVien.Contains(searchString));
            }

            if (Id_TheLoai != 0)
            {
                result = result.Where(p => p.Id_LoaiPhim == Id_TheLoai);
            }

            if (movieStatus == "nowPlaying")
            {
                result = result.Where(p => p.NgayCongChieu.Date <= today && p.NgayKetThuc.Date >= today);
            }
            else if (movieStatus == "comingSoon")
            {
                result = result.Where(p => p.NgayCongChieu.Date > today);
            }

            if (sortMovie == "newest")
            {
                result = result.OrderByDescending(p => p.NgayCongChieu);
            }
            else if (sortMovie == "mostView")
            {
                result = result
                    .Where(p => p.LichChieus.Any(lc => lc.Ves.Any()))
                    .OrderByDescending(p => p.LichChieus.Sum(lc => lc.Ves.Count()));
            }

            ViewBag.Id_TheLoai = new SelectList(db.LoaiPhims, "Id_LoaiPhim", "TenLoai", null);

            if (page == null) page = 1;
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            return View(result.ToPagedList(pageNumber, pageSize));
        }
    }
}