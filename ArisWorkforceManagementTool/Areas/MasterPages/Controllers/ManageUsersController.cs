﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ArisWorkforceManagementTool.Models;
using Aris.Data;
using Aris.Data.Entities;
using Aris.Models.ViewModel;
using Microsoft.Extensions.Logging;
using Users = Aris.Models.ViewModel.Users;
using Aris.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;

namespace ArisWorkforceManagementTool.Areas.MasterPages.Controllers
{
    [Area("MasterPages")]
    public class ManageUsersController : Controller
    {
        private readonly ILogger<ManageUsersController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private string imagePath = string.Empty;
        UnitOfWork UnitOfWork = new UnitOfWork();
        UnitOfWork objUnitOfWorkFetch = new UnitOfWork();


        public ManageUsersController(IWebHostEnvironment hostEnvironment)
        {
            this.webHostEnvironment = hostEnvironment;
        }

        // GET: ManageUsers
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetUsers() => Json(UnitOfWork.UserRepository.Get(null, x => x.OrderBy(id => id.UserId)).ToList());

        // GET: ManageUsers/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ManageUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManageUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }



        // POST: ManageUsers/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult UpdateUser(Users users)
        {
            try
            {
                if (users.UserId != 0)
                {
                  var fetchedUser = objUnitOfWorkFetch.UserRepository.Get(x => x.UserId == users.UserId).ToList();
                    string img = fetchedUser[0].UserImage;
                    string pwd = fetchedUser[0].Password;
                    DateTime createdDate = Convert.ToDateTime(fetchedUser[0].CreatedDate);
                    int createdBy = Convert.ToInt32( fetchedUser[0].CreatedBy);

                  var  user = new Aris.Data.Entities.Users
                    {
                        UserId = users.UserId,
                        UserName = users.UserName,
                        FullName = users.FullName,
                        UserTypeID = users.UserTypeID,
                        MailAddress = users.MailAddress,
                        IsActive = users.IsActive,
                        CreatedBy = createdBy,
                        CreatedDate=createdDate,
                        ModifiedBy = 1,
                        ModifiedDate = DateTime.Now,
                        UserImage = users.ProfileImage==null?img: users.ProfileImage,
                        Password = pwd
                    };

                    UnitOfWork.UserRepository.Update(user);
                    UnitOfWork.Save();

                    return Json(new { success = true, responseText = "User updated successfully." });
                }
                else
                {
                    return Json(new { success = false, responseText = "Something went wrong." });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = "Something went wrong." });
            }
        }

        // GET: ManageUsers/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ManageUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadImage(IList<IFormFile> files)
        {
            foreach (IFormFile source in files)
            {
                string filename = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.Trim('"');

                filename = DateTime.Now.ToFileTime() + "_" + this.EnsureCorrectFilename(filename);
                imagePath = filename;

                using (FileStream output = System.IO.File.Create(this.GetPathAndFilename(filename)))
                    await source.CopyToAsync(output);
            }

            return Json(new { success = true, responseText = "Profile Image updated successfully.", profileImagePath = imagePath });
        }
        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        private string GetPathAndFilename(string filename)
        {
            return this.webHostEnvironment.WebRootPath + "\\Uploads\\UserUploads\\" + filename;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult SubmitRequest(Users userObj)
        {
            try
            {
               
                var user = new Aris.Data.Entities.Users()
                {
                    UserName = userObj.UserName,
                    MailAddress = userObj.MailAddress,
                    UserTypeID = userObj.UserTypeID,
                    IsActive = userObj.IsActive,
                    CreatedDate = DateTime.Now,
                    CreatedBy = 1,
                    FullName = userObj.FullName,
                    Password = new AuthHelper().HashPassword(),
                    UserImage = userObj.ProfileImage
                };

                UnitOfWork.UserRepository.Insert(user);
                UnitOfWork.Save();

                return Json(new { success = true, responseText = "User Added Successfully." });
            }
            catch
            {
                return Json(new { success = true, responseText = "Something Went Wrong!" });
            }
        }
      
        [HttpGet]
        public JsonResult IsUserNameExists(Users userObj)
        {
            var users = UnitOfWork.UserRepository.Get();
            bool has = users.ToList().Any(x => x.UserName.ToLower() == (userObj.UserName==null?"":userObj.UserName.ToLower()));
            if (has)
            {
                return Json(new { value = true, responseText = "User name exists" });
            }
            else
            {
                return Json(new { value = false, responseText = "User name is not exists" });
            }

        }
    }
}