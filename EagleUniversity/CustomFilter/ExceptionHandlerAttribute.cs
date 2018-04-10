using EagleUniversity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace EagleUniversity.CustomFilter
{
    public class ExceptionHandlerAttribute : FilterAttribute, IExceptionFilter
    {

        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {


                ExceptionLogger logger = new ExceptionLogger()
                {
                    ExceptionMessage = filterContext.Exception.Message,
                    ExceptionStackTrace = filterContext.Exception.StackTrace,
                    ControllerName = filterContext.RouteData.Values["controller"].ToString(),
                    LogTime = DateTime.Now,
                    //userId= User.Identity.GetUserId()

                };

                ApplicationDbContext ctx = new ApplicationDbContext();
                ctx.ExceptionLoggers.Add(logger);
                ctx.SaveChanges();

                filterContext.ExceptionHandled = true;

                var model = new HandleErrorInfo(filterContext.Exception, "Controller", "Action");

                filterContext.Result = new ViewResult()
                {
                    ViewName = "Error",
                    ViewData = new ViewDataDictionary(model)
                };

            }
        }

    }
}