using System;
using System.Web.Mvc;
using WebApplication2.HtmlParser;


namespace WebApplication2.Controllers
{
    public class BashPostController : Controller
    {

        public ActionResult Add()
        {
            Dispatcher dispatcher = new Dispatcher();
            string flag = dispatcher.RunThreads();
            ViewBag.Message = "Complete " + Convert.ToString(flag);
            return View();
        }
    }



}