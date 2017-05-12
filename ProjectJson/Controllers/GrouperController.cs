using Classes;
using ProjectJson.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

using ProjectJson.DAOs;
using System.Collections;
using System.Web.Http.Description;

namespace ProjectJson.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = false)]
    public class GrouperController : ApiController
    {
        [HttpGet]
        [Route("Groupers")]
        [ResponseType(typeof(IList<Grouper>))]
        public HttpResponseMessage getGroupers(HttpRequestMessage request)
        {
            var GrouperDAO = new GrouperDAO();
            var Groupers = GrouperDAO.getAll();
            GrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, Groupers);
        }

        [HttpGet]
        [Route("Grouper/{id}")]
        [ResponseType(typeof(string))]
        public HttpResponseMessage getGroupers(HttpRequestMessage request, string id)
        {
            var GroupersDAO = new GrouperDAO();
            var Groupers = GroupersDAO.get(id);
            GroupersDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, Groupers);
        }

        [HttpPut]
        [Route("Grouper/create")]
        [ResponseType(typeof(Grouper))]
        public HttpResponseMessage UpdatetGrouper(HttpRequestMessage request, Grouper Grouper)
        {
            var GrouperDAO = new GrouperDAO();
            var createdItem = GrouperDAO.Create(Grouper);
            GrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, createdItem);
        }

        [HttpPut]
        [Route("Grouper/update/{id}")]
        public HttpResponseMessage UpdatetGrouper(HttpRequestMessage request, string id, Grouper Grouper)
        {
            var GrouperDAO = new GrouperDAO();
            GrouperDAO.Update(id, Grouper);
            GrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("Grouper/{id}")]
        public HttpResponseMessage Delete(HttpRequestMessage request, int id)
        {
            var GrouperDAO = new GrouperDAO();
            GrouperDAO.Delete(id);
            GrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK);
        }

    }
}