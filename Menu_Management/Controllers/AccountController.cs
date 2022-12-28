using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.WebPages;
using Menu_Management.Models;
using System;
using PagedList;
using System.Web.UI.WebControls.Expressions;
using System.Data.Entity;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Restaurant_Management.Controllers
{
    #region
    public class AccountController : Controller
    {
        RestaurantManagementEntities Context = new RestaurantManagementEntities();
        // GET: Account

        #region Index
        public ActionResult Index()
        {
            return View();
        }
        #endregion


        #region Register
        public ActionResult Register()
        {
            return View();
        }
        #endregion


        #region Save User Details
        ///Method to save user details
        [HttpPost]
        public ActionResult SaveRegisterDetails(register_user registerdetails)
        {
            if (ModelState.IsValid)
            {
                var isEmailIdExists = Context.register_user.Any(x => x.EMAIL_ID == registerdetails.EMAIL_ID);
                if (isEmailIdExists)
                {
                    ModelState.AddModelError("Email", "User is already registered.");
                    ViewBag.Message = "User is already registered. Please login to your account";
                    return View("Register");
                }

                register_user reglog = new register_user
                {
                    FIRST_NAME = registerdetails.FIRST_NAME,
                    LAST_NAME = registerdetails.LAST_NAME,
                    EMAIL_ID = registerdetails.EMAIL_ID,
                    GENDER = registerdetails.GENDER,
                    PASSWORD = registerdetails.PASSWORD,
                    CONFIRM_PASSWORD = registerdetails.CONFIRM_PASSWORD,
                    PHONE = registerdetails.PHONE,

                };

                Context.register_user.Add(reglog);
                Context.SaveChanges();
                ViewBag.Message = "User Details Saved : Please login to your account";
                return View("Login");
            }
            else
            {
                return View("Register", registerdetails);
            }
        }
        #endregion


        #region Check iF User Exists
        ///Method to check if user already exists
        [HttpPost]
        public ActionResult EmailExists(string EMAIL_ID)
        {
            var user = Membership.GetUser(EMAIL_ID);
            return Json(user == null);
        }
        #endregion


        #region Login
        /// <summary>
        /// Login Method
        /// </summary>

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isValidUser = Context.register_user.Where(a => a.EMAIL_ID.Equals(model.EMAIL_ID) && (a.PASSWORD).Equals((model.PASSWORD))).FirstOrDefault();

                if (isValidUser != null)
                {
                    Session["USER_ID"] = isValidUser.USER_ID.ToString();
                    Session["EMAIL_ID"] = isValidUser.EMAIL_ID.ToString();
                    /*FormsAuthentication.SetAuthCookie(model.EMAIL_ID, false);*/
                    return RedirectToAction("Read");
                }
                else
                {
                    ModelState.AddModelError("Failure", "Wrong Username and Password!");
                    return View();
                }
            }
            else
            {
                return View(model);
            }
        }
        #endregion


        #region Check if User is Valid
        /// <summary>
        /// Method to check if user is valid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public register_user IsValidUser(LoginViewModel model)
        {
            using (var dataContext = new RestaurantManagementEntities())
            {
                register_user user = dataContext.register_user.Where(query => query.EMAIL_ID.Equals(model.EMAIL_ID) && (query.PASSWORD).Equals((model.PASSWORD))).SingleOrDefault();
                if (model == null)
                    return null;
                else
                    return user;
            }
        }
        #endregion


        #region Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
        #endregion


        #region Add Records
        public ActionResult Create()
        {
            if (!IsLoggedin())
            {
                return View("Error");
            }
            ViewBag.mealTypes = GetMealTypesList();
            return View();
        }

        // Specify the type of attribute i.e.
        // it will add the record to the database
        [HttpPost]
        ///Method to add records in DB
        public ActionResult Create(menu model)
        {
            if (!IsLoggedin())
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                menu obj = new menu();
                var currentuser = User.Identity.Name;
                obj.DISH_NAME = model.DISH_NAME;
                obj.PRICE = model.PRICE;
                obj.CREATED_BY = (string)Session["EMAIL_ID"];
                obj.CATEGORY_ID = model.CATEGORY_ID;
                obj.CREATED_TIMESTAMP = DateTime.Now;
                Context.menus.Add(obj);
                Context.SaveChanges();
            }
            ModelState.Clear();
            return RedirectToAction("Read");
        }
        #endregion


        #region Read Records
        /// <summary>
        /// Method to read data from DB
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="searchString"></param>
        /// <param name="currentFilter"></param>
        /// <param name="Page_No"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Read(string sortOrder, string searchString, string currentFilter, int? Page_No)
        {
            if (!IsLoggedin())
            {
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                ViewBag.CurrentSortOrder = sortOrder;
                ViewBag.SortingName = String.IsNullOrEmpty(sortOrder) ? "Dish_Name" : "Name_Dish";
                if (searchString != null)
                {
                    Page_No = 1;
                }
                else
                {
                    searchString = currentFilter;
                }
                ViewBag.FilterValue = searchString;
                var data = Context.menus.Include(x => x.category);
                if (!String.IsNullOrEmpty(searchString))
                {
                    data = data.Where(ml => ml.DISH_NAME.ToUpper().Contains(searchString.ToUpper()));
                }

                data = data.OrderBy(x => x.DISH_ID);
                switch (sortOrder)
                {
                    case "Dish_Name":
                        data = data.OrderBy(ml => ml.DISH_NAME);
                        break;
                    case "Name_Dish":
                        data = data.OrderByDescending(ml => ml.DISH_NAME);
                        break;
                }
                int pageSize = 3;
                int pageNumber = (Page_No ?? 1);
                //return View(data.ToList());
                return View(data.ToPagedList(pageNumber, pageSize));
            }
            return View("Error");
        }
        #endregion


        #region Delete Menu Item
        /// <summary>
        /// Metgod to delete record from database
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Delete(int Id)
        {
            var data = Context.menus.Where(x => x.DISH_ID == Id).First();
            Context.menus.Remove(data);
            Context.SaveChanges();
            var list = Context.menus.ToList();
            return RedirectToAction("Read");
        }
        [HttpPost]
        public ActionResult Edit(menu menu)
        {
            if (!IsLoggedin())
            {
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                menu item = Context.menus.Where(i => i.DISH_ID.Equals(menu.DISH_ID)).FirstOrDefault();
                item.DISH_NAME = menu.DISH_NAME;
                item.CATEGORY_ID = menu.CATEGORY_ID;
                item.PRICE = menu.PRICE;
                item.UPDATED_BY = (string)Session["EMAIL_ID"];
                item.UPDATED_TIMESTAMP = DateTime.Now;
                Context.SaveChanges();
                return RedirectToAction("Read");
            }
            return View();

        }
        #endregion


        #region Edit Menu Item
        /// <summary>
        /// Method to 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(int id)
        {
            if (!IsLoggedin())
            {
                return View("Error");
            }
            menu item = Context.menus.Where(i => i.DISH_ID.Equals(id)).FirstOrDefault();
            ViewBag.categoryid = GetMealTypesList();
            return View(item);


        }
        #endregion


        #region Search by Menu Item
        /// <summary>
        /// Method to search data from list
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult searchByName(string searchString, int? page = 1)
        {
            if (searchString == null || searchString.IsEmpty())
            {
                return RedirectToAction("Read");
            }
            ViewBag.categoryid = GetMealTypesList();
            var menu = Context.menus.Where(s => s.DISH_NAME.Contains(searchString)).ToList();
            return View("Read", menu);

        }
        #endregion


        #region Dropdown
        /// <summary>
        /// Method for creating dropdown
        /// </summary>
        /// <returns></returns>
        private SelectListItem[] GetMealTypesList()
        {
            SelectListItem[] mealTypes = null;

            var items = Context.categories.Select(a => new SelectListItem()
            {
                Text = a.CATEGORY_NAME,
                Value = a.CATEGORY_ID.ToString()
            }).ToList();

            mealTypes = items.ToArray();


            // Have to create new instances via projection
            // to avoid ModelBinding updates to affect this
            // globally
            return mealTypes
                .Select(d => new SelectListItem()
                {
                    Value = d.Value,
                    Text = d.Text
                })
             .ToArray();
        }
        #endregion


        #region Password Encryption and Decryption

        private Boolean IsLoggedin()
        {
            if (Session["EMAIL_ID"] == null)
            {
                return false;
            }
            return true;
        }
        public string EncryptPassword(string Message)
        {
            string EncriptionKey = "MAKVKKKBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(Message);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncriptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    Message = Convert.ToBase64String(ms.ToArray());
                }
            }
            return Message;
        }


        public static string DecryptPassword(string cipherText)
        {
            string EncryptionKey = "MAKVKKKBNI9212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
                return cipherText;
            }
        }
        #endregion
    }
    #endregion
}