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
    public class LaborsController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: Labors
        public ActionResult Index(int workOrderId)
        {
            ViewBag.WorkOrderId = workOrderId;
            var labors = context.Labors.Where(l => l.WorkOrderId == workOrderId).OrderBy(p => p.ServiceItemCode);
            return PartialView("_Index", labors.ToList());
        }

        // GET: Labors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await context.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }
            return View(labor);
        }

        // GET: Labors/Create
        public ActionResult Create(int workOrderId)
        {
            Labor labor = new Labor
            {
                WorkOrderId = workOrderId
            };

            return PartialView("_Create", labor);

        }

        // POST: Labors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LaborId,WorkOrderId,ServiceItemCode,ServiceItemName,LaborHours,Rate,PercentComplete,Notes")] Labor labor)
        {
            if (ModelState.IsValid)
            {
                context.Labors.Add(labor);
                await context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_Create", labor);
        }

        // GET: Labors/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await context.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Edit", labor);
        }

        // POST: Labors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "LaborId,WorkOrderId,ServiceItemCode,ServiceItemName,LaborHours,Rate,PercentComplete,Notes")] Labor labor)
        {
            if (ModelState.IsValid)
            {
                context.Entry(labor).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Json(new { success = true });

            }

            return PartialView("_Edit", labor);
        }

        // GET: Labors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Labor labor = await context.Labors.FindAsync(id);
            if (labor == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", labor);
        }

        // POST: Labors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Labor labor = await context.Labors.FindAsync(id);
            context.Labors.Remove(labor);
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
