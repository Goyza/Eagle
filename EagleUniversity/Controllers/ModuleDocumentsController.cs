﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EagleUniversity.Models;
using Microsoft.AspNet.Identity;

namespace EagleUniversity.Controllers
{
    [Authorize]
    public class ModuleDocumentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ModuleDocuments
        public ActionResult Index()
        {
            var moduleDocuments = db.ModuleDocuments.Include(m => m.AssignedDocument).Include(m => m.AssignedModule);
            return View(moduleDocuments.ToList());
        }

        // GET: ModuleDocuments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModuleDocument moduleDocument = db.ModuleDocuments.Find(id);
            if (moduleDocument == null)
            {
                return HttpNotFound();
            }
            return View(moduleDocument);
        }

        // GET: ModuleDocuments/Create
        public ActionResult Create(int DocumentId)
        {
            var alreadyExist = db.ModuleDocuments.Where(r => r.DocumentId == DocumentId);

            var document = db.Documents.Where(r => r.Id == DocumentId).SingleOrDefault();

            if (alreadyExist.Count() > 0)
            {
                return RedirectToAction("Index", "Documents");
            }


            var viewModel = new ModuleDocument()
            { DocumentId = DocumentId, AssignedDocument = document };

            //ViewBag.DocumentId = new SelectList(db.Documents, "Id", "DocumentName");
            ViewBag.ModuleId = new SelectList(db.Modules, "Id", "ModuleName");
            return View(viewModel);
        }

        // POST: ModuleDocuments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DocumentId,ModuleId")] ModuleDocument moduleDocument)
        {
            moduleDocument.OwnerId = User.Identity.GetUserId();
            moduleDocument.AssignDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.ModuleDocuments.Add(moduleDocument);
                db.SaveChanges();
                return RedirectToAction("Index", "Documents");
            }

            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "DocumentName", moduleDocument.DocumentId);
            ViewBag.ModuleId = new SelectList(db.Modules, "Id", "ModuleName", moduleDocument.ModuleId);
            return View(moduleDocument);
        }

        // GET: ModuleDocuments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModuleDocument moduleDocument = db.ModuleDocuments.Find(id);
            if (moduleDocument == null)
            {
                return HttpNotFound();
            }
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "DocumentName", moduleDocument.DocumentId);
            ViewBag.ModuleId = new SelectList(db.Modules, "Id", "ModuleName", moduleDocument.ModuleId);
            return View(moduleDocument);
        }

        // POST: ModuleDocuments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DocumentId,ModuleId,AssignDate,OwnerId")] ModuleDocument moduleDocument)
        {
            if (ModelState.IsValid)
            {
                db.Entry(moduleDocument).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DocumentId = new SelectList(db.Documents, "Id", "DocumentName", moduleDocument.DocumentId);
            ViewBag.ModuleId = new SelectList(db.Modules, "Id", "ModuleName", moduleDocument.ModuleId);
            return View(moduleDocument);
        }

        // GET: ModuleDocuments/Delete/5
        public ActionResult Delete(int? DocumentId)
        {
            if (DocumentId == null)
            {
                return RedirectToAction("Index", "Documents");
            }
            ModuleDocument moduleDocument = db.ModuleDocuments.Where(r => r.DocumentId == DocumentId).SingleOrDefault();
            if (moduleDocument == null)
            {
                return RedirectToAction("Index", "Documents");
            }
            return View(moduleDocument);
        }

        // POST: ModuleDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(CourseDocument document)
        {
            ModuleDocument moduleDocument = db.ModuleDocuments.Where(r => r.DocumentId == document.DocumentId).SingleOrDefault();
            db.ModuleDocuments.Remove(moduleDocument);
            db.SaveChanges();
            return RedirectToAction("Index", "Documents");
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
