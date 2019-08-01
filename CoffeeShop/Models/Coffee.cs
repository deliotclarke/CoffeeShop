using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class Coffee
    {
        public int Id { get; set; }

        // if you're trying to post and you don't have a title on the object this will deny it
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        // setting min max on a property
        //can also set two
        [Required]
        [MaxLength(50)]
        public string BeanType { get; set; }

    }
}