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
using Monitoring.Filters;
using System.Net.Mail;
using System.Data.Entity.Migrations;

namespace Monitoring.Controllers
{
    public class CuratorController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: Curator
        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult Index()
        {
            int site = Convert.ToInt32(Session["curator_site_id"]);
            List<reviews_user> rewiews = db.reviews_user.OrderByDescending(p => p.date_create).Where(p=>p.state_id==2 && p.audit_object_id==site).Take(3).ToList();
            return View(rewiews);
        }


        // GET: Curator
        [Log(7)]
        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult Reviews(int page=1,string word="")
        {
            int site = Convert.ToInt32(Session["curator_site_id"]);
            List<reviews_user> reviews = db.reviews_user.OrderByDescending(p => p.date_create).Where(p => p.state_id == 2 && p.audit_object_id == site).ToList();
            if (!word.Equals(""))
            {
                reviews = reviews.Where(p => (p.date_create.ToString().Contains(word) || p.text.Contains(word) || p.author_name.Contains(word))).ToList();
            }
            if (Request["type"] != null && !Request["type"].Equals(""))
            {
                string[] a = Request["type"].Split(',');
                for(int i = 0; i < reviews.Count(); ++i)
                {
                    if (Array.IndexOf(a, reviews.ElementAt(i).type_id.ToString()) == -1)
                    {
                        reviews.RemoveAt(i);
                        --i;
                    }
                }
            }
            if (Request["answer"] != null && !Request["answer"].Equals(""))
            {
                string[] a = Request["answer"].Split(',');
                for (int i = 0; i < reviews.Count(); ++i)
                {
                    string answ = (reviews.ElementAt(i).curator_answer != null) ? "1" : "2";
                    if (Array.IndexOf(a, answ) == -1)
                    {
                        reviews.RemoveAt(i);
                        --i;
                    }
                }
            }
            int pageSize = 5; // количество объектов на страницу
            IEnumerable<reviews_user> phonesPerPages = reviews.Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = reviews.Count };
            IndexViewModel ivm = new IndexViewModel { PageInfo = pageInfo, reviews = phonesPerPages };
            return View(ivm);
        }



        // GET: Reviews/Details/5
        [Log(19)]
        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult OneReview(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            reviews_user reviews_user = db.reviews_user.Find(id);
            if (reviews_user == null)
            {
                return HttpNotFound();
            }
            return View(reviews_user);
        }


        // GET: Reviews/Details/5
        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult SendAnswer(string message, int id)
        {
            int user_id = Convert.ToInt32(Session["user_id"]);
            User user = db.User.Find(user_id);

            reviews_user review = db.reviews_user.Find(id);
            
            string smtpHost = "mail.unibel.by";
            string pass = "1qaz%TGB";
            string from = "monitoring@unibel.by";
            string to = review.author_email;
            string subject = "Ответ на отзыв сайте " + review.audit_object.adress_site;
            string mess = "Здравствуйте! \n На Ваш отзыв о сайте " + review.audit_object.adress_site + " был добавлен ответ: \n\n \"" + message + "\" \n\n С уважением\n -- \n АИС \"Мониторинг\"";
            MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(new MailAddress(to));
                mail.Subject = subject;
                mail.Body = mess;
                SmtpClient client = new SmtpClient();
                client.Host = smtpHost;
                client.Port = 25;
                client.Credentials = new NetworkCredential(from.Split('@')[0], pass);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                try
                {
                    client.Send(mail);
                    mail.Dispose();
                    review.curator_answer = message;
                    db.reviews_user.AddOrUpdate(review);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new Exception("Mail.Send: " + e.Message);
                }
            return Redirect("/Curator/OneReview/"+id);
        }


        // GET: Reviews/Details/5
        public ActionResult _SiteList()
        {
            int user = Convert.ToInt32(Session["user_id"]);
            var sites = db.audit_object.Where(p=>p.education__institution.Count()>0 && p.education__institution.Where(s=>s.curators_and_controlers.FirstOrDefault(t=>t.curator_id==user)!=null).Count()>0).ToList();
            return PartialView(sites);
        }

        public ActionResult _DateMonitoring(int? site)
        {
            int s =  (site!=null) ? Convert.ToInt32(site) : Convert.ToInt32(Session["curator_site_id"]);
            var monitoring = db.monitoring.Where(p => p.technical_rating.Where(t=>t.audit_object_id==s).Count() > 0 && p.date_end<DateTime.Now).ToList();
            return PartialView(monitoring);
        }


        public ActionResult _DateMonitoring1(int? site)
        {
            int s = Convert.ToInt32(site);
            ViewBag.edu1 = db.education__institution.First(p=>p.audit_object_id==s).id;
            var monitoring = db.monitoring.Where(p => p.Rating.Where(t => t.audit_object_id == s).Count() > 0 && p.date_end < DateTime.Now).ToList();
            return PartialView(monitoring);
        }


        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult _rating(int? id)
        {
            int site = (id!=null) ? Convert.ToInt32(id) : Convert.ToInt32(Session["curator_site_id"]);
            audit_object s = db.audit_object.Find(site);
            monitoring m = db.monitoring.OrderByDescending(p => p.date_end).FirstOrDefault(p => p.date_end < DateTime.Now && p.Rating.Count(a=>a.audit_object_id==site)>0);
            List<Rating> rating = new List<Rating>();
            int place = 0;
            if (m != null)
            {
                rating = db.Rating.Where(p => p.monitoring_id == m.id).OrderByDescending(p => p.sum).ToList();
                for (int i = 0; i < rating.Count(); ++i)
                {
                    if (rating.ElementAt(i).audit_object_id == site)
                    {
                        place = i + 1;
                        break;
                    }
                }
            }
            ViewBag.place = place;
            return PartialView(s);
        }


        [Authorize(Roles = "Куратор, Контролер, Эксперт")]
        public ActionResult _t_rating(int? edu1, int? m)
        {
            if (m == null) { m = db.technical_rating.OrderByDescending(p => p.monitoring.date_start).FirstOrDefault().monitoring_id; }
            int site =Convert.ToInt32(db.education__institution.Find(edu1).audit_object_id);
            technical_rating t = new technical_rating();
            if (m != null)
            {
                t = db.technical_rating.FirstOrDefault(p => p.monitoring_id == m && p.audit_object_id == site);
            }
            return PartialView(t);
        }


        [Log(8)]
        [Authorize(Roles = "Куратор, Контролер")]
        public ActionResult ChangeSite(int site)
        {
            int user = Convert.ToInt32(Session["user_id"]);
            Session["curator_site_id"] = site;
            return Redirect("/Users/Details/"+user);
        }


        [HttpPost]
        public JsonResult Get_Place(int? site)
        {
            int user = Convert.ToInt32(Session["user_id"]);
            if (site == null) { site=Convert.ToInt32(Session["curator_site_id"]); }
            List<Rating> ratings = db.Rating.Where(p => p.audit_object_id == site).OrderByDescending(p=>p.monitoring.date_end).Take(6).OrderBy(p=>p.monitoring.date_end).ToList();
            string date = "";
            string place = "";
            int count = ratings.Count();
            for (int i=0;i<ratings.Count();++i)
            {
                date += "" + ratings.ElementAt(i).monitoring.date_end.ToShortDateString() + "";
                int m = ratings.ElementAt(i).monitoring_id;
                int sum = ratings.ElementAt(i).sum;
                int pr =  db.Rating.Count(p => p.monitoring_id == m && p.sum>sum)+1;
                place += "" + pr + "";
                if (i < (count - 1))
                {
                    date += ",";
                    place += ",";
                }
            }
            string res = "[" + date + "];[" + place + "]";
            return Json(res);
        }


        // GET: Reviews/Details/5
        public ActionResult Monitoring(int? id)
        {
            ViewBag.groups = db.Groups.ToList();
            int site = Convert.ToInt32(Session["curator_site_id"]);
            technical_rating r = new technical_rating();
            if (id == null && db.technical_rating.Where(p=>p.audit_object_id==site).Count()>0)
            {
                id = db.technical_rating.FirstOrDefault(p => p.audit_object_id == site).monitoring_id;
            }
            else if(db.technical_rating.Where(p => p.audit_object_id == site).Count() == 0)
            {
                id = 0;
                r = null;
            }
            if (id != 0)
            {
                r = db.technical_rating.FirstOrDefault(p => p.monitoring_id == id && p.audit_object_id == site);
            }
            ViewBag.monitoring = id;
            return View(r);
        }

        [HttpGet]
        public ActionResult SubordinateOrganizations()
        {
            int site = Convert.ToInt32(Session["curator_site_id"]);
            education__institution e = db.education__institution.FirstOrDefault(p => p.audit_object_id == site);
            List<education__institution> edu = db.education__institution.Where(p=>p.UNP_superior_management.Contains(e.UNP)).ToList();
            return View(edu);
        }


        // GET: Experts/Details/5
        public ActionResult _answer_end(int site_id, int group_id, int? m)
        {
            int type_organization = Convert.ToInt32(db.audit_object.Find(site_id).education__institution.First(p => p.audit_object_id == site_id).type_education_institution_id);
            int not_type_organization = (type_organization == 1) ? 1 : 2;
            if (m == null)
            {
                audit_object a = db.audit_object.Find(site_id);
                monitoring monitoring = db.monitoring.OrderByDescending(p => p.date_end).FirstOrDefault(p => p.date_end < DateTime.Now && p.Rating.Count(t => t.audit_object_id == site_id) > 0);
                if (monitoring != null)
                {
                    m = monitoring.id;
                }
            }
            List<Experts_comments> answers = db.Experts_comments.Where(p => p.site_experts.audit_object_id == site_id && p.site_experts.monitoring_id == m && p.Criteria.group_id == group_id && p.Criteria.type_organization != not_type_organization).ToList();
            List<Experts_comments> result = new List<Experts_comments>();
            int id = 1;
            List<Criteria> cr = db.Criteria.Where(p => p.group_id == group_id && p.type_organization != not_type_organization).ToList();
            foreach(Criteria c in cr)
            {
                List<Experts_comments> cr_answer = answers.Where(p => p.criteria_id == c.id).ToList();
                Experts_comments one = new Experts_comments();
                one.id= id++;
                one.criteria_id = c.id;
                int count_cr = cr_answer.Count();
                one.Criteria = c;
                int sum = 0;
                for(int i = 0; i < cr_answer.Count(); ++i)
                {
                    one.answer += cr_answer.ElementAt(i).answer;
                    one.site_experts_id = cr_answer.ElementAt(i).site_experts_id;
                    one.comment+=" " + cr_answer.ElementAt(i).comment;
                }
                if (count_cr != 0){
                    one.answer = Convert.ToInt32(Math.Round(Convert.ToDouble(one.answer) / count_cr));
                }
                else
                {
                    one.answer = 0;
                }
                result.Add(one);
            }
            return PartialView(result);
        }


    }
}