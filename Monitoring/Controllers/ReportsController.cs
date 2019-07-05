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
using System.IO;
using OfficeOpenXml;

namespace Monitoring.Controllers
{
    public class ReportsController : Controller
    {
        private MonitoringEntities db = new MonitoringEntities();

        //Страница со списком всех отчетов по результатам Мониторинга
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllResults()
        {
            return View();
        }

        public ActionResult ExportResults()
        {
            
            monitoring m = db.monitoring.OrderByDescending(p=>p.date_end).FirstOrDefault(p => p.date_end < DateTime.Now); //last monitoring
            List<education__institution> edu = db.education__institution.Where(p => p.audit_object.technical_rating.Where(s=>s.monitoring_id==m.id).Count() > 0 && p.audit_object.experts_ratinng.Where(s => s.monitoring_id == m.id).Count() > 0).ToList();
            List<technical_rating> tech = db.technical_rating.Where(p => p.monitoring_id == m.id).ToList();
            List<experts_ratinng> exp = db.experts_ratinng.Where(p => p.monitoring_id == m.id).ToList();
            List<Groups> groups = db.Groups.ToList();
            List<Criteria> criteria = db.Criteria.ToList();
            List<Experts_comments> e_comment = db.Experts_comments.ToList();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Результаты");

            workSheet.Cells[1, 1].Value = "Учреждение";
            workSheet.Cells[1, 2].Value = "Технический рейтинг";
            workSheet.Cells[2, 2].Value = "Доступность сайта";
            workSheet.Cells[2, 3].Value = "Код ответа HTTP";
            workSheet.Cells[2, 4].Value = "Использование HTTPS";
            workSheet.Cells[2, 5].Value = "Время отклика";
            workSheet.Cells[2, 6].Value = "Время полной загрузки сайта";
            workSheet.Cells[2, 7].Value = "Наличие подписей картинок";
            workSheet.Cells[2, 8].Value = "Наличие заголовка";
            workSheet.Cells[2, 9].Value = "Файл robot.txt";
            workSheet.Cells[2, 10].Value = "Максимальный уровень вложенности";
            workSheet.Cells[2, 11].Value = "Количество битых ссылок";
            workSheet.Cells[1, 12].Value = "Экспертный рейтинг";

            int rows = 4;
            int cols = 12;
            for (int i = 0; i < groups.Count(); ++i)
            {
                workSheet.Cells[2, cols].Value = groups.ElementAt(i).name;
                int group_id = groups.ElementAt(i).id;
                List<Criteria> cr = criteria.Where(p => p.group_id == group_id).ToList();
                for(int j = 0; j < cr.Count(); ++j)
                {
                    workSheet.Cells[3, cols++].Value = cr.ElementAt(j).short_name;
                }
            }

            for(int i = 0; i < edu.Count(); ++i)
            {
                workSheet.Cells[rows, 1].Value = edu.ElementAt(i).full_name;
                int audit_object_id = Convert.ToInt32(edu.ElementAt(i).audit_object_id);
                technical_rating t = tech.First(p => p.audit_object_id == audit_object_id);
                workSheet.Cells[rows, 2].Value = t.site_accessibility;
                workSheet.Cells[rows, 3].Value = ((t.status_code!=null) && t.status_code.Contains("True")) ? "Доступен" : "Не доступен";
                workSheet.Cells[rows, 4].Value = ((t.using_HTTPS != null) && t.using_HTTPS.Contains("True")) ? "Да" : "Нет";
                workSheet.Cells[rows, 5].Value = t.response_time;
                workSheet.Cells[rows, 6].Value = t.full_load;
                workSheet.Cells[rows, 7].Value = ((t.img_title != null) && t.img_title.Contains("True")) ? "Да" : "Нет";
                workSheet.Cells[rows, 8].Value = ((t.has_title != null) &&  t.has_title.Contains("True")) ? "Да" : "Нет";
                workSheet.Cells[rows, 9].Value = ((t.robots_txt != null) && t.robots_txt.Contains("True")) ? "Да" : "Нет";
                workSheet.Cells[rows, 10].Value = t.nesting_level;
                workSheet.Cells[rows, 11].Value = t.broken_links_count;

                experts_ratinng e = exp.First(p => p.audit_object_id == audit_object_id);
                for(int s = 0; s < groups.Count(); ++s)
                {
                    int group_id = groups.ElementAt(s).id;
                    List<Criteria> cr = criteria.Where(p => p.group_id == group_id).ToList();
                    for (int j = 0; j < cr.Count(); ++j)
                    {
                        int cr_id = cr.ElementAt(j).id;
                        Experts_comments e1 = e_comment.FirstOrDefault(p => p.criteria_id == cr_id && p.site_experts.audit_object_id == audit_object_id);
                        workSheet.Cells[rows, cols++].Value = (e1!=null) ? e1.answer.ToString() : "";
                    }
                }
                cols = 12;

                rows++;
            }

            var path = Path.Combine(Server.MapPath("~/Content/"), "results_monitoring.xlsx");
            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + path);
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            FileInfo fi = new FileInfo(path);
            long sz = fi.Length;
            Response.ClearContent();
            Response.ContentType = Path.GetExtension(path);
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename = {0}", System.IO.Path.GetFileName(path)));
            Response.AddHeader("Content-Length", sz.ToString("F0"));
            Response.TransmitFile(path);
            Response.End();

            System.IO.File.Delete(path);
            return Redirect("/Reports/AllResults");
        }

        // GET: Reports
        public ActionResult Report(int type)
        {
            List<education__institution> edu = db.education__institution.Where(p => p.audit_object != null).ToList();
            if (type == 1)
            {
                edu = edu.Where(p => p.type_education_institution_id == 3 && p.type_edu_id == 5).ToList();
            }
            else if (type == 2)
            {
                edu = edu.Where(p => p.type_education_institution_id == 3 && p.type_edu_id != 5 && p.type_edu_id != 13 && p.type_edu_id != 14).ToList();
            }
            else if (type == 3)
            {
                edu = edu.Where(p => (p.type_education_institution_id == 3 || p.type_education_institution_id == 2) && (p.type_edu_id == 13 || p.type_edu_id == 14)).ToList();
            }
            ViewBag.type = type;
            ViewBag.monitoring = db.monitoring.OrderByDescending(p => p.id).First().id;
            return View(edu);
        }

        //Вывод отчетов по областям
        public ActionResult Area(int type)
        {
            List<education__institution> edu = db.education__institution.Where(p => p.audit_object != null).ToList();
            if (type == 1)
            {
                edu = edu.Where(p => (p.type_education_institution_id == 3 || p.type_education_institution_id == 2) && (p.type_edu_id == 13 || p.type_edu_id == 14)).ToList();
            }
            else if (type == 3)
            {
                edu = edu.Where(p => p.type_edu_id == 1).ToList();
            }
            ViewBag.type = type;
            ViewBag.areas = db.area.ToList();
            ViewBag.monitoring = db.monitoring.OrderByDescending(p => p.id).First().id;
            return View(edu);
        }


        //Вывод отчета по группам критериев, в качестве параметра принимаются номер группы и тип
        public ActionResult Groups(int group, int? type)
        {
            List<education__institution> edu = db.education__institution.Where(p => p.audit_object != null).ToList();
            if (type == null)
            {
                edu = edu.Where(p => (p.type_education_institution_id == 1 || p.type_education_institution_id == 2) && p.type_edu_id != 13).ToList();
            }
            else if (type == 1)
            {
                edu = edu.Where(p => p.type_edu_id == 1).ToList();
            }
            ViewBag.areas = db.area.ToList();
            ViewBag.group = group;
            ViewBag.monitoring = db.monitoring.OrderByDescending(p => p.id).First().id;
            if (group == 1)
            {
                ViewBag.name = "Группа 1 (Требования к размещению информации)";
                ViewBag.err20 = 4;
                ViewBag.err70 = 13;
            }
            else if (group == 2)
            {
                ViewBag.name = "Группа 2 (Структура)";
                ViewBag.err20 = 0;
                ViewBag.err70 = 3;
            }
            else if (group == 3)
            {
                ViewBag.name = "Группа 3 (Поиск)";
                ViewBag.err20 = 3;
                ViewBag.err70 = 8;
            }
            else if (group == 4)
            {
                ViewBag.name = "Группа 4 (Информация об органе (организации))";
                ViewBag.err20 = 5;
                ViewBag.err70 = 16;
            }
            else if (group == 5)
            {
                ViewBag.name = "Группа 5 (Обращения)";
                ViewBag.err20 = 1;
                ViewBag.err70 = 5;
            }
            else if (group == 6)
            {
                ViewBag.name = "Группа 6 (Контактная информация)";
                ViewBag.err20 = 0;
                ViewBag.err70 = 3;
            }
            else if (group == 7)
            {
                ViewBag.name = "Группа 7 (Административные процедуры)";
                ViewBag.err20 = 3;
                ViewBag.err70 = 10;
            }
            else if (group == 8)
            {
                ViewBag.name = "Группа 8 (Товары (работы, услуги))";
                ViewBag.err20 = 0;
                ViewBag.err70 = 2;
            }
            else if (group == 9)
            {
                ViewBag.name = "Группа 9 (Электронные обращения)";
                ViewBag.err20 = 2;
                ViewBag.err70 = 7;
            }
            else if (group == 10)
            {
                ViewBag.name = "Группа 10 (Навигация)";
                ViewBag.err20 = 1;
                ViewBag.err70 = 5;
            }
            else if (group == 11)
            {
                ViewBag.name = "Группа 11 (Дизайн)";
                ViewBag.err20 = 1;
                ViewBag.err70 = 5;
            }
            return View(edu);
        }


        //Генерирует отчет о сайтах дошкольны учреждений, в которых в экспертном рейтинге ошибок >=70%
        public ActionResult ErrorListSite()
        {
            List<education__institution> edu = db.education__institution.Where(p => p.audit_object != null && p.type_edu_id == 1 && p.audit_object.site_experts.FirstOrDefault().Experts_comments.Count(s => s.answer == 0) >= 78).ToList();
            return View(edu);
        }

    }
}
