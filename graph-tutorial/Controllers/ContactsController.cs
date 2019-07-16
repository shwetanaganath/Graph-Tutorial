﻿using graph_tutorial.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace graph_tutorial.Controllers
{
    public class ContactsController : BaseController
    {
        // GET: Contacts
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var contact = await GraphHelper.CreateContact();
            

            return View(contact);
        }
    }
}