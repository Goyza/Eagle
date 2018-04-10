using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using EagleUniversity.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using EagleUniversity.Models.ViewModels;
using System.Net;
using System.Data.Entity;
using System.Web.Security;
using System.Collections.Generic;

namespace EagleUniversity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public static bool ActivityDocToUser(int DocumentId, string userId)
        {
            var visible = true;

            ApplicationDbContext _db = new ApplicationDbContext();
            var documents = _db.ActivityDocuments.Where(r => r.DocumentId == DocumentId).FirstOrDefault();
            var ownerRole = UserViewModel.userToRole(documents.OwnerId).Name;
            var userRole =  UserViewModel.userToRole(userId).Name;
            if (userRole=="Student" && ownerRole=="Student" && userId!=documents.OwnerId)
            {
                visible = false;
            }

            return visible;

        }


        //Manage User Part
        [AllowAnonymous]
        public ActionResult UserListPartial(int CourseId,  bool isEmpty=false)
        {
            var role = (from r in _db.Roles where r.Name.Contains("Student") select r).FirstOrDefault();


            var viewModel = _db.Users
    .Where(x => x.Roles.Select(r => r.RoleId)
    .Contains(role.Id)
    )
    .Where(
        x => x.CourseUserAssigments.Select
        (k => k.CourseId)
        .Contains(CourseId)
    )
    .Select(r => new UserViewModel
    {
    Id = r.Id,
    FirstName = r.FirstName,
    Email = r.Email,
    RegistrationTime = r.RegistrationTime,
    LastName = r.LastName, requestedCourseId= CourseId, Avatar= r.Avatar.FirstOrDefault()
    });
            if (isEmpty)
            {
                viewModel = _db.Users
                .Where(
                x => x.Roles.Select(r => r.RoleId)
                .Contains(role.Id)
                )
                .Where(
                            x => !x.CourseUserAssigments.Select
                            (k => k.CourseId)
                            .Contains(CourseId)
                    )
                .Select(r => new UserViewModel
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    Email = r.Email,
                    RegistrationTime = r.RegistrationTime,
                    LastName = r.LastName, requestedCourseId= CourseId ,
                    Avatar = r.Avatar.FirstOrDefault()
                });
            }

            //if (viewModel.Count() <= 0)
            //{
            //    return HttpNotFound();
            //}
            

            return PartialView("_UserList", viewModel);
        }
        //Index Get
        public ActionResult Index(string userRoleId = "Teacher")
        {
            //Requested list
            ViewBag.RolesList = userRoleId;
            var userId = User.Identity.GetUserId();
            var role = (from r in _db.Roles where r.Name.Contains(userRoleId) select r).FirstOrDefault();
            var course = _db.Assignments.Where(r=>r.ApplicationUserId.Contains(userId)).Select(r=> r.Course).FirstOrDefault();
            var courseItem = 0;

            if (course!=null)
            {
                courseItem = course.Id;
            }
            //For the students should be implemented restriction to assigned course 
            //Resticted select
            var viewModel = _db.Users
            .Where(
            x => x.Roles.Select(r => r.RoleId)
            .Contains(role.Id)
            ).Select(r => new UserViewModel
            {
            Id = r.Id,
            FirstName = r.FirstName,
            Email = r.Email,
            RegistrationTime = r.RegistrationTime,
            LastName = r.LastName
            });

            if (User.IsInRole("Student"))
            {
                viewModel = _db.Users
                        .Where(
                        x => x.Roles.Select(r => r.RoleId)
                        .Contains(role.Id)
                        )
                        .Where(
                            x => x.CourseUserAssigments.Select
                            (k => k.CourseId)
                            .Contains(courseItem)
                            )
                        .Select(r => new UserViewModel
                        {
                            Id = r.Id,
                            FirstName = r.FirstName,
                            Email = r.Email,
                            RegistrationTime = r.RegistrationTime,
                        //Role = userRoleId,
                        LastName = r.LastName
                        });
            }

            if (userRoleId!="Student" && User.IsInRole("Student") || role==null)
             {
                return RedirectToAction("Index", "Home");
             }
            return View(viewModel);
        }
        //Get /Account/CreateUser
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult CreateUser(string userRoleId = "Teacher", int CourseId=0)
        {            
            var role = (from r in _db.Roles where r.Name.Contains(userRoleId) select r).FirstOrDefault();

            if (role==null)
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new CreateUserViewModel()
            {
                Role = role.Name
            };
            return View(viewModel);
        }
        // POST: /Account/CreateUser
        [HttpPost]
        [Authorize(Roles = "Teacher, Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(CreateUserViewModel model)
        {
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    RegistrationTime = DateTime.Now,
                     
                };
                var result = await UserManager.CreateAsync(user, "Password12345");
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //Roles
                    var idResult = userManager.AddToRole(user.Id, model.Role);
                    //?
                    _db.SaveChanges();
                    return RedirectToAction("Index", "Account", new { userRoleId = model.Role });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        // Get: /Account/DeleteUser
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult DeleteUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = _db.Users
            .Where(r => r.Id==id )
            .Select(r => new UserViewModel
            {
            Id = r.Id,
            FirstName = r.FirstName,
            Email = r.Email,
            RegistrationTime = r.RegistrationTime,
            LastName = r.LastName
        }).SingleOrDefault();


            if (viewModel == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }

        //POST: Account/DeleteUser
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult DeleteUserConfirmed(string id)
        {
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var deletedUser = userManager.FindById(id);
            var roleUser  = UserViewModel.userToRole(id)?.Name ?? "Not Assigned";
            userManager.Delete(deletedUser);
            _db.SaveChanges();
            
            return RedirectToAction("Index", "Account", new { userRoleId = roleUser});
        }
        // Get: /Account/DetailUser
        public ActionResult DetailUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = _db.Users
            .Where(r => r.Id == id)
            .Select(r => new UserViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                Email = r.Email,
                RegistrationTime = r.RegistrationTime,
                LastName = r.LastName
            }).SingleOrDefault();


            if (viewModel == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }
        // Get: /Account/EditUser
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult EditUser(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var viewModel = _db.Users
            .Where(r => r.Id == id)
            .Select(r => new UserViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                Email = r.Email,
                LastName = r.LastName
            }).SingleOrDefault();


            if (viewModel == null)
            {
                return HttpNotFound();
            }
            return View(viewModel);
        }
        //POST: Account/EditUser
        [HttpPost, ActionName("EditUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult EditUserConfirmed(UserViewModel model)
        {
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var roleUser = UserViewModel.userToRole(model.Id)?.Name ?? "Not Assigned";
            if (ModelState.IsValid)
            {
                var changedUser = userManager.FindById(model.Id);
                changedUser.LastName = model.LastName;
                changedUser.FirstName = model.FirstName;
                changedUser.Email = model.Email;
                _db.Entry(changedUser).State = EntityState.Modified;
                _db.SaveChanges();
            }
            return RedirectToAction("Index", "Account", new { userRoleId = model.Role });
        }

        //-------------------------------
        //Edit Ajax section
        public ActionResult EditAjaxUser(UserEntity userEntity)
        {


            var viewModel = _db.Users
            .Where(r => r.Id == userEntity.UserId)
            .Select(r => new UserViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                Email = r.Email,
                LastName = r.LastName
            }).SingleOrDefault();

            viewModel.assignedEntity = userEntity;

            if (viewModel == null)
            {
                return RedirectToAction(userEntity.returnMethod, userEntity.returnController, new { id = userEntity.returnId, redirect = userEntity.returnTarget });
            }
            return View(viewModel);
        }
        //POST: Account/EditAjaxUser
        [HttpPost, ActionName("EditAjaxUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult EditAjaxUserConfirmed(UserViewModel model)
        {
            var tempEntity = new UserEntity()
            {
                UserId = model.assignedEntity.UserId,
                returnController = model.assignedEntity.returnController,
                returnId = model.assignedEntity.returnId,
                returnMethod = model.assignedEntity.returnMethod,
                returnTarget = model.assignedEntity.returnTarget
            };
            model.assignedEntity = tempEntity;
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            if (ModelState.IsValid)
            {
                var changedUser = userManager.FindById(model.Id);
                changedUser.LastName = model.LastName;
                changedUser.FirstName = model.FirstName;
                changedUser.Email = model.Email;
                _db.Entry(changedUser).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction(model.assignedEntity.returnMethod, model.assignedEntity.returnController, new { id = model.assignedEntity.returnId, redirect = model.assignedEntity.returnTarget });
            } 

           return View(model);
        }

        //Edit Ajax section
        public ActionResult DeleteAjaxUser(UserEntity userEntity)
        {
            var viewModel = _db.Users
            .Where(r => r.Id == userEntity.UserId)
            .Select(r => new UserViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                Email = r.Email,
                LastName = r.LastName
            }).SingleOrDefault();

            viewModel.assignedEntity = userEntity;

            if (viewModel == null)
            {
                return RedirectToAction(userEntity.returnMethod, userEntity.returnController, new { id = userEntity.returnId, redirect = userEntity.returnTarget });
            }
            return View(viewModel);
        }
        //POST: Account/DeleteAjaxUser
        [HttpPost, ActionName("DeleteAjaxUser")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher, Admin")]
        public ActionResult DeleteAjaxUserConfirmed(UserViewModel model)
        {
            var tempEntity = new UserEntity()
            {
                UserId = model.assignedEntity.UserId,
                returnController = model.assignedEntity.returnController,
                returnId = model.assignedEntity.returnId,
                returnMethod = model.assignedEntity.returnMethod,
                returnTarget = model.assignedEntity.returnTarget
            };
            model.assignedEntity = tempEntity;

            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            if (ModelState.IsValid)
            {
                var deletedUser = userManager.FindById(model.Id);
                userManager.Delete(deletedUser);
                _db.SaveChanges();
                return RedirectToAction(model.assignedEntity.returnMethod, model.assignedEntity.returnController, new { id = model.assignedEntity.returnId, redirect = model.assignedEntity.returnTarget });
            }

            return View(model);
        }
        // Get: /Account/DetailAjaxUser
        public ActionResult DetailAjaxUser(UserEntity userEntity)
        {
            var viewModel = _db.Users
                //.Include(s => s.Avatar)
            .Where(r => r.Id == userEntity.UserId)
            .Select(r => new UserViewModel
            {
                Id = r.Id,
                FirstName = r.FirstName,
                Email = r.Email,
                LastName = r.LastName,
                RegistrationTime=r.RegistrationTime, Avatar=r.Avatar.FirstOrDefault()

            }).SingleOrDefault();

            viewModel.assignedEntity = userEntity;

            if (viewModel == null)
            {
                return RedirectToAction(userEntity.returnMethod, userEntity.returnController, new { id = userEntity.returnId, redirect = userEntity.returnTarget });
            }
            return View(viewModel);
        }
        //CreateAjaxUser
        public ActionResult CreateAjaxUser(UserEntity userEntity)
        {
            var role = (from r in _db.Roles where r.Name.Contains("Student") select r).FirstOrDefault();

            if (role == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new CreateUserViewModel()
            {
                Role = role.Name, assignedEntity=userEntity
            };

            return View(viewModel);
        }
        //        // POST: /Account/CreateUser
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("CreateAjaxUser")]
        public ActionResult CreateAjaxUserConfirmed(CreateUserViewModel model, HttpPostedFileBase upload)
        {
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var tempEntity = new UserEntity()
            {
                UserId = model.assignedEntity.UserId,
                returnController = model.assignedEntity.returnController,
                returnId = model.assignedEntity.returnId,
                returnMethod = model.assignedEntity.returnMethod,
                returnTarget = model.assignedEntity.returnTarget
            };
            model.assignedEntity = tempEntity;
            //
            var addDocument = new Document()
            {
                //Id = document.Id,
                DocumentTypeId = 1,
                DocumentName = "Avatar",
                DueDate = DateTime.Now,
                UploadDate = DateTime.Now
            };

            if (upload != null && upload.ContentLength > 0)
            {
                            addDocument.DocumentName = System.IO.Path.GetFileName(upload.FileName);
                            addDocument.FileType = upload.ContentType;
                            using (var reader = new System.IO.BinaryReader(upload.InputStream))
                            {
                                addDocument.Content = reader.ReadBytes(upload.ContentLength);
                            }
                        
                        //_db.Documents.Add(addDocument);
                        //_db.SaveChanges();


            }

            //
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    RegistrationTime = DateTime.Now,
                    Avatar = new List<Document> { addDocument }

            };
                var result = UserManager.Create(user, "Passoword12345");
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    //Roles
                    var idResult = userManager.AddToRole(user.Id, model.Role);
                    //?
                    _db.SaveChanges();
                    return RedirectToAction(model.assignedEntity.returnMethod, model.assignedEntity.returnController, new { id = model.assignedEntity.returnId, redirect = model.assignedEntity.returnTarget });
                }
            }            
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        //------------------------------------------------------
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [Authorize(Roles = "Admin")]
        public ActionResult Register()
        {
            var viewModel = new RegisterViewModel()
            {
                Roles = _db.Roles.ToList()
            };
            return View(viewModel);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var userStore = new UserStore<ApplicationUser>(_db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var rolestore = new RoleStore<IdentityRole>(_db);
            var roleManager = new RoleManager<IdentityRole>(rolestore);

            if (ModelState.IsValid)
            {              
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email
                    , LastName=model.LastName, FirstName=model.FirstName, RegistrationTime=DateTime.Now };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //Roles

                    var role = roleManager.FindById(model.Role);
                    var idResult = userManager.AddToRole(user.Id, role.Name);


                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}