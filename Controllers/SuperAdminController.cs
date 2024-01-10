using System.Diagnostics;
using IntegradorP.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorP.Controllers
{
    public class SuperAdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}