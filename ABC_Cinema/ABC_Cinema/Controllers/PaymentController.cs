using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using ABC_Cinema.Models;
using ABC_Cinema.Other;
using Newtonsoft.Json.Linq;

namespace ABC_Cinema.Controllers
{
    public class PaymentController : Controller
    {
        dbDataContext db = new dbDataContext();

        // GET:
        public ActionResult Payment(int Id_Ve)
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();
            var ve = db.Ves.SingleOrDefault(v => v.Id_Ve == Id_Ve);

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", (ve.GiaVe * 100).ToString().Replace(".00", "")); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan ve xem phim"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", ve.Id_Ve.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }
        public void sendMailConfirm(Ve v, string lstGhe)
        {
            var email = Session["email"];
            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/Templates/MailConfirm.html"));
            contentCustomer = contentCustomer.Replace("{{Id_Ve}}", v.Id_Ve.ToString());
            contentCustomer = contentCustomer.Replace("{{TenPhim}}", v.LichChieu.Phim.TenPhim);
            contentCustomer = contentCustomer.Replace("{{TenRap}}", v.LichChieu.RapPhim.TenRapChieu);
            contentCustomer = contentCustomer.Replace("{{PhongChieu}}", v.LichChieu.Phong.TenPhong);
            contentCustomer = contentCustomer.Replace("{{NgayChieu}}", v.LichChieu.NgayChieu.ToString("dd/MM/yyyy"));
            contentCustomer = contentCustomer.Replace("{{GioBatDau}}", v.LichChieu.GioBatDau.ToString());
            contentCustomer = contentCustomer.Replace("{{lstGhe}}", lstGhe);
            contentCustomer = contentCustomer.Replace("{{GiaVe}}", string.Format("{0:#,##0,0}", v.GiaVe));
            string mailFrom = "abc.cinema.website@gmail.com";
            var mail = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(mailFrom, "upmr ufet flkr kgts"),
                EnableSsl = true
            };
            var message = new MailMessage();
            message.From = new MailAddress(mailFrom);
            message.ReplyToList.Add(mailFrom);
            message.To.Add(new MailAddress(email.ToString()));
            message.Subject = "ABC Cinema: Đặt Vé Thành Công";
            message.Body = contentCustomer;
            message.IsBodyHtml = true;

            mail.Send(message);
        }

        public void deleteVe(Ve ve)
        {
            var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == ve.Id_Ve).ToList();
            if (ve_ghe != null)
            {
                db.Ve_Ghes.DeleteAllOnSubmit(ve_ghe);
                db.SubmitChanges();
            }

            db.Ves.DeleteOnSubmit(ve);
            db.SubmitChanges();
        }

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                var ve = db.Ves.SingleOrDefault(n => n.Id_Ve == orderId);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Lấy danh sách ghế
                        var ve_ghe = db.Ve_Ghes.Where(vg => vg.Id_Ve == ve.Id_Ve).ToList();
                        var danhSachTenGhe = string.Join(",", ve_ghe.Select(item => item.TenGhe));
                        ViewBag.DSGhe = danhSachTenGhe;

                        //Gửi mail về khách hàng
                        sendMailConfirm(ve, danhSachTenGhe);

                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công vé: " + orderId + " | Mã giao dịch: " + vnpayTranId;
                        return View(ve);
                    }
                    else
                    {
                        //Hủy vé thanh toán không thành công.
                        deleteVe(ve);

                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý vé của bạn: " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    deleteVe(ve);
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý!";
                }
            }

            return View();
        }
    }
}