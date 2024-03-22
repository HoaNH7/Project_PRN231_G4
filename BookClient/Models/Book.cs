using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookClient.Models
{
    public partial class Book
    {
        public Book()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; } = null!;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;

        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
