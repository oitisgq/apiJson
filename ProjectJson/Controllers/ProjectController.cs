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
using ProjectJson.Models.Project;
using System.Collections;
using System.Web.Http.Description;

namespace ProjectJson.Controllers
{
    // [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = false)]
    public class ProjectController : ApiController
    {
        [HttpGet]
        [Route("Project/Projects")]
        [ResponseType(typeof(IList<Project>))]
        public HttpResponseMessage getProjects(HttpRequestMessage request)
        {
            var projectDAO = new ProjectDAO();
            var projects = projectDAO.getProjects();
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, projects);
        }

        [HttpGet]
        [Route("Project/ProjectsByIds/{ids}")]
        [ResponseType(typeof(IList<Project>))]
        public HttpResponseMessage getProjectsByIds(HttpRequestMessage request, string ids)
        {
            var projectDAO = new ProjectDAO();
            var projects = projectDAO.getProjectsByIds(ids);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, projects);
        }

        [HttpPut]
        [Route("Project/Project/{id:int}")] 
        public HttpResponseMessage UpdateProject(int id, project item)
        {
            try
            {
                var connection = new Connection(Bancos.Sgq);

                if (item.trafficLight == null)
                    item.trafficLight = "";

                if (item.rootCause == null)
                    item.rootCause = "";

                if (item.actionPlan == null)
                    item.actionPlan = "";

                if (item.informative == null)
                    item.informative = "";

                if (item.attentionPoints == null)
                    item.attentionPoints = "";

                if (item.attentionPointsOfIndicators == null)
                    item.attentionPointsOfIndicators = "";

                bool resultado = false;
                if (item == null) throw new ArgumentNullException("item");
                if (id == 0) throw new ArgumentNullException("id");
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection.connection;
                    command.CommandText = @"
                        update sgq_projetos
                        set
                            Farol = @trafficLight,
                            Causa_Raiz = @rootCause,
                            Plano_Acao = @actionPlan,
                            Informativo = @informative,
                            Pontos_Atencao = @attentionPoints,
                            Pontos_Atencao_Indicadores = @attentionPointsOfIndicators
                        where
                            id = @id";

                    command.Parameters.AddWithValue("id", item.id);
                    command.Parameters.AddWithValue("trafficLight", item.trafficLight);
                    command.Parameters.AddWithValue("rootCause", item.rootCause);
                    command.Parameters.AddWithValue("actionPlan", item.actionPlan);
                    command.Parameters.AddWithValue("informative", item.informative);
                    command.Parameters.AddWithValue("attentionPoints", item.attentionPoints);
                    command.Parameters.AddWithValue("attentionPointsOfIndicators", item.attentionPointsOfIndicators);

                    int i = command.ExecuteNonQuery();
                    resultado = i > 0;
                }
                connection.Dispose();
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }

        [HttpGet]
        [Route("Project/Project/{subproject}/{delivery}")]
        [ResponseType(typeof(Project))]
        public HttpResponseMessage getProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var project = projectDAO.getProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, project);
        }

        /*
        [HttpGet]
        [Route("Project/ProjectFull/{subproject}/{delivery}")]
        [ResponseType(typeof(ProjectFull))]
        public HttpResponseMessage getProjectFull(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var projectFull = projectDAO.getProjectFull(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, projectFull);
        }
        */

        [HttpGet]
        [Route("Project/DefectsDensity/{subproject}/{delivery}")]
        [ResponseType(typeof(DefectDensity))]
        public HttpResponseMessage getDefectsDensityByProject(HttpRequestMessage request, string subproject, string delivery )
        {
            var projectDAO = new ProjectDAO();
            var densityDefects = projectDAO.getDefectsDensityByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, densityDefects);
        }

        [HttpGet]
        [Route("Project/DefectsAverangeTime/{subproject}/{delivery}")]
        [ResponseType(typeof(DefectAverangeTime))]
        public HttpResponseMessage getDefectsAverageTimeByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var defectAverangeTime = projectDAO.getDefectsAverageTimeByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, defectAverangeTime);
        }

        [HttpGet]
        [Route("Project/DefectsAverangeTime/{subproject}/{delivery}/{severity}")]
        [ResponseType(typeof(DefectAverangeTime))]
        public HttpResponseMessage getDefectsAverageTimeByProject(HttpRequestMessage request, string subproject, string delivery, string severity)
        {
            var projectDAO = new ProjectDAO();
            var defectAverangeTime = projectDAO.getDefectsAverageTimeByProject(subproject, delivery, severity);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, defectAverangeTime);
        }

        [HttpGet]
        [Route("Project/DefectsAverangeTimeGroupSeverity/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectAverangeTimeGroupSeverity>))]
        public HttpResponseMessage getDefectsAverangeTimeGroupSeverityByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var defectAverangeTimeGroupSeverity = projectDAO.getDefectAverangeTimeGroupSeverityByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, defectAverangeTimeGroupSeverity);
        }

        [HttpGet]
        [Route("Project/DefectsReopened/{subproject}/{delivery}")]
        [ResponseType(typeof(DefectReopened))]
        public HttpResponseMessage getDefectReopenedByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var densityDefects = projectDAO.getDefectReopenedByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, densityDefects);
        }

        [HttpGet]
        [Route("Project/DefectsDetectableInDev/{subproject}/{delivery}")]
        [ResponseType(typeof(DetectableInDev))]
        public HttpResponseMessage getDetectableInDevByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var detectableInDev = projectDAO.getDetectableInDevByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, detectableInDev);
        }

        [HttpGet]
        [Route("Project/StatusLastDays/{subproject}/{delivery}")]
        [ResponseType(typeof(StatusLastDays))]
        public HttpResponseMessage getStatusLastDaysByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var statusLastDays = projectDAO.getStatusLastDaysByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, statusLastDays);
        }

        [HttpGet]
        [Route("Project/StatusGroupMonth/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<Status>))]
        public HttpResponseMessage getStatusGroupMonthByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getStatusLastMonthByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/DefectsStatus/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectStatus>))]
        public HttpResponseMessage getDefectStatusByProject(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectStatusByProject(subproject, delivery);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/DefectsGroupOrigin/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectStatus>))]
        public HttpResponseMessage getDefectsGroupOrigin(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsGroupOrigin(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/CtsImpactedXDefects/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<CtsImpactedXDefects>))]
        public HttpResponseMessage getCtsImpactedXDefects(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getCtsImpactedXDefects(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/DefectsOpenInDevManuf/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectsOpen>))]
        public HttpResponseMessage getDefectsOpenInDevManuf(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsOpenInDevManuf(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/DefectsOpenInTestManuf/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectsOpen>))]
        public HttpResponseMessage getDefectsOpenInTestManuf(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsOpenInTestManuf(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/ProductivityXDefects/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<ProductivityXDefects>))]
        public HttpResponseMessage getProductivityXDefects(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getProductivityXDefects(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/ProductivityXDefectsGroupWeekly/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<ProductivityXDefectsGroupWeekly>))]
        public HttpResponseMessage getProductivityXDefectsGroupWeekly(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getProductivityXDefectsGroupWeekly(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        // ITERATIONS

        [HttpGet]
        [Route("Project/Iterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<iteration>))]
        public HttpResponseMessage getIterations(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getIterations(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }


        [HttpGet]
        [Route("Project/IterationsActive/{subproject}/{delivery}")]
        [ResponseType(typeof(List<string>))]
        public HttpResponseMessage getIterationsActive(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getIterationsActive(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpGet]
        [Route("Project/IterationsSelected/{subproject}/{delivery}")]
        [ResponseType(typeof(List<string>))]
        public HttpResponseMessage getIterationsSelected(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getIterationsSelected(subproject, delivery);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/UpdateIterationsActive/{id:int}")]
        [ResponseType(typeof(Boolean))]
        public HttpResponseMessage UpdateIterationsActive(HttpRequestMessage request, int id, IList<string> iterations)
        {
            try
            {
                var connection = new Connection(Bancos.Sgq);

                bool resultado = false;
                if (iterations == null) throw new ArgumentNullException("iterations");
                if (id == 0) throw new ArgumentNullException("id");
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection.connection;
                    command.CommandText = @"
                        update sgq_projetos
                        set
                            IterationsActive = @iterations
                        where
                            id = @id";
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("iterations", string.Join("','", iterations));

                    int i = command.ExecuteNonQuery();
                    resultado = i > 0;
                }
                connection.Dispose();
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpPut]
        [Route("Project/UpdateIterationsSelected/{id:int}")]
        [ResponseType(typeof(Boolean))]
        public HttpResponseMessage UpdateIterationsSelected(HttpRequestMessage request, int id, IList<string> iterations)
        {
            try
            {
                var connection = new Connection(Bancos.Sgq);

                bool resultado = false;
                if (iterations == null) throw new ArgumentNullException("iterations");
                if (id == 0) throw new ArgumentNullException("id");
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection.connection;
                    command.CommandText = @"
                        update sgq_projetos
                        set
                            IterationsSelected = @iterations
                        where
                            id = @id";
                    command.Parameters.AddWithValue("id", id);
                    command.Parameters.AddWithValue("iterations", string.Join("','", iterations));

                    int i = command.ExecuteNonQuery();
                    resultado = i > 0;
                }
                connection.Dispose();
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpGet]
        [Route("Project/ClearIterations/{id:int}")]
        [ResponseType(typeof(string))]
        public HttpResponseMessage ClearIterations(HttpRequestMessage request, int id)
        {
            try
            {
                var connection = new Connection(Bancos.Sgq);

                bool resultado = false;
                if (id == 0) throw new ArgumentNullException("id");
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection.connection;
                    command.CommandText = @"
                        update sgq_projetos
                        set
                            iterations = ''
                        where
                            id = @id";
                    command.Parameters.AddWithValue("id", id);

                    int i = command.ExecuteNonQuery();
                    resultado = i > 0;
                }
                connection.Dispose();
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // ----------

        [HttpGet]
        [Route("Project/DefectsDensityByProjectIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(DefectDensity))]
        public HttpResponseMessage getDefectsDensityByProjectIterations(HttpRequestMessage request, string subproject, string delivery)
        {
            var projectDAO = new ProjectDAO();
            List<string> iterations = projectDAO.getIterationsSelected(subproject, delivery);
            var result = projectDAO.getDefectsDensityByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        [Route("Project/DefectsAverangeTimeIterations/{subproject}/{delivery}/{severity}")]
        [ResponseType(typeof(DefectAverangeTime))]
        public HttpResponseMessage getDefectsAverageTimeByProjectIterations(HttpRequestMessage request, string subproject, string delivery, string severity)
        {
            var projectDAO = new ProjectDAO();
            List<string> iterations = projectDAO.getIterationsSelected(subproject, delivery);
            var result = projectDAO.getDefectsAverageTimeByProjectIterations(subproject, delivery, severity, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPut]
        [Route("Project/DefectsReopenedIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(DefectReopened))]
        public HttpResponseMessage getDefectReopenedByProjectIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var item = projectDAO.getDefectReopenedByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, item);
        }

        [HttpPut]
        [Route("Project/DefectsDetectableInDevIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(DetectableInDev))]
        public HttpResponseMessage getDetectableInDevByProjectIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var item = projectDAO.getDetectableInDevByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, item);
        }

        [HttpPut]
        [Route("Project/StatusLastDaysIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(StatusLastDays))]
        public HttpResponseMessage getStatusLastDaysByProjectIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var item = projectDAO.getStatusLastDaysByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, item);
        }

        [HttpPut]
        [Route("Project/StatusGroupMonthIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<Status>))]
        public HttpResponseMessage getStatusGroupMonthByProjectIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getStatusGroupMonthByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/DefectsStatusIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectStatus>))]
        public HttpResponseMessage getDefectStatusByProjectIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectStatusByProjectIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/DefectsGroupOriginIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectStatus>))]
        public HttpResponseMessage getDefectsGroupOriginIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsGroupOriginIterations(subproject, delivery, iterations);
            projectDAO.Dispose();
            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/CtsImpactedXDefectsIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<CtsImpactedXDefects>))]
        public HttpResponseMessage getCtsImpactedXDefectsIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getCtsImpactedXDefectsIterations(subproject, delivery, iterations);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/DefectsOpenInDevManufIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectsOpen>))]
        public HttpResponseMessage getDefectsOpenInDevManufIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsOpenInDevManufIterations(subproject, delivery, iterations);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/DefectsOpenInTestManufIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<DefectsOpen>))]
        public HttpResponseMessage getDefectsOpenInTestManufIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getDefectsOpenInTestManufIterations(subproject, delivery, iterations);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/ProductivityXDefectsIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<ProductivityXDefects>))]
        public HttpResponseMessage getProductivityXDefectsIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getProductivityXDefectsIterations(subproject, delivery, iterations);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

        [HttpPut]
        [Route("Project/ProductivityXDefectsGroupWeeklyIterations/{subproject}/{delivery}")]
        [ResponseType(typeof(IList<ProductivityXDefectsGroupWeekly>))]
        public HttpResponseMessage getProductivityXDefectsGroupWeeklyIterations(HttpRequestMessage request, string subproject, string delivery, List<string> iterations)
        {
            var projectDAO = new ProjectDAO();
            var list = projectDAO.getProductivityXDefectsGroupWeeklyIterations(subproject, delivery, iterations);
            projectDAO.Dispose();

            return request.CreateResponse(HttpStatusCode.OK, list);
        }

    }
}