using Classes;
using ProjectJson.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ProjectJson.DAOs;
using ProjectJson.Models.Project;

namespace ProjectJson.Controllers
{
    public class ProjectController : ApiController
    {
        [HttpGet]
        [Route("Project/Projects")]
        public IList<Project> getProjects()
        {
            var projectDAO = new ProjectDAO();
            var projects = projectDAO.getProjects();
            projectDAO.Dispose();
            return projects;
        }

        [HttpGet]
        [Route("Project/Project/{subproject}/{delivery}")]
        public Project getProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var project = projectDAO.getProject(subproject, delivery);
            projectDAO.Dispose();
            return project;
        }

        [HttpGet]
        [Route("Project/ProjectFull/{subproject}/{delivery}")]
        public ProjectFull getProjectFull(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var projectFull = projectDAO.getProjectFull(subproject, delivery);
            projectDAO.Dispose();
            return projectFull;
        }

        #region DefectsDensity

            [HttpGet]
            [Route("Project/DefectsDensity/{subproject}/{delivery}")]
            public DefectDensity getDefectsDensityByProject(string subproject, string delivery)
            {
                var projectDAO = new ProjectDAO();
                var densityDefects = projectDAO.getDefectsDensityByProject(subproject, delivery);
                projectDAO.Dispose();
                return densityDefects;
            }

            [HttpGet]
            [Route("Project/DefectsAverangeTime/{subproject}/{delivery}")]
            public DefectAverangeTime getDefectsAverageTimeByProject(string subproject, string delivery)
            {
                var projectDAO = new ProjectDAO();
                var defectAverangeTime = projectDAO.getDefectsAverageTimeByProject(subproject, delivery);
                projectDAO.Dispose();
                return defectAverangeTime;
            }

            [HttpGet]
            [Route("Project/DefectsAverangeTimeGroupSeverity/{subproject}/{delivery}")]
            public IList<DefectAverangeTimeGroupSeverity> getDefectsAverangeTimeGroupSeverityByProject(string subproject, string delivery)
            {
                var projectDAO = new ProjectDAO();
                var defectAverangeTimeGroupSeverity = projectDAO.getDefectAverangeTimeGroupSeverityByProject(subproject, delivery);
                projectDAO.Dispose();
                return defectAverangeTimeGroupSeverity;
            }

        #endregion

        [HttpGet]
        [Route("Project/DefectReopened/{subproject}/{delivery}")]
        public DefectReopened getDefectReopenedByProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var densityDefects = projectDAO.getDefectReopenedByProject(subproject, delivery);
            projectDAO.Dispose();
            return densityDefects;
        }

        [HttpGet]
        [Route("Project/DetectableInDev/{subproject}/{delivery}")]
        public DetectableInDev getDetectableInDevByProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var detectableInDev = projectDAO.getDetectableInDevByProject(subproject, delivery);
            projectDAO.Dispose();
            return detectableInDev;
        }

        [HttpGet]
        [Route("Project/StatusLastDays/{subproject}/{delivery}")]
        public StatusLastDays getStatusLastDaysByProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var statusLastDays = projectDAO.getStatusLastDaysByProject(subproject, delivery);
            projectDAO.Dispose();

            return statusLastDays;
        }

        [HttpGet]
        [Route("Project/StatusGroupMonth/{subproject}/{delivery}")]
        public IList<Status> getStatusGroupMonthByProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var statusLastMonthByProject = projectDAO.getStatusLastMonthByProject(subproject, delivery);
            projectDAO.Dispose();

            return statusLastMonthByProject;
        }

        [HttpGet]
        [Route("Project/DefectStatus/{subproject}/{delivery}")]
        public IList<DefectStatus> getDefectStatusByProject(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var DefectStatusByProject = projectDAO.getDefectStatusByProject(subproject, delivery);
            projectDAO.Dispose();

            return DefectStatusByProject;
        }

        [HttpGet]
        [Route("Project/DefectsGroupOrigin/{subproject}/{delivery}")]
        public IList<DefectStatus> getDefectsGroupOrigin(string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var defectsGroupOrigin = projectDAO.getDefectsGroupOrigin(subproject, delivery);
            projectDAO.Dispose();

            return defectsGroupOrigin;
        }

    }
}