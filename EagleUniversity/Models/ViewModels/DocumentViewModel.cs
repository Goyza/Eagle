﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EagleUniversity.Models.ViewModels
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }
        [Display(Name = "Document Content")]
        public string DocumentContent { get; set; }
        [Display(Name = "Upload Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime UploadDate { get; set; }
        [Display(Name = "Due Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public int DocumentTypeId { get; set; }

        //public string AssignedEntity
        //{
        //    get
        //    {
        //        return Document.DocToEtity(Id);
        //    }
        //}

        public DocumentEntity assignedEntity { get; set; }
    }
}