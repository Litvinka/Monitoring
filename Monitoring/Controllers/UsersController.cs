using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Monitoring.Models;
using System.Data.Entity.Migrations;
using Monitoring.Filters;
using System.Web.Security;
using System.Net.Mail;
using System.IO;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Monitoring.Controllers
{
    public class UsersController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // Список всех пользователей
        [Log(2)]
        [Authorize(Roles = "Администратор")]
        public async Task<ActionResult> Index()
        {
            var user = db.User.Include(u => u.Role).Include(u => u.users_state).Where(p=>p.users_state.number>0);
            return View(await user.ToListAsync());
        }

        public ActionResult Login()
        {
            if (Session["user"]!=null)
            {
                return Redirect("/Home/Index/");
            }
            ViewBag.error = "";
            return View();
        }

        [Log(1)]
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            int user_count = 0;
            email = email.Trim();
            int user_id = 0;
            List<User> list_u = db.User.Where(p => p.email.Contains(email)).ToList();
            foreach (var u in list_u)
            {
                if(Models.User.VerifyHashedPassword(u.password, password)){
                    user_count++;
                    user_id = u.id;
                }
            }
            if (user_count > 1 && Request.Form["role"] == null)
            {
                ViewBag.email = email;
                ViewBag.password = password;
                return Redirect("/Users/Login?step=2");
            }
            else if (user_count == 0)
            {
                ViewBag.error = "Email или пароль неверный!";
                return View();
            }
            User user = new User();
            if (Request.Form["role"] == null)
            {
                user= db.User.Find(user_id);
            }
            else
            {
                int role = Convert.ToInt32(Request.Form["role"]);
                user = db.User.FirstOrDefault(p => p.email.Contains(email) && p.role_id==role);
            }
            if (!Models.User.VerifyHashedPassword(user.password,password))
            {
                user = null;
            }
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.email, true);
                Session["user"] = user.Name + " " + user.Surname;
                Session["user_id"] = user.id;
                Session["role"] = user.role_id;
                Session["rolename"] = user.Role.name;
                Session["photo"] = user.Photo;
                Session["notification"] = user.countNotification();
                Session["message"] = user.countMessage();
                if (user.role_id == 1)
                {
                    return Redirect("/Users/Create");
                }
                if (user.role_id == 2)
                {
                    Session["curator_site_id"] = db.curators_and_controlers.FirstOrDefault(p => p.curator_id == user.id).education__institution.audit_object_id;
                    Session["edu_id"] = db.curators_and_controlers.FirstOrDefault(p => p.curator_id == user.id).education__institution.id;
                    return Redirect("/Curator/Index");
                }
                if (user.role_id == 3)
                {
                    Session["curator_site_id"] = db.curators_and_controlers.FirstOrDefault(p=>p.curator_id==user.id).education__institution.audit_object_id;
                    Session["edu_id"] = db.curators_and_controlers.FirstOrDefault(p => p.curator_id == user.id).education__institution.id;
                    return Redirect("/Curator/Index");
                }
                if (user.role_id == 4)
                {
                    return Redirect("/Experts/Index");
                }
                if (user.role_id == 5)
                {
                    return Redirect("/Reviews/Index");
                }
            }
            ViewBag.error = "Email или пароль неверный!";
            return View();
        }


        [HttpPost]
        [Authorize]
        [Log(9)]
        public JsonResult SendMessage(string text_message, string title_message, int recipient, int author)
        {
            message e= new message();
            e.id = (db.message.Count() > 0) ? (db.message.Max(p=>p.id)+1) : 1;
            e.recipient_id = recipient;
            e.author_id = author;
            e.date_create = DateTime.Now;
            e.state_id = 1;
            e.text = text_message;
            e.title = title_message;
            db.message.Add(e);
            db.SaveChanges();
            return Json("");
        }

        //Поиск учреждений по области, району и типу
        [HttpPost]
        public JsonResult GetNameEdu(int? area, int? district, int? type)
        {
            List<education__institution> all = db.education__institution.ToList();
            if (area != null)
            {
                all = all.Where(p => p.district.area_id == area).ToList();
            }
            if (district != null)
            {
                all = all.Where(p => p.district_id == district).ToList();
            }
            if (type != null)
            {
                all = all.Where(p => p.type_edu_id==type).ToList();
            }
            List<String> list = new List<String>();
            foreach (education__institution p in all)
            {
                list.Add(Convert.ToString(p.full_name));
            }
            var d = System.Web.Helpers.Json.Encode(list);
            return Json(d);
        }

        // Поиск учреждения по полному названию
        [HttpPost]
        public JsonResult FindEdu(string name)
        {
            List<education__institution> e = db.education__institution.Where(p=>p.full_name.Contains(name)).ToList();
            string d = "0";
            foreach(var item in e)
            {
                if (item.full_name.CompareTo(name) == 0)
                {
                    d = System.Web.Helpers.Json.Encode(item.id);
                }
            }
            return Json(d);
        }


        [Authorize]
        [Log(10)]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["user"] = null;
            Session["user_id"] = null;
            Session["role"] = null;
            Session["rolename"] = null;
            Session["photo"] = null;
            Session["notification"] = null;
            Session["message"] = null;
            return Redirect("/Users/Login");
        }


        // GET: Users/Details/5
        [Authorize(Roles = "Администратор")]
        public ActionResult Distribution()
        {
            ViewBag.count_expert = db.User.Count(p => p.role_id == 4 && p.users_state.number > 0);
            ViewBag.count_site = db.audit_object.Count();
            return View();
        }


        [Authorize(Roles = "Администратор")]
        [HttpPost]
        [Log(11)]
        public ActionResult StartMonitoring()
        {
            //List<technical_rating> t = db.technical_rating.OrderByDescending(p => p.date).ToList();
            //List<experts_ratinng> e = db.experts_ratinng.OrderByDescending(p => p.id).ToList();
            //foreach (var item in e)
            //{
            //    int a = item.audit_object_id;
            //    int t1 = 0, e1 = item.sum;
            //    if (t.FirstOrDefault(p => p.audit_object_id == a) != null)
            //    {
            //        t1 = Convert.ToInt32(t.FirstOrDefault(p => p.audit_object_id == a).sum);
            //    }
            //    Rating r = new Rating();
            //    r.id = (db.Rating.Count() > 0) ? (db.Rating.Max(p => p.id) + 1) : 1;
            //    r.audit_object_id = a;
            //    r.monitoring_id = 1;
            //    r.sum = e1 + t1;
            //    db.Rating.Add(r);
            //    db.SaveChanges();
            //}

            monitoring monitoring = new monitoring(); //начался новый Мониторинг
            monitoring.id = (db.monitoring.Count() > 0) ? (db.monitoring.Max(p => p.id) + 1) : 1;
            monitoring.date_start = DateTime.Now;
            System.TimeSpan duration = new System.TimeSpan(30, 0, 0, 0);
            monitoring.date_end = monitoring.date_start.Add(duration);
            monitoring.number_experts_one_site = 0;
            db.monitoring.Add(monitoring);

            List<experts> user_list = db.experts.Where(p => p.User.state_id == 1).ToList(); //список экспертов
            List<education__institution> edu = db.education__institution.Where(p => p.state_id == 1 && p.audit_object != null).ToList();
            List<audit_object> a = db.audit_object.Where(p => !p.adress_site.Equals(" ")).ToList(); //список УО для проверки


            List<site_experts> site_experts = new List<site_experts>(); //список для записи новых проверок
            List<technical_rating> tech = new List<technical_rating>();
            int t_rating_count = (db.technical_rating.Count() > 0) ? (db.technical_rating.Max(p => p.id) + 1) : 1;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"files/");

            List<audit_object> errors_site = new List<audit_object>();

            for (int i = 0; i < a.Count(); ++i)//запись технического рейтинга в бд
            {
                if (!a[i].adress_site.Contains("\\") && !a[i].adress_site.Contains("/") && !a[i].adress_site.Contains("@"))
                {
                    try
                    {
                        string[] dirs = Directory.GetFiles(path, a[i].adress_site.Trim() + ".json");
                        if (dirs.Count() != 0)
                        {
                            technical_rating t = new technical_rating();
                            t.id = t_rating_count++;
                            t.monitoring_id = monitoring.id;
                            t.audit_object_id = Convert.ToInt32(a[i].id);
                            t.date = DateTime.Now;
                            using (StreamReader r = new StreamReader(dirs[0]))
                            {
                                string json = r.ReadToEnd();
                                dynamic array = JsonConvert.DeserializeObject(json);
                                t.site_accessibility = array["audits"]["time-to-first-byte"]["rawValue"];
                                t.status_code = array["audits"]["http-status-code"]["rawValue"];
                                t.using_HTTPS = array["audits"]["is-on-https"]["rawValue"];
                                t.response_time = array["audits"]["estimated-input-latency"]["rawValue"];
                                t.img_title = array["audits"]["image-alt"]["rawValue"];
                                t.has_title = array["audits"]["bypass"]["rawValue"];
                                t.full_load = array["audits"]["interactive"]["rawValue"];
                                t.robots_txt = array["audits"]["robots-txt"]["rawValue"];

                                string[] dirs2 = Directory.GetFiles(path + "/majento", a[i].adress_site + ".json");
                                if (dirs2.Count() != 0)
                                {
                                    StreamReader reader;
                                    using (reader = new StreamReader(dirs2[0]))
                                    {
                                        string js = reader.ReadToEnd();

                                        dynamic arr = System.Web.Helpers.Json.Decode(js);
                                        t.nesting_level = arr[0]["maxLevel"];
                                        t.broken_links_count = arr[0]["numberOfBrokenLinks"];
                                        t.broken_links_text = string.Join(", ", arr[0]["brokenLinks"]);
                                    }
                                }
                                int sum = 0;
                                if (t.status_code.Contains("True"))
                                {
                                    sum += 600;
                                }
                                if (t.site_accessibility < 50)
                                {
                                    sum += 400;
                                }
                                else if (t.site_accessibility < 60)
                                {
                                    sum = sum + (400 - (Convert.ToInt32(t.site_accessibility) - 49) * 40);
                                }
                                if (t.robots_txt.Contains("True"))
                                {
                                    sum += 50;
                                }
                                if (t.broken_links_count == 0)
                                {
                                    sum += 200;
                                }
                                else if (t.broken_links_count > 0 && t.broken_links_count < 20)
                                {
                                    sum = sum + (200 - (Convert.ToInt32(t.broken_links_count) * 10));
                                }
                                t.sum = sum;

                            }
                            tech.Add(t);
                        }
                    }
                    catch (Exception e)
                    {
                        errors_site.Add(a[i]);
                    }
                }
                else
                {
                    errors_site.Add(a[i]);
                }
            }
            db.technical_rating.AddRange(tech);
            db.SaveChanges();

            List<site_experts> experts = new List<site_experts>();
            int expert_id = (db.site_experts.Count() > 0) ? (db.site_experts.Max(p => p.id) + 1) : 1;
            foreach (var u in user_list)
            {
                string UNP = u.education__institution.UNP;
                List<education__institution> p_unp = edu.Where(p => p.UNP_superior_management.Contains(UNP)).ToList();
                foreach (var e in p_unp)
                {
                    site_experts site = new site_experts();
                    site.id = expert_id++;
                    site.expert_id = u.id;
                    site.monitoring_id = monitoring.id;
                    site.date = DateTime.Now;
                    site.audit_object_id = Convert.ToInt32(e.audit_object_id);
                    experts.Add(site);
                }
            }
            db.site_experts.AddRange(experts);
            db.SaveChanges();

            List<User> all_user = db.User.Where(p => p.state_id == 1).ToList();
            List<events> all_events = new List<events>();
            int event_id = (db.events.Count() > 0) ? (db.events.Max(p => p.id) + 1) : 0;
            foreach (User item in all_user)
            {
                events e = new events();
                e.state_id = 1;
                e.id = event_id;
                event_id++;
                e.text = "Мониторинг сайтов государственных органов и организаций начался.";
                e.date_created = DateTime.Now;
                e.recipient_id = item.id;
                all_events.Add(e);
            }
            db.events.AddRange(all_events);
            db.SaveChanges();
            return Redirect("/Experts/SiteExperts");
        }


        //[Authorize(Roles = "Администратор")]
        //[HttpPost]
        //[Log(11)]
        //public ActionResult Distribution(int number)
        //{
        //    monitoring monitoring = new monitoring(); //начался новый Мониторинг
        //    monitoring.id = (db.monitoring.Count() > 0) ? (db.monitoring.Max(p=>p.id)+1) : 1;
        //    monitoring.date_start = DateTime.Now;
        //    System.TimeSpan duration = new System.TimeSpan(30, 0, 0, 0);
        //    monitoring.date_end = monitoring.date_start.Add(duration);
        //    monitoring.number_experts_one_site = number;
        //    db.monitoring.Add(monitoring);

        //    ViewBag.count_expert = db.User.Count(p => p.role_id == 4 && p.users_state.number>0);
        //    ViewBag.count_site = db.education__institution.Count(p=>p.audit_object_id!=null && p.state_id==1);
        //    int limit = (ViewBag.count_site * number) / ViewBag.count_expert; //вычисление максимального числа сайтов для проверки одним экспертом
        //    int end = (ViewBag.count_site * number) % ViewBag.count_expert; //Остаток
        //    if (end > 0)
        //    {
        //        limit += 1;
        //    }
        //    int max = ViewBag.count_site * number; //количество проверок экспертами
        //    List<User> user_list = db.User.Where(p => p.role_id == 4 && p.state_id == 1).ToList(); //список экспертов
        //    List<audit_object> site_list = db.audit_object.Where(p=>p.education__institution.Where(s=>s.state_id==1).Count()>0).ToList(); //список сайтов для проверки
        //    List<site_experts> site_experts = new List<site_experts>(); //список для записи новых проверок


        //    List<technical_rating> tech = new List<technical_rating>();
        //    int t_rating_count = (db.technical_rating.Count() > 0) ? (db.technical_rating.Max(p => p.id) + 1) : 1;
        //    for(int i=0; i<site_list.Count();++i)//запись технического рейтинга в бд
        //    {
        //        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"files/");
        //        string[] dirs = Directory.GetFiles(path, "*"+ site_list[i].adress_site+"*" + ".json");
        //        if (dirs.Count() != 0)
        //        {
        //            technical_rating t = new technical_rating();
        //            t.id = t_rating_count++;
        //            t.monitoring_id = monitoring.id;
        //            t.audit_object_id = site_list[i].id;
        //            t.date = DateTime.Now;
        //            using (StreamReader r = new StreamReader(dirs[0]))
        //            {
        //                string json = r.ReadToEnd();
        //                dynamic array = JsonConvert.DeserializeObject(json);
        //                t.site_accessibility = array["audits"]["time-to-first-byte"]["rawValue"];
        //                t.status_code= array["audits"]["http-status-code"]["rawValue"];
        //                t.using_HTTPS = array["audits"]["is-on-https"]["rawValue"];
        //                t.response_time = array["audits"]["estimated-input-latency"]["rawValue"];
        //                t.img_title = array["audits"]["image-alt"]["rawValue"];
        //                t.has_title = array["audits"]["bypass"]["rawValue"];
        //                t.full_load = array["audits"]["interactive"]["rawValue"];
        //                t.robots_txt = array["audits"]["robots-txt"]["rawValue"];

        //                string[] dirs2 = Directory.GetFiles(path+ "/majento", "*" + site_list[i].adress_site + "*"+".json");
        //                if (dirs2.Count() != 0)
        //                {
        //                    StreamReader reader;
        //                    using (reader = new StreamReader(dirs2[0]))
        //                    {
        //                        string js = reader.ReadToEnd();

        //                        dynamic arr = System.Web.Helpers.Json.Decode(js);
        //                        t.nesting_level = arr[0]["maxLevel"];
        //                        t.broken_links_count = arr[0]["numberOfBrokenLinks"];
        //                        t.broken_links_text = string.Join(", ", arr[0]["brokenLinks"]);
        //                    }
        //                }

        //                    int sum = 0;
        //                if (t.status_code.Contains("true"))
        //                {
        //                    sum += 6000;
        //                }
        //                if (t.site_accessibility < 50)
        //                {
        //                    sum += 4000;
        //                }
        //                else if (t.site_accessibility < 61)
        //                {
        //                    sum = sum + (4000-(Convert.ToInt32(t.site_accessibility)-49)*400);
        //                }
        //                if (t.robots_txt.Contains("true"))
        //                {
        //                    sum += 500;
        //                }
        //                if (t.broken_links_count == 0)
        //                {
        //                    sum += 2000;
        //                }
        //                else if(t.broken_links_count>0 && t.broken_links_count < 21)
        //                {
        //                    sum = sum + (2000-(Convert.ToInt32(t.broken_links_count)*100));
        //                }
        //                t.sum = sum;
        //            }
        //            tech.Add(t);
        //        }
        //        else
        //        {
        //            site_list.RemoveAt(i);
        //            --i;
        //        }
        //    }
        //    db.technical_rating.AddRange(tech);
        //    db.SaveChanges();


        //    List<User> experts = new List<User>();
        //    experts.AddRange(user_list.ToArray()); //массив для использования в цикле

        //    Random rand = new Random(DateTime.Now.Second);
        //    int max_site_experts = (db.site_experts.Count()>0) ? db.site_experts.Max(p=>p.id) : 1; //максимальное значение идентификатора
        //    for (int i = 0; i < site_list.Count(); ++i)
        //    {
        //        for (int j = 0; j < number; ++j)
        //        {

        //            while (max > 0)
        //            {
        //                int r = rand.Next(experts.Count());
        //                if (site_experts.Count(p => p.expert_id == experts[r].id) < limit && site_experts.Count(p => p.expert_id == experts[r].id && p.audit_object_id == site_list[i].id) == 0)
        //                {
        //                    site_experts expert = new site_experts();
        //                    expert.id = max_site_experts + 1;
        //                    max_site_experts++;
        //                    expert.expert_id = experts[r].id;
        //                    expert.audit_object_id = site_list[i].id;
        //                    expert.monitoring_id = monitoring.id;
        //                    expert.date = DateTime.Now;
        //                    site_experts.Add(expert);
        //                    --max;
        //                    experts.RemoveAt(r);
        //                    if (experts.Count() == 0)
        //                    {
        //                        experts.AddRange(user_list.ToArray());
        //                    }
        //                    break;
        //                } //если у данного эксперта число сайтов для проверки не превышает максимальное и данный эксперт не проверяет данный сайт, то его добавляют в список для проверки
        //            }
        //        }
        //    }
        //    //db.Database.ExecuteSqlCommand("UPDATE site_experts SET is_active=0"); //все предыдущие проверки сайтов экспертами становятся неактивными
        //    db.site_experts.AddRange(site_experts); //добавление новых проверок
        //    db.SaveChanges();

        //    List<User> all_user = db.User.Where(p => p.state_id == 1).ToList();
        //    List<events> all_events=new List<events>();
        //    int event_id = (db.events.Count()>0) ? (db.events.Max(p=>p.id)+1) : 0;
        //    foreach(User item in all_user)
        //    {
        //        events e = new events();
        //        e.state_id = 1;
        //        e.id = event_id;
        //        event_id++;
        //        e.text = "Мониторинг сайтов государственных органов и организаций начался.";
        //        e.date_created = DateTime.Now;
        //        e.recipient_id = item.id;
        //        all_events.Add(e);
        //    }
        //    db.events.AddRange(all_events);
        //    db.SaveChanges();
        //    return Redirect("/Experts/SiteExperts");
        //}


        // GET: Users/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.User.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (db.User.Count(p => p.email.Contains(user.email)) > 1)
            {
                ViewBag.user = db.User.FirstOrDefault(p => p.email.Contains(user.email) && p.id != user.id);
            }
            return View(user);
        }


        public ActionResult ChangeProfile(int user)
        {
            User u = db.User.Find(user);
            FormsAuthentication.SetAuthCookie(u.email, true);
            Session["user"] = u.Name + " " + u.Surname;
            Session["user_id"] = u.id;
            Session["role"] = u.role_id;
            Session["rolename"] = u.Role.name;
            Session["photo"] = u.Photo;
            Session["notification"] = u.countNotification();
            Session["message"] = u.countMessage();
            if (u.role_id == 2 || u.role_id==3)
            {
                Session["curator_site_id"] = db.curators_and_controlers.FirstOrDefault(p => p.curator_id == u.id).education__institution.audit_object_id;
            }
            return Redirect("/Users/Details?id="+u.id);
        }


        // GET: Users/Create
        [Authorize(Roles = "Администратор")]
        public ActionResult Create()
        {
            ViewBag.role_id = new SelectList(db.Role, "id", "name");
            ViewBag.state_id = new SelectList(db.users_state, "id", "name");
            return View();
        }

        [Log(3)]
        [Authorize(Roles = "Администратор")]
        [HttpPost]
        public ActionResult Create(User user)
        {
            user.id = (db.User.Count() > 0) ? (db.User.Max(p=>p.id)+1) : 1;
            String pass_user = "";
            if (!Request.Form["surname"].Equals(""))
            {
                user.Surname = Request.Form["surname"];
            }
            if (!Request.Form["name"].Equals(""))
            {
                user.Name = Request.Form["name"];
            }
            if (!Request.Form["patronymic"].Equals(""))
            {
                user.Patronumic = Request.Form["patronymic"];
            }
            if (!Request.Form["email"].Equals(""))
            {
                user.email = Request.Form["email"];
            }
            if (!Request.Form["tel"].Equals(""))
            {
                user.phone = Request.Form["tel"];
            }
            if (!Request.Form["password"].Equals(""))
            {
                pass_user = Request.Form["password"];
                user.password = Models.User.HashPassword(Request.Form["password"]);
            }
            if (!Request.Form["user-role"].Equals(""))
            {
                user.role_id = Convert.ToInt32(Request.Form["user-role"]);
            }
            user.state_id = 1;
            db.User.Add(user);
            db.SaveChanges();

            string smtpHost = "mail.unibel.by";
            string pass = "1qaz%TGB";
            string from = "monitoring@unibel.by";
            string to = user.email;
            string subject = "Регистрация в Системе АИС \"Мониторинг\"";

            string message = "<table style = 'max-width:600px; width:100%; margin:0; padding:0; font-family: Times New Roman; font-size: 16px;' border = '0' cellpadding = '1' cellspacing = '5'><tr><td style = 'text-align:center; font-size:18px'><b>Уважаемый " + user.Surname+" " + user.Name + " " + user.Patronumic + "!</b></td></tr>";
            message = message + "<tr><td style = 'text-align:justify'> С целью оценки качества функционирования официальных интернет-сайтов органов государственного управления, подведомственных им учреждений образования и организаций, подчиненных Министерству образования Республики Беларусь Вам предоставлен доступ к автоматизированной информационной системе аудита информационных ресурсов учреждений образования Республики Беларусь (АИС Мониторинг).</td></tr><tr><td><b> Для авторизации в АИС Мониторинг необходимо:</b></td></tr><tr><td>&nbsp;– перейти по адресу в глобальной сети Интернет: <a target = '_blank' href = 'http://ais-monitoring.unibel.by' > http://ais-monitoring.unibel.by</a>;</td></tr><tr><td>&nbsp;– нажать в правом верхнем углу открывшегося окна режим входа в систему (кнопка «Войти»);</td></tr> ";
            message = message + "<tr><td>&nbsp;– ввести следующие данные: логин <b>" + user.email.Trim() + "</b> пароль <b>"+ pass_user + "</b> .</td></tr> <tr><td> После успешной процедуры регистрации и входа Вам доступна роль ";
            if (user.role_id == 2) { message += "Куратор."; }
            else if (user.role_id == 3) { message += "Контролер."; }
            else if (user.role_id == 4) { message += "Эксперт."; }
            message = message + "</td></tr><tr><td>В разделе «Профиль» Вы можете отредактировать данные или изменить пароль.</td></tr><tr><td>Перед работой в АИС Мониторинг ознакомьтесь с разделом «Справка», где представлено описание принципа работы.</td></tr> <tr><td>Вопросы, замечания и предложения по работе с АИС Мониторинг Вы можете присылать по адресу электронной почты: <a href = 'mailto:monitoring@unibel.by'> monitoring@unibel.by </a>.";
            if (user.role_id == 2) { message += "За дополнительной информацией обращайтесь в вышестоящие управления по образованию."; }
            message = message + "</td></tr><tr><td style = 'height:10px;'></td></tr> <tr><td> <span style='color:#f44336;'>*</span>";
            if (user.role_id == 2) { message += "Куратор – ответственное лицо от учреждений образования Республики Беларусь."; }
            else if (user.role_id == 3) { message += "Контролер – ответственное лицо от местных исполнительных и распорядительных органов в сфере образования, специалист вышестоящей организации, которому доступны результаты мониторинга сайтов подведомственных ей учреждений."; }
            else if (user.role_id == 4) { message += "Эксперт – ответственный сотрудник, отвечающий за проведение экспертиз интернет-сайтов, а также проверка и поддержание в актуальном состоянии перечня подведомственных учреждений образования."; }
            message = message + "</td></tr><tr><td style = 'height:20px;'></td> </tr> <tr><td>-- <br/>АИС Мониторинг</td> </tr></table>";

            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.From = new MailAddress(from);
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = message;
            SmtpClient client = new SmtpClient();
            client.Host = smtpHost;
            client.Port = 25;
            client.Credentials = new NetworkCredential(from.Split('@')[0], pass);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception("Mail.Send: " + e.Message);
            }

            if (user.role_id==2 || user.role_id == 3)
            {
                if (!Request.Form["name-edu"].Equals(""))
                {
                    string name_edu = Request.Form["name-edu"];
                    curators_and_controlers c = new curators_and_controlers();
                    foreach (education__institution a in db.education__institution)
                    {
                        if (a.full_name.ToString().Equals(name_edu))
                        {
                            c.education_institution_id = a.id;
                            break;
                        }
                    }
                    c.id = (db.curators_and_controlers.Count() > 0) ? (db.curators_and_controlers.Max(p => p.id) + 1) : 1;
                    c.curator_id = user.id;
                    db.curators_and_controlers.Add(c);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        //public ActionResult SendMessages()
        //{
        //    List<User> users = db.User.Where(p => p.id== 4203 || p.id== 3867).ToList();
        //    List<string> errors = new List<string>();
        //    string smtpHost = "mail.unibel.by";
        //    string pass = "1qaz%TGB";
        //    string from = "monitoring@unibel.by";
        //    string subject = "Регистрация в Системе АИС \"Мониторинг\"";

        //    int i = 0;
        //    foreach (var u in users)
        //    {
        //        string to = u.email.Trim();
        //        string pass_user = u.NewRandomPassword(10);
        //        u.password = Models.User.HashPassword(pass_user);
        //        db.User.AddOrUpdate(u);
        //        db.SaveChanges();

        //        string message = "<table style = 'max-width:600px; width:100%; margin:0; padding:0; font-family: Times New Roman; font-size: 16px;' border = '0' cellpadding = '1' cellspacing = '5'><tr><td style = 'text-align:center; font-size:18px'><b>Уважаемый " + u.Surname + " " + u.Name + " " + u.Patronumic + "!</b></td></tr>";
        //        message = message + "<tr><td style = 'text-align:justify'> С целью оценки качества функционирования официальных интернет-сайтов органов государственного управления, подведомственных им учреждений образования и организаций, подчиненных Министерству образования Республики Беларусь Вам предоставлен доступ к автоматизированной информационной системе аудита информационных ресурсов учреждений образования Республики Беларусь (АИС Мониторинг).</td></tr><tr><td><b> Для авторизации в АИС Мониторинг необходимо:</b></td></tr><tr><td>&nbsp;– перейти по адресу в глобальной сети Интернет: <a target = '_blank' href = 'http://ais-monitoring.unibel.by' > http://ais-monitoring.unibel.by</a>;</td></tr><tr><td>&nbsp;– нажать в правом верхнем углу открывшегося окна режим входа в систему (кнопка «Войти»);</td></tr> ";
        //        message = message + "<tr><td>&nbsp;– ввести следующие данные: логин <b>" + u.email.Trim() + "</b> пароль <b>" + pass_user + "</b> .</td></tr> <tr><td> После успешной процедуры регистрации и входа Вам доступна роль ";
        //        if (u.role_id == 2) { message += "Куратор."; }
        //        else if (u.role_id == 3) { message += "Контролер."; }
        //        else if (u.role_id == 4) { message += "Эксперт."; }
        //        message = message + "</td></tr><tr><td>В разделе «Профиль» Вы можете отредактировать данные или изменить пароль.</td></tr><tr><td>Перед работой в АИС Мониторинг ознакомьтесь с разделом «Справка», где представлено описание принципа работы.</td></tr> <tr><td>Вопросы, замечания и предложения по работе с АИС Мониторинг Вы можете присылать по адресу электронной почты: <a href = 'mailto:monitoring@unibel.by'> monitoring@unibel.by </a>.";
        //        if (u.role_id == 2) { message += "За дополнительной информацией обращайтесь в вышестоящие управления по образованию."; }
        //        message = message + "</td></tr><tr><td style = 'height:10px;'></td></tr> <tr><td> <span style='color:#f44336;'>*</span>";
        //        if (u.role_id == 2) { message += "Куратор – лицо, ответственное за работу сайта своей организации."; }
        //        else if (u.role_id == 3) { message += "Контролер – ответственное лицо от местных исполнительных и распорядительных органов в сфере образования, специалист вышестоящей организации, которому доступны результаты мониторинга сайтов подведомственных ей учреждений."; }
        //        else if (u.role_id == 4) { message += "Эксперт – ответственный сотрудник, отвечающий за проведение экспертиз интернет-сайтов, а также проверка и поддержание в актуальном состоянии перечня подведомственных учреждений образования."; }
        //        message = message + "</td></tr><tr><td style = 'height:20px;'></td> </tr> <tr><td>-- <br/>АИС Мониторинг</td> </tr></table>";

        //        try
        //        {
        //            MailMessage mail = new MailMessage();
        //            mail.IsBodyHtml = true;
        //            mail.From = new MailAddress(from);
        //            mail.To.Add(new MailAddress(to));
        //            mail.Subject = subject;
        //            mail.Body = message;
        //            SmtpClient client = new SmtpClient();
        //            client.Host = smtpHost;
        //            client.Port = 25;
        //            client.Credentials = new NetworkCredential(from.Split('@')[0], pass);
        //            client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //            client.Send(mail);
        //            mail.Dispose();
        //        }
        //        catch (Exception e)
        //        {
        //            errors.Add(u.email);
        //            //throw new Exception("Mail.Send: " + e.Message);
        //        }
        //        ++i;
        //    }

        //    return Redirect("/Users/Index");
        //}

        public ActionResult Address()
        {
            Microsoft.Office.Interop.Excel.Application ObjWorkExcel = new Microsoft.Office.Interop.Excel.Application(); //открыть эксель
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(@"d:\addres_site.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ObjWorkBook.Sheets[1]; //получить 1 лист
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell);//1 ячейку
            int iLastRow = ObjWorkSheet.Cells[ObjWorkSheet.Rows.Count, "A"].End[Microsoft.Office.Interop.Excel.XlDirection.xlUp].Row;  //последняя заполненная строка в столбце А
            var arrData = (object[,])ObjWorkSheet.Range["A1:B" + iLastRow].Value;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing); //закрыть не сохраняя
            ObjWorkExcel.Quit(); // выйти из экселя

            List<audit_object> a = db.audit_object.ToList();

            for (int i = 1; i <= iLastRow; ++i)
            {
                int k = Convert.ToInt32(arrData[i, 1]);
                audit_object b = a.FirstOrDefault(p => p.id == k);
                if (b != null)
                {
                    b.adress_site = arrData[i, 2].ToString();
                    db.audit_object.AddOrUpdate(b);
                    db.SaveChanges();
                }
            }
                return Redirect("/Institution/Index");
        }



        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.User.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }


        [HttpPost]
        [Log(12)]
        [Authorize]
        public ActionResult Edit(User user, HttpPostedFileBase photo)
        {
            int id = Convert.ToInt32(Request.Form["user_id"]);
            user = db.User.Find(id);
            // Verify that the user selected a file
            if (photo != null && photo.ContentLength > 0)
            {
                // extract only the fielname
                var fileName = System.IO.Path.GetFileName(photo.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = System.IO.Path.Combine(Server.MapPath("~/Content/profile_photo"), fileName);
                photo.SaveAs(path);
                user.Photo = "/Content/profile_photo/" + fileName;
                Session["photo"] = user.Photo;
            }
            if (!Request.Form["surname"].Equals(""))
            {
                user.Surname = Request.Form["surname"];
            }
            if (!Request.Form["name"].Equals(""))
            {
                user.Name = Request.Form["name"];
            }
            if (!Request.Form["patronymic"].Equals(""))
            {
                user.Patronumic = Request.Form["patronymic"];
            }
            else
            {
                user.Patronumic = null;
            }
            if (!Request.Form["tel"].Equals(""))
            {
                user.phone = Request.Form["tel"];
            }
            else
            {
                user.phone = null;
            }
            if (!Request.Form["password_new"].Equals("") && !Request.Form["password"].Equals("") && Request.Form["password"].Equals(Request.Form["password_new"]) && !Request.Form["password_old"].Equals("") && Models.User.VerifyHashedPassword(user.password,Request.Form["password_old"]))
            {
                user.password = Models.User.HashPassword(Request.Form["password_new"]);
            }
            db.User.AddOrUpdate(user);
            db.SaveChanges();
            return View(user);
        }


        public ActionResult RecoveryPassword()
        {
            ViewBag.message = "";
            return View();
        }

        [Log(20)]
        [HttpPost]
        public ActionResult RecoveryPassword(string email, int? role)
        {
            int count = db.User.Where(p=>p.email==email).Count();
            User user = new User();
            if(count>1 && role == null)
            {
                List <Role> roles= db.Role.Where(p => p.User.Count(s => s.email == email)>0).ToList();
                ViewBag.email = email;
                return View(roles);
            }
            else if(count>1 && role!=null)
            {
                int r = Convert.ToInt32(role);
                user = db.User.FirstOrDefault(p => p.email == email && p.role_id==r);
            }
            else if (count == 1)
            {
                user = db.User.FirstOrDefault(p => p.email == email);
            }
            if (user==null)
            {
                ViewBag.message = "Такого email нет в базе данных!";
                return View();
            }
            else
            {
                string smtpHost = "mail.unibel.by";
                string pass = "1qaz%TGB";
                string from = "monitoring@unibel.by";
                string to = email;
                string subject = "Восстановление пароля";
                string new_pass = user.NewRandomPassword(8);
                string message = "<table style='max - width:600px; width: 100 %; margin: 0; padding: 0; font - family: Times New Roman; font - size: 16px;' border='0' cellpadding='1' cellspacing='5'><tr>";
                message += "<td>Здравствуйте, " + user.Name + " " + user.Surname + " " + user.Patronumic + "!</b></td></tr><tr><td> Ваш новый пароль для входа в cистему АИС Мониторинг:</td></tr><tr>";
                message += "<td><b>"+ new_pass+"</b></td></tr><tr><td style = 'height:20px;'></td></tr><tr><td>-- <br/>АИС Мониторинг</td></tr></table> ";
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(new MailAddress(to));
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = message;
                SmtpClient client = new SmtpClient();
                client.Host = smtpHost;
                client.Port = 25;
                client.Credentials = new NetworkCredential(from.Split('@')[0], pass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                try
                {
                    client.Send(mail);
                    mail.Dispose();
                    user.password = Models.User.HashPassword(new_pass);
                    db.User.AddOrUpdate(user);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new Exception("Mail.Send: " + e.Message);
                }
            }
            return Redirect("/Users/Login");
        }


        [HttpPost]
        [Authorize(Roles = "Администратор")]
        [Log(13)]
        public JsonResult Delete(int? id)
        {
            if (id != null)
            {
                User user = db.User.Find(id);
                user.state_id = 2;
                db.User.AddOrUpdate(user);
                db.SaveChanges();
            }
            return Json("");
        }


        [HttpPost]
        public JsonResult GetAllDistrict(int param)
        {
            var district = db.district.Where(p => p.area_id==param).OrderBy(p => p.name);
            List<List<String>> list = new List<List<String>>();
            List<String> row;
            foreach (district p in district)
            {
                row = new List<String>();
                row.Add(Convert.ToString(p.id));
                row.Add(p.name);
                list.Add(row);
            }
            var d = System.Web.Helpers.Json.Encode(list);
            return Json(d);
        }
       
        [HttpPost]
        public JsonResult GetJsonEdu()
        {
            var sites = db.audit_object.ToList();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"sites.json");
            List<List<String>> list = new List<List<String>>();
            string d = "[";
            for(int i=0; i<sites.Count();++i)
            {
                //row = new List<String>();
                //row.Add(Convert.ToString(p.id));
                //row.Add(p.adress_site);
                //list.Add(row);
                d += "{\"id\":\"" + sites[i].id + "\", \"url\":\"http://" + sites[i].adress_site + "\"," + "\"name\":\"" + sites[i].education__institution.First(p=>p.audit_object_id==sites[i].id).full_name + "\"}";
                if (i < sites.Count() - 1)
                {
                    d += ",";
                }
                
            }
            d += "]";
            //string d = System.Web.Helpers.Json.Encode(list).ToString();
            using (FileStream fstream = new FileStream(path, FileMode.OpenOrCreate))
            {
                byte[] array = System.Text.Encoding.UTF8.GetBytes(d);
                fstream.Write(array, 0, array.Length);
            }
            return Json("1");
        }

        [HttpPost]
        public JsonResult HaveUserByEmail(string email)
        {
            if (db.User.FirstOrDefault(p => p.email.Equals(email)) != null)
            {
                return Json(true);
            }
            return Json(false);
        }

        public ActionResult Results()
        {
            List<audit_object> list = db.audit_object.ToList();
            return View(list);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
