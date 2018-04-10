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
using PagedList;

namespace MasterDetail.Controllers
{
    public class InventoryItemsController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: InventoryItems
        public async Task<ActionResult> Index(string sort, string search, int? page)
        {
            ViewBag.CategorySort = string.IsNullOrEmpty(sort) ? "category_desc" : string.Empty;
            ViewBag.ItemCodeSort = sort == "itemcode" ? "itemcode_desc" : "itemcode";
            ViewBag.NameSort = sort == "name" ? "name_desc" : "name";
            ViewBag.UnitPriceSort = sort == "unitprice" ? "unitprice" : "unitprice";

            ViewBag.CurrentSort = sort;
            ViewBag.CurrentSearch = search;

            var inventoryItems = context.InventoryItems.Include(i => i.Category);

            if (!string.IsNullOrEmpty(search))
                inventoryItems = inventoryItems.Where(i => i.InventoryItemCode.StartsWith(search) || i.InventoryItemName.StartsWith(search));

            switch (sort)
            {
                case "category_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.Category.CategoryName).ThenBy(i => i.InventoryItemName);
                    break;
                case "itemcode":
                    inventoryItems = inventoryItems.OrderBy(i => i.InventoryItemCode);
                    break;
                case "itemcode_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.InventoryItemCode);
                    break;
                case "name":
                    inventoryItems = inventoryItems.OrderBy(i => i.InventoryItemName);
                    break;
                case "name_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.InventoryItemName);
                    break;
                case "unitprice":
                    inventoryItems = inventoryItems.OrderBy(i => i.UnitPrice).ThenBy(i => i.InventoryItemName);
                    break;
                case "unitprice_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.UnitPrice).ThenBy(i => i.InventoryItemName);
                    break;
                default: inventoryItems = inventoryItems.OrderBy(i => i.Category.CategoryName).ThenBy(i => i.InventoryItemName);
                    break;
            }

            int pageSize = 3;
            int pageNumber = page ?? 1;

            return View(await Task.FromResult(inventoryItems.ToPagedList(pageNumber, pageSize)));
        }

        // GET: InventoryItems/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = await context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // GET: InventoryItems/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(context.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: InventoryItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "InventoryItemId,InventoryItemCode,InventoryItemName,UnitPrice,CategoryId")] InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                context.InventoryItems.Add(inventoryItem);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(context.Categories, "Id", "CategoryName", inventoryItem.CategoryId);
            return View(inventoryItem);
        }

        // GET: InventoryItems/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = await context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(context.Categories, "Id", "CategoryName", inventoryItem.CategoryId);
            return View(inventoryItem);
        }

        // POST: InventoryItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "InventoryItemId,InventoryItemCode,InventoryItemName,UnitPrice,CategoryId")] InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                context.Entry(inventoryItem).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(context.Categories, "Id", "CategoryName", inventoryItem.CategoryId);
            return View(inventoryItem);
        }

        // GET: InventoryItems/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryItem inventoryItem = await context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return HttpNotFound();
            }
            return View(inventoryItem);
        }

        // POST: InventoryItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            InventoryItem inventoryItem = await context.InventoryItems.FindAsync(id);
            context.InventoryItems.Remove(inventoryItem);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
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
