using System.Diagnostics;
using IntegradorP.Models;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorP.Controllers{
public class ClienteController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}

}