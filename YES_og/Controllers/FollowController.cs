using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using YES_og.Models;
using static System.Net.Mime.MediaTypeNames;
using static YES_og.Controllers.FollowController;

namespace YES_og.Controllers
{
    public class FollowController : _BaseController
    {
        private DBePowerDataContext _db = new DBePowerDataContext();
        private static Random random = new Random();
        // GET: Follow
        public ActionResult Index()
        {
            List<sts000> sts = _db.sts000.ToList();
            ViewBag.sts = sts;
            List<zip000> zip000 = _db.zip000.ToList();
            ViewBag.zip000 = zip000;

            var cnc000 = _db.cnc000.OrderByDescending(x=>x.crt_date);
            return View(cnc000);
        }

        // GET: Follow/Create
        public ActionResult Create()
        {
            #region 準備欄位
            //案件編號流水號
            //string followNum = string.Format("{0,1:000000000}", int.Parse("1"));
            //string cncId = int.Parse("1").ToString("000000");
            //ViewBag.followNum = followNum;
            int cnc_id = 0;
            List<cnc000> data = _db.cnc000.OrderByDescending(o => o.cnc_id).ToList();
            cnc_id = data.Count == 0 ? (cnc_id + 1) : (data.FirstOrDefault().cnc_id + 1);
            ViewBag.cnc_id = cnc_id;
            string cncId = int.Parse(cnc_id.ToString()).ToString("000000");
            ViewBag.cncId = cncId;
            
            #endregion

            #region 選單
            //狀態
            var selectStatusList = new List<SelectListItem>();
            var s = _db.sts000.Where(x => x.fun_id == "cnc_status" && x.prog_id == "cnc000").OrderBy(x => x.item_seq);
            foreach (var item in s)
            {
                selectStatusList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectStatusList = selectStatusList;

            //住宅類型
            var selectLocList = new List<SelectListItem>();
            var L = _db.sts000.Where(x => x.fun_id == "loc_class" && x.prog_id == "cnc000").OrderBy(x => x.item_seq);
            foreach (var item in L)
            {
                selectLocList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.selectLocList = selectLocList;

            //車款
            var selectVehList = new List<SelectListItem>();
            var v = (from a in _db.veh000
                     select new { a.veh_brand }).ToList().Distinct();
            foreach (var item in v)
            {
                selectVehList.Add(new SelectListItem { Text = item.veh_brand, Value = item.veh_brand });
            }
            ViewBag.selectVehList = selectVehList;

            //郵遞區號
            var selectZipList = new List<SelectListItem>();
            var z = (from a in _db.zip000
                     select new { a.zip_id }).ToList();
            //var z = _db.zip000.ToList();
            foreach (var item in z)
            {
                //selectZipList.Add(new SelectListItem { Text = item.country + " - " +item.district, Value = item.zip_id });
                selectZipList.Add(new SelectListItem { Text = item.zip_id, Value = item.zip_id });
            }
            ViewBag.selectZipList = selectZipList;
            #endregion 

            return View();
        }

        // POST: Follow/Create
        [HttpPost]
        public ActionResult Create(cncForm cncForm,HttpPostedFileBase[] aUpload, HttpPostedFileBase[] bUpload, HttpPostedFileBase[] cUpload, HttpPostedFileBase[] dUpload)
        {
            if (ModelState.IsValid)
            {
                #region cnc000
                //cnc000
                cnc000 cnc000 = new cnc000();
                cnc000.cnc_id = cncForm.cnc_id;
                cnc000.veh_brand = cncForm.veh_brand;
                cnc000.contact_person = cncForm.contact_person;
                cnc000.contact_mobile = cncForm.contact_mobile;
                cnc000.contact_email = cncForm.contact_email;
                cnc000.install_address = cncForm.install_address;
                cnc000.zip_id = cncForm.zip_id;
                cnc000.loc_class = cncForm.loc_class;
                cnc000.contact_time = cncForm.contact_time;
                cnc000.dealer_name = cncForm.dealer_name;
                cnc000.sales_info = cncForm.sales_info;
                cnc000.note = cncForm.note;
                cnc000.note1 = cncForm.note1;
                cnc000.cnc_status = cncForm.cnc_status;

                SaveCreate(_db.cnc000, cnc000);

                #endregion

                #region cnc001
                //cnc001
                cnc001 cnc001 = new cnc001();
                //程式給號
                int record_id = 0;
                List<cnc001> data = _db.cnc001.OrderByDescending(o => o.record_id).ToList();
                record_id = data.Count == 0 ? (record_id + 1) : (data.FirstOrDefault().record_id + 1);
                cnc001.record_id = record_id;
                cnc001.cnc_id = cncForm.cnc_id;
                DateTime sDate = DateTime.ParseExact(cncForm.surveyDate+" "+cncForm.surveyTime, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                DateTime iDate = DateTime.ParseExact(cncForm.installDate + " " + cncForm.installTime, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                //string[] s = new string[] { cncForm.surveyDate, cncForm.surveyTime };
                //string survey_time = String.Join(" ", s);
                //cnc001.survey_time = DateTime.Parse(survey_time);
                //string[] i = new string[] { cncForm.installDate, cncForm.installTime };
                //string install_time = String.Join(" ", i);
                //cnc001.install_time = DateTime.Parse(install_time);
                cnc001.survey_time = sDate;
                cnc001.install_time = iDate;
                cnc001.note1 = cncForm.note1;
                cnc001.cnc_status = cncForm.cnc_status;

                SaveCreate(_db.cnc001, cnc001);

                #endregion

                #region cnc002
                //type1
                if (aUpload != null)
                {
                    foreach (var item in aUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id,"00");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "00";
                        cnc002.file_status = "01";
                        SaveCreate(_db.cnc002, cnc002);
                    }
                }

                //type2
                if (bUpload != null)
                {
                    foreach (var item in bUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "01");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "01";
                        cnc002.file_status = "01";
                        SaveCreate(_db.cnc002, cnc002);
                    }
                }

                //type3
                if (cUpload != null)
                {
                    foreach (var item in cUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "02");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "02";
                        cnc002.file_status = "01";
                        SaveCreate(_db.cnc002, cnc002);
                    }
                }

                //type4
                if (dUpload != null)
                {
                    foreach (var item in dUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "03");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "03";
                        cnc002.file_status = "01";
                        SaveCreate(_db.cnc002, cnc002);
                    }
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

        // GET: Follow/Edit/5
        public ActionResult Edit(int id)
        {
            cnc000 cnc000 = _db.cnc000.Where(x => x.cnc_id == id).FirstOrDefault();
            ViewBag.cnc000 = cnc000;
            cnc001 cnc001 = _db.cnc001.Where(x => x.cnc_id == id).FirstOrDefault();
            ViewBag.cnc001 = cnc001;
            List<cnc002> cnc002s = _db.cnc002.Where(x=> x.cnc_id == id).ToList();
            ViewBag.cnc002s = cnc002s;
            var fileName = _db.cnc002.Where(x => x.cnc_id == id && x.file_status=="01").ToList().Select(a=>a.File_name);
            List<string> fname = new List<string>() { };
            var fnstr = String.Join(",", fileName);
            ViewBag.fnstr = fnstr;

            #region 選單
            //狀態
            var selectStatusList = new List<SelectListItem>();
            var s = _db.sts000.Where(x => x.fun_id == "cnc_status" && x.prog_id == "cnc000" ).OrderBy(x => x.item_seq);
            foreach (var item in s)
            {
                selectStatusList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id});
            }
            ViewBag.cnc_statusList = new SelectList(selectStatusList, "Value", "Text", cnc000.cnc_status);

            //住宅類型
            var selectLocList = new List<SelectListItem>();
            var L = _db.sts000.Where(x => x.fun_id == "loc_class" && x.prog_id == "cnc000" ).OrderBy(x => x.item_seq);
            foreach (var item in L)
            {
                selectLocList.Add(new SelectListItem { Text = item.item_desc, Value = item.item_id });
            }
            ViewBag.loc_classList = new SelectList(selectLocList, "Value", "Text", cnc000.loc_class);
            //車款
            var selectVehList = new List<SelectListItem>();
            var v = (from a in _db.veh000
                     select new { a.veh_brand }).ToList().Distinct();
            foreach (var item in v)
            {
                selectVehList.Add(new SelectListItem { Text = item.veh_brand, Value = item.veh_brand });
            }
            //ViewBag.veh_brandList = new SelectList(selectVehList, "Value", "Text", cnc000.veh_brand);
            ViewBag.selectVehList = new SelectList(selectVehList, "Value", "Text", cnc000.veh_brand);


            //郵遞區號
            var selectZipList = new List<SelectListItem>();
            var z = (from a in _db.zip000
                     select new { a.zip_id }).ToList();
            //var z = _db.zip000.ToList();
            foreach (var item in z)
            {
                //selectZipList.Add(new SelectListItem { Text = item.country + " - " + item.district, Value = item.zip_id });
                selectZipList.Add(new SelectListItem { Text = item.zip_id, Value = item.zip_id });
            }
            ViewBag.selectZipList = new SelectList(selectZipList, "Value", "Text", cnc000.zip_id);
            #endregion 

            return View(cnc000);
        }

        // POST: Follow/Edit/5
        [HttpPost]
        public ActionResult Edit(cncForm cncForm, HttpPostedFileBase[] aUpload, HttpPostedFileBase[] bUpload, HttpPostedFileBase[] cUpload, HttpPostedFileBase[] dUpload)
        {
            if (ModelState.IsValid)
            {
                #region cnc000
                //cnc000
                var cnc000 = _db.cnc000.Where(x => x.cnc_id == cncForm.cnc_id).FirstOrDefault();
                cnc000.veh_brand = cncForm.veh_brand;
                cnc000.contact_person = cncForm.contact_person;
                cnc000.contact_mobile = cncForm.contact_mobile;
                cnc000.contact_email = cncForm.contact_email;
                cnc000.install_address = cncForm.install_address;
                cnc000.zip_id = cncForm.zip_id;
                cnc000.loc_class = cncForm.loc_class;
                cnc000.contact_time = cncForm.contact_time;
                cnc000.dealer_name = cncForm.dealer_name;
                cnc000.sales_info = cncForm.sales_info;
                cnc000.note = cncForm.note;
                cnc000.note1 = cncForm.note1;
                cnc000.cnc_status = cncForm.cnc_status;

                SaveUpdate(_db.cnc000, cnc000);

                #endregion

                #region cnc001
                //cnc001
                var cnc001 = _db.cnc001.Where(x => x.cnc_id == cncForm.cnc_id).FirstOrDefault();
                DateTime sDate = DateTime.ParseExact(cncForm.surveyDate + " " + cncForm.surveyTime, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                DateTime iDate = DateTime.ParseExact(cncForm.installDate + " " + cncForm.installTime, "yyyy/MM/dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                
                cnc001.survey_time = sDate;
                cnc001.install_time = iDate;
                cnc001.note1 = cncForm.note1;
                cnc001.cnc_status = cncForm.cnc_status;

                SaveUpdate(_db.cnc001, cnc001);

                #endregion

                #region cnc002
                //type1
                if (aUpload != null)
                {
                    foreach (var item in aUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "00");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "00";
                        cnc002.file_status = "01";

                        var newDB = new DBePowerDataContext();
                        SaveCreate(_db.cnc002, cnc002 , false, newDB.cnc002);
                    }
                }

                //type2
                if (bUpload != null)
                {
                    foreach (var item in bUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "01");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "01";
                        cnc002.file_status = "01";

                        var newDB = new DBePowerDataContext();
                        SaveCreate(_db.cnc002, cnc002, false, newDB.cnc002);
                    }
                }

                //type3
                if (cUpload != null)
                {
                    foreach (var item in cUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "02");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "02";
                        cnc002.file_status = "01";

                        var newDB = new DBePowerDataContext();
                        SaveCreate(_db.cnc002, cnc002, false, newDB.cnc002);
                    }
                }

                //type4
                if (dUpload != null)
                {
                    foreach (var item in dUpload)
                    {
                        fileDtail f = SaveUploadFile(item, cncForm.cnc_id, "03");

                        cnc002 cnc002 = new cnc002();
                        //程式給號
                        int file_id = 0;
                        List<cnc002> dataId = _db.cnc002.OrderByDescending(o => o.file_id).ToList();
                        file_id = dataId.Count == 0 ? (file_id + 1) : (dataId.FirstOrDefault().file_id + 1);
                        cnc002.file_id = file_id;
                        cnc002.cnc_id = cncForm.cnc_id;
                        cnc002.File_name = f.FileName;
                        cnc002.File_path = f.savedPath;
                        cnc002.file_type = "03";
                        cnc002.file_status = "01";

                        var newDB = new DBePowerDataContext();
                        SaveCreate(_db.cnc002, cnc002, false, newDB.cnc002);
                    }
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
        /// 發送btn，get detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult sendMailDetail(int id)
        {
            string jsonContent;
            JObject jObjectDetail = new JObject();
            var c = _db.cnc000.Where(x => x.cnc_id == id).FirstOrDefault();
            var c1 = _db.cnc001.Where(x => x.cnc_id == id).FirstOrDefault();
            string s = c1.survey_time.ToString();
            string i = c1.install_time.ToString();
            jObjectDetail.Add(new JProperty("cnc_id", id));
            jObjectDetail.Add(new JProperty("contact_person", c.contact_person));
            jObjectDetail.Add(new JProperty("contact_mobile", c.contact_mobile));
            jObjectDetail.Add(new JProperty("contact_email", c.contact_email));
            jObjectDetail.Add(new JProperty("install_address", c.install_address));
            jObjectDetail.Add(new JProperty(!string.IsNullOrEmpty(s)
                    ? new JProperty("survey_time", String.Format("{0:yyyy/MM/dd HH:mm:ss}", s))
                    : new JProperty("survey_time", "")));
            jObjectDetail.Add(new JProperty(!string.IsNullOrEmpty(i)
                    ? new JProperty("install_time", String.Format("{0:yyyy/MM/dd HH:mm:ss}", i))
                    : new JProperty("install_time", "")));

            string cncId = int.Parse(id.ToString()).ToString("000000");
            jObjectDetail.Add(new JProperty("cncId", cncId));

            jsonContent = JsonConvert.SerializeObject(jObjectDetail, Formatting.Indented);
            return new ContentResult { Content = jsonContent, ContentType = "application/json" };
        }

        //寄信功能
        [HttpPost]
        public ActionResult SendEmail(emailDetail formData, HttpPostedFileBase[] tUpload)
        {
            string failedMsg = "";
            try
            {
                MailMessage msg1 = new MailMessage();
                //收件者，以逗號分隔不同收件者 ex "test@gmail.com,test2@gmail.com"
                List<string> MailList = new List<string>();
                MailList.Add(formData.recipientMail);
                msg1.To.Add(string.Join(",", MailList.ToArray()));

                //MailAddress(寄信的帳號,寄信帳號的名稱,System.Text.Encoding.UTF8)
                msg1.From = new MailAddress("hannie.peng@yes-charging.com.tw", "裕電能源", System.Text.Encoding.UTF8);

                msg1.Subject = formData.subject;
                //郵件標題編碼  
                msg1.SubjectEncoding = System.Text.Encoding.UTF8;
                //郵件內容
                msg1.Body = formData.content.Replace("\r\n", "<br/>");
                msg1.IsBodyHtml = true;
                msg1.BodyEncoding = System.Text.Encoding.UTF8; //郵件內容編碼 
                msg1.Priority = MailPriority.Normal; //郵件優先級 
                                                     //建立 SmtpClient 物件 並設定 Gmail的smtp主機及Port 

                //附件
                if (tUpload != null)
                {
                    foreach (var item in tUpload)
                    {
                        fileDtail f = SaveEmailUploadFile(item, formData.cnc_id);
                        msg1.Attachments.Add(new Attachment(f.savedPath));
                    }
                }
                //foreach(var item in tUpload)
                //{
                //    msg1.Attachments.Add(new Attachment(Path + "\\" + item));
                //}

                //宣告寄信郵件伺服器的連接
                SmtpClient mySmtp = new SmtpClient("smtp.gmail.com", 587);

                //設定你的帳號密碼
                mySmtp.Credentials = new System.Net.NetworkCredential("hannie.peng@yes-charging.com.tw", "hanni8712");
                //Gmial 的 smtp 使用 SSL
                mySmtp.EnableSsl = true;
                mySmtp.Send(msg1);

                var returnData = new
                {
                    IsSuccess = true
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }
            catch (Exception ex)
            {
                failedMsg = ex.ToString();
                var returnData = new
                {
                    IsSuccess = false
                };
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(returnData), "application/json");
            }

            
        }

        public class emailDetail
        {
            public string subject { get; set; }
            public string content { get; set; }
            public string recipientMail { get; set; }
            public int cnc_id { get; set; }
        }

        static private fileDtail SaveEmailUploadFile(HttpPostedFileBase upfile,int cnc_id)
        {
            string filePath = System.Web.HttpContext.Current.Server.MapPath("~/upfiles/emailFile/" + cnc_id + "/");

            //建立檔案資料夾
            string docupath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "upfiles\\emailFile\\" +
                              cnc_id + "\\" ;
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

        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Follow/Delete/5
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

        /// <summary>
        /// 自訂Model (class)
        /// </summary>
        public class cncForm
        {
            /// <summary>
            /// 單號ID
            /// </summary>
            public int cnc_id { get; set; }
            /// <summary>
            /// 車款品牌
            /// </summary>
            public string veh_brand { get; set; }
            /// <summary>
            /// 聯絡人姓名
            /// </summary>
            public string contact_person { get; set; }
            /// <summary>
            /// 聯絡人手機
            /// </summary>
            public string contact_mobile { get; set; }
            /// <summary>
            /// 聯絡人e-mail
            /// </summary>
            public string contact_email { get; set; }
            /// <summary>
            /// 安裝地址
            /// </summary>
            public string install_address { get; set; }
            /// <summary>
            /// 郵遞區號
            /// </summary>
            public string zip_id { get; set; }
            /// <summary>
            /// 站點屬性
            /// </summary>
            public string loc_class { get; set; }
            /// <summary>
            /// 聯繫時間
            /// </summary>
            public string contact_time { get; set; }
            /// <summary>
            /// 購車據點
            /// </summary>
            public string dealer_name { get; set; }
            /// <summary>
            /// 購車資訊:業代/訂單編號
            /// </summary>
            public string sales_info { get; set; }
            /// <summary>
            /// 客戶備註
            /// </summary>
            public string note { get; set; }
            /// <summary>
            /// 備註
            /// </summary>
            public string note1 { get; set; }
            /// <summary>
            /// cnc狀態
            /// </summary>
            public string cnc_status { get; set; }
            /// <summary>
            /// 申請時間
            /// </summary>
            public DateTime crt_date { get; set; }
            //public DateTime survey_time { get; set; }
            //public DateTime install_time { get; set; }
            /// <summary>
            /// 場勘日期
            /// </summary>
            public string surveyDate { get; set; }
            /// <summary>
            /// 場勘時間
            /// </summary>
            public string surveyTime { get; set; }
            /// <summary>
            /// 施工日期
            /// </summary>
            public string installDate { get; set; }
            /// <summary>
            /// 施工時間
            /// </summary>
            public string installTime { get; set; }
            /// <summary>
            /// 檔案名稱
            /// </summary>
            public string File_name { get; set; }
            /// <summary>
            /// 檔案路徑
            /// </summary>
            public string File_path { get; set; }

        }

        /// <summary>
        /// 儲存上傳檔案，
        /// 檔名格式為yyyyMMddxx_原檔名，xx為流水號
        /// </summary>
        /// <param name="upfile">HttpPostedFile 物件</param>
        /// <param name="type">上傳檔案類別</param>
        /// <returns>儲存檔名</returns>
        static private fileDtail SaveUploadFile(HttpPostedFileBase upfile, int cnc_id,string type)
        {
            string filePath =System.Web.HttpContext.Current.Server.MapPath("~/upfiles/cncFiles/" + cnc_id + "/" + type);

            //建立檔案資料夾
            string docupath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "upfiles\\cncFiles\\" +
                              cnc_id + "\\" + type + "\\";
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
        public object GetFile(int id, string type, string fileName)
        {
            var Form = _db.cnc002.Where(w => w.cnc_id == id);

            string extension = fileName.Split('.')[fileName.Split('.').Length - 1];
            extension = extension.ToLower();

            if (extension.Equals("xlsx") || extension.Equals("xls"))
            {
                return File(
                    Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                    "application/vnd.ms-excel", fileName);
            }

            if (extension.Equals("docx") || extension.Equals("doc"))
            {
                return File(
                    Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                    "application/msword", fileName);
            }

            if (extension.Equals("pptx") || extension.Equals("ppt"))
            {
                return File(
                    Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                    "application/vnd.ms-powerpoint", fileName);
            }

            if (extension.Equals("jpg") || extension.Equals("jpeg"))
            {
                return File(
                    Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                    "image/jpeg", fileName);
            }

            if (extension.Equals("png"))
            {
                return File(
                    Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                    "image/png", fileName);
            }

            return File(Server.MapPath("~/upfiles/cncFiles/" + id + "/" + type + "/" + fileName),
                "application/pdf", fileName);
        }

        // 功能：刪除檔案(更新欄位-隱藏檔案)
        public ActionResult RemoveUploadFile(int id, string type, string fileName)
        {
            //var cnc002Form = _db.cnc002.Where(w => w.cnc_id == id && w.File_name == fileName).Single();
            //cnc002Form.file_status = 0;

            //SaveUpdate(_db.cnc002, cnc002Form);

            //return RedirectToAction("Edit", new { id = id });
            try
            {
                var cnc002Form = _db.cnc002.Where(w => w.cnc_id == id && w.File_name == fileName).Single();
                cnc002Form.file_status = "00";

                SaveUpdate(_db.cnc002, cnc002Form);

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
