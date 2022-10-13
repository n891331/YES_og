using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using YES_og.Models;

namespace YES_og.Controllers
{
    public class HomeController : _BaseController
    {
        private DBePowerDataContext _db = new DBePowerDataContext();
        public ActionResult Index()
        {
            _db.dm_car_list.ToList();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}