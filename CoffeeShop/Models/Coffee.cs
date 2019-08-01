using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class Coffee
    {
        public int Id { get; set; }

        // if you're trying to post and you don't have a title on the object this will deny it
        // these are only for data being posted up
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        // setting min max on a property
        // can also set two
        // and also create a custom error message
        [Required]
        [MaxLength(50, ErrorMessage = "BeanType way too loooooong")]
        // ignore data member will ignore this prop during serialization - it won't show up in the JSON you're getting back in a request
        [IgnoreDataMember]
        public string BeanType { get; set; }
    }
}