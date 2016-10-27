using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class BashPost
    {
        public int BashPostID { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public int rating { get; set; }
        public DateTime dt { get; set; }

        public BashPost(string text, string title, string dt, string rating) {
            this.text = text;
            this.title = title;
            this.dt = Convert.ToDateTime(dt);
            this.rating = Convert.ToInt32(rating);
        }
    }
}