using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterDetail.Models
{
    public class WorkOrder
    {
        public WorkOrder()
        {
            WorkOrderStatus = WorkOrderStatus.Creating;
        }
        public int WorkOrderId { get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "You must choose a customer.")]
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDateTime { get; set; }

        [Display(Name = "Target Date")]
        public DateTime? TargetDateTime { get; set; }

        [Display(Name = "Drop Dead Date")]
        public DateTime? DropDeadDateTime { get; set; }

        [StringLength(256, ErrorMessage = "The description must be 256 characters or shorter.")]
        public string Description { get; set; }

        [Display(Name = "Status")]
        public WorkOrderStatus WorkOrderStatus { get; set; }

        public decimal Total { get; set; }

        [Display(Name = "Certification Requirements")]
        [StringLength(120, ErrorMessage = "The certification requirements must be 120 characters or shorter.")]
        public string CertificationRequirements { get; set; }

        public virtual ApplicationUser CurrentWorker { get; set; }

        public string CurrentWorkerId { get; set; }

        public virtual List<Part> Parts { get; set; } = new List<Part>();

        public virtual List<Labor> Labors { get; set; } = new List<Labor>();

        [Display(Name = "Rework Notes")]
        [StringLength(256, ErrorMessage = "Rework Notes must be 256 characters or fewer.")]
        public string ReworkNotes { get; set; }


        public PromotionResult ClaimWorkOrder(string userId)
        {
            PromotionResult promotionResult = new PromotionResult();

            //if (!promotionResult.Success)
            //{
            //    return promotionResult;
            //}

            switch (WorkOrderStatus)
            {
                case WorkOrderStatus.Rejected:
                    promotionResult = PromoteToProcessing();
                    break;
                case WorkOrderStatus.Created:
                    promotionResult = PromoteToProcessing();
                    break;
                case WorkOrderStatus.Processed:
                    promotionResult = PromoteToCertifying();
                    break;
                case WorkOrderStatus.Certified:
                    promotionResult = PromoteToApproving();
                    break;
            }

            if (promotionResult.Success)
            {
                CurrentWorkerId = userId;
            }

            return promotionResult;
        }

        public PromotionResult PromoteWorkOrder(string command)
        {
            PromotionResult promotion = new PromotionResult();
            switch (command)
            {
                case "PromoteToCreated":
                    promotion = PromoteToCreated();
                    break;
                case "PromoteToProcessed":
                    promotion = PromoteToProcessed();
                    break;
                case "PromoteToCertified":
                    promotion = PromoteToCertified();
                    break;
                case "PromoteToApproved":
                    promotion = PromoteToApproved();
                    break;
                case "DemoteToCreated":
                    promotion = DemoteToCreated();
                    break;
                case "DemoteToRejected":
                    promotion = DemoteToRejected();
                    break;
                case "DemoteToCanceled":
                    promotion = DemoteToCanceled();
                    break;
            }

            if (promotion.Success)
            {
                CurrentWorker = null;
                CurrentWorkerId = null;

                // Attempt to auto-promote from certified to Approved
                if (WorkOrderStatus == WorkOrderStatus.Certified && Parts.Sum(p => p.ExtendedPrice) + Labors.Sum(l => l.ExtendedPrice) < 5000)
                {
                    PromotionResult autoPromote = PromoteToApproved();

                    if (autoPromote.Success)
                    {
                        promotion = autoPromote;
                        promotion.Message = $"AUTOMATIC PROMOTION: {promotion.Message}";
                    }
                }
            }

            return promotion;
        }

        private PromotionResult PromoteToCreated()
        {
            PromotionResult promotion = new PromotionResult
            {
                Success = true
            };

            if (WorkOrderStatus != WorkOrderStatus.Creating)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Created status because its current status prevented it.";
            }

            else if (string.IsNullOrWhiteSpace(TargetDateTime.ToString()) || string.IsNullOrWhiteSpace(DropDeadDateTime.ToString()) || string.IsNullOrWhiteSpace(Description))
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Created status because its current status prevented it.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Created;
                promotion.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult PromoteToProcessing()
        {
            if (WorkOrderStatus == WorkOrderStatus.Created || WorkOrderStatus == WorkOrderStatus.Rejected)
                WorkOrderStatus = WorkOrderStatus.Processing;

            PromotionResult promotion = new PromotionResult();
            promotion.Success = WorkOrderStatus == WorkOrderStatus.Processing;

            if (promotion.Success)
                promotion.Message = 
                    $"Work order {WorkOrderId} successfullly created by " +
                    $"{HttpContext.Current.User.Identity.Name} and " +
                    $"promoted to status {WorkOrderStatus}";

            else
            {
                promotion.Message = "Failed to promote the work order to approving status because of it's current status";
            }

            return promotion;
        }

        private PromotionResult PromoteToProcessed()
        {
            PromotionResult promotion = new PromotionResult
            {
                Success = true
            };

            if (WorkOrderStatus != WorkOrderStatus.Processing)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Processed status because its current status prevented it.";
            }

            else if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Processed status because it did not contain at least one part and at least one labor item.";
            }

            else if (string.IsNullOrWhiteSpace(Description))
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Processed status because it requires a Description.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Processed;
                promotion.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult PromoteToApproving()
        {
            if (WorkOrderStatus == WorkOrderStatus.Certified)
                WorkOrderStatus = WorkOrderStatus.Approved;

            PromotionResult promotion = new PromotionResult();
            promotion.Success = WorkOrderStatus == WorkOrderStatus.Approving;

            if (promotion.Success)
                promotion.Message = 
                    $"Work order {WorkOrderId} successfully claimed by " +
                    $"{HttpContext.Current.User.Identity.Name} and " +
                    $"promoted to status {WorkOrderStatus}";

            else
            {
                promotion.Message = "Failed to promote the work order to approving status because of it's current status";
            }

            return promotion;
        }

        private PromotionResult PromoteToCertifying()
        {
            if (WorkOrderStatus == WorkOrderStatus.Processed)
                WorkOrderStatus = WorkOrderStatus.Certifying;

            PromotionResult promotion = new PromotionResult();
            promotion.Success = WorkOrderStatus == WorkOrderStatus.Certifying;

            if (promotion.Success)
                promotion.Message = 
                    $"Work order {WorkOrderId} successfully claimed by " +
                    $"{HttpContext.Current.User.Identity.Name} and " +
                    $"promoted to status {WorkOrderStatus}";

            else
            {
                promotion.Message = "Failed to promote the work orderFailed to promote the work order to Certifying status because its current status prevented it.";
            }

            return promotion;
        }

        private PromotionResult PromoteToCertified()
        {
            PromotionResult promotion = new PromotionResult();
            promotion.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Certifying)
            {
                promotion.Success = false;
                promotion.Message = "";
            }

            if (string.IsNullOrWhiteSpace(CertificationRequirements))
            {
                promotion.Success = false;
                promotion.Message = "";
            }

            else if (Parts.Count == 0 || Labors.Count == 0)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Certified status because it did not contain at least one part and at least one labor item.";
            }

            else if (Parts.Count(p => p.IsInstalled == false) > 0 || Labors.Count(l => l.PercentComplete < 100) > 0)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Certified status because not all parts have been installed and labor completed.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Certified;
                promotion.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult PromoteToApproved()
        {
            PromotionResult promotion = new PromotionResult
            {
                Success = true
            };

            if (WorkOrderStatus != WorkOrderStatus.Approving && WorkOrderStatus != WorkOrderStatus.Certified)
            {
                promotion.Success = false;
                promotion.Message = "Failed to promote the work order to Approved status because its current status prevented it.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Approved;
                promotion.Message = $"Work order {WorkOrderId} successfully promoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult DemoteToCreated()
        {
            PromotionResult promotion = new PromotionResult()
            {
                Success = true
            };

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotion.Success = false;
                promotion.Message = "Failed to demote the work order to Created status because its current status prevented it.";
            }

            if (String.IsNullOrWhiteSpace(ReworkNotes))
            {
                promotion.Success = false;
                promotion.Message = "Failed to demote the work order to Created status because Rework Notes must be present.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Created;
                promotion.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult DemoteToRejected()
        {
            PromotionResult promotion = new PromotionResult
            {
                Success = true
            };

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotion.Success = false;
                promotion.Message = "Failed to demote the work order to Rejected status because its current status prevented it.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Rejected;
                promotion.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

        private PromotionResult DemoteToCanceled()
        {
            PromotionResult promotion = new PromotionResult();
            promotion.Success = true;

            if (WorkOrderStatus != WorkOrderStatus.Approving)
            {
                promotion.Success = false;
                promotion.Message = "Failed to demote the work order to Canceled status because its current status prevented it.";
            }

            if (promotion.Success)
            {
                WorkOrderStatus = WorkOrderStatus.Canceled;
                promotion.Message = $"Work order {WorkOrderId} successfully demoted to status {WorkOrderStatus}";
            }

            return promotion;
        }

    }

    public enum WorkOrderStatus
    {
        Creating = 5,
        Created = 10,
        Processing = 15,
        Processed = 20,
        Certifying = 25,
        Certified = 30,
        Approving = 35,
        Approved = 40,
        Rejected = -10,
        Canceled = -20
    }
}