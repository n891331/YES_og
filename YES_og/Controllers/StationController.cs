using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using YES_og.Models;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection.Emit;
using static YES_og.Controllers.FollowController;
using System.Web.Util;
using System.Web.Helpers;
using System.Security.Cryptography;
using Microsoft.Ajax.Utilities;


namespace YES_og.Controllers
{
    public class StationController : _BaseController
    {
        private DBePowerDataContext _db = new DBePowerDataContext();
        private static Random random = new Random();
        // GET: Station
        public ActionResult Index(int? page, FormCollection fc)
        {
            GetCatcheRoutes(page, fc);
            int currentPageIndex = getCurrentPage(page, fc);

            var stn000 = _db.stn000.AsQueryable();


            if (hasViewData("SearchById"))
            {
                string SearchById = getViewDateStr("SearchById");
                stn000 = stn000.Where(w => w.station_id.Contains(SearchById));
            }
            if (hasViewData("SearchByStationOwner"))
            {
                string SearchByStationOwner = getViewDateStr("SearchByStationOwner");
                stn000 = stn000.Where(w => w.station_owner.ToString() == SearchByStationOwner);
            }
            if (hasViewData("SearchByStationName"))
            {
                string SearchByStationName = getViewDateStr("SearchByStationName");
                stn000 = stn000.Where(w => w.station_name.Contains(SearchByStationName));
            }

            if (hasViewData("SearchByStationType"))
            {
                string SearchByStationType = getViewDateStr("SearchByStationType");
                stn000 = stn000.Where(w => w.station_type.Contains(SearchByStationType));
            }

            //站點類型(顯示資料用)
            List<sts000> stationType = _db.sts000.Where(x => x.prog_id == "stn000" && x.fun_id == "station_type").ToList();
            ViewBag.stationType = stationType;

            //客戶ID(顯示資料用)
            List<cus000> cus000 = _db.cus000.ToList();
            ViewBag.cus000 = cus000;

            // 站點類型
            var selectTypeList = new List<SelectListItem>();
            var t = _db.sts000.Where(x => x.fun_id == "station_type" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            selectTypeList.Add(new SelectListItem { Text = "請選擇", Value = "" });
            foreach (var item in t)
            {
                selectTypeList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectTypeList = selectTypeList;

            //客戶ID
            var selectCustomerIDList = new List<SelectListItem>();
            var cId = _db.cus000.ToList();
            foreach (var item in cId)
            {
                selectCustomerIDList.Add(new SelectListItem { Text = item.customer_fullname, Value = item.customer_id.ToString() });
            }
            ViewBag.selectCustomerIDList = selectCustomerIDList;

            return View(stn000);
        }

        // GET: Station/Create
        public ActionResult Create()
        {
            #region 選單
            // 站點類型
            var selectTypeList = new List<SelectListItem>();
            var t = _db.sts000.Where(x => x.fun_id == "station_type" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in t)
            {
                selectTypeList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectTypeList = selectTypeList;
            // 場域類別
            var selectClassList = new List<SelectListItem>();
            var c = _db.sts000.Where(x => x.fun_id == "station_class" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in c)
            {
                selectClassList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectClassList = selectClassList;
            //站點狀態
            var selectStatusList = new List<SelectListItem>();
            var s = _db.sts000.Where(x => x.fun_id == "station_status" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in s)
            {
                selectStatusList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id});
            }
            ViewBag.selectStatusList = selectStatusList;

            //住宅類型
            //var selectLocList = new List<SelectListItem>();
            //var L = _db.sts000.Where(x => x.fun_id == "loc_class").OrderBy(x => x.item_seq);
            //foreach (var item in L)
            //{
            //    selectLocList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id.ToString() });
            //}
            //ViewBag.selectLocList = selectLocList;

            //車款
            var selectVehList = new List<SelectListItem>();
            var v = (from a in _db.veh000
                     select new { a.veh_brand }).ToList().Distinct();
            foreach (var item in v)
            {
                selectVehList.Add(new SelectListItem { Text = item.veh_brand, Value = item.veh_brand });
            }
            ViewBag.selectVehList = selectVehList;

            //縣市
            var selectCountryList = new List<SelectListItem>();
            var co = (from a in _db.zip000
                     select new { a.country }).ToList().Distinct();
            selectCountryList.Add(new SelectListItem { Text = "請選擇縣市", Value = "" });
            foreach (var item in co)
            {
                selectCountryList.Add(new SelectListItem { Text = item.country, Value = item.country });
            }
            ViewBag.selectCountryList = selectCountryList;

            //型號
            var selectDevModelList = new List<SelectListItem>();
            var d = _db.dev000.ToList();
            selectDevModelList.Add(new SelectListItem { Text = "請選擇充電樁型號", Value = "" });
            foreach (var item in d)
            {
                selectDevModelList.Add(new SelectListItem { Text = item.device_model, Value = item.device_id.ToString() });
            }
            ViewBag.selectDevModelList = selectDevModelList;

            //客戶ID
            var selectCustomerIDList = new List<SelectListItem>();
            var cId = _db.cus000.ToList();
            foreach (var item in cId)
            {
                selectCustomerIDList.Add(new SelectListItem { Text = item.customer_fullname, Value = item.customer_id.ToString() });
            }
            ViewBag.selectCustomerIDList = selectCustomerIDList;
            #endregion 

            #region 準備充電站ID
            //string station_id = "";
            //List<stn000> data = _db.stn000.OrderByDescending(o => o.station_id).ToList();
            //station_id = data.Count == 0 ? (station_id + 1) : (data.FirstOrDefault().station_id + 1);
            int station_id = 0;
            station_id = _db.stn000.Count() == 0? (station_id + 1) : (Convert.ToInt32(_db.stn000.Select(x=>x.station_id).Max()) + 1);
            ViewBag.station_id = station_id;
            //string cncId = int.Parse(cnc_id.ToString()).ToString("000000");
            //ViewBag.cncId = cncId;

            #endregion

            //充電站網路設定檔
            var stn102 = _db.stn102.Where(x=>x.station_id == "").ToList();
            ViewBag.stn102 = stn102;

            return View();
        }

        /// <summary>
        /// 第二層區選單
        /// </summary>
        /// <param name="selectData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetDistrict(string selectData)
        {
            //資料組成
            List<zip000> zip000s = _db.zip000.Where(w => w.country.Contains(selectData)).ToList();

            //生成主項目清單
            string jsonContent;
            JObject jObjectDistrict = new JObject();
            JArray jArrayDistrict = new JArray();

            foreach (var item in zip000s)
            {
                JObject jObjectDistrictList = new JObject();

                jObjectDistrictList.Add(new JProperty("Id", item.district));
                jObjectDistrictList.Add(new JProperty("Level2Name", item.district));

                jArrayDistrict.Add(jObjectDistrictList);
            }

            jObjectDistrict.Add(new JProperty("DdlDistrictList", jArrayDistrict));

            jsonContent = JsonConvert.SerializeObject(jObjectDistrict, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        /// <param name="country"></param>
        /// <param name="district"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetZipId(string country, string district)
        {
            //資料組成
            var data = _db.zip000.Where(w => w.country == country && w.district == district).FirstOrDefault();

            //生成主項目清單
            string jsonContent;
            JObject jObjectZipId= new JObject();

            jObjectZipId.Add(new JProperty("ZipId", data.zip_id));

            jsonContent = JsonConvert.SerializeObject(jObjectZipId, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        [HttpPost]
        public ActionResult GetnwDetail()
        {
            //資料組成
            var data = _db.stn102.Where(x => x.station_id == "").ToList();

            //生成主項目清單
            string jsonContent;
            JObject jObjectNetWorkList = new JObject();
            JArray jArrayNetWork = new JArray();

            foreach (var i in data)
            {
                JObject jObjectNetWork = new JObject();

                jObjectNetWork.Add(new JProperty("network_id", i.network_id));
                jObjectNetWork.Add(new JProperty("network_no", i.network_no));
                jObjectNetWork.Add(new JProperty("network_type", i.network_type));
                jObjectNetWork.Add(new JProperty("network_ip", i.network_ip));
                jObjectNetWork.Add(new JProperty("network_sn", i.network_sn));
                jObjectNetWork.Add(new JProperty("network_fee", i.network_fee));
                jObjectNetWork.Add(new JProperty("active_date", setDateFormat(i.active_date)));

                jArrayNetWork.Add(jObjectNetWork);
            }

            jObjectNetWorkList.Add(new JProperty("jObjectNetWorkList", jArrayNetWork));

            jsonContent = JsonConvert.SerializeObject(jObjectNetWorkList, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        /// <summary>
        /// 選擇手機門號/線路編號(匯入站點)的list
        /// </summary>
        /// <param name="idStr"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetNetWorkList(string idStr, string sId)
        {
            string[] nId = idStr.Split(',');

            foreach(var i in nId)
            {
                var stn102 = _db.stn102.Where(x => x.network_id.ToString() == i).FirstOrDefault();
                stn102.station_id = sId;

                SaveUpdate(_db.stn102, stn102);
            }

            //資料組成
            var data = _db.stn102.Where(x => x.station_id == sId).ToList();

            //生成主項目清單
            string jsonContent;
            JObject jObjectNetWorkList = new JObject();
            JArray jArrayNetWork = new JArray();

            foreach (var i in data)
            {
                JObject jObjectNetWork = new JObject();

                jObjectNetWork.Add(new JProperty("network_id", i.network_id));
                jObjectNetWork.Add(new JProperty("network_no", i.network_no));
                jObjectNetWork.Add(new JProperty("network_type", i.network_type));
                jObjectNetWork.Add(new JProperty("network_ip", i.network_ip));
                jObjectNetWork.Add(new JProperty("network_sn", i.network_sn));
                jObjectNetWork.Add(new JProperty("network_fee", i.network_fee));
                jObjectNetWork.Add(new JProperty("active_date", setDateFormat(i.active_date)));

                jArrayNetWork.Add(jObjectNetWork);
            }

            jObjectNetWorkList.Add(new JProperty("jObjectNetWorkList", jArrayNetWork));

            jsonContent = JsonConvert.SerializeObject(jObjectNetWorkList, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }
        
        [HttpPost]
        public ActionResult DeleteNetwork(string nId)
        {
            try
            {
                var data = _db.stn102.Where(x => x.network_id.ToString() == nId).FirstOrDefault();
                data.station_id = "";

                SaveUpdate(_db.stn102, data);


                var returnData = new
                {
                    IsSuccess = true
                };

                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch (Exception ex)
            {
                var returnData = new
                {
                    IsSuccess = false,
                    ErrorInfo = ex.Message
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }

        }

        /// <summary>
        /// 生成充電樁 item
        /// </summary>
        /// <param name="devId">設備ID</param>
        /// <param name="sId">充電站ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSpotItem(string devId, string sId)
        {
            //1.先看需要幾個item
            //2.把item append回去

            #region 1
            //設備基本資料 找數量
            dev000 dev = _db.dev000.Where(x => x.device_id.ToString() == devId).FirstOrDefault();
            int Cqty = (int)dev.charger_qty;

            //生成充電樁ID
            var spotId = 0;
            var countData = _db.stn001.Where(x => x.station_id == sId);
            if (countData.Count() == 0)
            {
                spotId = 1;
            }
            else
            {
                spotId = _db.stn001.Select(x => Convert.ToInt32(x.chargespot_seq)).Max() + 1;
            }

            //廠牌
            var device_supplier = _db.sup000.Where(x => x.supplier_id == dev.device_supplier).FirstOrDefault();

            #region 新增stn001
            //新增幾個充電槍item
            for (int i = 0; i < Cqty; i++)
            {
                stn001 stn001 = new stn001();
                //充電槍號(槍ID)，唯一值
                int chargespot_id = 0;
                List<stn001> data = _db.stn001.OrderByDescending(o => o.chargespot_id).ToList();
                chargespot_id = data.Count == 0 ? (Convert.ToInt32(chargespot_id) + 1) : (Convert.ToInt32(data.FirstOrDefault().chargespot_id) + 1);
                stn001.chargespot_id = chargespot_id;
                //充電站ID
                stn001.station_id = sId;
                //充電樁序號(充電樁ID) 
                stn001.chargespot_seq = spotId;

                //充電槍序號(充電樁第幾支槍)
                stn001.chargegun_seq = i + 1;

                stn001.spot_floor = "";
                stn001.spot_car_space = "";
                stn001.spot_note = "";
                stn001.spot_status = 1;

                SaveCreate(_db.stn001, stn001);

            }
            #endregion


            #region 新增stn101
            //資料 (過濾=>充電樁ID、充電站ID)
            var datas = _db.stn001.Where(x => x.chargespot_seq == spotId && x.station_id == sId).ToList();

            var snCount = _db.stn101.Count();
            foreach (var d in datas)
            {
                stn101 stn101 = new stn101();
                //DB裡不明欄位
                //stn101.tran_no = 0;
                //充電站ID             -----------key
                stn101.station_id = sId;
                //充電槍ID
                stn101.chargespot_id = d.chargespot_id;
                //設備ID              -----------key
                stn101.device_id = device_supplier.supplier_id;
                //出貨單號 => 先給隨便值
                stn101.dely_id = 0;
                //設備序號(充電樁序號) -----------key
                int sn = snCount++;
                stn101.device_sn = sn.ToString();
                //驗收時間 => 先給隨便值
                stn101.acpt_time = DateTime.Now;
                //保固開始時間
                stn101.wty_start = DateTime.Now;
                //保固結束時間
                stn101.wty_end = DateTime.Now;

                SaveCreate(_db.stn101, stn101);
            }
            #endregion

            
            #endregion

            #region 2
            //生成主項目清單
            string jsonContent;
            JObject jObjectSpot = new JObject();

            #region 充電槍狀態 的下拉選單
            JObject jObjectSpotStatus = new JObject();

            List<sts000> sts000 = _db.sts000.Where(x => x.prog_id == "stn001" && x.fun_id == "spot_status").ToList();

            foreach (var e in sts000)
            {
                jObjectSpotStatus.Add(new JProperty(e.item_id, e.item_desc));
            }

            jObjectSpot.Add(new JProperty("DdlSSList", jObjectSpotStatus));
            #endregion

            #region 費率方案 的下拉選單
            JObject jObjectFee = new JObject();

            List<stn004> sts004 = _db.stn004.ToList();

            foreach (var e in sts004)
            {
                jObjectFee.Add(new JProperty(e.cal_unit, e.rate_name));
            }

            jObjectSpot.Add(new JProperty("DdlFeeList", jObjectFee));
            #endregion

            JArray jArraySpot = new JArray();

            //廠牌
            //var device_supplier = _db.sup000.Where(x => x.supplier_id == dev.device_supplier).FirstOrDefault();

            //資料 (過濾=>充電樁ID、充電站ID)
            //var datas = _db.stn001.Where(x => x.chargespot_seq == spotId && x.station_id == sId).ToList();
            if (datas.Count() > 0)
            {
                foreach (var s in datas)
                {
                    //var item = _db.stn001.Where(x => x.chargespot_id.ToString() == s).FirstOrDefault();
                    JObject jObjectSpotItem = new JObject();
                    //充電槍ID
                    jObjectSpotItem.Add(new JProperty("chargespot_id", s.chargespot_id));
                    //充電站ID
                    jObjectSpotItem.Add(new JProperty("station_id", s.station_id));
                    //充電樁序號(充電樁ID)
                    jObjectSpotItem.Add(new JProperty("chargespot_seq", s.chargespot_seq));
                    //充電槍序號(充電樁第幾隻槍)
                    jObjectSpotItem.Add(new JProperty("chargegun_seq", s.chargegun_seq));

                    jArraySpot.Add(jObjectSpotItem);
                }
                jObjectSpot.Add(new JProperty("jObjectSpotList", jArraySpot));
            }

            //型號
            jObjectSpot.Add(new JProperty("device_model", dev.device_model));
            //廠牌(供應商)
            jObjectSpot.Add(new JProperty("device_id", device_supplier.supplier_id));
            jObjectSpot.Add(new JProperty("company_name", device_supplier.company_name));
            //規格
            jObjectSpot.Add(new JProperty("charger_plug", dev.charger_plug));

            #endregion

            jsonContent = JsonConvert.SerializeObject(jObjectSpot, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        /// <summary>
        /// 刪除型號
        /// </summary>
        /// <param name="btnData"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteModel(string stationId, int btnData,string soptId)
        {
            try
            {
                var s001 = _db.stn001.Where(x =>x.station_id == stationId && x.chargespot_seq == btnData).ToList();

                List<int> chargespotIdList = new List<int>();

                foreach (var i in s001)
                {
                    int cId = new int();
                    cId = Convert.ToInt32(i.chargespot_id);
                    chargespotIdList.Add(cId);

                    SaveDelete(_db.stn001, i);
                }

                foreach(var j in chargespotIdList)
                {
                    var s101 = _db.stn101.Where(x => x.station_id == stationId && x.chargespot_id == j).FirstOrDefault();
                    SaveDelete(_db.stn101, s101);
                }

                var returnData = new
                {
                    IsSuccess = true
                };

                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch (Exception ex)
            {
                var returnData = new
                {
                    IsSuccess = false,
                    ErrorInfo = ex.Message
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            
        }


        // POST: Station/Create  SEstn000(站基本資料), SEstnCS(樁基資+設備檔), SEstn003(營業時間), ,stn005(圖片)
        [HttpPost]
        public ActionResult Create(stn000 stn000,List<SEstnCS> SpotItems, SEstn003 SEstn003, HttpPostedFileBase[] rUpload, HttpPostedFileBase[] sUpload)
        {
            if (ModelState.IsValid)
            {
                #region stn000 充電站基本資料 新增
                stn000 s0 = new stn000();
                s0.station_id = stn000.station_id;
                s0.station_owner = stn000.station_owner;
                s0.station_name = stn000.station_name;
                s0.station_address = stn000.station_address;
                s0.station_gate_address = "";
                s0.station_type = stn000.station_type;
                s0.station_class = stn000.station_class;
                s0.station_zip = stn000.station_zip;
                //經緯度
                s0.station_longitude = stn000.station_longitude;
                s0.station_latitude = stn000.station_latitude;
                //暫時無用
                s0.station_gate_pic = stn000.station_gate_pic;
                s0.station_parking_pic = stn000.station_parking_pic;

                s0.station_floor = stn000.station_floor;
                s0.station_car_space = stn000.station_car_space;
                s0.contact_person_local = stn000.contact_person_local;
                s0.contact_mobile_local = stn000.contact_mobile_local;
                s0.customer_phone2_local = stn000.customer_phone2_local;
                s0.contact_email_local = stn000.contact_email_local;
                s0.contact_lineid_local = stn000.contact_lineid_local;
                s0.contact_fax_local = stn000.contact_fax_local;
                s0.station_note = stn000.station_note;
                s0.station_note1 = stn000.station_note1;
                s0.station_status = stn000.station_status;

                SaveCreate(_db.stn000, stn000);

                #endregion

                #region stn003 充電站營業時間 新增

                #region w1
                stn003 w1 = new stn003();
                w1.station_id = stn000.station_id;
                w1.week = SEstn003.week1;
                w1.time_start = SEstn003.tStart1.Remove(2,1);
                w1.time_end = SEstn003.tEnd1.Remove(2, 1);
                SaveCreate(_db.stn003, w1);
                #endregion

                #region w2
                stn003 w2 = new stn003();
                w2.station_id = stn000.station_id;
                w2.week= SEstn003.week2;
                w2.time_start = SEstn003.tStart2.Remove(2, 1);
                w2.time_end = SEstn003.tEnd2.Remove(2, 1);
                SaveCreate(_db.stn003, w2);
                #endregion

                #region w3
                stn003 w3 = new stn003();
                w3.station_id = stn000.station_id;
                w3.week = SEstn003.week3;
                w3.time_start = SEstn003.tStart3.Remove(2, 1);
                w3.time_end = SEstn003.tEnd3.Remove(2, 1);
                SaveCreate(_db.stn003, w3);
                #endregion

                #region w4
                stn003 w4 = new stn003();
                w4.station_id = stn000.station_id;
                w4.week = SEstn003.week4;
                w4.time_start = SEstn003.tStart4.Remove(2, 1);
                w4.time_end = SEstn003.tEnd4.Remove(2, 1);
                SaveCreate(_db.stn003, w4);
                #endregion

                #region w5
                stn003 w5 = new stn003();
                w5.station_id = stn000.station_id;
                w5.week = SEstn003.week5;
                w5.time_start = SEstn003.tStart5.Remove(2, 1);
                w5.time_end = SEstn003.tEnd5.Remove(2, 1);
                SaveCreate(_db.stn003, w5);
                #endregion

                #region w6
                stn003 w6 = new stn003();
                w6.station_id = stn000.station_id;
                w6.week = SEstn003.week6;
                w6.time_start = SEstn003.tStart6.Remove(2, 1);
                w6.time_end = SEstn003.tEnd6.Remove(2, 1);
                SaveCreate(_db.stn003, w6);
                #endregion

                #region w7
                stn003 w7 = new stn003();
                w7.station_id = stn000.station_id;
                w7.week = SEstn003.week7;
                w7.time_start = SEstn003.tStart7.Remove(2, 1);
                w7.time_end = SEstn003.tEnd7.Remove(2, 1);
                SaveCreate(_db.stn003, w7);
                #endregion

                #endregion

                #region stn005 充電站圖片檔 新增

                #region 主圖
                if (rUpload != null)
                {
                    foreach (var item in rUpload)
                    {
                        fileDtail f = SaveUploadFile(item, stn000.station_id, "01");

                        stn005 stn005 = new stn005();
                        //程式給號
                        int picmst_id = 0;
                        List<stn005> dataId = _db.stn005.OrderByDescending(o => o.picmst_id).ToList();
                        picmst_id = dataId.Count == 0 ? (picmst_id + 1) : (dataId.FirstOrDefault().picmst_id + 1);
                        stn005.picmst_id = picmst_id;
                        stn005.station_id = stn000.station_id;
                        stn005.station_pic = f.FileName;
                        stn005.pic_status = "01";
                        SaveCreate(_db.stn005, stn005);
                    }
                }
                #endregion

                #region 其他圖
                if (sUpload != null)
                {
                    foreach (var item in sUpload)
                    {
                        fileDtail f = SaveUploadFile(item, stn000.station_id, "00");

                        stn005 stn005 = new stn005();
                        //程式給號
                        int picmst_id = 0;
                        List<stn005> dataId = _db.stn005.OrderByDescending(o => o.picmst_id).ToList();
                        picmst_id = dataId.Count == 0 ? (picmst_id + 1) : (dataId.FirstOrDefault().picmst_id + 1);
                        stn005.picmst_id = picmst_id;
                        stn005.station_id = stn000.station_id;
                        stn005.station_pic = f.FileName;
                        stn005.pic_status = "00";
                        SaveCreate(_db.stn005, stn005);
                    }
                }
                #endregion

                #endregion

                #region stn001 更新
                var reDatas = SpotItems.Where(x => x.chargespot_seq != 0);
                foreach (var item in reDatas)
                {
                    #region stn001
                    var stn001 = _db.stn001.Where(x => x.chargespot_id == item.chargespot_id && x.station_id == stn000.station_id).FirstOrDefault();
                    stn001.spot_floor = item.spot_floor;
                    stn001.spot_car_space = item.spot_car_space;
                    stn001.spot_note = item.spot_note;
                    stn001.spot_status = item.spot_status;

                    SaveUpdate(_db.stn001, stn001);
                    #endregion

                }
                #endregion

                #region stn101 更新
                foreach (var item in reDatas)
                {
                    var stn101 = _db.stn101.Where(x => x.chargespot_id == item.chargespot_id).FirstOrDefault();
                    stn101.device_sn = item.device_sn;
                    stn101.acpt_time = item.acpt_time;
                    stn101.wty_start = item.wty_start;
                    stn101.wty_end = item.wty_end;

                    SaveUpdate(_db.stn101, stn101);
                }
                #endregion

                var returnData = new
                {
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

        /// <summary>
        /// 營業時間 自定義
        /// </summary>
        public class SEstn003
        {
            public int week1 { get; set; }
            public string tStart1 { get; set; }
            public string tEnd1 { get; set; }
            public int week2 { get; set; }
            public string tStart2 { get; set; }
            public string tEnd2 { get; set; }
            public int week3 { get; set; }
            public string tStart3 { get; set; }
            public string tEnd3 { get; set; }
            public int week4 { get; set; }
            public string tStart4 { get; set; }
            public string tEnd4 { get; set; }
            public int week5 { get; set; }
            public string tStart5 { get; set; }
            public string tEnd5 { get; set; }
            public int week6 { get; set; }
            public string tStart6 { get; set; }
            public string tEnd6 { get; set; }
            public int week7 { get; set; }
            public string tStart7 { get; set; }
            public string tEnd7 { get; set; }

        }

        /// <summary>
        /// stn001 + stn101 (充電樁基本資料+充電站設備檔) 自定義
        /// </summary>
        public class SEstnCS
        {
            #region stn001 充電樁基本資料
            /// <summary>
            /// 充電槍ID
            /// </summary>
            public decimal chargespot_id { get; set; }
            /// <summary>
            /// 充電站ID
            /// </summary>
            public string station_id { get; set; }
            /// <summary>
            /// 充電樁序號(充電樁ID)
            /// </summary>
            public int chargespot_seq { get; set; }
            /// <summary>
            /// 充電槍序號(充電樁第幾支槍)
            /// </summary>
            public int chargegun_seq { get; set; }
            /// <summary>
            /// 充電樁所在樓層
            /// </summary>
            public string spot_floor { get; set; }
            /// <summary>
            /// 充電槍對應停車格
            /// </summary>
            public string spot_car_space { get; set; }
            /// <summary>
            /// 註記
            /// </summary>
            public string spot_note { get; set; }
            /// <summary>
            /// 充電槍狀態
            /// </summary>
            public int spot_status { get; set; }
            #endregion

            #region stn101 充電站設備檔
            /// <summary>
            /// 設備ID
            /// </summary>
            public int device_id { get; set; }
            /// <summary>
            /// 出貨單到 =>暫時沒有
            /// </summary>
            public int dely_id { get; set; }
            /// <summary>
            /// 設備序號(充電樁序號)
            /// </summary>
            public string device_sn { get; set; }
            /// <summary>
            /// 驗收時間
            /// </summary>
            public DateTime? acpt_time { get; set; }
            /// <summary>
            /// 保固開始時間
            /// </summary>
            public DateTime? wty_start { get; set; }
            /// <summary>
            /// 保固結束時間
            /// </summary>
            public DateTime? wty_end { get; set; }

            #endregion
        }

        // GET: Station/Edit/5
        public ActionResult Edit(string station_id)
        {
            stn000 stn000 = _db.stn000.Where(x => x.station_id == station_id).FirstOrDefault();
            ViewBag.stn000 = stn000;

            //stn003
            List<stn003> stn003 = _db.stn003.Where(x=> x.station_id == station_id).ToList();
            ViewBag.stn003 = stn003;
            //stn005
            var fileName = _db.stn005.Where(x => x.station_id == station_id).ToList().Select(a => a.station_pic);
            List<string> fname = new List<string>() { };
            var fnstr = String.Join(",", fileName);
            ViewBag.fnstr = fnstr;

            #region 選單
            // 站點類型
            var selectTypeList = new List<SelectListItem>();
            var t = _db.sts000.Where(x => x.fun_id == "station_type" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in t)
            {
                selectTypeList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectTypeList = new SelectList(selectTypeList, "Value", "Text", stn000.station_type);
            // 場域類別
            var selectClassList = new List<SelectListItem>();
            var c = _db.sts000.Where(x => x.fun_id == "station_class" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in c)
            {
                selectClassList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectClassList = new SelectList(selectClassList, "Value", "Text", stn000.station_class);
            //站點狀態
            var selectStatusList = new List<SelectListItem>();
            var s = _db.sts000.Where(x => x.fun_id == "station_status" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in s)
            {
                selectStatusList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectStatusList = new SelectList(selectStatusList, "Value", "Text", stn000.station_status);

            //縣市
            var selectCountryList = new List<SelectListItem>();
            var co = (from a in _db.zip000
                      select new { a.country }).ToList().Distinct();
            selectCountryList.Add(new SelectListItem { Text = "請選擇縣市", Value = "" });
            foreach (var item in co)
            {
                selectCountryList.Add(new SelectListItem { Text = item.country, Value = item.country });
            }
            ViewBag.selectCountryList = selectCountryList;

            //型號
            var selectDevModelList = new List<SelectListItem>();
            var d = _db.dev000.ToList();
            selectDevModelList.Add(new SelectListItem { Text = "請選擇充電樁型號", Value = "" });
            foreach (var item in d)
            {
                selectDevModelList.Add(new SelectListItem { Text = item.device_model, Value = item.device_id.ToString() });
            }
            ViewBag.selectDevModelList = selectDevModelList;

            //客戶ID
            var selectCustomerIDList = new List<SelectListItem>();
            var cId = _db.cus000.ToList();
            foreach (var item in cId)
            {
                selectCustomerIDList.Add(new SelectListItem { Text = item.customer_fullname, Value = item.customer_id.ToString() });
            }
            ViewBag.selectCustomerIDList = new SelectList(selectCustomerIDList, "Value", "Text", stn000.station_owner);
            #endregion 

            //充電站網路設定檔
            var stn102 = _db.stn102.ToList();
            ViewBag.stn102 = stn102;

            return View();
        }

        /// <summary>
        /// GetEditData stn001 + stn101
        /// </summary>
        /// <param name="station_id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetEditData(string station_id)
        {
            List<stn001> stn001s = _db.stn001.Where(x => x.station_id == station_id).ToList();
            var list = (from a in stn001s
                        join b in _db.stn101 on a.chargespot_id equals b.chargespot_id
                        where a.station_id == b.station_id
                        select new
                        {
                            chargespot_id = a.chargespot_id,
                            station_id = a.station_id,
                            chargespot_seq = a.chargespot_seq,
                            chargegun_seq = a.chargegun_seq,
                            spot_floor = a.spot_floor,
                            spot_car_space = a.spot_car_space,
                            spot_note = a.spot_note,
                            spot_status = a.spot_status,
                            device_id = b.device_id,
                            dely_id = b.dely_id,
                            device_sn = b.device_sn,
                            acpt_time = b.acpt_time,
                            wty_start = b.wty_start,
                            wty_end = b.wty_end
                        }).ToList();

            //生成主項目清單
            string jsonContent;
            JObject jObjectSpotEditData = new JObject();
            JArray jArraySpot = new JArray();

            if (list.Count > 0)
            {
                foreach(var i in list)
                {
                    JObject jObjectSpotItem = new JObject();
                    #region stn001
                    //充電槍ID
                    jObjectSpotItem.Add(new JProperty("chargespot_id", i.chargespot_id));
                    //充電站ID
                    jObjectSpotItem.Add(new JProperty("station_id", i.station_id));
                    //充電樁序號(充電樁ID)
                    jObjectSpotItem.Add(new JProperty("chargespot_seq", i.chargespot_seq));
                    //充電槍序號(充電樁第幾隻槍)
                    jObjectSpotItem.Add(new JProperty("chargegun_seq", i.chargegun_seq));
                    jObjectSpotItem.Add(new JProperty("spot_floor", i.spot_floor));
                    jObjectSpotItem.Add(new JProperty("spot_car_space", i.spot_car_space));
                    jObjectSpotItem.Add(new JProperty("spot_note", i.spot_note));
                    jObjectSpotItem.Add(new JProperty("spot_status", i.spot_status));
                    #endregion
                    #region stn101
                    jObjectSpotItem.Add(new JProperty("device_id", i.device_id));
                    jObjectSpotItem.Add(new JProperty("dely_id", i.dely_id));
                    jObjectSpotItem.Add(new JProperty("device_sn", i.device_sn));
                    jObjectSpotItem.Add(new JProperty("acpt_time", setDateFormat(i.acpt_time)));
                    jObjectSpotItem.Add(new JProperty("wty_start", setDateFormat(i.wty_start)));
                    jObjectSpotItem.Add(new JProperty("wty_end", setDateFormat(i.wty_end)));
                    #endregion
                    jArraySpot.Add(jObjectSpotItem);
                }
                jObjectSpotEditData.Add(new JProperty("jObjectSpotList", jArraySpot));
            }

            #region 充電槍狀態 的下拉選單
            JObject jObjectSpotStatus = new JObject();

            List<sts000> sts000 = _db.sts000.Where(x => x.prog_id == "stn001" && x.fun_id == "spot_status").ToList();

            foreach (var e in sts000)
            {
                jObjectSpotStatus.Add(new JProperty(e.item_id, e.item_desc));
            }

            jObjectSpotEditData.Add(new JProperty("DdlSSList", jObjectSpotStatus));
            #endregion

            #region 費率方案 的下拉選單
            JObject jObjectFee = new JObject();

            List<stn004> sts004 = _db.stn004.ToList();

            foreach (var e in sts004)
            {
                jObjectFee.Add(new JProperty(e.cal_unit, e.rate_name));
            }

            jObjectSpotEditData.Add(new JProperty("DdlFeeList", jObjectFee));
            #endregion

            jsonContent = JsonConvert.SerializeObject(jObjectSpotEditData, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        [HttpPost]
        public ActionResult GetnwEditData(string station_id)
        {
            List<stn102> stn102 = _db.stn102.Where(x => x.station_id == station_id).ToList();

            //生成主項目清單
            string jsonContent;
            JObject jObjectNetWorkList = new JObject();
            JArray jArrayNetWork = new JArray();

            foreach (var i in stn102)
            {
                JObject jObjectNetWork = new JObject();

                jObjectNetWork.Add(new JProperty("network_id", i.network_id));
                jObjectNetWork.Add(new JProperty("network_no", i.network_no));
                jObjectNetWork.Add(new JProperty("network_type", i.network_type));
                jObjectNetWork.Add(new JProperty("network_ip", i.network_ip));
                jObjectNetWork.Add(new JProperty("network_sn", i.network_sn));
                jObjectNetWork.Add(new JProperty("network_fee", i.network_fee));
                jObjectNetWork.Add(new JProperty("active_date", setDateFormat(i.active_date)));

                jArrayNetWork.Add(jObjectNetWork);
            }

            jObjectNetWorkList.Add(new JProperty("jObjectNetWorkList", jArrayNetWork));

            jsonContent = JsonConvert.SerializeObject(jObjectNetWorkList, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        // POST: Station/Edit/5
        [HttpPost]
        public ActionResult Edit(stn000 stn000, List<SEstnCS> SpotItems, SEstn003 SEstn003, HttpPostedFileBase[] rUpload, HttpPostedFileBase[] sUpload)
        {
            if (ModelState.IsValid)
            {
                #region stn003 充電站營業時間 新增
                //先全部刪除再新增
                var list = _db.stn003.Where(x => x.station_id == stn000.station_id).ToList();
                foreach (var i in list)
                {
                    SaveDelete(_db.stn003, i);
                }
                #region w1
                stn003 w1 = new stn003();
                w1.station_id = stn000.station_id;
                w1.week = SEstn003.week1;
                w1.time_start = SEstn003.tStart1.Remove(2, 1);
                w1.time_end = SEstn003.tEnd1.Remove(2, 1);
                SaveCreate(_db.stn003, w1);
                #endregion

                #region w2
                stn003 w2 = new stn003();
                w2.station_id = stn000.station_id;
                w2.week = SEstn003.week2;
                w2.time_start = SEstn003.tStart2.Remove(2, 1);
                w2.time_end = SEstn003.tEnd2.Remove(2, 1);
                SaveCreate(_db.stn003, w2);
                #endregion

                #region w3
                stn003 w3 = new stn003();
                w3.station_id = stn000.station_id;
                w3.week = SEstn003.week3;
                w3.time_start = SEstn003.tStart3.Remove(2, 1);
                w3.time_end = SEstn003.tEnd3.Remove(2, 1);
                SaveCreate(_db.stn003, w3);
                #endregion

                #region w4
                stn003 w4 = new stn003();
                w4.station_id = stn000.station_id;
                w4.week = SEstn003.week4;
                w4.time_start = SEstn003.tStart4.Remove(2, 1);
                w4.time_end = SEstn003.tEnd4.Remove(2, 1);
                SaveCreate(_db.stn003, w4);
                #endregion

                #region w5
                stn003 w5 = new stn003();
                w5.station_id = stn000.station_id;
                w5.week = SEstn003.week5;
                w5.time_start = SEstn003.tStart5.Remove(2, 1);
                w5.time_end = SEstn003.tEnd5.Remove(2, 1);
                SaveCreate(_db.stn003, w5);
                #endregion

                #region w6
                stn003 w6 = new stn003();
                w6.station_id = stn000.station_id;
                w6.week = SEstn003.week6;
                w6.time_start = SEstn003.tStart6.Remove(2, 1);
                w6.time_end = SEstn003.tEnd6.Remove(2, 1);
                SaveCreate(_db.stn003, w6);
                #endregion

                #region w7
                stn003 w7 = new stn003();
                w7.station_id = stn000.station_id;
                w7.week = SEstn003.week7;
                w7.time_start = SEstn003.tStart7.Remove(2, 1);
                w7.time_end = SEstn003.tEnd7.Remove(2, 1);
                SaveCreate(_db.stn003, w7);
                #endregion

                #endregion

                #region stn005 充電站圖片檔 新增

                #region 主圖
                if (rUpload != null)
                {
                    foreach (var item in rUpload)
                    {
                        fileDtail f = SaveUploadFile(item, stn000.station_id, "01");

                        stn005 stn005 = new stn005();
                        //程式給號
                        int picmst_id = 0;
                        List<stn005> dataId = _db.stn005.OrderByDescending(o => o.picmst_id).ToList();
                        picmst_id = dataId.Count == 0 ? (picmst_id + 1) : (dataId.FirstOrDefault().picmst_id + 1);
                        stn005.picmst_id = picmst_id;
                        stn005.station_id = stn000.station_id;
                        stn005.station_pic = f.FileName;
                        stn005.pic_status = "01";
                        SaveCreate(_db.stn005, stn005);
                    }
                }
                #endregion

                #region 其他圖
                if (sUpload != null)
                {
                    foreach (var item in sUpload)
                    {
                        fileDtail f = SaveUploadFile(item, stn000.station_id, "00");

                        stn005 stn005 = new stn005();
                        //程式給號
                        int picmst_id = 0;
                        List<stn005> dataId = _db.stn005.OrderByDescending(o => o.picmst_id).ToList();
                        picmst_id = dataId.Count == 0 ? (picmst_id + 1) : (dataId.FirstOrDefault().picmst_id + 1);
                        stn005.picmst_id = picmst_id;
                        stn005.station_id = stn000.station_id;
                        stn005.station_pic = f.FileName;
                        stn005.pic_status = "00";
                        SaveCreate(_db.stn005, stn005);
                    }
                }
                #endregion

                #endregion

                #region stn000 充電站基本資料 更新
                var s0 = _db.stn000.Where(x => x.station_id == stn000.station_id).FirstOrDefault();
                s0.station_id = stn000.station_id;
                s0.station_owner = stn000.station_owner;
                s0.station_name = stn000.station_name;
                s0.station_address = stn000.station_address;
                s0.station_gate_address = "";
                s0.station_type = stn000.station_type;
                s0.station_class = stn000.station_class;
                s0.station_zip = stn000.station_zip;
                //經緯度
                s0.station_longitude = stn000.station_longitude;
                s0.station_latitude = stn000.station_latitude;
                //暫時無用
                s0.station_gate_pic = stn000.station_gate_pic;
                s0.station_parking_pic = stn000.station_parking_pic;

                s0.station_floor = stn000.station_floor;
                s0.station_car_space = stn000.station_car_space;
                s0.contact_person_local = stn000.contact_person_local;
                s0.contact_mobile_local = stn000.contact_mobile_local;
                s0.customer_phone2_local = stn000.customer_phone2_local;
                s0.contact_email_local = stn000.contact_email_local;
                s0.contact_lineid_local = stn000.contact_lineid_local;
                s0.contact_fax_local = stn000.contact_fax_local;
                s0.station_note = stn000.station_note;
                s0.station_note1 = stn000.station_note1;
                s0.station_status = stn000.station_status;
                s0.crt_user = stn000.crt_user;
                s0.crt_date = stn000.crt_date;

                SaveUpdate(_db.stn000, stn000);
                #endregion

                #region stn001 更新
                var reDatas = SpotItems.Where(x => x.chargespot_seq != 0);
                foreach (var item in reDatas)
                {
                    #region stn001
                    var stn001 = _db.stn001.Where(x => x.chargespot_id == item.chargespot_id && x.station_id == stn000.station_id).FirstOrDefault();
                    stn001.spot_floor = item.spot_floor;
                    stn001.spot_car_space = item.spot_car_space;
                    stn001.spot_note = item.spot_note;
                    stn001.spot_status = item.spot_status;

                    SaveUpdate(_db.stn001, stn001);
                    #endregion

                }
                #endregion

                #region stn101 更新
                foreach (var item in reDatas)
                {
                    var stn101 = _db.stn101.Where(x => x.chargespot_id == item.chargespot_id).FirstOrDefault();
                    stn101.device_sn = item.device_sn;
                    stn101.acpt_time = item.acpt_time;
                    stn101.wty_start = item.wty_start;
                    stn101.wty_end = item.wty_end;

                    SaveUpdate(_db.stn101, stn101);
                }
                #endregion

                var returnData = new
                {
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


        // POST: Station/Delete/5
        [HttpPost]
        public ActionResult Delete(string station_id)
        {
            try
            {
                //stn000
                var stn000 = _db.stn000.Where(x=>x.station_id == station_id).FirstOrDefault();
                SaveDelete(_db.stn000, stn000);

                //stn001
                var stn001 = _db.stn001.Where(x => x.station_id == station_id).ToList();
                foreach(var i in stn001)
                {
                    SaveDelete(_db.stn001, i);
                }

                //stn003
                var stn003 = _db.stn003.Where(x => x.station_id == station_id).ToList();
                foreach (var j in stn003)
                {
                    SaveDelete(_db.stn003, j);
                }

                //stn005
                var stn005 = _db.stn005.Where(x => x.station_id == station_id).ToList();
                foreach (var k in stn005)
                {
                    SaveDelete(_db.stn005, k);
                }

                //stn101
                var stn101 = _db.stn101.Where(x => x.station_id == station_id).ToList();
                foreach (var m in stn101)
                {
                    SaveDelete(_db.stn101, m);
                }

                var returnData = new
                {
                    IsSuccess = true
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch
            {
                var returnData = new
                {
                    // 成功與否
                    IsSuccess = false
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
        }

        /// <summary>
        /// 新增的取消，返回index (刪除已新增資料stn001、stn101，更新stn102)
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DelCreate(string sId)
        {
            try
            {
                //string[] nId = ntStr.Split(',');

                //foreach (var i in nId)
                //{
                //    var stn102 = _db.stn102.Where(x => x.network_id.ToString() == i.ToString()).FirstOrDefault();
                //    stn102.station_id = "";

                //    SaveUpdate(_db.stn102, stn102);
                //}

                //刪除已新增的stn001
                var stn001 = _db.stn001.Where(x=>x.station_id == sId).ToList();
                if(stn001.Count() > 0)
                {
                    foreach(var i in stn001)
                    {
                        SaveDelete(_db.stn001, i);
                    }
                }

                //刪除已新增的stn101
                var stn101 = _db.stn101.Where(x => x.station_id == sId).ToList();
                if (stn101.Count() > 0)
                {
                    foreach (var j in stn101)
                    {
                        SaveDelete(_db.stn101, j);
                    }
                }

                //更新匯入的站點
                var stn102 = _db.stn102.Where(x => x.station_id == sId).ToList();
                if (stn102.Count() > 0)
                {
                    foreach (var k in stn102)
                    {
                        k.station_id = "";
                        SaveUpdate(_db.stn102, k);
                    }
                }

                var returnData = new
                {
                    IsSuccess = true
                };

                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch (Exception ex)
            {
                var returnData = new
                {
                    IsSuccess = false,
                    ErrorInfo = ex.Message
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }

        }

        /// <summary>
        /// 編輯的取消，返回index (暫時先註解掉)
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult DelEdit(string sId)
        //{
        //    try
        //    {
        //        var stn001 = _db.stn001.Where(x => x.station_id == sId && x.spot_floor == "").ToList();

        //        List<string> cIdList = new List<string>();
        //        foreach (var k in stn001)
        //        {
        //            cIdList.Add(k.chargespot_id.ToString());
        //        }

        //        //刪除已新增的stn001
        //        if (stn001.Count() > 0)
        //        {
        //            foreach (var i in stn001)
        //            {
        //                SaveDelete(_db.stn001, i);
        //            }
        //        }

        //        //刪除已新增的stn101
        //        var stn101 = _db.stn101.Where(x => x.station_id == sId).ToList();
        //        if (stn101.Count() > 0)
        //        {
        //            foreach (var j in stn101)
        //            {
        //                foreach(var k in cIdList)
        //                {
        //                    if(j.chargespot_id.ToString() == k)
        //                    {
        //                        SaveDelete(_db.stn101, j);
        //                    }
        //                }
        //            }
        //        }

        //        var returnData = new
        //        {
        //            IsSuccess = true
        //        };

        //        return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
        //    }
        //    catch (Exception ex)
        //    {
        //        var returnData = new
        //        {
        //            IsSuccess = false,
        //            ErrorInfo = ex.Message
        //        };
        //        return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
        //    }

        //}

        /// <summary>
        /// 檢視
        /// </summary>
        /// <param name="station_id"></param>
        /// <returns></returns>
        public ActionResult Detail(string station_id)
        {
            stn000 stn000 = _db.stn000.Where(x => x.station_id == station_id).FirstOrDefault();
            ViewBag.stn000 = stn000;

            //stn003
            List<stn003> stn003 = _db.stn003.Where(x => x.station_id == station_id).ToList();
            ViewBag.stn003 = stn003;
            //stn005
            var fileName = _db.stn005.Where(x => x.station_id == station_id).ToList().Select(a => a.station_pic);
            List<string> fname = new List<string>() { };
            var fnstr = String.Join(",", fileName);
            ViewBag.fnstr = fnstr;

            #region 選單
            // 站點類型
            var selectTypeList = new List<SelectListItem>();
            var t = _db.sts000.Where(x => x.fun_id == "station_type" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in t)
            {
                selectTypeList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectTypeList = new SelectList(selectTypeList, "Value", "Text", stn000.station_type);
            // 場域類別
            var selectClassList = new List<SelectListItem>();
            var c = _db.sts000.Where(x => x.fun_id == "station_class" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in c)
            {
                selectClassList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectClassList = new SelectList(selectClassList, "Value", "Text", stn000.station_class);
            //站點狀態
            var selectStatusList = new List<SelectListItem>();
            var s = _db.sts000.Where(x => x.fun_id == "station_status" && x.prog_id == "stn000").OrderBy(x => x.item_seq);
            foreach (var item in s)
            {
                selectStatusList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectStatusList = new SelectList(selectStatusList, "Value", "Text", stn000.station_status);

            //縣市
            var selectCountryList = new List<SelectListItem>();
            var co = (from a in _db.zip000
                      select new { a.country }).ToList().Distinct();
            selectCountryList.Add(new SelectListItem { Text = "請選擇縣市", Value = "" });
            foreach (var item in co)
            {
                selectCountryList.Add(new SelectListItem { Text = item.country, Value = item.country });
            }
            ViewBag.selectCountryList = selectCountryList;

            //型號
            var selectDevModelList = new List<SelectListItem>();
            var d = _db.dev000.ToList();
            selectDevModelList.Add(new SelectListItem { Text = "請選擇型號", Value = "" });
            foreach (var item in d)
            {
                selectDevModelList.Add(new SelectListItem { Text = item.device_model, Value = item.device_id.ToString() });
            }
            ViewBag.selectDevModelList = selectDevModelList;

            //客戶ID
            var selectCustomerIDList = new List<SelectListItem>();
            var cId = _db.cus000.ToList();
            foreach (var item in cId)
            {
                selectCustomerIDList.Add(new SelectListItem { Text = item.customer_fullname, Value = item.customer_id.ToString() });
            }
            ViewBag.selectCustomerIDList = new SelectList(selectCustomerIDList, "Value", "Text", stn000.station_owner);
            #endregion 

            //充電站網路設定檔
            var stn102 = _db.stn102.Where(x=>x.station_id == station_id).ToList();
            ViewBag.stn102 = stn102;

            return View();
        }


        /// <summary>
        /// 儲存上傳檔案，
        /// 檔名格式為yyyyMMddxx_原檔名，xx為流水號
        /// </summary>
        /// <param name="upfile">HttpPostedFile 物件</param>
        /// <param name="type">上傳檔案類別</param>
        /// <returns>儲存檔名</returns>
        static private fileDtail SaveUploadFile(HttpPostedFileBase upfile, string station_id, string type)
        {
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/upfiles/stnFiles/" + station_id + "/" + type);

            //建立檔案資料夾
            string docupath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "upfiles\\stnFiles\\" +
                              station_id + "\\" + type + "\\";
            if (!Directory.Exists(docupath))
            {
                Directory.CreateDirectory(@docupath);
            }

            //取得副檔名
            string extension = upfile.FileName.Split('.')[upfile.FileName.Split('.').Length - 1].ToLower();
            //這是原本檔名
            //string fileNameWithoutExtension =
            //    upfile.FileName.Substring(0, upfile.FileName.Length - extension.Length - 1);
            //string fileName = fileNameWithoutExtension + "." + extension;

            //自己寫取亂碼
            string fileNameRandom = RandomString(6);
            string fileName = fileNameRandom + "." + extension;

            string yyyyMMdd = DateTime.Now.ToString("yyyyMMdd");

            //若有同一日有重複檔名，檔名需變成最大流水號+1
            int i = 0;
            foreach (string fp in Directory.GetFiles(filePath))
            {
                string fn = Path.GetFileName(fp);
                //舊檔案除去流水號的完整檔名
                string existedfileName = fn.Substring(fn.IndexOf('_') + 1);

                if (fn.Substring(0, 8).Equals(yyyyMMdd) && existedfileName.Equals(fileName))
                {
                    int seq = int.Parse(fn.Substring(8, 2));
                    if (i < seq)
                    {
                        i = seq;
                    }
                }
            }

            //string fileNameTemp = yyyyMMdd + (i + 1).ToString("00") + "_" + fileNameWithoutExtension;
            string fileNameTemp = yyyyMMdd + (i + 1).ToString("00") + "_" + fileNameRandom;
            string newfileName = string.Format("{0}.{1}", fileNameTemp, extension);
            string savedPath = Path.Combine(filePath, newfileName);
            upfile.SaveAs(savedPath);

            fileDtail fileDtail = new fileDtail();
            fileDtail.FileName = newfileName;
            fileDtail.savedPath = savedPath;

            return fileDtail;
        }

        // 功能：回傳既存檔案
        public object GetFile(string id, string type, string fileName)
        {
            var Form = _db.stn005.Where(w => w.station_id == id);

            string extension = fileName.Split('.')[fileName.Split('.').Length - 1];
            extension = extension.ToLower();

            if (extension.Equals("xlsx") || extension.Equals("xls"))
            {
                return File(
                    Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                    "application/vnd.ms-excel", fileName);
            }

            if (extension.Equals("docx") || extension.Equals("doc"))
            {
                return File(
                    Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                    "application/msword", fileName);
            }

            if (extension.Equals("pptx") || extension.Equals("ppt"))
            {
                return File(
                    Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                    "application/vnd.ms-powerpoint", fileName);
            }

            if (extension.Equals("jpg") || extension.Equals("jpeg"))
            {
                return File(
                    Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                    "image/jpeg", fileName);
            }

            if (extension.Equals("png"))
            {
                return File(
                    Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                    "image/png", fileName);
            }

            return File(Server.MapPath("~/upfiles/stnFiles/" + id + "/" + type + "/" + fileName),
                "application/pdf", fileName);
        }

        // 功能：刪除檔案
        public ActionResult RemoveUploadFile(string id, string type, string fileName)
        {
            try
            {
                var stn005 = _db.stn005.Where(w => w.station_id == id && w.station_pic == fileName).Single();

                SaveDelete(_db.stn005, stn005);

                var returnData = new
                {
                    IsSuccess = true,
                    fileName
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch
            {
                var returnData = new
                {
                    // 成功與否
                    IsSuccess = false
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
        }

        /// <summary>
        /// 檔案使用自定義
        /// </summary>
        public class fileDtail
        {
            public string FileName { get; set; }
            public string savedPath { get; set; }
        }

        /// <summary>
        /// 取亂碼
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
