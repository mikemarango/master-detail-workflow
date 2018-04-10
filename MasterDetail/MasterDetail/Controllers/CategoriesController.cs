using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MasterDetail.DataLayer;
using MasterDetail.Models;
using TreeUtility;
using MasterDetail.ViewModels;
using System.Data.Entity.Infrastructure;

namespace MasterDetail.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        private List<Category> GetListOfNodes()
        {
            List<Category> sourceCategories = context.Categories.ToList();
            List<Category> categories = new List<Category>();
            foreach (var item in sourceCategories)
            {
                Category category = new Category()
                {
                    Id = item.Id,
                    CategoryName = item.CategoryName
                };

                if (item.ParentCategoryId != null)
                {
                    category.Parent = new Category
                    {
                        Id = (int)item.ParentCategoryId
                    };
                }

                categories.Add(category);
            }

            return categories;
        }

        private void ValidateParentAreOrphans(Category category)
        {
            // There is no parent
            if (category.ParentCategoryId == null)
            {
                return;
            }

            // The parent has a parent
            Category parentCategory = context.Categories.Find(category.ParentCategoryId);
            if (parentCategory.ParentCategoryId != null)
            {
                throw new InvalidOperationException("You cannot nest this category more than two levels deep.");

            }

            // The parent is an orphan but the category being nested has children
            int numberOfChildren = context.Categories.Count(c => c.ParentCategoryId == category.Id);
            if (numberOfChildren > 0)
                throw new InvalidOperationException("You cannot nest this category's children more than two levels deep");
        }

        private SelectList PopulateParentCategorySelectList(int? id)
        {
            SelectList selectList;

            if (id == null)
                selectList = new SelectList(context.Categories.Where(c => c.ParentCategoryId == null), "Id", "CategoryName");

            else if (context.Categories.Count(c => c.ParentCategoryId == id) == 0)
                selectList = new SelectList(context.Categories.Where(c => c.ParentCategoryId == null && c.Id != id), "Id", "CategoryName");

            else
                selectList = new SelectList(context.Categories.Where(c => false), "Id", "CategoryName");

            return selectList;
        }

        public ActionResult Index()
        {
            // Start the outermost list
            string fullString = "<ul>";

            IList<Category> listOfNodes = GetListOfNodes();
            IList<Category> topLevelCategories = TreeHelper.ConvertToForest(listOfNodes);

            foreach (var category in topLevelCategories)
                fullString += EnumerateNodes(category);

            // End the outermost list
            fullString += "</ul>";

            return View((object)fullString);
        }

        public string EnumerateNodes(Category categoryParent)
        {
            // Init an empty string
            string content = String.Empty;

            // Add <li> category name
            content += "<li class=\"treenode\">";
            content += categoryParent.CategoryName;
            content += String.Format("<a href=\"/Categories/Edit/{0}\" class=\"btn btn-primary btn-xs treenodeeditbutton\">Edit</a>", categoryParent.Id);
            content += String.Format("<a href=\"/Categories/Delete/{0}\" class=\"btn btn-danger btn-xs treenodedeletebutton\">Delete</a>", categoryParent.Id);

            // If there are no children, end the </li>
            if (categoryParent.Children.Count == 0)
                content += "</li>";
            else   // If there are children, start a <ul>
                content += "<ul>";

            // Loop one past the number of children
            int numberOfChildren = categoryParent.Children.Count;
            for (int i = 0; i <= numberOfChildren; i++)
            {
                // If this iteration's index points to a child,
                // call this function recursively
                if (numberOfChildren > 0 && i < numberOfChildren)
                {
                    Category child = categoryParent.Children[i];
                    content += EnumerateNodes(child);
                }

                // If this iteration's index points past the children, end the </ul>
                if (numberOfChildren > 0 && i == numberOfChildren)
                    content += "</ul>";
            }

            // Return the content
            return content;
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        public ActionResult Create()
        {
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,ParentCategoryId,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    ValidateParentAreOrphans(category);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(null);
                    return View(category);
                }
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(null);
            return View(category);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            CategoryViewModel categoryViewModel = new CategoryViewModel()
            {
                Id = category.Id,
                ParentCategoryId = category.ParentCategoryId,
                CategoryName = category.CategoryName
            };


            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id);
            return View(categoryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ParentCategoryId,CategoryName")] CategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                Category category = new Category();

                try
                {
                    category.Id = categoryViewModel.Id;
                    category.ParentCategoryId = categoryViewModel.ParentCategoryId;
                    category.CategoryName = categoryViewModel.CategoryName;
                    ValidateParentAreOrphans(category);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id); 
                    return View("Edit", categoryViewModel);
                }

                context.Entry(category).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ParentCategoryIdSelectList = PopulateParentCategorySelectList(categoryViewModel.Id);
            return View(categoryViewModel);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Category category = await context.Categories.FindAsync(id);

            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");

            }

            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "You attempted to delete a category that had child categories");
            }

            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View("Delete", category);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
