using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using ABC_Cinema.ViewModel;

namespace ABC_Cinema.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private dbDataContext db = new dbDataContext();

        public List<ThongKe_PhimViewModel> getThongKe_Phim()
        {
            var result = db.Phims.Join(db.LichChieus, p => p.Id_Phim, lc => lc.Id_Phim, (p, lc) => new { p, lc })
                .Join(db.Ves, plc => plc.lc.Id_LichChieu, v => v.Id_LichChieu, (plc, v) => new { plc, v })
                .GroupBy(vp => new
                {
                    vp.plc.p.TenPhim,
                })
                .Select(g => new ThongKe_PhimViewModel
                {
                    TenPhim = g.Key.TenPhim,
                    LuotVe = g.Select(vp => vp.v.Id_LichChieu).Count(),
                    DoanhThu = g.Sum(vp => vp.v.GiaVe)
                }).OrderByDescending(g => g.DoanhThu);
            return result.Take(10).ToList();
        }

        public List<ThongKe_RapViewModel> getThongKe_Rap()
        {
            var result = db.RapPhims.Join(db.LichChieus, r => r.Id_RapChieu, lc => lc.Id_RapChieu, (r, lc) => new { r, lc })
                .Join(db.Ves, rlc => rlc.lc.Id_LichChieu, v => v.Id_LichChieu, (rlc, v) => new { rlc, v })
                .GroupBy(vr => new
                {
                    vr.rlc.r.TenRapChieu,
                    vr.rlc.r.TongSoPhong
                })
                .Select(g => new ThongKe_RapViewModel
                {
                    TenRap = g.Key.TenRapChieu,
                    SoPhong = g.Key.TongSoPhong,
                    DoanhThu = g.Sum(vr => vr.v.GiaVe)
                }).OrderByDescending(g => g.DoanhThu);
            return result.Take(10).ToList();
        }

        // GET: Admin/Home
        public ActionResult Index()
        {
            ViewData["ThongKe_Phim"] = getThongKe_Phim();
            ViewData["ThongKe_Rap"] = getThongKe_Rap();
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f)
        {
            var sTenDN = f["username"];
            var sMatKhau = f["password"];
            TaiKhoan ad = db.TaiKhoans.SingleOrDefault(n => n.TenDangNhap == sTenDN && n.MatKhau == sMatKhau && n.PhanQuyen.Equals("MANAGER"));
            if (ad != null)
            {
                Session["Admin"] = ad;
                Session["Id_TaiKhoan"] = ad.Id_TaiKhoan;
                Session["TenQTV"] = ad.ThongTin.TenNguoiDung;
                Session["EmailQTV"] = ad.ThongTin.Email;
                return RedirectToAction("Index", "KhachHang");
            }
            else
            {
                ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Remove("Admin");
            Session.Remove("Id_TaiKhoan");
            Session.Remove("TenQTV");
            Session.Remove("EmailQTV");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult change_Pass(int Id_TaiKhoan)
        {
            ViewBag.Title_Header = "ĐỔI MẬT KHẨU";
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == Id_TaiKhoan);
            return View(tk);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult change_Pass(FormCollection f)
        {
            ViewBag.Title_Header = "ĐỔI MẬT KHẨU";
            var maKH = int.Parse(f["Id_TaiKhoan"]);
            var oldPass = f["sOldPass"];
            var newPass = f["sNewPass"];
            var reNewPass = f["sReNewPass"];
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == maKH);
            if (tk.MatKhau == oldPass)
            {
                if (newPass == reNewPass)
                {
                    tk.MatKhau = newPass;
                    db.SubmitChanges();

                    return RedirectToAction("Logout");
                }
                else
                {
                    ViewBag.ReNewPass = "Mật khẩu không trùng khớp!";
                    return View(tk);
                }
            }
            else
            {
                ViewBag.OldPass = "Mật khẩu không chính xác!";
                return View(tk);
            }
            return View(tk);
        }

        private string GenerateRandomPassword()
        {
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int passwordLength = 8;

            Random random = new Random();
            char[] password = new char[passwordLength];

            for (int i = 0; i < passwordLength; i++)
            {
                password[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }

            return new string(password);
        }

        public void sendMailConfirm(TaiKhoan tk)
        {
            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templates/forget_Pass.html"));
            contentCustomer = contentCustomer.Replace("{{username}}", tk.TenDangNhap);
            contentCustomer = contentCustomer.Replace("{{pass}}", tk.MatKhau);
            string mailFrom = "abc.cinema.website@gmail.com";
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(mailFrom, "upmr ufet flkr kgts"),
                EnableSsl = true
            };
            var message = new MailMessage();
            message.From = new MailAddress(mailFrom);
            message.ReplyToList.Add(mailFrom);
            message.To.Add(new MailAddress(tk.ThongTin.Email));
            message.Subject = "ABC Cinema: Lấy Lại Mật Khẩu";
            message.Body = contentCustomer;
            message.IsBodyHtml = true;

            mail.Send(message);
        }

        [HttpGet]
        public ActionResult forget_Pass()
        {
            ViewBag.Title_Header = "LẤY LẠI MẬT KHẨU";
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult forget_Pass(FormCollection f)
        {
            ViewBag.Title_Header = "LẤY LẠI MẬT KHẨU";
            var email = f["sEmail"];
            if (db.TaiKhoans.Any(t => t.ThongTin.Email == email && t.PhanQuyen == "MANAGER"))
            {
                var tk = db.TaiKhoans.SingleOrDefault(t => t.ThongTin.Email == email);
                var newPassword = GenerateRandomPassword();
                tk.MatKhau = newPassword;
                db.SubmitChanges();

                sendMailConfirm(tk);

                ViewBag.ThongBao = "Chúng tôi đã gửi cho bạn mật khẩu mới, vui lòng kiểm tra Email!";
                return View();
            }
            else
            {
                ViewBag.Email = "Email này chưa được đăng ký trong hệ thống!";
                return View();
            }
            return View();
        }
    }
}