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

namespace Monitoring.Controllers
{
    public class HomeController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        public ActionResult Index()
        {
            ViewBag.area = new SelectList(db.area.OrderBy(p => p.name), "id", "name");
            ViewBag.district = new SelectList(db.district.OrderBy(p => p.name), "id", "name");
            ViewBag.type_edu = new SelectList(db.type_edu.OrderBy(p=>p.name), "id", "name");
            return View();
        }


        [HttpPost]
        public ActionResult Index(string name, string email, string enter_type_edu, int type, string message)
        {
            reviews_user review = new reviews_user();
            review.id=(db.reviews_user.Count()>0) ? (db.reviews_user.Max(p=>p.id+1)+1) : 1;
            review.state_id = 1;
            review.audit_object_id = Convert.ToInt32(db.education__institution.First(p => p.full_name.Contains(enter_type_edu)).audit_object_id);
            review.author_email = email;
            review.author_name = name;
            review.date_create = DateTime.Now;
            review.text = message;
            review.type_id = type;
            review.title = " ";
            db.reviews_user.Add(review);
            db.SaveChanges();
            ViewBag.area = new SelectList(db.area.OrderBy(p => p.name), "id", "name");
            ViewBag.district = new SelectList(db.district.OrderBy(p => p.name), "id", "name");
            ViewBag.type_edu = new SelectList(db.type_edu.OrderBy(p => p.name), "id", "name");
            return View();
        }


        public ActionResult Help()
        {
            return View();
        }

        //Вывод рейтингов сайтов учреждений на главной странице в зависимости от типов, к которым данные учреждения относятся
        public ActionResult _rating(int type)
        {
            monitoring m = db.monitoring.OrderByDescending(p => p.date_end).FirstOrDefault(p => p.date_end < DateTime.Now);
            List<Rating> rating = new List<Rating>();
            if (m != null)
            {
                rating = db.Rating.Where(p => p.monitoring_id == m.id).ToList();
                if (type == 1)
                {
                    rating = rating.Where(p=>(p.audit_object.education__institution.First().type_edu_id>=1 && p.audit_object.education__institution.First().type_edu_id<=4) || p.audit_object.education__institution.First().type_edu_id==6 || p.audit_object.education__institution.First().type_edu_id==7 || (p.audit_object.education__institution.First().type_edu_id>=9 && p.audit_object.education__institution.First().type_edu_id<=12)).ToList();
                }
                else if (type == 2)
                {
                    rating = rating.Where(p => p.audit_object.education__institution.First().type_edu_id == 5 || p.audit_object.education__institution.First().type_edu_id == 8).ToList();
                }
                else if (type == 3)
                {
                    rating = rating.Where(p => p.audit_object.education__institution.First().type_edu_id == 13 || p.audit_object.education__institution.First().type_edu_id == 14).ToList();
                }
                else if (type == 4)
                {
                    rating = rating.Where(p => p.audit_object.education__institution.First().type_edu_id == 15).ToList();
                }
                rating =rating.OrderByDescending(p=>p.sum).Take(10).ToList();
            }
            return PartialView(rating);
        }

    }
}