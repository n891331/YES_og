using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Web;
using System.Web.Mvc;
using YES_og.Models;
using Microsoft.SqlServer.Server;
using System.Web.Helpers;


namespace YES_og.Controllers
{
    public class CarController : _BaseController
    {
        private DBePowerDataContext _db = new DBePowerDataContext();
        // GET: Car
        public ActionResult Index()
        {
            var carList = _db.dm_car_list.ToList();
            return View(carList);
        }

        // GET: Car/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult addCar(string data)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        // POST: Car/Create
        [HttpPost]
        public ActionResult Create(dm_car_list formData)
        {

            //Step 2:資料庫更改
            if (ModelState.IsValid)
            {
                //程式給號
                int car_no = 0;
                List<dm_car_list> data = _db.dm_car_list.OrderByDescending(o => o.car_no).ToList();
                car_no = data.Count == 0 ? (car_no + 1) : (data.FirstOrDefault().car_no + 1);
                formData.car_no = car_no;

                formData.update_time = DateTime.Now;
                SaveCreate(_db.dm_car_list, formData);

                var returnData = new {
                    IsSuccess = true
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            else
            {
                var returnData = new
                {
                    // 成功與否
                    IsSuccess = false,
                    // ModelState錯誤訊息 
                    ModelStateErrors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(k => k.Key, k => k.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            
        }

        // GET: Car/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Car/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Car/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Car/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
