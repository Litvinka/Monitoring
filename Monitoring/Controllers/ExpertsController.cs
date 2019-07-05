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

namespace Monitoring.Controllers
{
    public class ExpertsController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: Experts
        [Authorize(Roles = "Эксперт")]
        public ActionResult Index()
        {
            int user = Convert.ToInt32(Session["user_id"]);
            experts e = db.experts.First(p => p.expert_id == user);
            var site_experts = db.site_experts.Where(p=>p.expert_id==e.id);
            return View(site_experts.ToList());
        }

        // GET: Experts/Details/5
        [Authorize(Roles = "Эксперт")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            site_experts site_experts = await db.site_experts.FindAsync(id);
            if (site_experts == null)
            {
                return HttpNotFound();
            }
            ViewBag.count = db.Experts_comments.Count(p => p.site_experts_id == id);
            ViewBag.groups = db.Groups.ToList();
            return View(site_experts);
        }


        // GET: Experts/Details/5
        [Authorize(Roles = "Администратор")]
        public ActionResult SiteExperts()
        {
            var experts = db.experts.Where(p=>p.User.users_state.number>0);
            if (db.monitoring.Count() > 0)
            {
                int m_id = db.monitoring.OrderByDescending(p => p.date_end).First().id;
                ViewBag.monitoring_id = m_id;
                ViewBag.all = db.site_experts.Where(p=>p.monitoring_id==m_id).GroupBy(p=>p.audit_object_id).Count();
                ViewBag.comment = db.site_experts.Count(p => p.monitoring_id == m_id && p.Experts_comments.Count()>0);
            }
            return View(experts);
        }

        // GET: Experts/Details/5
        [Authorize(Roles = "Администратор")]
        public ActionResult Sites(int user_id)
        {
            int monitoring = (db.monitoring.Count() > 0) ? db.monitoring.OrderByDescending(p=>p.date_end).First().id : 0;
            var sites = db.site_experts.Where(p => p.expert_id == user_id && p.monitoring_id==monitoring);
            return PartialView(sites);
        }


        // GET: Experts/Details/5
        [Authorize(Roles = "Эксперт")]
        public ActionResult _criterias(int site_experts_id, int group_id)
        {
            int type_organization = Convert.ToInt32(db.site_experts.Find(site_experts_id).audit_object.education__institution.First(p => p.audit_object_id == db.site_experts.Find(site_experts_id).audit_object.id).type_education_institution_id);
            int not_type_organization = (type_organization == 1) ? 1 : 2;
            List<Criteria> criterias = db.Criteria.Where(p => p.group_id==group_id && p.type_organization!=not_type_organization).ToList();
            return PartialView(criterias);
        }


        // GET: Experts/Details/5
        public ActionResult _answers(int site_experts_id, int group_id)
        {
            int type_organization = Convert.ToInt32(db.site_experts.Find(site_experts_id).audit_object.education__institution.First(p => p.audit_object_id == db.site_experts.Find(site_experts_id).audit_object.id).type_education_institution_id);
            int not_type_organization = (type_organization == 1) ? 1 : 2;
            List<Experts_comments> answers = db.Experts_comments.Where(p => p.site_experts_id == site_experts_id && p.Criteria.group_id == group_id && p.Criteria.type_organization!=not_type_organization).ToList();
            return PartialView(answers);
        }


        [Log(15)]
        [HttpPost]
        [Authorize(Roles = "Эксперт")]
        public ActionResult AddComment(int site_expert, int site_id)
        {
            if (db.Experts_comments.Count(p => p.site_experts.audit_object_id == site_id && p.site_experts.id==site_expert) > 0)
            {
                return RedirectToAction("Index");
            }
            int type_organization = Convert.ToInt32(db.audit_object.Find(site_id).education__institution.First(p => p.audit_object_id == site_id).type_education_institution_id);
            int not_type_organization = (type_organization == 1) ? 1 : 2;
            List<Criteria> criterias = db.Criteria.Where(p => p.type_organization != not_type_organization).ToList();
            List<Experts_comments> comments = new List<Experts_comments>();
            int id= (db.Experts_comments.Count() > 0) ? (db.Experts_comments.Max(p => p.id) + 1) : 1;
            for (int i = 0; i < criterias.Count(); ++i)
            {
                Experts_comments comment = new Experts_comments();
                comment.id = id++;
                comment.site_experts_id = site_expert;
                comment.answer = Convert.ToInt32(Request.Form["ask_"+criterias.ElementAt(i).id]);
                if (Request.Form["comment_" + criterias.ElementAt(i).id] != null) {
                    comment.comment=Request.Form["comment_" + criterias.ElementAt(i).id];
                }
                comment.criteria_id = criterias.ElementAt(i).id;
                comments.Add(comment);
            }
            db.Experts_comments.AddRange(comments);
            db.SaveChanges();
            int monitoring = Convert.ToInt32(db.site_experts.Find(site_expert).monitoring_id);
            int all_site_experts = db.site_experts.Count(p => p.audit_object_id == site_id && p.monitoring_id==monitoring);
            int all_site_experts_with_comment = db.site_experts.Count(p => p.audit_object_id == site_id && p.monitoring_id == monitoring && p.Experts_comments.Count()>0);
            if (all_site_experts == all_site_experts_with_comment)
            {
                List<Experts_comments> com = db.Experts_comments.Where(p => p.site_experts.audit_object_id == site_id && p.site_experts.monitoring_id == monitoring).ToList();
                decimal sum = 0;
                foreach(Experts_comments c in com)
                {
                    sum += (c.answer*c.Criteria.Coefficients.value*100);
                }

                experts_ratinng e_r = new experts_ratinng(); //добавление экспертного рейтинга
                e_r.id = (db.experts_ratinng.Count() > 0) ? (db.experts_ratinng.Max(p=>p.id)+1) : 1;
                e_r.monitoring_id = monitoring;
                e_r.audit_object_id = site_id;
                e_r.sum = Convert.ToInt32(Math.Round(sum));
                db.experts_ratinng.Add(e_r);
                db.SaveChanges();

                technical_rating t=db.technical_rating.FirstOrDefault(p => p.monitoring_id == monitoring && p.audit_object_id == site_id);
                Rating rat = new Rating();
                rat.id= (db.Rating.Count() > 0) ? (db.Rating.Max(p => p.id) + 1) : 1;
                rat.monitoring_id= monitoring;
                rat.audit_object_id = site_id;
                rat.sum= (t!=null) ? Convert.ToInt32(Math.Round(sum + Convert.ToDecimal(t.sum))) : Convert.ToInt32(Math.Round(sum));
                db.Rating.Add(rat);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
