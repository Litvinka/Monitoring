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
using System.Net.Mail;

namespace Monitoring.Controllers
{
    public class messagesController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: messages
        [Authorize]
        [Log(16)]
        public ActionResult Index(int? type)
        {
            int user = Convert.ToInt32(Session["user_id"]);
            List<message> message = db.message.OrderByDescending(p=>p.date_create).ToList();
            if(type==null || type == 1)
            {
                message = message.Where(p => p.recipient_id == user).ToList();
            }
            else if (type == 2)
            {
                message = message.Where(p => p.author_id == user).ToList();
            }
            return View(message);
        }

        // GET: messages/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            message message = await db.message.FindAsync(id);
            if (message == null)
            {
                return HttpNotFound();
            }
            if (message.state_id == 1 && message.recipient_id==Convert.ToInt32(Session["user_id"]))
            {
                message.state_id = 2;
                db.message.AddOrUpdate(message);
                db.SaveChanges();
                int user_id = Convert.ToInt32(Session["user_id"]);
                User user = db.User.Find(user_id);
                Session["message"] = user.countMessage();
            }
            return View(message);
        }



       


    }
}
