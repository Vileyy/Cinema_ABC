using ABC_Cinema.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using PagedList;

namespace ABC_Cinema.Controllers
{
    public class UserController : Controller
    {
        private dbDataContext db = new dbDataContext();

        [HttpPost]
        public JsonResult Login(string sTenDN, string sMatKhau, string sRemember)
        {
            TaiKhoan tk = db.TaiKhoans.SingleOrDefault(n => n.TenDangNhap == sTenDN && n.MatKhau == sMatKhau && n.PhanQuyen.Equals("USER"));
            if (tk != null)
            {
                Session["taikhoan"] = tk;
                Session["Id_TaiKhoan"] = tk.Id_TaiKhoan;
                Session["hoten"] = tk.ThongTin.TenNguoiDung;
                Session["username"] = tk.TenDangNhap;
                Session["email"] = tk.ThongTin.Email;
                if (sRemember == "true")
                {
                    Response.Cookies["TenDN"].Value = sTenDN;
                    Response.Cookies["MatKhau"].Value = sMatKhau;
                    Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(1);
                    Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(1);
                }
                else
                {
                    Response.Cookies["TenDN"].Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies["MatKhau"].Expires = DateTime.Now.AddDays(-1);
                }

                return Json(1);
            }

            return Json(0);
        }

        private int SaveTT(ThongTin thongtin)
        {
            db.ThongTins.InsertOnSubmit(thongtin);
            db.SubmitChanges();
            return thongtin.Id_ThongTin;
        }

        [HttpPost]
        public JsonResult Register(string username, string email, string password)
        {
            TaiKhoan tk = new TaiKhoan();
            ThongTin tt = new ThongTin();
            if (db.TaiKhoans.SingleOrDefault(n => n.TenDangNhap == username) != null)
            {
                return Json(2);
            }
            else if (db.TaiKhoans.SingleOrDefault(n => n.ThongTin.Email.Equals(email)) != null)
            {
                return Json(3);
            }
            else if (ModelState.IsValid)
            {
                tt.Email = email;
                int temp = SaveTT(tt);
                tk.TenDangNhap = username;
                tk.MatKhau = password;
                tk.TinhTrang = true;
                tk.PhanQuyen = "USER";
                tk.Id_ThongTin = temp;
                db.TaiKhoans.InsertOnSubmit(tk);
                db.SubmitChanges();
                return Json(1);
            }

            return Json(0);
        }

        public ActionResult LogOut()
        {
            Session.Remove("taikhoan");
            Session.Remove("Id_TaiKhoan");
            Session.Remove("hoten");
            Session.Remove("username");
            Session.Remove("email");
            return RedirectToAction("Index", "TrangChu");
        }

        [HttpGet]
        public ActionResult user_Info(int Id_TaiKhoan)
        {
            ViewBag.Title_Header = "THÔNG TIN CÁ NHÂN";
            var userInfo = db.TaiKhoans.SingleOrDefault(tk => tk.Id_TaiKhoan == Id_TaiKhoan);
            return View(userInfo);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult user_Info(FormCollection f)
        {
            ViewBag.Title_Header = "THÔNG TIN CÁ NHÂN";
            var maKH = int.Parse(f["Id_TaiKhoan"]);
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == maKH);
            var tt = db.ThongTins.SingleOrDefault(t => t.Id_ThongTin == tk.Id_ThongTin);
            bool gioiTinh = true;
            var gt = int.Parse(f["bGioiTinh"]);
            if (gt == 0)
            {
                gioiTinh = false;
            }

            if (ModelState.IsValid)
            {
                tt.TenNguoiDung = f["sTenKH"];
                tt.GioiTinh = gioiTinh;
                tt.NgaySinh = Convert.ToDateTime(f["dNgaySinh"]);
                tt.DiaChi = f["sDiaChi"];
                tt.NgayCapNhat = DateTime.Now;
                db.SubmitChanges();

                Session["hoten"] = tt.TenNguoiDung;
                Session["email"] = tt.Email;
                ViewBag.ThongBao = "Cập nhật thông tin thành công!";
                return View(tk);
            }
            ViewBag.ThongBao = "Cập nhật thông tin thất bại!";
            return View(tk);
        }

        public ActionResult user_Ticket(int? page, int Id_TaiKhoan)
        {
            ViewBag.Title_Header = "VÉ CỦA TÔI";
            int iPageNum = (page ?? 1);
            int iPageSize = 10;
            var userTicket = db.Ves.Where(ve => ve.Id_TaiKhoan == Id_TaiKhoan).OrderByDescending(ve => ve.NgayDat);
            return View(userTicket.ToPagedList(iPageNum, iPageSize));
        }

        public ActionResult Details_Ve(int Id_Ve)
        {
            ViewBag.Message = "THÔNG TIN CHI TIẾT VÉ: " + Id_Ve;
            var ve = db.Ves.SingleOrDefault(v => v.Id_Ve == Id_Ve);
            return View(ve);
        }

        [HttpGet]
        public ActionResult user_ChangePass(int Id_TaiKhoan)
        {
            ViewBag.Title_Header = "ĐỔI MẬT KHẨU";
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == Id_TaiKhoan);
            return View(tk);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult user_ChangePass(FormCollection f)
        {
            ViewBag.Title_Header = "ĐỔI MẬT KHẨU";
            var maKH = int.Parse(f["Id_TaiKhoan"]);
            var oldPass = f["sOldPass"];
            var newPass = f["sNewPass"];
            var reNewPass = f["sReNewPass"];
            var tk = db.TaiKhoans.SingleOrDefault(t => t.Id_TaiKhoan == maKH);
            if (tk.MatKhau == oldPass)
            {
                if (oldPass != newPass)
                {
                    if (newPass == reNewPass)
                    {
                        tk.MatKhau = newPass;
                        db.SubmitChanges();

                        return RedirectToAction("LogOut");
                    }
                    else
                    {
                        ViewBag.ReNewPass = "Mật khẩu không trùng khớp!";
                        return View(tk);
                    }
                }
                else
                {
                    ViewBag.NewPass = "Mật khẩu mới không được trùng mật khẩu cũ!";
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
            if (db.TaiKhoans.Any(t => t.ThongTin.Email == email && t.PhanQuyen == "USER"))
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