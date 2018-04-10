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
    public class ServiceItemsController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        // GET: ServiceItems
        public async Task<ActionResult> Index()
        {
            return View(await context.ServiceItems.ToListAsync());
        }

        // GET: ServiceItems/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceItem serviceItem = await context.ServiceItems.FindAsync(id);
            if (serviceItem == null)
            {
                return HttpNotFound();
            }
            return View(serviceItem);
        }

        // GET: ServiceItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ServiceItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ServiceItemId,ServiceItemCode,ServiceItemName,Rate")] ServiceItem serviceItem)
        {
            if (ModelState.IsValid)
            {
                context.ServiceItems.Add(serviceItem);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(serviceItem);
        }

        // GET: ServiceItems/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceItem serviceItem = await context.ServiceItems.FindAsync(id);
            if (serviceItem == null)
            {
                return HttpNotFound();
            }
            return View(serviceItem);
        }

        // POST: ServiceItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ServiceItemId,ServiceItemCode,ServiceItemName,Rate")] ServiceItem serviceItem)
        {
            if (ModelState.IsValid)
            {
                context.Entry(serviceItem).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(serviceItem);
        }

        // GET: ServiceItems/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceItem serviceItem = await context.ServiceItems.FindAsync(id);
            if (serviceItem == null)
            {
                return HttpNotFound();
            }
            return View(serviceItem);
        }

        // POST: ServiceItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ServiceItem serviceItem = await context.ServiceItems.FindAsync(id);
            context.ServiceItems.Remove(serviceItem);
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
