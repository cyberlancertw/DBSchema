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

        [HttpPost]
        public IActionResult Database()
        {
            if (HttpContext.Session.GetString("ServerName") == null)
                return RedirectToAction("Index");
            if (HttpContext.Session.GetString("Catalog") != null)
                HttpContext.Session.Remove("Catalog");
            TempData["Server"] = HttpContext.Session.GetString("ServerName");
            return View();
        }

        [HttpPost]
        public IActionResult Table(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName) || HttpContext.Session.GetString("ServerName") == null)
                return RedirectToAction("Index");
            HttpContext.Session.SetString("Catalog", databaseName);
            TempData["Catalog"] = databaseName;
            return View();
        }

        [HttpPost]
        public IActionResult Column(string tableName, string tableID, string tableSchema)
        {
            if (HttpContext.Session.GetString("ServerName") == null)
                return RedirectToAction("Index");
            TempData["Catalog"] = HttpContext.Session.GetString("Catalog");
            TempData["Table"] = tableName;
            TempData["TableID"] = tableID;
            TempData["TableSchema"] = tableSchema;
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
            return Json(new { success = info.Success, data = info.ObjectData, message = info.Message });
        }

        [HttpPost]
        public JsonResult QueryTable([FromBody] Ajax.QueryTable query)
        {
            SqlInfo info = new SqlInfo();
            service.QueryTable(
                HttpContext.Session.GetString("ServerName"),
                HttpContext.Session.GetString("UserName"),
                HttpContext.Session.GetString("Password"),
                HttpContext.Session.GetString("Catalog"),
                query,
                info);
            return Json(new { success = info.Success, data = info.ObjectData, message = info.Message });
        }

        [HttpPost]
        public JsonResult QueryColumn([FromBody] Ajax.QueryColumn query)
        {
            SqlInfo info = new SqlInfo();
            service.QueryColumn(
                HttpContext.Session.GetString("ServerName"),
                HttpContext.Session.GetString("UserName"),
                HttpContext.Session.GetString("Password"),
                HttpContext.Session.GetString("Catalog"),
                query,
                info);
            return Json(new { success = info.Success, data = info.ObjectData, message = info.Message });
        }

        [HttpPost]
        public JsonResult GetTableDescription([FromBody] Ajax.QueryTableDescription model)
        {
            SqlInfo Info = new SqlInfo();
            model.Server = HttpContext.Session.GetString("ServerName");
            model.User = HttpContext.Session.GetString("UserName");
            model.Pwd = HttpContext.Session.GetString("Password");
            model.Catalog = HttpContext.Session.GetString("Catalog");
            service.GetTableDescription(model, Info);
            return Json(new { success = Info.Success, data = Info.StringData, message = Info.Message });
        }

        [HttpPost]
        public JsonResult UpdateTableDescription([FromBody] Ajax.UpdateTableDescription model)
        {
            SqlInfo info = new SqlInfo();
            model.Server = HttpContext.Session.GetString("ServerName");
            model.User = HttpContext.Session.GetString("UserName");
            model.Pwd = HttpContext.Session.GetString("Password");
            model.Catalog = HttpContext.Session.GetString("Catalog");
            service.UpdateTableDescription(model, info);
            return Json(new { success = info.Success, message = info.Message });
        }

        [HttpPost]
        public JsonResult GetColumnDescription([FromBody] Ajax.QueryColumnDescription model)
        {
            SqlInfo Info = new SqlInfo();
            model.Server = HttpContext.Session.GetString("ServerName");
            model.User = HttpContext.Session.GetString("UserName");
            model.Pwd = HttpContext.Session.GetString("Password");
            model.Catalog = HttpContext.Session.GetString("Catalog");
            service.GetColumnDescription(model, Info);
            return Json(new { success = Info.Success, data = Info.StringData, message = Info.Message });
        }

        [HttpPost]
        public JsonResult UpdateColumnDescription([FromBody] Ajax.UpdateColumnDescription model)
        {
            SqlInfo info = new SqlInfo();
            model.Server = HttpContext.Session.GetString("ServerName");
            model.User = HttpContext.Session.GetString("UserName");
            model.Pwd = HttpContext.Session.GetString("Password");
            model.Catalog = HttpContext.Session.GetString("Catalog");
            service.UpdateColumnDescription(model, info);
            return Json(new { success = info.Success, message = info.Message });
        }

        [HttpPost]
        public JsonResult ReadExportTable()
        {
            SqlInfo info = new SqlInfo();
            service.ReadExportTable(
                HttpContext.Session.GetString("ServerName"),
                HttpContext.Session.GetString("UserName"),
                HttpContext.Session.GetString("Password"),
                HttpContext.Session.GetString("Catalog"), info);
            return Json(new { success = info.Success, fromData = info.ObjectData, message = info.Message });
        }
        [HttpPost]
        public JsonResult ExportFile([FromBody]Ajax.ExportFile model)
        {
            HttpContext.Session.SetString(model.ProgressID, "0");
            model.Server = HttpContext.Session.GetString("ServerName");
            model.User = HttpContext.Session.GetString("UserName");
            model.Pwd = HttpContext.Session.GetString("Password");
            model.Catalog = HttpContext.Session.GetString("Catalog");
            SqlInfo info = new SqlInfo();
            service.ExportFile(model, info);
            if(info.Success && info.StringData != null)
            {
                string uuid = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(uuid, info.StringData.Replace("wwwroot", ""));
                info.StringData = uuid;
            }
            return Json(new { success = info.Success, token = info.StringData, message = info.Message });
        }

        [HttpPost]
        public IActionResult GetFile(string token)
        {
            string? path = HttpContext.Session.GetString(token);
            if (path == null) return new EmptyResult();
            string contentType = string.Empty;
            if(path.IndexOf(".xlsx") > -1)
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            else if(path.IndexOf(".pdf") > -1)
                contentType = "application/pdf";
            else if(path.IndexOf(".docx") > -1)
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if(path.IndexOf(".csv") > -1)
                contentType = "text/csv";
            else
                contentType = "application/vnd.ms-excel";
            string fileName = path.Substring(6);
            return File(path, contentType, fileName);
        }

        [HttpPost]
        public JsonResult GetProgress([FromBody] string progressID)
        {
            if (string.IsNullOrEmpty(progressID) || HttpContext.Session.GetString(progressID) == null) return Json(new { progress = 0 });
            double percent = Convert.ToDouble(HttpContext.Session.GetString(progressID));
            return Json(new { progress = percent });
        }
    }
}