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
using Microsoft.AspNet.Identity;

namespace MasterDetail.Controllers
{
    [Authorize]
    public class WorkOrdersController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: WorkOrders
        public async Task<ActionResult> Index()
        {
            var workOrders = context.WorkOrders.Include(w => w.CurrentWorker).Include(w => w.Customer);
            return View(await workOrders.ToListAsync());
        }

        // GET: WorkOrders/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await context.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }
            return View(workOrder);
        }

        // GET: WorkOrders/Create
        public ActionResult Create()
        {
            //ViewBag.CurrentWorkerId = new SelectList(context.ApplicationUsers, "Id", "FirstName");
            ViewBag.CustomerId = new SelectList(context.Customers, "CustomerId", "CompanyName");
            return View();
        }

        // POST: WorkOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId,ReworkNotes")] WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                workOrder.CurrentWorkerId = User.Identity.GetUserId();
                context.WorkOrders.Add(workOrder);
                await context.SaveChangesAsync();
                return RedirectToAction("Edit", new { controller = "WorkOrders", action = "Edit", Id = workOrder.WorkOrderId });
            }

            //ViewBag.CurrentWorkerId = new SelectList(context.ApplicationUsers, "Id", "FirstName", workOrder.CurrentWorkerId);
            ViewBag.CustomerId = new SelectList(context.Customers, "CustomerId", "AccountNumber", workOrder.CustomerId);
            return View(workOrder);
        }

        // GET: WorkOrders/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await context.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }
            //ViewBag.CurrentWorkerId = new SelectList(context.ApplicationUsers, "Id", "FirstName", workOrder.CurrentWorkerId);
            //ViewBag.CustomerId = new SelectList(context.Customers, "CustomerId", "AccountNumber", workOrder.CustomerId);
            return View(workOrder);
        }

        // POST: WorkOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "WorkOrderId,CustomerId,OrderDateTime,TargetDateTime,DropDeadDateTime,Description,WorkOrderStatus,CertificationRequirements,CurrentWorkerId")] WorkOrder workOrder, string command)
        {
            if (ModelState.IsValid)
            {
                // Populate Parts and Labors
                workOrder.Parts = context.Parts.Where(p => p.WorkOrderId == workOrder.WorkOrderId).ToList();
                workOrder.Labors = context.Labors.Where(l => l.WorkOrderId == workOrder.WorkOrderId).ToList();

                PromotionResult promotionResult = new PromotionResult();

                if (command == "Save")
                    promotionResult.Success = true;
                else if (command == "Claim")
                    promotionResult = workOrder.ClaimWorkOrder(User.Identity.GetUserId());
                else
                    promotionResult = workOrder.PromoteWorkOrder(command);

                if (!promotionResult.Success)
                    TempData["MessageToClient"] = promotionResult.Message;

                context.Entry(workOrder).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            //ViewBag.CurrentWorkerId = new SelectList(context.ApplicationUsers, "Id", "FirstName", workOrder.CurrentWorkerId);
            //ViewBag.CustomerId = new SelectList(context.Customers, "CustomerId", "AccountNumber", workOrder.CustomerId);
            return View(workOrder);
        }

        // GET: WorkOrders/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkOrder workOrder = await context.WorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }
            return View(workOrder);
        }

        // POST: WorkOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            WorkOrder workOrder = await context.WorkOrders.FindAsync(id);
            context.WorkOrders.Remove(workOrder);
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
