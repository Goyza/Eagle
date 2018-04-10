using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EagleUniversity.Models;

namespace EagleUniversity.Controllers
{
    [Authorize]
    public class ModulesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Modules
        public ActionResult Index()
        {
            var modules = db.Modules.Include(m => m.Course);
            return View(modules.ToList());
        }

        // GET: Modules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }

        // GET: Modules/Create
        public ActionResult Create(int courseId=0)
        {
            ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseName");
            var course = db.Courses.Where(r => r.Id==(courseId)).SingleOrDefault();
            var viewModel = new Module()
            { CourseId = courseId, Course = course, StartDate = course.StartDate, EndDate = course.EndDate };
            return View(viewModel);
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ModuleName,StartDate,EndDate,CourseId")] Module module)
        {
            if (ModelState.IsValid)
            {
                DateTime e = module.EndDate;
                module.EndDate = new DateTime(e.Year, e.Month, e.Day, 23, 59, 59);
                db.Modules.Add(module);
                db.SaveChanges();             
 
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }

            ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // GET: Modules/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            //ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // POST: Modules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ModuleName,StartDate,EndDate,CourseId")] Module module)
        {
            if (ModelState.IsValid)
            {
                DateTime e = module.EndDate;
                module.EndDate = new DateTime(e.Year, e.Month, e.Day, 23, 59, 59);
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Courses", new { id = module.CourseId });
            }
            //ViewBag.CourseId = new SelectList(db.Courses, "Id", "CourseName", module.CourseId);
            return View(module);
        }

        // GET: Modules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Module module = db.Modules.Find(id);
            var courseId = module.CourseId;
            db.Modules.Remove(module);
            db.SaveChanges();
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
        //Need to secure
        [HttpPost]
        public ActionResult DeleteAjax(int id)
        {
            Module module = db.Modules.Find(id);
            var courseId = module.CourseId;
            db.Modules.Remove(module);
            db.SaveChanges();
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
