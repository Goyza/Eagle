using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EagleUniversity.Models
{
    public class Document
    {
        public int Id { get; set; }
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }
        [Display(Name = "Document Content")]
        public string DocumentContent { get; set; }
        //
        public byte[] Content { get; set; }
        public string FileType { get; set; }
        //
        [Display(Name = "Upload Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime UploadDate { get; set; }
        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public int DocumentTypeId { get; set; }
        //Nav Prop
        public string AssignedEntity
        {
            get
            {
                return Document.DocToEtity(Id);
            }
        }


        public virtual DocumentType DocumentTypes { get; set; }
        public virtual ICollection<CourseDocument> CourseDocumentAssignments { get; set; }
        public virtual ICollection<ModuleDocument> ModuleDocumentAssignments { get; set; }
        public virtual ICollection<ActivityDocument> ActivityDocumentAssignments { get; set; }
        public virtual ICollection<ApplicationUser> ApplicationUser { get; set; }

        //Users Views
        public static string DocToEtity(int documentId)
        {
            ApplicationDbContext _db = new ApplicationDbContext();
            var docToCour = (from r in _db.CourseDocuments where r.DocumentId==documentId select r).FirstOrDefault();
            if (docToCour!=null)
            {
                return $"Course {docToCour.AssignedCourse.CourseName}";
            }

            var docToMod = (from r in _db.ModuleDocuments where r.DocumentId == documentId select r).FirstOrDefault();
            if (docToMod != null)
            {
                return $"Module {docToMod.AssignedModule.ModuleName}";
            }

            var docToAct = (from r in _db.ActivityDocuments where r.DocumentId == documentId select r).FirstOrDefault();
            if (docToAct != null)
            {
                return $"Activity {docToAct.AssignedActivity.ActivityName}";
            }

            return "Not Assigned";
        }
    }
}