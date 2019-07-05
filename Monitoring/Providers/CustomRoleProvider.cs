using System;
using System.Linq;
using System.Web.Security;
using Monitoring.Models;
using System.Data.Entity;
using System.Web;

namespace CustomRoleProviderApp.Providers
{
    public class CustomRoleProvider : RoleProvider
    {
        public override string[] GetRolesForUser(string username)
        {
            string[] roles = new string[] { };
            using (MonitoringEntities db = new MonitoringEntities())
            {
                int role=Convert.ToInt32(HttpContext.Current.Session["role"]);
                User user = db.User.Include(u => u.Role).FirstOrDefault(u => u.email == username && u.role_id==role);
                if (user != null && user.Role != null)
                {
                    HttpContext.Current.Session["message"] = user.countMessage();
                    HttpContext.Current.Session["notification"] = user.countNotification();
                    roles = new string[] { user.Role.name };
                }
                return roles;
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (MonitoringEntities db = new MonitoringEntities())
            {
                // Получаем пользователя
                User user = db.User.Include(u => u.Role).FirstOrDefault(u => u.email == username);

                if (user != null && user.Role != null && user.Role.name == roleName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}