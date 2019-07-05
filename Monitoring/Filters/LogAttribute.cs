using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Monitoring.Models;

namespace Monitoring.Filters
{
    public class LogAttribute : System.Web.Mvc.ActionFilterAttribute
    {

        int _kind_event;
        public LogAttribute(int kind_event)
        {
            _kind_event = kind_event;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            MonitoringEntities db = new MonitoringEntities();
            var request = filterContext.HttpContext.Request;
            int _id=(db.log_event.Count()>0) ? (db.log_event.Max(p => p.id) + 1) : 1;
            log_event log_event = new log_event()
            {
                id = _id,
                title = request.RawUrl,
                date = DateTime.Now,
                kind_event_id = _kind_event,
                IP_adress_object = request.ServerVariables["HTTP_X_FORWARED_FOR"] ?? request.UserHostAddress,
                description = request.RequestType
                //login = filterContext.HttpContext.User.Identity.Name.ToString(),
            };
            if (Convert.ToInt32(filterContext.HttpContext.Session["user_id"]) != 0)
            {
                log_event.author_id = Convert.ToInt32(filterContext.HttpContext.Session["user_id"]);
            }
            db.log_event.Add(log_event);
            db.SaveChanges();

            base.OnActionExecuting(filterContext);
        }
    }
}