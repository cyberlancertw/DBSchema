using DBSchema.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DBSchema.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly sHome service;

        public HomeController(ILogger<HomeController> logger, sHome service)
        {
            _logger = logger;
            this.service = service;
        }

        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public JsonResult CheckValid([FromBody] Ajax.CheckValid model)
        {
            SqlInfo info = new SqlInfo();
            service.CheckValid(model, info);
            if (info.Success)
            {
                HttpContext.Session.SetString("ServerName", model.server);
                HttpContext.Session.SetString("UserName", model.user);
                HttpContext.Session.SetString("Password", model.pwd);
            }
            return Json(new { success = info.Success, message = info.Message });
        }

        public IActionResult Database()
        {
            if (HttpContext.Session.GetString("ServerName") == null)
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public JsonResult QueryDatabase([FromBody] Ajax.QueryDatabase query)
        {
            SqlInfo info = new SqlInfo();
            service.QueryDatabase(
                HttpContext.Session.GetString("ServerName"),
                HttpContext.Session.GetString("UserName"),
                HttpContext.Session.GetString("Password"),
                query,
                info);
            return Json(new { success = info.Success, data = info.ObjectData });
        }
    }
}