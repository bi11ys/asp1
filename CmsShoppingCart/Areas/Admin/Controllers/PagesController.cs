using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare list of PageVM
            List<PageVM> pagesList;

            
            using (Db db=new Db())
            {
                //init the list
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //return view with list
            return View(pagesList);
        }

        //GET:Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        //Post:Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                //declare slug
                string slug;
                //init PageDTO
                PageDTO dto = new PageDTO();
                //DTO title
                dto.Title = model.Title;
                //check for and set slug if need
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();

                }
                //Make sure title and slug are unique
                if (db.Pages.Any(x=>x.Title==model.Title) ||db.Pages.Any(x=>x.Slug==slug))
                {
                    ModelState.AddModelError("","That title or slug already exists");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //Save DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Set TempData message
            TempData["SM"] = "You have a new page!";
            //redirect
            return RedirectToAction("AddPage");
        }
        //GET:Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare pagevm
            PageVM model;
            using (Db db = new Db())
            {
                //get the page
                PageDTO dto = db.Pages.Find(id);

                //confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist");
                }

                //init pagevm
                model= new PageVM(dto);

            }

          





            return View(model);
        }
        //POST:Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {

            //Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                //get page id
                int id = model.Id;
                //declare slug
                string slug= "home";
                //get the page
                PageDTO dto = db.Pages.Find(id);
                //DTO the title
                dto.Title = model.Title;

                //check slug and set if need
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                //title and slug unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("","That title or slug already exists.");
                    return View(model);
                }
                //dto the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //save the dto
                db.SaveChanges();
            }
            //set tempadata message
            TempData["SM"] = "You have edited the page";
            //redirect

            
            return RedirectToAction("EditPage");
        }

        //GET:Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //declare pagevm
            PageVM model;

            using (Db db = new Db())
            {


                //get the page
                PageDTO dto = db.Pages.Find(id);
                //confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //init pagevm
                model=new PageVM(dto);
                //return view with model
                return View(model);
            }
        }
        //GET:Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get the page
                PageDTO dto = db.Pages.Find(id);
                //remove the page
                db.Pages.Remove(dto);
                //save
                db.SaveChanges();

            }
            //redirect
            return RedirectToAction("Index");
        }
        //POST:Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;

                //declare pagedto
                PageDTO dto;

                //set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //GET:Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //declare model

            SidebarVM model;

            using (Db db = new Db())
            {
                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);

                //init model
                model = new SidebarVM();
                
            }


            //return view with model
            return View(model);

        }

        //POST:Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {


                //get the dto
                SidebarDTO dto = db.Sidebar.Find(1);
                //dto the body
                dto.Body = model.Body;

                //save
                db.SaveChanges();
            }
            //set tempdata message
            TempData["SM"] = "You have edited the sidebar!";

            //redirect
            return RedirectToAction("EditSidebar");
        }
    }
}