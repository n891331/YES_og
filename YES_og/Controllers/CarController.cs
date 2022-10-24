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
            #region 充電槍選單
            //AC
            var selectACList = new List<SelectListItem>();
            var a = _db.dev100.Where(x => x.spot_ac_dc == "AC").OrderBy(x => x.type_id);
            foreach (var item in a)
            {
                selectACList.Add(new SelectListItem { Text = item.spot_type, Value = item.spot_type });
            }
            selectACList.Add(new SelectListItem { Text = "N/A", Value = "N/A" });
            ViewBag.selectACList = selectACList;

            //DC
            var selectDCList = new List<SelectListItem>();
            var d = _db.dev100.Where(x => x.spot_ac_dc == "DC").OrderBy(x => x.type_id);
            foreach (var item in d)
            {
                selectDCList.Add(new SelectListItem { Text = item.spot_type, Value = item.spot_type });
            }
            selectDCList.Add(new SelectListItem { Text = "N/A", Value = "N/A" });
            ViewBag.selectDCList = selectDCList;
            #endregion 
            var carList = _db.veh000.ToList();
            return View(carList);
        }

        // GET: Car/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // POST: Car/Create
        [HttpPost]
        public ActionResult Create(veh000 formData)
        {

            //Step 2:資料庫更改
            if (ModelState.IsValid)
            {
                //程式給號
                int veh_id = 0;
                List<veh000> data = _db.veh000.OrderByDescending(o => o.veh_id).ToList();
                veh_id = data.Count == 0 ? (veh_id + 1) : (data.FirstOrDefault().veh_id + 1);
                formData.veh_id = veh_id;

                formData.upd_date = DateTime.Now;
                SaveCreate(_db.veh000, formData);

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
