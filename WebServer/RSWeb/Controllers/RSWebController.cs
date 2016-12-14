using Microsoft.AspNetCore.Mvc;
using System;

using Microarea.RSWeb.Models;

namespace Microarea.RSWeb.Controllers
{
    [Route("api/[controller]")]
    public class RSWebController : Controller
    {
        // GET api/RSWeb/namespace
        /// <summary>
        /// the method is called on startup 
        /// accepts the namespace and saves in the aux structure
        /// assigns exclusive guid for the first socket connection
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        ////[HttpGet("{ns}")]
        ////public IActionResult RecieveRequest(string ns)
        ////{
        ////    // IsValidNamespace(ns); 
        ////    // HandleErrors();

        ////    /// generates guid
        ////    Guid guid = Guid.NewGuid();

        ////    /// assigns guid to the namespace just passed
        ////    SocketHandler s = new SocketHandler();
        ////    NSHash.AddAssoc(guid.ToString(), ns);
        ////    string guidMessage = MessageBuilder.GetJSONMessage(MessageBuilder.CommandType.GUID, guid.ToString());
           
        ////    ///  sends guid to client
        ////    return new ObjectResult(guidMessage);
        ////}

    }
}
