using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EagleUniversity.Models;
using EagleUniversity.Models.ViewModels;

namespace EagleUniversity.Controllers
{
    [Authorize]
    public class ActivitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Activities
        public ActionResult Index()
        {
            var xxx = db.Activities.Select(r => r.ActivityTypes.ActivityTypeName);


            var activities = db.Activities.Include(a => a.ActivityTypes).Include(a => a.Modules);
            return View(activities.ToList());
        }

        // GET: Activities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // GET: Activities/Create
        public ActionResult Create(commonEntity entity)
        {
            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "ActivityTypeName");
            var module = db.Modules.Where(r => r.Id == (entity.Id)).SingleOrDefault();
            var viewModel = new ActivityViewModel()
            { redirectProperty=entity, ModuleId=entity.Id, EndDateAm=false, StartDateAm=true , StartDate= module.StartDate, EndDate= module.EndDate };
            return View(viewModel);
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ActivityViewModel activity)
        {
            var tempEntity = new commonEntity()
            {
                Id = activity.redirectProperty.Id,
                returnController = activity.redirectProperty.returnController,
                returnId = activity.redirectProperty.returnId,
                returnMethod = activity.redirectProperty.returnMethod,
                returnTarget = activity.redirectProperty.returnTarget
            };
            activity.redirectProperty = tempEntity;

            
            if (ModelState.IsValid)
            {
                var addActivity = new Activity()
                {
                    ActivityName = activity.ActivityName,
                    ActivityTypeId = activity.ActivityTypeId,
                    ModuleId =activity.ModuleId,
                };
                DateTime s = activity.StartDate;
                DateTime e = activity.EndDate;
                if (activity.StartDateAm)
                {                    
                    addActivity.StartDate = new DateTime(s.Year, s.Month, s.Day, 0, 0, 0);
                }
                else
                {
                    addActivity.StartDate = new DateTime(s.Year, s.Month, s.Day, 12, 0, 0);
                }
                if (activity.EndDateAm)
                {
                    addActivity.EndDate = new DateTime(e.Year, e.Month, e.Day, 11, 59, 59);
                }
                else
                {
                    addActivity.EndDate = new DateTime(e.Year, e.Month, e.Day, 23, 59, 59);
                }

                db.Activities.Add(addActivity);
                db.SaveChanges();
                return RedirectToAction(activity.redirectProperty.returnMethod, activity.redirectProperty.returnController, new { id = activity.redirectProperty.returnId, redirect = activity.redirectProperty.returnTarget });
            }

            ViewBag.ActivityTypeId = new SelectList(db.ActivityTypes, "Id", "ActivityTypeName", activity.ActivityTypeId);
            return View(activity);
        }

        // GET: Activities/Edit/5
        public ActionResult Edit(commonEntity entity)
        {

            Activity activity = db.Activities.Find(entity.Id);
            if (activity == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ActivityViewModel()
            { redirectProperty = entity, ModuleId = activity.ModuleId
            , EndDateAm = false, StartDateAm = true
            , StartDate = activity.StartDate
            , EndDate = activity.EndDate
            , Id=activity.Id
            , ActivityName=activity.ActivityName};

            return View(viewModel);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ActivityViewModel activity)
        {
            var tempEntity = new commonEntity()
            {
                Id = activity.redirectProperty.Id,
                returnController = activity.redirectProperty.returnController,
                returnId = activity.redirectProperty.returnId,
                returnMethod = activity.redirectProperty.returnMethod,
                returnTarget = activity.redirectProperty.returnTarget
            };
            activity.redirectProperty = tempEntity;

            if (ModelState.IsValid)
            {
                Activity editActivity = db.Activities.Find(activity.redirectProperty.Id);
                editActivity.ActivityName = activity.ActivityName;
                DateTime s = activity.StartDate;
                DateTime e = activity.EndDate;
                if (activity.StartDateAm)
                {
                    editActivity.StartDate = new DateTime(s.Year, s.Month, s.Day, 0, 0, 0);
                }
                else
                {
                    editActivity.StartDate = new DateTime(s.Year, s.Month, s.Day, 12, 0, 0);
                }
                if (activity.EndDateAm)
                {
                    editActivity.EndDate = new DateTime(e.Year, e.Month, e.Day, 11, 59, 59);
                }
                else
                {
                    editActivity.EndDate = new DateTime(e.Year, e.Month, e.Day, 23, 59, 59);
                }

                //throw new Exception("Edit Activity");

                db.Entry(editActivity).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction(activity.redirectProperty.returnMethod, activity.redirectProperty.returnController, new { id = activity.redirectProperty.returnId, redirect = activity.redirectProperty.returnTarget });
            }
            return View(activity);
        }

        // GET: Activities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Activity activity = db.Activities.Find(id);
            if (activity == null)
            {
                return HttpNotFound();
            }
            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Activity activity = db.Activities.Find(id);
            var courseId = activity.Modules.CourseId;
            db.Activities.Remove(activity);
            db.SaveChanges();
            return RedirectToAction("Details", "Courses", new { id = courseId  });
        }
        //-----------------------------Delete post
        public ActionResult DeleteAjax(commonEntity entity)
        {
            return View(entity);
        }
        //Need to secure
        [HttpPost, ActionName("DeleteAjax")]
        public ActionResult DeleteAjaxConfimed(commonEntity entity)
        {
            Activity activity = db.Activities.Find(entity.Id);
            db.Activities.Remove(activity);
            db.SaveChanges();
            return RedirectToAction(entity.returnMethod, entity.returnController, new { id = entity.returnId, redirect = entity.returnTarget });
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
