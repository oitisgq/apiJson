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
    public class ProjectXGrouperController : ApiController
    {
        [HttpGet]
        [Route("ProjectXGrouper/All")]
        [ResponseType(typeof(IList<ProjectXGrouper>))]
        public HttpResponseMessage getProjectXGrouper(HttpRequestMessage request)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            var list = ProjectXGrouperDAO.GetAll();
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("ProjectXGrouper/ByProject/{id}")]
        [ResponseType(typeof(IList<ProjectXGrouper>))]
        public HttpResponseMessage getByProject(HttpRequestMessage request, string id)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            var list = ProjectXGrouperDAO.GetByProject(id);
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("ProjectXGrouper/ByProject/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<ProjectXGrouper>))]
        public HttpResponseMessage getProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            var list = ProjectXGrouperDAO.GetByProject(subproject, delivery);
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("ProjectXGrouper/ByGrouper/{id}")]
        [ResponseType(typeof(IList<ProjectXGrouper>))]
        public HttpResponseMessage getByGrouper(HttpRequestMessage request, string id)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            var list = ProjectXGrouperDAO.GetByGroup(id);
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("ProjectXGrouper/Create/{GrouperId}/{ProjectId}/{Subproject}/{Delivery}")]
        public HttpResponseMessage CreateProjectXGrouper(HttpRequestMessage request, string GrouperId, string ProjectId, string Subproject, string Delivery)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            ProjectXGrouperDAO.Create(GrouperId, ProjectId, Subproject, Delivery);
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("ProjectXGrouper/Delete/{GrouperId}/{ProjectId}")]
        public HttpResponseMessage DeleteProjectXGrouper(HttpRequestMessage request, string GrouperId, string ProjectId)
        {
            var ProjectXGrouperDAO = new ProjectXGrouperDAO();
            ProjectXGrouperDAO.Delete(GrouperId, ProjectId);
            ProjectXGrouperDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK);
        }

        //[HttpGet]
        //[Route("ProjectXGrouper/One/{id}")]
        //[ResponseType(typeof(ProjectXGrouper))]
        //public HttpResponseMessage getGroups(HttpRequestMessage request, string id)
        //{
        //    var GroupsDAO = new GroupDAO();
        //    var Groups = GroupsDAO.getOne(id);
        //    GroupsDAO.Dispose();
        //    return request.CreateResponse(HttpStatusCode.OK, Groups);
        //}

    }
}