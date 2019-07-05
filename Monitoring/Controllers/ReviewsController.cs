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
    public class ReviewsController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: Reviews
        [Log(7)]
        [Authorize(Roles = "Модератор")]
        public ActionResult Index(int? type,int page=1)
        {
            List<reviews_user> reviews_user = db.reviews_user.Include(r => r.audit_object).Include(r => r.state_reviews).Include(r => r.type_reviews).ToList();
            if (type == null || type != 2)
            {
                reviews_user = reviews_user.Where(p => p.state_id == 1).ToList();
            }
            else if (type != null && type==2)
            {
                reviews_user = reviews_user.Where(p => p.state_id == 2).ToList();
            }
            int pageSize = 5; // количество объектов на страницу
            IEnumerable<reviews_user> phonesPerPages = reviews_user.Skip((page - 1) * pageSize).Take(pageSize);
            PageInfo pageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = reviews_user.Count };
            IndexViewModel ivm = new IndexViewModel { PageInfo = pageInfo, reviews = phonesPerPages };
            return View(ivm);
        }

        // GET: Reviews/Details/5
        [Log(19)]
        [Authorize(Roles = "Модератор")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            reviews_user reviews_user = await db.reviews_user.FindAsync(id);
            if (reviews_user == null)
            {
                return HttpNotFound();
            }
            return View(reviews_user);
        }


        [Log(17)]
        [Authorize(Roles = "Модератор")]
        public ActionResult Publish(int? id)
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
            reviews_user.state_id = 2;
            db.reviews_user.AddOrUpdate(reviews_user);
            db.SaveChanges();
            return Redirect(Url.Action("Index","Reviews",new {type=2}));
        }

        [Log(18)]
        [HttpPost]
        [Authorize(Roles = "Модератор")]
        public JsonResult Delete(int? id)
        {
            if (id != null)
            {
                reviews_user reviews_user = db.reviews_user.Find(id);
                db.reviews_user.Remove(reviews_user);
                db.SaveChanges();
            }
            return Json("");
        }

    }
}
