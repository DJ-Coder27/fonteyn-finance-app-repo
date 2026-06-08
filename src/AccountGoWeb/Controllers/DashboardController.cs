using Microsoft.AspNetCore.Mvc;

namespace AccountGoWeb.Controllers
{
    public class DashboardController : BaseController
    {
        public DashboardController(IConfiguration config)
        {
            _baseConfig = config;
            Models.SelectListItemHelper._config = config;
        }

        public IActionResult Index()
        {
            ViewBag.PageContentHeader = "Dashboard";
            ViewBag.ApiMontlySales = "/SPAProxy?endpoint=sales/getmonthlysales";
            return View("MonthlySales");
        }

        public IActionResult MonthlySales()
        {
            ViewBag.PageContentHeader = "Dashboard";
            ViewBag.ApiMontlySales = "/SPAProxy?endpoint=sales/getmonthlysales";
            return View();
        }
    }
}