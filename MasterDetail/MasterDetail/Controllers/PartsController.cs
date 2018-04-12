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

namespace MasterDetail.Controllers
{
    public class PartsController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Parts
        public ActionResult Index(int workOrderId)
        {
            ViewBag.WorkOrderId = workOrderId;
            var parts = context.Parts.Where(p => p.WorkOrderId == workOrderId).OrderBy(p => p.InventoryItemCode);
            return PartialView("_Index", parts.ToList());
        }


        // GET: Parts/Create
        public ActionResult Create(int workOrderId)
        {
            Part part = new Part
            {
                WorkOrderId = workOrderId
            };

            return PartialView("_Create", part);

        }

        // POST: Parts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "PartId,WorkOrderId,InventoryItemCode,InventoryItemName,Quantity,UnitPrice,Notes,IsInstalled")] Part part)
        {
            if (ModelState.IsValid)
            {
                context.Parts.Add(part);
                await context.SaveChangesAsync();
                return Json(new { success = true });

            }

            return PartialView("_Create", part);
        }

        // GET: Parts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = await context.Parts.FindAsync(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            
            return PartialView("_Edit", part);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "PartId,WorkOrderId,InventoryItemCode,InventoryItemName,Quantity,UnitPrice,Notes,IsInstalled")] Part part)
        {
            if (ModelState.IsValid)
            {
                context.Entry(part).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return PartialView("_Edit", part);
        }

        // GET: Parts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Part part = await context.Parts.FindAsync(id);
            if (part == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Part part = await context.Parts.FindAsync(id);
            context.Parts.Remove(part);
            await context.SaveChangesAsync();
            return Json(new { success = true });
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
