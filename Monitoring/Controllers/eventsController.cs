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

namespace Monitoring.Controllers
{
    public class eventsController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: events
        [Log(14)]
        [Authorize]
        public async Task<ActionResult> Index()
        {
            int user = Convert.ToInt32(Session["user_id"]);
            var events = db.events.Where(p=>p.recipient_id==user).OrderByDescending(s=>s.date_created);
            if (db.events.Count(p => p.state_id == 1) > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE events SET state_id=2 Where recipient_id="+ user);
            }
            Session["notification"] = db.User.Find(user).countNotification();
            return View(await events.ToListAsync());
        }

    }
}
