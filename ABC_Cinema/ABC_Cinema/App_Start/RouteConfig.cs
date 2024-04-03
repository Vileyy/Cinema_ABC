using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ABC_Cinema
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Trang chu",
                url: "",
                defaults: new { controller = "TrangChu", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Phim dang chieu",
                url: "phim-dang-chieu",
                defaults: new { controller = "TrangChu", action = "PhimDangChieu" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Phim sap chieu",
                url: "phim-sap-chieu",
                defaults: new { controller = "TrangChu", action = "PhimSapChieu" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Phim thinh hanh",
                url: "phim-thinh-hanh",
                defaults: new { controller = "TrangChu", action = "PhimThinhHanh" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Tim kiem",
                url: "tim-kiem",
                defaults: new { controller = "Search", action = "Search" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Chi tiet phim",
                url: "chi-tiet-phim-{Id_Phim}",
                defaults: new { controller = "TrangChu", action = "detailsPhim", Id_Phim = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );
            routes.MapRoute(
                name: "Thong tin ca nhan",
                url: "thong-tin-ca-nhan-{Id_TaiKhoan}",
                defaults: new { controller = "User", action = "user_Info", Id_TaiKhoan = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Doi mat khau",
                url: "doi-mat-khau-{Id_TaiKhoan}",
                defaults: new { controller = "User", action = "user_ChangePass", Id_TaiKhoan = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Lay lai mat khau",
                url: "lay-lai-mat-khau",
                defaults: new { controller = "User", action = "forget_Pass" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Ve cua toi",
                url: "ve-cua-toi-{Id_TaiKhoan}",
                defaults: new { controller = "User", action = "user_Ticket", Id_TaiKhoan = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Thong tin ve",
                url: "thong-tin-ve-{Id_Ve}",
                defaults: new { controller = "User", action = "Details_Ve", Id_Ve = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Mua ve",
                url: "mua-ve",
                defaults: new { controller = "MuaVe", action = "MuaVe" },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Trang tin",
                url: "{metatitle}",
                defaults: new { controller = "TrangChu", action = "TrangTin", metatitle = UrlParameter.Optional },
                namespaces: new string[] { "ABC_Cinema.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "ABC_Cinema.Controllers" }
            );
        }
    }
}
