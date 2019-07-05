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
    public class InstitutionController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        // GET: Institution
        [Log(4)]
        [Authorize(Roles = "Администратор")]
        public async Task<ActionResult> Index()
        {
            var education__institution = db.education__institution.Include(e => e.audit_object).Include(e => e.department_subordination).Include(e => e.district).Include(e => e.kind_edu).Include(e => e.ownership_type).Include(e => e.type_edu).Include(e => e.type_education_institution).Where(p=>p.state_id==1);
            return View(await education__institution.ToListAsync());
        }


        // GET: Institution/Details/5
        [Authorize(Roles = "Администратор, Контролер, Куратор")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            education__institution education__institution = await db.education__institution.FindAsync(id);
            if (education__institution == null)
            {
                return HttpNotFound();
            }
            if (education__institution.phone != null)
            {
                ViewBag.phone = "+375" + education__institution.phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            }
            return View(education__institution);
        }


        // GET: Institution/Create
        [Authorize(Roles = "Администратор")]
        public ActionResult Create()
        {
            ViewBag.audit_object_id = new SelectList(db.audit_object, "id", "title_site");
            ViewBag.department_subordination_id = new SelectList(db.department_subordination, "id", "name");
            ViewBag.area_id = new SelectList(db.area, "id", "name");
            ViewBag.district_id = new SelectList(db.district, "id", "name");
            ViewBag.kind_edu_id = new SelectList(db.kind_edu, "id", "name");
            ViewBag.ownership_type_id = new SelectList(db.ownership_type, "id", "name");
            ViewBag.type_edu_id = new SelectList(db.type_edu, "id", "name");
            ViewBag.type_education_institution_id = new SelectList(db.type_education_institution, "id", "nane");
            return View();
        }

        
        [HttpPost]
        [Log(5)]
        [Authorize(Roles = "Администратор")]
        public ActionResult Create(education__institution education__institution)
        {
            education__institution.id = (db.education__institution.Count() > 0) ? (db.education__institution.Max(p=>p.id)+1) : 1;
            audit_object a = new audit_object();
            a.id = (db.audit_object.Count() > 0) ? (db.audit_object.Max(p=>p.id)+1) : 1;
            a.adress_site = education__institution.audit_object.adress_site;
            a.title_site = education__institution.full_name;
            db.audit_object.Add(a);
            db.SaveChanges();
            education__institution.audit_object = null;
            education__institution.state_id = 1;
            education__institution.audit_object_id = a.id;
            db.education__institution.Add(education__institution);
            db.SaveChanges();
            return Redirect("/Institution/Index");
        }


        public ActionResult GetExcelUO()
        {
            Microsoft.Office.Interop.Excel.Application ObjWorkExcel = new Microsoft.Office.Interop.Excel.Application(); //открыть эксель
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(@"d:\novopolock_forma.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ObjWorkBook.Sheets[1]; //получить 1 лист
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell);//1 ячейку
            int iLastRow = ObjWorkSheet.Cells[ObjWorkSheet.Rows.Count, "A"].End[Microsoft.Office.Interop.Excel.XlDirection.xlUp].Row;  //последняя заполненная строка в столбце А
            var arrData = (object[,])ObjWorkSheet.Range["A1:Z" + iLastRow].Value;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing); //закрыть не сохраняя
            ObjWorkExcel.Quit(); // выйти из экселя

            int id1 = (db.audit_object.Count() > 0) ? (db.audit_object.Max(p => p.id) + 1) : 1, id2 = (db.education__institution.Count() > 0) ? (db.education__institution.Max(p => p.id) + 1) : 1, id3 = (db.User.Count() > 0) ? (db.User.Max(p => p.id) + 1) : 1, id4 = (db.curators_and_controlers.Count() > 0) ? (db.curators_and_controlers.Max(p => p.id) + 1) : 1;

            for (int i =5; i <= iLastRow; ++i)
            {
                string site= (arrData[i, 20] != null) ? arrData[i, 20].ToString() : " ";
                audit_object a = new audit_object();
                if (!site.Equals(" ")){
                    a.id = id1++;
                    a.adress_site = site;
                    a.title_site = (arrData[i, 3] != null) ? arrData[i, 3].ToString() : " ";
                    db.audit_object.Add(a);
                    db.SaveChanges();
                }
                education__institution e = new education__institution();
                e.id = id2++;
                e.full_name = (arrData[i, 2] != null) ? arrData[i, 2].ToString() : " ";
                e.short_name = (arrData[i, 3] != null) ? arrData[i, 3].ToString() : " ";
                if(!site.Equals(" "))
                {
                    e.audit_object_id = a.id;
                }
                if(arrData[i, 5]!=null){
                    e.district_id = Convert.ToInt32(arrData[i, 5]);
                }
                e.address= (arrData[i, 6] != null) ? arrData[i, 6].ToString() : " ";
                e.email= (arrData[i, 7] != null) ? arrData[i, 7].ToString() : " ";
                e.phone = (arrData[i, 8] != null) ? arrData[i, 8].ToString() : " ";
                if (arrData[i, 9] != null)
                {
                    e.type_edu_id = Convert.ToInt32(arrData[i, 9]);
                }
                if (arrData[i, 10] != null)
                {
                    e.kind_edu_id = Convert.ToInt32(arrData[i, 10]);
                }
                e.OKPO= (arrData[i, 11] != null) ? arrData[i, 11].ToString() : " ";
                e.UNP = (arrData[i, 12] != null) ? arrData[i, 12].ToString() : " ";
                e.director = (arrData[i, 13] != null) ? arrData[i, 13].ToString() : " ";
                e.is_application = 0;
                if (arrData[i, 15] != null)
                {
                    e.type_education_institution_id = Convert.ToInt32(arrData[i, 15]);
                }
                if (arrData[i, 16] != null)
                {
                    e.ownership_type_id = Convert.ToInt32(arrData[i, 16]);
                }
                if (arrData[i, 17] != null)
                {
                    e.department_subordination_id = Convert.ToInt32(arrData[i, 17]);
                }
                e.state_id = 1;
                e.UNP_superior_management= (arrData[i, 19] != null) ? arrData[i, 19].ToString() : " ";
                db.education__institution.Add(e);
                db.SaveChanges();

                if (!site.Equals(" "))
                {
                    User u = new User();
                    u.id = id3++;
                    u.Surname = (arrData[i, 21] != null) ? arrData[i, 21].ToString() : " ";
                    u.Name = (arrData[i, 22] != null) ? arrData[i, 22].ToString() : " ";
                    u.Patronumic = (arrData[i, 23] != null) ? arrData[i, 23].ToString() : " ";
                    u.email = (arrData[i, 25] != null) ? arrData[i, 25].ToString() : " ";
                    u.phone = (arrData[i, 26] != null) ? arrData[i, 26].ToString().Trim() : " ";
                    u.position = (arrData[i, 24] != null) ? arrData[i, 24].ToString() : " ";
                    u.password = Models.User.HashPassword("password");
                    u.state_id = 1;
                    u.role_id = (e.type_education_institution_id > 1) ? 3 : 2;
                    db.User.Add(u);
                    db.SaveChanges();

                    curators_and_controlers c = new curators_and_controlers();
                    c.id = id4++;
                    c.curator_id = u.id;
                    c.education_institution_id = e.id;
                    db.curators_and_controlers.Add(c);
                    db.SaveChanges();
                }
            }

            return Redirect("/Institution/Index");
        }


        public ActionResult InstitutionMonitoring(int edu_id, int?m)
        {
            education__institution edu = db.education__institution.Find(edu_id);
            ViewBag.groups = db.Groups.ToList();
            if (m == null)
            {
                monitoring mm = db.monitoring.OrderByDescending(p => p.date_end).FirstOrDefault(p => p.date_end < DateTime.Now && p.Rating.Count(a => a.audit_object_id == edu.audit_object_id) > 0);
                if (mm != null) {
                    m = mm.id;
                }
            }
            
            int place = 0;
            if (m != null)
            {
                Rating rating = db.Rating.First(p => p.monitoring_id == m && p.audit_object_id == edu.audit_object_id);
                place = db.Rating.Count(p => p.monitoring_id == m && p.sum > rating.sum) + 1;
            }
            ViewBag.place = place;
            ViewBag.monitoring = m;
            return View(edu);
        }


        public ActionResult GetExpertsExcel()
        {
            Microsoft.Office.Interop.Excel.Application ObjWorkExcel = new Microsoft.Office.Interop.Excel.Application(); //открыть эксель
            Microsoft.Office.Interop.Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(@"d:\1_experts.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing); //открыть файл
            Microsoft.Office.Interop.Excel.Worksheet ObjWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)ObjWorkBook.Sheets[1]; //получить 1 лист
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell);//1 ячейку
            int iLastRow = ObjWorkSheet.Cells[ObjWorkSheet.Rows.Count, "A"].End[Microsoft.Office.Interop.Excel.XlDirection.xlUp].Row;  //последняя заполненная строка в столбце А
            var arrData = (object[,])ObjWorkSheet.Range["A1:J" + iLastRow].Value;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing); //закрыть не сохраняя
            ObjWorkExcel.Quit(); // выйти из экселя

            for (int i = 5; i <= iLastRow; ++i)
            {
                User u = new User();
                u.id = db.User.Max(p => p.id) + 1;
                u.Surname = (arrData[i, 5] != null) ? arrData[i, 5].ToString() : " ";
                u.Name = (arrData[i, 6] != null) ? arrData[i, 6].ToString() : " ";
                u.Patronumic = (arrData[i, 7] != null) ? arrData[i, 7].ToString() : " ";
                u.email = (arrData[i, 9] != null) ? arrData[i, 9].ToString() : " ";
                u.phone = (arrData[i, 10] != null) ? arrData[i, 10].ToString() : " ";
                u.position = (arrData[i, 8] != null) ? arrData[i, 8].ToString() : " ";
                u.password = Models.User.HashPassword("password");
                u.state_id = 1;
                u.role_id = 4;
                db.User.Add(u);
                db.SaveChanges();

                string unp= (arrData[i, 4] != null) ? arrData[i, 4].ToString() : " ";
                experts e = new experts();
                e.id = (db.experts.Count() > 0) ? (db.experts.Max(p => p.id) + 1) : 1;
                e.expert_id = u.id;
                e.education_institution_id = db.education__institution.First(p => p.UNP.Contains(unp)).id;
                db.experts.Add(e);
                db.SaveChanges();
            }

            return Redirect("/Users/Index");
        }


        // GET: Institution/Edit/5
        [Authorize(Roles = "Администратор, Контролер, Куратор")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            education__institution education__institution = await db.education__institution.FindAsync(id);
            if (education__institution == null)
            {
                return HttpNotFound();
            }
            ViewBag.audit_object_id = new SelectList(db.audit_object, "id", "title_site", education__institution.audit_object_id);
            ViewBag.department_subordination_id = new SelectList(db.department_subordination, "id", "name", education__institution.department_subordination_id);
            ViewBag.district_id = new SelectList(db.district.Where(p=>p.area_id==education__institution.district.area_id), "id", "name", education__institution.district_id);
            if (education__institution.district_id > 0)
            {
                ViewBag.area_id = new SelectList(db.area, "id", "name", db.district.Find(education__institution.district_id).area_id);
            }
            else
            {
                ViewBag.area_id = new SelectList(db.area, "id", "name");
            }
            ViewBag.kind_edu_id = new SelectList(db.kind_edu, "id", "name", education__institution.kind_edu_id);
            ViewBag.ownership_type_id = new SelectList(db.ownership_type, "id", "name", education__institution.ownership_type_id);
            ViewBag.type_edu_id = new SelectList(db.type_edu, "id", "name", education__institution.type_edu_id);
            ViewBag.type_education_institution_id = new SelectList(db.type_education_institution, "id", "nane", education__institution.type_education_institution_id);
            return View(education__institution);
        }


        [HttpPost]
        [Log(6)]
        [Authorize(Roles = "Администратор, Контролер, Куратор")]
        public ActionResult Edit(education__institution education__institution)
        {
            if (ModelState.IsValid)
            {
                audit_object site = db.audit_object.Find(education__institution.audit_object_id);
                site.adress_site = education__institution.audit_object.adress_site;
                db.audit_object.AddOrUpdate(site);
                db.SaveChanges();
                education__institution.audit_object = null;
                db.Entry(education__institution).State = EntityState.Modified;
                db.SaveChanges();
            }
            ViewBag.audit_object_id = new SelectList(db.audit_object, "id", "title_site", education__institution.audit_object_id);
            ViewBag.department_subordination_id = new SelectList(db.department_subordination, "id", "name", education__institution.department_subordination_id);
            ViewBag.district_id = new SelectList(db.district, "id", "name", education__institution.district_id);
            if (education__institution.district_id > 0)
            {
                ViewBag.area_id = new SelectList(db.area, "id", "name", db.district.Find(education__institution.district_id).area_id);
            }
            else
            {
                ViewBag.area_id = new SelectList(db.area, "id", "name");
            }
            ViewBag.kind_edu_id = new SelectList(db.kind_edu, "id", "name", education__institution.kind_edu_id);
            ViewBag.ownership_type_id = new SelectList(db.ownership_type, "id", "name", education__institution.ownership_type_id);
            ViewBag.type_edu_id = new SelectList(db.type_edu, "id", "name", education__institution.type_edu_id);
            ViewBag.type_education_institution_id = new SelectList(db.type_education_institution, "id", "nane", education__institution.type_education_institution_id);
            return View(education__institution);
        }


        // GET: Institution/Delete/5
        [HttpPost]
        [Authorize(Roles = "Администратор")]
        public JsonResult Delete(int? id)
        {
            if (id != null)
            {
                education__institution edu = db.education__institution.Find(id);
                edu.state_id = 2;
                db.education__institution.AddOrUpdate(edu);
                db.SaveChanges();
            }
            return Json("");
        }
    }
}
