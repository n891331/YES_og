using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using static System.Collections.Specialized.BitVector32;
using System.Web.Mvc;
using System.Web.Security;
using YES_og.Models;

namespace YES_og.Controllers
{
    public class _BaseController:Controller
    {
        DBePowerDataContext _db = new DBePowerDataContext();

        #region 搜尋暫存
        /// <summary>
        /// 取得正確的頁面
        /// </summary>
        /// <param name="page">若-1時需要還原page</param>
        /// <param name="fc">搜尋時時page=0(第一頁)</param>
        /// <param name="customKey">自訂key</param>
        /// <returns></returns>
        public int getCurrentPage(int? page, FormCollection fc, string customKey = null)
        {
            if (fc != null && fc.Count > 0)
            {
                return 0;
            }

            string Page = string.Format("{0}Page", customKey);

            if (page.HasValue && page == -1)
            {

                if (ViewData[Page] != null && !string.IsNullOrEmpty(ViewData[Page].ToString()))
                {
                    int pageVD = 0;
                    int.TryParse(ViewData[Page].ToString(), out pageVD);
                    page = pageVD;
                }
            }

            return page.HasValue && page != -1 ? page.Value - 1 : 0;
        }

        /// <summary>
        /// 取得CacheKey
        /// </summary>
        /// <returns></returns>
        private string getCacheKey()
        {
            //string cacheKey = string.Format("{0}_{1}_{2}", HttpContext.Session.SessionID, this.GetType().Name, ControllerContext.RouteData.Values["action"]);
            string cacheKey = string.Format("{0}_{1}_{2}", "SessionID", this.GetType().Name, ControllerContext.RouteData.Values["action"]);
            return cacheKey;
        }

        /// <summary>
        /// CacheKey+自訂key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string getCacheKey(string key)
        {
            //string cacheKey = string.Format("{0}_{1}_{2}", HttpContext.Session.SessionID, this.GetType().Name, ControllerContext.RouteData.Values["action"]);
            string cacheKey = string.Format("{0}_{1}_{2}", "SessionID", this.GetType().Name, ControllerContext.RouteData.Values["action"]);
            return string.Format("{0}_{1}", cacheKey, key);
        }

        /// <summary>
        /// 記住搜尋條件
        /// </summary>
        /// <param name="page"></param>
        /// <param name="fc"></param>
        /// <param name="customKey">自訂key</param>
        protected void GetCatcheRoutes(int? page, FormCollection fc, string customKey = null)
        {
            string SearchBy = string.Format("{0}SearchBy", customKey);
            string Page = string.Format("{0}Page", customKey);

            if (fc != null && fc.Count > 0)
            {
                //搜尋時
                foreach (var key in fc.AllKeys)
                {
                    if (key.StartsWith(SearchBy))
                    {
                        string cacheKey = getCacheKey(key);
                        //HttpContext.Cache[cacheKey] = fc[key];
                        Session[cacheKey] = fc[key];
                        ViewData[key] = fc[key];
                    }
                }


                string pageKey = getCacheKey(Page);
                //HttpContext.Cache[pageKey] = 1;
                Session[pageKey] = 1;
                ViewData[Page] = 1;
            }
            else
            {
                setPage(Page, page);          //設定page

                string cacheKey = getCacheKey(SearchBy);
                string pageKey = getCacheKey(Page);
                //var keysToSearch = (from System.Collections.DictionaryEntry dict in HttpContext.Cache
                //                    let key = dict.Key.ToString()
                //                    where key.StartsWith(cacheKey) || key.Equals(pageKey)
                //                    select key).ToList();

                foreach (string key in Session.Keys)
                {
                    if (key.StartsWith(cacheKey) || key.Equals(pageKey))
                    {
                        string oKey = getCacheKey() + "_";
                        string vkey = key.Replace(oKey, "");
                        //ViewData[vkey] = HttpContext.Cache[key];
                        ViewData[vkey] = Session[key];
                    }

                }

            }
        }

        /// <summary>
        /// 設定page
        /// </summary>
        /// <param name="pageKeyName"></param>
        /// <param name="page"></param>
        private void setPage(string pageKeyName, int? page)
        {
            if (page.HasValue && page != -1)
            {
                string pageKey = getCacheKey(pageKeyName);
                //HttpContext.Cache[pageKey] = page;
                Session[pageKey] = page;
                ViewData[pageKeyName] = page;
            }
        }
        #endregion

        public static String GetCookie()
        {
            if (System.Web.HttpContext.Current.Request.Cookies["JSESSIONID"] == null)
            {
                return null;
            }
            else
            {
                return System.Web.HttpContext.Current.Request.Cookies["JSESSIONID"].Value;
            }
        }


        #region ViewData

        /// <summary>
        /// 取得ViewData，回傳時為int
        /// </summary>
        /// <param name="viewDataKey"></param>
        /// <returns></returns>
        public int getViewDateInt(string viewDataKey)
        {
            int id = 0;
            int.TryParse(ViewData[viewDataKey].ToString(), out id);
            return id;
        }

        /// <summary>
        /// 取得ViewData，回傳時為String
        /// </summary>
        /// <param name="viewDataKey"></param>
        /// <returns></returns>
        public string getViewDateStr(string viewDataKey)
        {
            string title = ViewData[viewDataKey].ToString();
            return title;
        }

        /// <summary>
        /// 取得ViewData，回傳時為DateTime
        /// </summary>
        /// <param name="viewDataKey"></param>
        /// <returns></returns>
        public DateTime getViewDateDateTime(string viewDataKey)
        {
            string title = ViewData[viewDataKey].ToString();

            return DateTime.Parse(title);
        }

        /// <summary>
        /// 是否有ViewData
        /// </summary>
        /// <param name="viewDataKey"></param>
        /// <returns></returns>
        public bool hasViewData(string viewDataKey)
        {
            if (ViewData[viewDataKey] != null && !string.IsNullOrEmpty(ViewData[viewDataKey].ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        #region string id to string[]

        /// <summary>
        /// 將 1,2,3,4 轉成 字串陣列
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string[] StringToIDs(FormCollection fc, string Key)
        {
            string sID = fc[Key] == null ? "" : fc[Key];
            string[] IDs = sID.Split(',');
            return IDs;
        }

        #endregion

        //分頁程式
        public IEnumerable<TSource> getPageData<TSource>(int currentPageIndex, int DefaultPageSize, IQueryable<TSource> sources)
        {
            if (currentPageIndex < 0) currentPageIndex = 0;
            ViewBag.nowPage = currentPageIndex + 1;
            int TotalCount = sources.Count();//查詢後的總筆數
            ViewBag.TotalCount = TotalCount;
            int TotalPage = TotalCount / DefaultPageSize;
            if (TotalCount % DefaultPageSize != 0) TotalPage++;//總頁數
            if (currentPageIndex > TotalPage) currentPageIndex = TotalPage - 1;
            ViewBag.TotalPage = TotalPage;
            if (currentPageIndex > 0) currentPageIndex *= DefaultPageSize; //分頁起始筆數 
            IEnumerable<TSource> sourcesList = sources.Skip(currentPageIndex).Take(DefaultPageSize).ToList();//分頁
            return sourcesList;
        }

        //NullToEmpty => 是否將Null補空白,預設否
        public void SaveCreate<T>(System.Data.Linq.Table<T> table, T element, bool NullToEmpty = false, Table<T> newTable = null) where T : class
        {
            TimeZoneInfo TPZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            DateTime NOWTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TPZone);

            CommonInfo infoItem = new CommonInfo();
            infoItem.InitDate = NOWTime;
            infoItem.UpdateDate = NOWTime;

            string tmpPoster = string.Empty;
            string tmpUpdateID = string.Empty;
            if (string.IsNullOrEmpty(System.Web.HttpContext.Current.User.Identity.Name))
            {
                tmpPoster = "system";
                tmpUpdateID = "system";
            }
            else
            {
                tmpPoster = System.Web.HttpContext.Current.User.Identity.Name;
                tmpUpdateID = System.Web.HttpContext.Current.User.Identity.Name;
            }
            if (string.IsNullOrEmpty(infoItem.Poster))
            {
                infoItem.Poster = tmpPoster;
            }
            if (string.IsNullOrEmpty(infoItem.UpdateID))
            {
                infoItem.UpdateID = tmpUpdateID;
            }

            //set value
            if (NullToEmpty) { AddEmptyString(element); }
            element.GetType().GetProperty("crt_user").SetValue(element, infoItem.Poster);
            element.GetType().GetProperty("crt_date").SetValue(element, infoItem.InitDate);
            element.GetType().GetProperty("udp_user").SetValue(element, infoItem.UpdateID);
            element.GetType().GetProperty("upd_date").SetValue(element, infoItem.UpdateDate);
            if(newTable != null)
            {
                table = newTable;
            }
            table.InsertOnSubmit(element);
            try
            {
                table.Context.SubmitChanges();
            }
            catch (System.Data.Linq.ChangeConflictException ex)
            {
                table.Context.ChangeConflicts.ResolveAll(RefreshMode.KeepCurrentValues);  //保持当前的值
                table.Context.ChangeConflicts.ResolveAll(RefreshMode.OverwriteCurrentValues);//保持原来的更新,放弃了当前的值.
                table.Context.ChangeConflicts.ResolveAll(RefreshMode.KeepChanges);//保存原来的值 有冲突的话保存当前版本

                // 注意：解决完冲突后还得 SubmitChanges() 一次，不然一样是没有更新到数据库的
                table.Context.SubmitChanges();
            }
        }

        //NullToEmpty => 是否將Null補空白,預設否
        public void SaveUpdate<T>(System.Data.Linq.Table<T> table, T element, bool NullToEmpty = false) where T : class
        {
            TimeZoneInfo TPZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            DateTime NOWTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TPZone);

            CommonInfo infoItem = new CommonInfo();
            infoItem.UpdateDate = NOWTime;
            infoItem.UpdateID = string.Empty;
            if (string.IsNullOrEmpty(System.Web.HttpContext.Current.User.Identity.Name))
            {
                infoItem.UpdateID = "system";
            }
            else
            {
                infoItem.UpdateID = System.Web.HttpContext.Current.User.Identity.Name;
            }

            //set value
            if (NullToEmpty) { AddEmptyString(element); }
            element.GetType().GetProperty("udp_user").SetValue(element, infoItem.UpdateID);
            element.GetType().GetProperty("upd_date").SetValue(element, infoItem.UpdateDate);

            using (DBePowerDataContext db = new DBePowerDataContext())
            {
                var newTable = db.GetTable<T>();
                newTable.Attach(element);
                db.Refresh(RefreshMode.KeepCurrentValues, element);
                db.SubmitChanges();
            }
        }

        public void SaveDelete<T>(System.Data.Linq.Table<T> table, T element) where T : class
        {
            table.DeleteOnSubmit(element);
            table.Context.SubmitChanges();
        }

        //刪除檔案
        public void RemoveFile(string fn, string dir)
        {
            if (System.IO.Directory.Exists(Server.MapPath(dir)))
            {
                if (System.IO.File.Exists(Server.MapPath(dir + "/" + fn)))
                {
                    FileInfo fi = new FileInfo(fn);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(Server.MapPath(dir + "/" + fn));
                }
            }
        }

        //刪除整個資料夾
        public void RemoveFolder(string dir)
        {
            if (System.IO.Directory.Exists(Server.MapPath(dir)))
            {
                foreach (string d in Directory.GetFileSystemEntries(Server.MapPath(dir)))
                {
                    if (System.IO.File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        System.IO.File.Delete(d);
                    }
                    else
                        RemoveFolder(d);
                }
                Directory.Delete(Server.MapPath(dir));
            }
        }

        //將NULL欄位補上空字串
        static public void AddEmptyString<T>(T data) where T : class
        {
            var strProps = data.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));
            foreach (var prop in strProps)
            {
                var plain = (string)Convert.ChangeType(prop.GetValue(data), typeof(string));
                var EmptyString = "";
                if (plain == null)
                {
                    prop.SetValue(data, EmptyString);
                }
            }
        }

        ///// <summary>
        /////包裝UserData 成為Json For Member
        ///// </summary>
        //static public string JsonForUserData(Members userData)
        //{

        //    //data 內涵資料數量(不可以太多)
        //    Members data = new Members();

        //    data.Id = userData.Id;
        //    data.Email = userData.Email;
        //    //data.CName = userData.CName;
        //    //data.EName = userData.EName;
        //    //data.UpdateDate = DateTime.Now;
        //    return JsonConvert.SerializeObject(data);
        //}

        /// <summary>
        ///包裝UserData 成為Json For Member
        /// </summary>
        //static public string JsonForUserData(Members userData)
        //{

        //    //data 內涵資料數量(不可以太多)
        //    tempUser data = new tempUser();

        //    data.Id = userData.Id;
        //    data.Email = userData.Email;
        //    data.CName = userData.CName;
        //    data.UserType = userData.UserType;

        //    return JsonConvert.SerializeObject(data);
        //}

        /// <summary>
        ///包裝UserData 成為Json For References
        /// </summary>
        //static public string JsonForUserData(FrontReferences userData)
        //{

        //    //data 內涵資料數量(不可以太多)
        //    tempUser data = new tempUser();

        //    data.Id = userData.Id;
        //    data.Email = userData.Email;
        //    data.CName = userData.CName;
        //    data.UserType = userData.UserType;

        //    return JsonConvert.SerializeObject(data);
        //}

        /// <summary>
        ///包裝UserData 成為Json For CorpInfo
        /// </summary>
        //static public string JsonForUserData(CorpInfo userData)
        //{

        //    //data 內涵資料數量(不可以太多)
        //    tempUser data = new tempUser();

        //    data.Id = userData.Id;
        //    data.CorpId = userData.CorpId;
        //    data.CName = userData.CorpName;
        //    data.UserType = userData.UserType;
        //    return JsonConvert.SerializeObject(data);
        //}

        /// <summary>
        ///包裝UserData 成為Json For Employee
        /// </summary>
        //static public string JsonForUserData(Employees userData)
        //{

        //    //data 內涵資料數量(不可以太多)
        //    tempUser data = new tempUser();

        //    data.Id = userData.Id;
        //    data.Email = userData.Email;
        //    data.CName = userData.Name;
        //    data.UserType = userData.UserType;
        //    return JsonConvert.SerializeObject(data);
        //}

        #region "將使用者資料寫入cookie,產生AuthenTicket"
        /// <summary>
        /// 將使用者資料寫入cookie,產生AuthenTicket
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="userId">UserAccount</param>
        static public void SetAuthenTicket(string userData, string account)
        {
            TimeZoneInfo TPZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");
            DateTime NOWTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TPZone);

            //宣告一個驗證票
            //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, account, NOWTime, NOWTime.AddHours(3), false, userData);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(2,
                account,
                NOWTime,
                NOWTime.AddMonths(1),
                false,
                userData,
                FormsAuthentication.FormsCookiePath);

            //加密驗證票
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            //建立Cookie
            HttpCookie authenticationcookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            //將Cookie寫入回應

            System.Web.HttpContext.Current.Response.Cookies.Add(authenticationcookie);

        }
        #endregion

        #region "取得登入者詳細資料 Members"
        /// <summary>
        /// 取得登入者詳細資料 iEC_User
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="userId">UserAccount</param>
        //static public Members getUserData()
        //{

        //    DBePowerDataContext _db = new DBePowerDataContext();

        //    //是否有登入
        //    if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        //    {

        //        return null;

        //    }
        //    FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
        //    FormsAuthenticationTicket ticket = id.Ticket;

        //    //資料會少 先從DB丟
        //    var json = ticket.UserData;
        //    //Edit By Carlos Tsui :取得登入者ID
        //    var tempUser = JsonConvert.DeserializeObject<tempUser>(json);
        //    Members UserData = _db.Members.Where(x => x.Email == tempUser.Email).FirstOrDefault();
        //    return UserData;
        //}
        #endregion

        #region "取得登入者詳細資料 CorpInfo"
        /// <summary>
        /// 取得登入者詳細資料 iEC_User
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="userId">UserAccount</param>
        //static public CorpInfo getUserDataC()
        //{

        //    DBePowerDataContext _db = new DBePowerDataContext();

        //    //是否有登入
        //    if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        //    {

        //        return null;

        //    }
        //    FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
        //    FormsAuthenticationTicket ticket = id.Ticket;

        //    //資料會少 先從DB丟
        //    var json = ticket.UserData;
        //    //Edit By Carlos Tsui :取得登入者ID
        //    var tempUser = JsonConvert.DeserializeObject<tempUser>(json);
        //    CorpInfo UserData = _db.CorpInfo.Where(x => x.CorpId == tempUser.CorpId).FirstOrDefault();
        //    return UserData;
        //}
        #endregion
        #region "取得登入者推薦人詳細資料 FrontReferences"
        //static public FrontReferences getUserDataR()
        //{

        //    DBePowerDataContext _db = new DBePowerDataContext();

        //    //是否有登入
        //    if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        //    {

        //        return null;

        //    }
        //    FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
        //    FormsAuthenticationTicket ticket = id.Ticket;

        //    //資料會少 先從DB丟
        //    var json = ticket.UserData;
        //    //Edit By Carlos Tsui :取得登入者ID
        //    var tempUser = JsonConvert.DeserializeObject<tempUser>(json);
        //    FrontReferences UserData = _db.FrontReferences.Where(x => x.Email == tempUser.Email).FirstOrDefault();
        //    return UserData;
        //}
        #endregion

        #region "取得登入者詳細資料 Employees"
        /// <summary>
        /// 取得登入者詳細資料 iEC_User
        /// </summary>
        /// <param name="userData">使用者資料</param>
        /// <param name="userId">UserAccount</param>
        //static public Employees getUserDataE()
        //{

        //    DBePowerDataContext _db = new DBePowerDataContext();

        //    //是否有登入
        //    if (!System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
        //    {

        //        return null;

        //    }
        //    FormsIdentity id = (FormsIdentity)System.Web.HttpContext.Current.User.Identity;
        //    FormsAuthenticationTicket ticket = id.Ticket;

        //    //資料會少 先從DB丟
        //    var json = ticket.UserData;
        //    //Edit By Carlos Tsui :取得登入者ID
        //    var tempUser = JsonConvert.DeserializeObject<tempUser>(json);
        //    Employees UserData = _db.Employees.Where(x => x.Email == tempUser.Email).FirstOrDefault();
        //    return UserData;
        //}

        #endregion
        #region "將NULL 欄位補上空字串"
        /// <summary>
        /// 將NULL 欄位補上空字串
        /// </summary>
        static public void AddEmptyString(BackendBase data)
        {
            var strProps = data.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));
            foreach (var prop in strProps)
            {
                var plain = (string)Convert.ChangeType(prop.GetValue(data), typeof(string));
                var EmptyString = "";
                if (plain == null)
                {
                    prop.SetValue(data, EmptyString);
                }
            }
        }
        #endregion

        //時間顯示格式
        public static string setDateFormat(DateTime? date, bool b = false)
        {
            if (date == null) return "";
            if (b) return Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm");
            return Convert.ToDateTime(date).ToString("yyyy/MM/dd");
        }

        public class tempUser
        {
            public int Id { get; set; }
            public int UserID { get; set; }
            public string Email { get; set; }
            public string CName { get; set; }
            public int? CorpId { get; set; }
            public int UserType { get; set; }
        }



        public class CommonInfo
        {
            public string Poster { get; set; }
            public DateTime? InitDate { get; set; }
            public string UpdateID { get; set; }
            public DateTime? UpdateDate { get; set; }
        }
    }
}