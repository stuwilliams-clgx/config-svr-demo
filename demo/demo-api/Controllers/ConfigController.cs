using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigServerLibrary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace demo_api.Controllers
{
    /// <summary>
    /// Controller: Config
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private IConfigServerClient _clientCS;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="clientCS">IConfigServerClient</param>
        public ConfigController(IConfigServerClient clientCS)
        {
            _clientCS = clientCS;
        }

        /// <summary>
        /// Global Settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGlobal")]
        public ActionResult<IDictionary<string,string>> GetGlobal()
        {
            var d = _clientCS.GetGlobal();
            return Ok(d);
        }

        /// <summary>
        /// App Specific Settings
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetApp/{app}")]
        public ActionResult<IDictionary<string, string>> GetApp(string app)
        {
            var d = _clientCS.GetApp(app);
            return Ok(d);
        }

        /// <summary>
        /// App Environment Specific Settings
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAppEnv/{app}/{env}")]
        public ActionResult<IDictionary<string, string>> GetAppEnv(string app, string env)
        {
            var d = _clientCS.GetAppEnv(app, env);
            return Ok(d);
        }

        /// <summary>
        /// Get all settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAll")]
        public ActionResult<IDictionary<string, string>> GetAll()
        {
            var d = _clientCS.GetAll();
            return Ok(d);
        }

    }
}