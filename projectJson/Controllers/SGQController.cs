using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using projectJson.Models;
using Classes;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Diagnostics;

namespace projectJson.Controllers
{

    public class SGQController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public string ping()
        {
            return "{value:1}";
        }

        [HttpGet]
        [Route("devManufacturers")]
        public List<devManufacturers> GetdevManufacturers()
        {
            string sql = @"
                select distinct
	                --'{' +
	                --	'id:''' + id + '''' +
	                --'}, ' as json,
                    id
                from
                    (select distinct
		                fabrica_desenvolvimento as id
                    from
                        alm_cts
                    where
                        fabrica_desenvolvimento is not null

                    union all

                    select distinct
		                fabrica_desenvolvimento as id
                    from
                        alm_defeitos
                    where
                        fabrica_desenvolvimento is not null
	                ) aux
                where
                    id <> '' and 
                    id <> 'OMS'
                order by
                    1
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<devManufacturers> ListDevManufacturers = Connection.Executar<devManufacturers>(sql);

            //DataTable.AsEnumerable().Select(x => x[0].ToString()).ToList();
            return ListDevManufacturers;
        }

        [HttpGet]
        [Route("systems")]
        public List<systems> GetSystems()
        {
            string sql = @"
                select distinct
	                --'{ ' +
	                --	'id: ''' + id + ''', devManufacturing: ''' + devManufacturing + '''' +
	                --' }, ' as json,
                    id,
	                devManufacturing
                from
                    (select distinct
		                sistema as id,
		                fabrica_desenvolvimento as devManufacturing
                    from
                        alm_cts
                    where
		                sistema is not null and
                        fabrica_desenvolvimento is not null

                    union all

                    select distinct
		                sistema_defeito as id,
		                fabrica_desenvolvimento as devManufacturing
                    from
                        alm_defeitos
                    where
		                sistema_defeito is not null and
                        fabrica_desenvolvimento is not null
	                ) aux
                where
                    id <> ''
                order by
                    1
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<systems> ListSystems = Connection.Executar<systems>(sql);

            return ListSystems;
        }

        [HttpGet] // DEVERA SAIR, QUANDO IMPLATADO EM PRODUCAO
        [Route("projects")]
        public List<projects> GetProjects()
        {
            string sql = @"
                select distinct
	                '{' +
	                'id:''' + convert(varchar, cast(substring(re.Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(re.Entrega,8,8) as int)) + ''', ' +
	                'subproject:''' + re.Subprojeto + ''', ' +
	                'delivery:''' + re.Entrega + ''', ' +
	                'devManufacturing:''' + Fabrica_Desenvolvimento + ''', ' +
	                'system:''' + Sistema + ''', ' +
	                'name:''' + left(sp.Nome,30) + ''', ' +
	                'classification:''' + sp.Classificacao_Nome + ''', ' +
	                'release:''' + (select Sigla from sgq_meses m where m.id = re.release_mes) + ' ' + convert(varchar, re.release_ano) + '''' +
	                '}, ' as json,
	                convert(varchar, cast(substring(re.Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(re.Entrega,8,8) as int)) as id,
	                Fabrica_Desenvolvimento as devManufacturing,
	                Sistema as system,
	                re.Subprojeto as subproject,
	                re.Entrega as delivery,
	                sp.nome as name,
	                sp.Classificacao_Nome as classification,
	                (select Sigla from sgq_meses m where m.id = re.release_mes) + ' ' + convert(varchar, re.release_ano)  as release,
	                re.release_ano,
	                re.release_mes
                from 
	                SGQ_Releases_Entregas re WITH (NOLOCK)
	                inner join biti_Subprojetos sp WITH (NOLOCK)
		                on sp.id = re.Subprojeto
                    inner join biti_usuarios us WITH (NOLOCK)
		                on us.id = sp.lider_tecnico_id 
                    inner join 
						(
						select distinct
							Subprojeto, Entrega, fabrica_desenvolvimento, sistema
						from
							(
							select distinct 
								cts.Subprojeto,
								cts.Entrega,
								cts.Fabrica_Desenvolvimento,
								cts.sistema
							from 
								alm_cts cts WITH (NOLOCK)
							union all
							select distinct 
								d.Subprojeto,
								d.Entrega,
								d.Fabrica_Desenvolvimento,
								d.sistema_defeito as sistema
							from 
								alm_defeitos d WITH (NOLOCK)
							) aux
						) cts 
						on cts.Subprojeto = re.Subprojeto and
							cts.Entrega = re.Entrega
                where
	                re.id = (select top 1 re2.id from  SGQ_Releases_Entregas re2 where re2.subprojeto = re.subprojeto and re2.entrega = re.entrega order by re2.release_ano desc, re2.release_mes desc) and
	                Fabrica_Desenvolvimento is not null
                order by
	                Fabrica_Desenvolvimento,
	                Sistema,
	                re.Subprojeto,
	                re.Entrega,
	                sp.Classificacao_Nome,
	                re.release_ano,
	                re.release_mes
                ";
            var Connection = new Connection(Bancos.Sgq);

            List<projects> ListProjects = Connection.Executar<projects>(sql);

            return ListProjects;
        }


        [HttpGet]
        [Route("projects_")] // DEVERA ALTERAR O NOME QUANDO IMPLATADO EM PRODUCAO
        public List<project> GetProjects_()
        {
            string sql = @"
                select 
                    sgq_projetos.id,
                    sgq_projetos.subprojeto as subproject,
                    sgq_projetos.entrega as delivery,
                    convert(varchar, cast(substring(sgq_projetos.subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(sgq_projetos.entrega,8,8) as int)) as subprojectDelivery,
                    biti_subprojetos.nome as name,
					biti_subprojetos.classificacao_nome as classification,
					replace(replace(replace(replace(replace(biti_subprojetos.estado,'CONSOLIDAÇÃO E APROVAÇÃO DO PLANEJAMENTO','CONS/APROV. PLAN'),'PLANEJAMENTO','PLANEJ.'),'DESENHO DA SOLUÇÃO','DES.SOL'),'VALIDAÇÃO','VALID.'),'AGUARDANDO','AGUAR.') as state,
					(select Sigla from sgq_meses m where m.id = SGQ_Releases_Entregas.release_mes) + ' ' + convert(varchar, SGQ_Releases_Entregas.release_ano) as release,
					biti_subprojetos.Gerente_Projeto as GP,
			        biti_subprojetos.Gestor_Do_Gestor_LT as N3,
                    sgq_projetos.Farol as trafficLight,
                    sgq_projetos.Causa_Raiz as rootCause,
                    sgq_projetos.Plano_Acao as actionPlan,
                    sgq_projetos.Informativo as informative,
                    sgq_projetos.Pontos_Atencao as attentionPoints,
                    sgq_projetos.Pontos_Atencao_Indicadores as attentionPointsOfIndicators
                from 
                    sgq_projetos
                    inner join alm_projetos WITH (NOLOCK)
	                  on alm_projetos.subprojeto = sgq_projetos.subprojeto and
	                    alm_projetos.entrega = sgq_projetos.entrega and
	                    alm_projetos.ativo = 'Y'
                    left join biti_subprojetos WITH (NOLOCK)
	                  on biti_subprojetos.id = sgq_projetos.subprojeto
					left join SGQ_Releases_Entregas WITH (NOLOCK)
	                  on SGQ_Releases_Entregas.subprojeto = sgq_projetos.subprojeto and
					     SGQ_Releases_Entregas.entrega = sgq_projetos.entrega and
						 SGQ_Releases_Entregas.id = (select top 1 re2.id from SGQ_Releases_Entregas re2 
						                             where re2.subprojeto = SGQ_Releases_Entregas.subprojeto and 
													       re2.entrega = SGQ_Releases_Entregas.entrega 
													 order by re2.release_ano desc, re2.release_mes desc)
                order by 
                    sgq_projetos.subprojeto, 
                    sgq_projetos.entrega
                ";
            var Connection = new Connection(Bancos.Sgq);

            List<project> ListProjects = Connection.Executar<project>(sql);

            return ListProjects;
        }

        [HttpPut]
        [Route("projects_/{id:int}")] // DEVERA ALTERAR O NOME QUANDO IMPLATADO EM PRODUCAO
        public HttpResponseMessage UpdateProject(int id, project item)
        {
            try
            {
                var connection = new Connection(Bancos.Sgq);

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
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpGet]
        [Route("iterations/{subproject}/{delivery}")]
        public List<iteration> GetIterationByProject(string subproject, string delivery)
        {
            string sql = @"
                select distinct 
	                upper(iterations) as Name
                from 
	                alm_cts 
                where
	                iterations <> '' and
	                status_exec_teste <> 'CANCELLED' and 
	                status_exec_ct <> 'CANCELLED' and
	                subprojeto = '@subproject' and
	                entrega = '@delivery'
                order by 
	                1
                ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);

            List<iteration> List = Connection.Executar<iteration>(sql);

            return List;
        }


        [HttpGet]
        [Route("defectsDensity")]
        public List<densityDefects> GetDensity()
        {
            string sql = @"
                select 
	                --'{' +
	                --'date:''' + monthExecution + '/' + yearExecution + ''', ' +
	                --'devManufacturing:''' + devManufacturing + ''', ' +
	                --'system:''' + system + ''', ' +
	                --'project:''' + convert(varchar, cast(substring(subproject,4,8) as int)) + ' ' + convert(varchar,cast(substring(delivery,8,8) as int)) + ''', ' +
	                --'subproject:''' + subproject + ''', ' +
	                --'delivery:''' + delivery + ''', ' +
	                --'qtyDefects:''' + convert(varchar,sum(qte_defeitos)) + ''', ' +
	                --'qtyCTs:''' + convert(varchar,count(*)) + ''', ' +
	                --'density:''' + convert(varchar,round(convert(float,sum(qte_defeitos)) / (case when count(*) = 0 then 1 else count(*) end) * 100,2)) + '''' +
	                --'}, ' as json,
	                monthExecution + '/' + yearExecution as date,
	                devManufacturing,
	                system,
	                convert(varchar, cast(substring(subproject,4,8) as int)) + ' ' + convert(varchar,cast(substring(delivery,8,8) as int)) as project,
	                subproject,
	                delivery,
	                sum(qte_defeitos) as qtyDefects,
	                count(*) as qtyCTs,
	                round(convert(float,sum(qte_defeitos)) / (case when count(*) = 0 then 1 else count(*) end) * 100,2) as density
                from
	                (select 
		                cts.fabrica_desenvolvimento as devManufacturing,
		                cts.subprojeto as subproject,
		                cts.entrega as delivery,
		                cts.sistema as system,
		                substring(cts.dt_execucao,4,2) as monthExecution,
		                substring(cts.dt_execucao,7,2) as yearExecution,
		                (
		                select count(*) 
		                from alm_defeitos df 
		                where df.subprojeto = cts.subprojeto and
			                    df.entrega = cts.entrega and
			                    df.ct = cts.ct and
			                    df.status_atual = 'CLOSED' and
			                    df.Origem like '%CONSTRUÇÃO%' and
		                        (df.Ciclo like '%TI%' or df.Ciclo like '%UAT%')
		                ) as qte_defeitos
	                from 
		                alm_cts cts
	                where
		                status_exec_ct = 'PASSED' and
		                cts.fabrica_desenvolvimento is not null and
		                cts.massa_Teste <> 'SIM' and
		                (cts.ciclo like '%TI%' or cts.ciclo like '%UAT%') and
                        dt_execucao <> ''
	                ) Aux
                group by
	                devManufacturing,
	                subproject,
	                delivery,
	                devManufacturing, 
	                system,
	                monthExecution,
	                yearExecution
                order by
	                yearExecution,
	                monthExecution,
	                devManufacturing, 
	                system,
	                subproject,
	                delivery
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<densityDefects> List = Connection.Executar<densityDefects>(sql);

            return List;
        }

        [HttpGet]
        [Route("defectsDensity/{subproject}/{delivery}")]
        public List<densityDefects> GetDensityByProject(string subproject, string delivery)
        {
            Debug.WriteLine(subproject);
            
            string sql = @"
                select 
	                monthExecution + '/' + yearExecution as date,
	                devManufacturing,
	                system,
	                convert(varchar, cast(substring(subproject,4,8) as int)) + ' ' + convert(varchar,cast(substring(delivery,8,8) as int)) as project,
	                subproject,
	                delivery,
	                sum(qte_defeitos) as qtyDefects,
	                count(*) as qtyCTs,
	                round(convert(float,sum(qte_defeitos)) / (case when count(*) = 0 then 1 else count(*) end) * 100,2) as density
                from
	                (select 
		                cts.fabrica_desenvolvimento as devManufacturing,
		                cts.subprojeto as subproject,
		                cts.entrega as delivery,
		                cts.sistema as system,
		                substring(cts.dt_execucao,4,2) as monthExecution,
		                substring(cts.dt_execucao,7,2) as yearExecution,
		                (
		                select count(*) 
		                from alm_defeitos df 
		                where df.subprojeto = cts.subprojeto and
			                    df.entrega = cts.entrega and
			                    df.ct = cts.ct and
			                    df.status_atual = 'CLOSED' and
			                    df.Origem like '%CONSTRUÇÃO%' and
		                        (df.Ciclo like '%TI%' or df.Ciclo like '%UAT%')
		                ) as qte_defeitos
	                from 
		                alm_cts cts
	                where
	                    subprojeto = '@subproject' and
	                    entrega = '@delivery' and
		                status_exec_ct not in ('CANCELLED', 'NO RUN') and
		                cts.fabrica_desenvolvimento is not null and
		                cts.massa_Teste <> 'SIM' and
		                (cts.ciclo like '%TI%' or cts.ciclo like '%UAT%') and
                        dt_execucao <> ''
	                ) Aux
                group by
	                devManufacturing,
	                subproject,
	                delivery,
	                devManufacturing, 
	                system,
	                monthExecution,
	                yearExecution
                order by
	                yearExecution,
	                monthExecution,
	                devManufacturing, 
	                system,
	                subproject,
	                delivery
                ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);

            List<densityDefects> List = Connection.Executar<densityDefects>(sql);

            return List;
        }

        /*
        [HttpGet]
        [Route("agingDefects")]
        public List<agingDefects> GetAgingDefects()
        {
            string sql = @"
               	select 
					--'{ ' +
					--'date: ''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
					--'devManufacturing: ''' + fabrica_desenvolvimento + ''', ' +
					--'system: ''' + sistema_defeito + ''', ' +
					--'project: ''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
					--'subproject: ''' + subprojeto + ''', ' +
					--'delivery: ''' + entrega + ''', ' +
					--'qty: ' + convert(varchar,(convert(float, sum(Aging)))) + ',' + 
					--' }, ' as json,
					substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
					fabrica_desenvolvimento as devManufacturing,
					sistema_defeito as system,
					convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
					subprojeto as subproject,
					entrega as delivery,
					sum(Aging) as qty
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento 
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<agingDefects> List = Connection.Executar<agingDefects>(sql);

            return List;
        }
        */

        [HttpGet] // DEVERAR SAIR, QUANDO IMPLANTADO EM PRODUÇÃO
        [Route("DefectsMiddleAges")]
        public List<agingMedioDefects> GetAgingMedio()
        {
            string sql = @"
                select 
					--'{' +
					--'severity:''' + substring(severidade,3,10) + ''', ' +
					--'date:''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
					--'devManufacturing:''' + fabrica_desenvolvimento + ''', ' +
					--'system:''' + sistema_defeito + ''', ' +
					--'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
					--'subproject:''' + subprojeto + ''', ' +
					--'delivery:''' + entrega + ''', ' +
					--'qtyDefects:' + convert(varchar,count(*)) + ',' + 
					--'qtyHours:' + convert(varchar,round(sum(Aging),2)) + ',' + 
					--'Media:' + convert(varchar,round(sum(Aging) / count(*),2)) + 
					--'}, ' as json,
	                substring(severidade,3,10) as severity,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                count(*) as qtyDefects,
	                round(sum(Aging),2) as qtyHours,
	                round(sum(Aging) / count(*),2) as Media
                from 
	                alm_defeitos s
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(severidade,3,10),
	                severidade,
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                severidade,
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento 
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<agingMedioDefects> List = Connection.Executar<agingMedioDefects>(sql);

            return List;
        }


        [HttpGet]
        [Route("DefectsAverangeTime/{subproject}/{delivery}")]
        public List<defectAverangeTime> GetDefectsAverangeTime(string subproject, string delivery)
        {
            string sql = @"
            select 
	            substring(severidade,3,10) as severity,
	            substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	            fabrica_desenvolvimento as devManufacturing,
	            sistema_defeito as system,
	            convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	            subprojeto as subproject,
	            entrega as delivery,
	            count(*) as qtyDefects,
	            round(sum(Aging),2) as qtyHours,
	            round(sum(Aging) / count(*),2) as Media
            from 
	            alm_defeitos s
            where 
	            subprojeto = '@subproject' and
	            entrega = '@delivery' and
	            (ciclo like '%TI%' or ciclo like '%UAT%') and
	            status_atual = 'CLOSED' and
	            dt_final <> ''
            group by
	            substring(severidade,3,10),
	            severidade,
	            substring(dt_final,4,2),
	            substring(dt_final,7,2),
	            subprojeto,
	            entrega,
	            fabrica_desenvolvimento,
	            sistema_defeito
            order by 
	            severidade,
	            substring(dt_final,7,2),
	            substring(dt_final,4,2),
	            fabrica_desenvolvimento 
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);

            List<defectAverangeTime> List = Connection.Executar<defectAverangeTime>(sql);

            return List;
        }


        [HttpGet]
        [Route("defectsWrongClassif")]
        public List<wrongClassif> GetWrongClassificationDefectRate()
        {
            string sql = @"
                select 
	                --'{' +
	                --'date:''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''',' +
	                --'devManufacturing:''' + 
	                --	case when Aud_FD_Ofensora is not null 
	                --			then Aud_FD_Ofensora 
	                --			else fabrica_desenvolvimento 
	                --	end + ''',' +
	                --'system:''' + sistema_defeito + ''',' +
	                --'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''',' +
	                --'subproject:''' + subprojeto + ''',' +
	                --'delivery:''' + entrega + ''',' +
	                --'qtyTotal:' + convert(varchar,count(*)) + ',' + 
	                --'qty:' + 
	                --	convert(varchar,
	                --		sum(
	                --			case when 
	                --					Aud_Regra_Infringida is not null and
	                --					Aud_FD_Ofensora is not null
	                --				then 1
	                --				else 0
	                --			end	
	                --		)
	                --	) + ',' +
	                --'percentReference:5,' +
	                --'qtyReference:' + convert(varchar,round(convert(float,count(*) * 0.05),2)) + 
	                --'},' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,

	                case when Aud_FD_Ofensora is not null 
			                then Aud_FD_Ofensora 
			                else fabrica_desenvolvimento 
	                end as devManufacturing,

	                sistema_defeito as system,
	                convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                count(*) as qtyTotal,
	                sum(
		                case when 
				                Aud_Regra_Infringida is not null and
				                Aud_FD_Ofensora is not null
			                then 1
			                else 0
		                end	
	                ) as qty,
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and 
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                case when Aud_FD_Ofensora is not null 
			                then Aud_FD_Ofensora 
			                else fabrica_desenvolvimento 
	                end,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                case when Aud_FD_Ofensora is not null 
			                then Aud_FD_Ofensora 
			                else fabrica_desenvolvimento 
	                end
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<wrongClassif> List = Connection.Executar<wrongClassif>(sql);

            return List;
        }



        [HttpGet]
        [Route("defectsDetectableInDev")]
        public List<detectableInDev> GetDetectableInDev()
        {
            string sql = @"
                select 
	                --'{' +
	                --'date:''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
	                --'devManufacturing:''' + fabrica_desenvolvimento + ''', ' +
	                --'system:''' + sistema_defeito + ''', ' +
	                --'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
	                --'subproject:''' + subprojeto + ''', ' +
	                --'delivery:''' + entrega + ''', ' +
	                --'qtyTotal:' + convert(varchar,count(*)) + ',' + 
	                --'qty:' + 
	                --	convert(varchar,
	                --		sum(
	                --			case when Erro_Detectavel_Em_Desenvolvimento = 'SIM'
	                --				then 1
	                --				else 0
	                --			end	
	                --		)
	                --	) + ',' +
	                --'percentReference:5,' +
	                --'qtyReference:' + convert(varchar,round(convert(float,count(*) * 0.05),2)) + 
	                --'}, ' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                count(*) as qtyTotal,
	                sum(
		                case when Erro_Detectavel_Em_Desenvolvimento = 'SIM'
			                then 1
			                else 0
		                end	
	                ) as qty,
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and 
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<detectableInDev> List = Connection.Executar<detectableInDev>(sql);

            return List;
        }

        [HttpGet]
        [Route("defectsDetectableInDev/{subproject}/{delivery}")]
        public List<defectDetectableInDev> GetDetectableInDev(string subproject, string delivery)
        {
            string sql = @"
                select 
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                count(*) as qtyTotal,
	                sum(
		                case when Erro_Detectavel_Em_Desenvolvimento = 'SIM'
			                then 1
			                else 0
		                end	
	                ) as qtyDetectable,
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and 
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);

            List<defectDetectableInDev> List = Connection.Executar<defectDetectableInDev>(sql);

            return List;
        }



        [HttpGet]
        [Route("defectsReopened")]
        public List<reopenedDefects> GetReopened()
        {
            string sql = @"
                select
	                --'{' +   'date: ''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
	                --'devManufacturing:''' + fabrica_desenvolvimento + ''', ' +
	                --'system:''' + sistema_defeito + ''', ' +
	                --'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
	                --'subproject:''' + subprojeto + ''', ' +
	                --'delivery:''' + entrega + ''', ' +
	                --'qtyTotal:'  + '' + convert(varchar, count(*)) + ', ' +
	                --'qty:'  + '' + convert(varchar, sum(qtd_reopen)) + ', ' +
	                --'percent:'  + '' + convert(varchar, round(convert(float,sum(qtd_reopen)) / count(*) * 100,2)) + ', ' +
	                --'percentReference:5, ' +
	                --'qtyReference:'  + '' + convert(varchar, round(convert(float,count(*) * 0.05),2)) + 
	                --'}, ' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                entrega as delivery,
	                subprojeto as subproject,
	                count(*) as qtyTotal,
	                sum(qtd_reopen) as qty,
	                round(convert(float,sum(qtd_reopen)) / count(*) * 100,2) as [percent],
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<reopenedDefects> List = Connection.Executar<reopenedDefects>(sql);

            return List;
        }


        [HttpGet]
        [Route("defectsReopened/{subproject}/{delivery}")]
        public List<defectReopened> GetReopened(string subproject, string delivery)
        {
            string sql = @"
                select
	                --'{' +   'date: ''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
	                --'devManufacturing:''' + fabrica_desenvolvimento + ''', ' +
	                --'system:''' + sistema_defeito + ''', ' +
	                --'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
	                --'subproject:''' + subprojeto + ''', ' +
	                --'delivery:''' + entrega + ''', ' +
	                --'qtyTotal:'  + '' + convert(varchar, count(*)) + ', ' +
	                --'qty:'  + '' + convert(varchar, sum(qtd_reopen)) + ', ' +
	                --'percent:'  + '' + convert(varchar, round(convert(float,sum(qtd_reopen)) / count(*) * 100,2)) + ', ' +
	                --'percentReference:5, ' +
	                --'qtyReference:'  + '' + convert(varchar, round(convert(float,count(*) * 0.05),2)) + 
	                --'}, ' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                entrega as delivery,
	                subprojeto as subproject,
	                count(*) as qtyTotal,
	                sum(qtd_reopen) as qtyReopened,
	                round(convert(float,sum(qtd_reopen)) / count(*) * 100,2) as [percent],
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);

            List<defectReopened> List = Connection.Executar<defectReopened>(sql);

            return List;
        }



        /*
        [HttpGet]
        [Route("ReincidenciaDefeitos")]
        public List<recurrenceDefects> GetRecurrenceDefects()
        {
            string sql = @"
      	          select 
				    '{ ' +   'closed: ''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
					'project: ''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
	                'subproject: ''' + subprojeto + ''', ' +
	                'delivery: ''' + entrega + ''', ' +
	                'devManufacturing: ''' + fabrica_desenvolvimento + ''', ' +
	                'system: ''' + sistema_defeito + ''', ' +
					'qtyRecurrence:'  + '' + convert(varchar, sum(qtd_reincidencia)) +  + 
	                ' }, ' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as closed,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                sum(qtd_reincidencia) as qtyRecurrence
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<recurrenceDefects> List = Connection.Executar<recurrenceDefects>(sql);

            return List;
        }
        */


        [HttpGet]
        [Route("defectsNoPrediction")]
        public List<noPredictionDefects> GetnoPredictionDefects()
        {
            string sql = @"
                select
	                --'{' +   'date: ''' + substring(dt_final,4,2) + '/' + substring(dt_final,7,2) + ''', ' +
	                --'devManufacturing:''' + fabrica_desenvolvimento + ''', ' +
	                --'system:''' + sistema_defeito + ''', ' +
	                --'project:''' + convert(varchar, cast(substring(subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(entrega,8,8) as int)) + ''', ' +
	                --'subproject:''' + subprojeto + ''', ' +
	                --'delivery:''' + entrega + ''', ' +
	                --'qtyTotal:'  + '' + convert(varchar, count (*)) + ''', ' +
	                --'qty:'  + '' + convert(varchar, sum(case when Dt_Prevista_Solucao_Defeito <> '' then 0 else 1 end)) + ''', ' +
	                --'percent:'  + '' + convert(varchar, round(convert(float,sum(case when Dt_Prevista_Solucao_Defeito <> '' then 0 else 1 end)) / count(*) * 100,2)) + ''', ' +
	                --'percentReference:5, ' +
	                --'qtyReference:'  + '' + convert(varchar, round(convert(float,count(*) * 0.05),2)) + 
	                --'}, ' as json,
	                substring(dt_final,4,2) + '/' + substring(dt_final,7,2) as date,
	                fabrica_desenvolvimento as devManufacturing,
	                sistema_defeito as system,
	                convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	                subprojeto as subproject,
	                entrega as delivery,
	                count (*) as qtyTotal,
	                sum(case when Dt_Prevista_Solucao_Defeito <> '' then 0 else 1 end) as qty,
	                round(convert(float,sum(case when Dt_Prevista_Solucao_Defeito = '' then 1 else 0 end)) / count(*) * 100,2) as [percent],
	                5 as percentReference,
	                round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
	                alm_defeitos 
                where 
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                substring(dt_final,4,2),
	                substring(dt_final,7,2),
	                subprojeto,
	                entrega,
	                fabrica_desenvolvimento,
	                sistema_defeito
                order by 
	                substring(dt_final,7,2),
	                substring(dt_final,4,2),
	                fabrica_desenvolvimento
            ";

            var Connection = new Connection(Bancos.Sgq);

            List<noPredictionDefects> List = Connection.Executar<noPredictionDefects>(sql);

            return List;
        }





        // POST: api/Pessoa
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Pessoa/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Pessoa/5
        public void Delete(int id)
        {
        }
    }
}
