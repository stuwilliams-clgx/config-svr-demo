using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace demo_api.Controllers
{
    /// <summary>
    /// Controller: Home
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Index => Swagger
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return Redirect("~/swagger");
        }
    }
}