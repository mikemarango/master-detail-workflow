using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TreeUtility;

namespace MasterDetail.Models
{
    public class Category : ITreeNode<Category>
    {
        private int? _parentCategoryId;

        public int Id { get; set; }
        [Display(Name = "Parent Category")]
        public int? ParentCategoryId
        {
            get => _parentCategoryId; set
            {
                if (Id == value)
                    throw new InvalidOperationException("A category cannot have itself as its parent.");
                _parentCategoryId = value;
            }
        }
        [Required(ErrorMessage = "You must enter a category name.")]
        [StringLength(20, ErrorMessage = "Category names must be 20 characters or less")]
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        public virtual Category Parent { get; set; }
        public IList<Category> Children { get; set; }
    }
}