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
        public List<devManufacturers> getdevManufacturers()
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
            Connection.Dispose();

            //DataTable.AsEnumerable().Select(x => x[0].ToString()).ToList();
            return ListDevManufacturers;
        }

        [HttpGet]
        [Route("systems")]
        public List<systems> getSystems()
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
            Connection.Dispose();

            return ListSystems;
        }

        [HttpGet] // DEVERA SAIR, QUANDO IMPLATADO EM PRODUCAO
        [Route("projects")]
        public List<projects> getProjects()
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
            Connection.Dispose();

            return ListProjects;
        }


        [HttpGet]
        [Route("projects_")] // DEVERA ALTERAR O NOME QUANDO IMPLATADO EM PRODUCAO
        public List<project> getProjects_()
        {
            string sql = @"
                select 
                    sgq_projetos.id,
                    sgq_projetos.subprojeto as subproject,
                    sgq_projetos.entrega as delivery,
                    convert(varchar, cast(substring(sgq_projetos.subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(sgq_projetos.entrega,8,8) as int)) as subprojectDelivery,
                    biti_subprojetos.nome as name,
                    biti_subprojetos.objetivo as objective,
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
            Connection.Dispose();

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
                connection.Dispose();
                return Request.CreateResponse(HttpStatusCode.OK, resultado);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

        }


        [HttpGet]
        [Route("iterations/{subproject}/{delivery}")]
        public List<iteration> getIterationByProject(string subproject, string delivery)
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
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("defectsDensity")]
        public List<densityDefects> getDensity()
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
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("defectsDensity/{subproject}/{delivery}")]
        public List<densityDefects> getDensityByProject(string subproject, string delivery)
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
            Connection.Dispose();

            return List;
        }

        /*
        [HttpGet]
        [Route("agingDefects")]
        public List<agingDefects> getAgingDefects()
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
        public List<agingMedioDefects> getAgingMedio()
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
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("DefectsAverangeTime/{subproject}/{delivery}")]
        public List<defectAverangeTime> getDefectsAverangeTime(string subproject, string delivery)
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
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("defectsWrongClassif")]
        public List<wrongClassif> getWrongClassificationDefectRate()
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
            Connection.Dispose();

            return List;
        }



        [HttpGet]
        [Route("defectsDetectableInDev")]
        public List<detectableInDev> getDetectableInDev()
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
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("defectsDetectableInDev/{subproject}/{delivery}")]
        public List<defectDetectableInDev> getDetectableInDev(string subproject, string delivery)
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
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("defectDetail/{subproject}/{delivery}/{defect}")]
        public defectDetail getDefectDetailing(string subproject, string delivery, string defect)
        {
            string sql = @"
            select 
	            convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	            subprojeto as subproject,
	            entrega as delivery,

	            Defeito as id,
	            d.Nome as name,
	            Ciclo as cycle,
	            CT,
	            Sistema_CT as ctSystem,
                Sistema_Defeito as defectSystem,
	            Fabrica_Desenvolvimento as devManuf,
	            Fabrica_Teste as testManuf,
	            Encaminhado_Para as forwardedTo,
	            substring(Severidade,3,10) as severity,
	            Origem as source,
	            natureza as nature,
                Status_Atual as status,
				
				sgqD.auditStatus,
				sgqD.week,
				sgqD.release,
				sgqD.offender,
				sgqD.ruleInfringed,
				sgqD.trafficLight,

                Dt_Inicial as dtOpening,
	            Dt_Prevista_Solucao_Defeito as dtForecastingSolution,

                erro_detectavel_em_desenvolvimento as detectableInDev,

                Qtd_Reopen as qtyReopened,
                Qtd_CTs_Impactados as qtyImpactedCTs,

	            d.Ping_Pong as qtyPingPong,

      	       --round(
		           -- cast(
			          --    (select Sum(Tempo_Util) 
			          --     from ALM_Defeitos_Tempos dt WITH (NOLOCK)
			          --     where dt.Subprojeto = d.Subprojeto and 
			          --           dt.Entrega = d.Entrega and 
					        --     dt.Defeito = d.Defeito)
		           -- as float ) / 60, 2
	            --) as qtyBusinessHours,

                d.aging,

                (select top 1 novo_valor
                from ALM_Historico_Alteracoes_Campos h WITH(NOLOCK)
                    where
                      h.subprojeto = d.subprojeto and
                      h.entrega = d.entrega and
                      h.tabela_id = d.Defeito and
                      h.tabela = 'BUG' and
                      h.campo = 'COMENTÁRIOS'
                    order by
                      convert(datetime, dt_alteracao, 5) desc) as Comments
            from 
	            ALM_Defeitos d WITH (NOLOCK)
	            left join BITI_Subprojetos sp WITH (NOLOCK)
		            on sp.id = d.subprojeto
	            left join sgqDefects sgqD WITH (NOLOCK)
		            on sgqD.subproject = d.subprojeto and
		               sgqD.delivery = d.entrega and
		               sgqD.id = d.Defeito

            where
                subprojeto = '@subproject' and
                entrega = '@delivery' and
	            Defeito = @defect
                --subprojeto = '@subproject' and
                --entrega = '@delivery' and
	            --Defeito = 63
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@defect", defect);

            var Connection = new Connection(Bancos.Sgq);
            List<defectDetail> List = Connection.Executar<defectDetail>(sql);
            Connection.Dispose();

            return List[0];
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
            Connection.Dispose();

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
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("defectsOpenInTestManuf/{subproject}/{delivery}")]
        public List<defectsOpen> getDefectsOpenInTestManuf(string subproject, string delivery)
        {
            string sql = @"
            select 
	            convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	            subprojeto as subproject,
	            entrega as delivery,

	            df.Defeito as defect,

                case df.Status_Atual 
                    when 'ON_RETEST' then 'On Retest'
                    when 'PENDENT (RETEST)' then 'Pend.Retest'
                    when 'REJECTED' then 'Reject'
                    else 'Indefinido'
                end as status,

	            UPPER(LEFT(left(df.Encaminhado_Para,20),1))+LOWER(SUBSTRING(left(df.Encaminhado_Para,20),2,LEN(left(df.Encaminhado_Para,20)))) as forwardedTo,
	            UPPER(LEFT(left(df.Sistema_Defeito,20),1))+LOWER(SUBSTRING(left(df.Sistema_Defeito,20),2,LEN(left(df.Sistema_Defeito,20)))) as defectSystem,
	            UPPER(LEFT(substring(df.severidade,3,3),1))+LOWER(SUBSTRING(substring(df.severidade,3,3),2,LEN(substring(df.severidade,3,3)))) as severity,
	            df.Aging as aging,
                df.Ping_Pong as pingPong
            from 
	            ALM_Defeitos df WITH (NOLOCK)
            where
	            df.Status_Atual in ('ON_RETEST','PENDENT (RETEST)','REJECTED') and
	            df.subprojeto = '@subproject' and
	            df.entrega = '@delivery'
            order by 
                1
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<defectsOpen> List = Connection.Executar<defectsOpen>(sql);
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("defectsOpenInDevManuf/{subproject}/{delivery}")]
        public List<defectsOpen> getDefectsOpenInDevManuf(string subproject, string delivery)
        {
            string sql = @"
            select 
	            convert(varchar, cast(substring(Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(Entrega,8,8) as int)) as project,
	            subprojeto as subproject,
	            entrega as delivery,

	            df.Defeito as defect,

                case df.Status_Atual 
                    when 'NEW' then 'New'
                    when 'IN_PROGRESS' then 'In Progr.'
                    when 'MIGRATE' then 'Migrate'
                    when 'PENDENT (PROGRESS)' then 'Pend.Progr.'
                    when 'REOPEN' then 'Reopen'
                    else 'Indefinido'
                end as status,

	            UPPER(LEFT(left(df.Encaminhado_Para,20),1))+LOWER(SUBSTRING(left(df.Encaminhado_Para,20),2,LEN(left(df.Encaminhado_Para,20)))) as forwardedTo,
	            UPPER(LEFT(left(df.Sistema_Defeito,20),1))+LOWER(SUBSTRING(left(df.Sistema_Defeito,20),2,LEN(left(df.Sistema_Defeito,20)))) as defectSystem,
	            UPPER(LEFT(substring(severidade,3,3),1))+LOWER(SUBSTRING(substring(severidade,3,3),2,LEN(substring(severidade,3,3)))) as severity,
	            df.Aging as aging,
                df.Ping_Pong as pingPong
            from 
	            ALM_Defeitos df WITH (NOLOCK)
            where
	            df.Status_Atual in ('NEW','IN_PROGRESS','PENDENT (PROGRESS)','REOPEN','MIGRATE') and
	            df.subprojeto = '@subproject' and
	            df.entrega = '@delivery'
            order by 
                1
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<defectsOpen> List = Connection.Executar<defectsOpen>(sql);
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("defectsStatus/{subproject}/{delivery}")]
        public List<defectsStatus> getDefectsStatus(string subproject, string delivery)
        {
            string sql = @"
            select 
	            name,
	            qtyDefects,
	            totalDefects,
	            round(convert(float,qtyDefects) / (case when totalDefects <> 0 then totalDefects else 1 end) * 100,2) as [percent]
            from
	            (
	            select 
		            'Aberto-Fáb.Desen' as name,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK)
			            where d.subprojeto = '@subproject' and 
				            d.Entrega = '@delivery' and 
				            d.Status_Atual in ('ON_RETEST','PENDENT (RETEST)','REJECTED')
		            ) as qtyDefects,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK) where d.subprojeto = '@subproject' and d.Entrega = '@delivery') as totalDefects

	            union all

	            select 
		            'Aberto-Fáb.Teste' as name,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK)
		             where d.subprojeto = '@subproject' and 
				            d.Entrega = '@delivery' and 
				            d.Status_Atual in ('NEW','IN_PROGRESS','PENDENT (PROGRESS)','REOPEN','MIGRATE')
		            ) as qtyDefects,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK) where d.subprojeto = '@subproject' and d.Entrega = '@delivery') as totalDefects

	            union all

	            select 
		            'Fechado' as name,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK)
		             where d.subprojeto = '@subproject' and 
				            d.Entrega = '@delivery' and 
				            d.Status_Atual = 'CLOSED'
		            ) as qtyDefects,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK) where d.subprojeto = '@subproject' and d.Entrega = '@delivery') as totalDefects

	            union all

	            select 
		            'Cancelado' as name,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK)
		             where d.subprojeto = '@subproject' and 
				            d.Entrega = '@delivery' and 
				            d.Status_Atual = 'CANCELLED'
		            ) as qtyDefects,
		            (select count(*) from ALM_Defeitos d WITH (NOLOCK) where d.subprojeto = '@subproject' and d.Entrega = '@delivery') as totalDefects
	            ) aux
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<defectsStatus> List = Connection.Executar<defectsStatus>(sql);
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("defectsGroupOrigin/{subproject}/{delivery}")]
        public List<defectsStatus> getDefectsGroupOrigin(string subproject, string delivery)
        {
            string sql = @"
                select 
	                UPPER(LEFT(name,1))+LOWER(SUBSTRING(name,2,LEN(name))) as name,
	                qtyDefects,
	                totalDefects,
	                round(convert(float,qtyDefects) / (case when totalDefects <> 0 then totalDefects else 1 end) * 100,2) as [percent]
                from
	                (
	                select 
		                (case when Origem <> '' then Origem else 'INDEFINIDO' end) as name,
		                count(*) as qtyDefects,
		                (select 
			                count(*)
		                from 
			                ALM_Defeitos d WITH (NOLOCK)
		                where
			                subprojeto = '@subproject' and
			                entrega = '@delivery' and
			                Status_Atual = 'CLOSED'
		                ) as totalDefects
	                from 
		                ALM_Defeitos d WITH (NOLOCK)
	                where
		                subprojeto = '@subproject' and
		                entrega = '@delivery' and
		                Status_Atual = 'CLOSED'
	                group by 
		                Origem
	                ) aux
                order by
	                2 desc
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<defectsStatus> List = Connection.Executar<defectsStatus>(sql);
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("statusGroupDay/{subproject}/{delivery}")]
        public List<status> getStatusGroupDay(string subproject, string delivery)
        {
            string sql = @"
            declare @t table (
	            date varchar(8), 
	            dateOrder varchar(8), 
	            active int, 
	            activeUAT int, 
	            planned int, 
	            realized int,
	            productivity int,
	            approvedTI int,
	            approvedUAT int
            )
            insert into @t (
	            date, 
	            dateOrder,
	            active, 
	            activeUAT, 
	            planned, 
	            realized,
	            productivity,
	            approvedTI,
	            approvedUAT
            )
	        select 
		        date,
		        substring(date,7,2)+substring(date,4,2)+substring(date,1,2) as dateOrder,
		        sum(active) as active,
		        sum(activeUAT) as activeUAT,
		        sum(planned) as planned,
		        sum(realized) as realized,
		        sum(productivity) as productivity,
		        sum(approvedTI) as approvedTI,
		        sum(approvedUAT) as approvedUAT
	        from
		        (
				select 
					left(dt_criacao,8) as date, 
					1 as active,
					case when uat = 'SIM' then 1 else 0 end as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and
					dt_criacao <> ''

				union all

				select 
					left(dt_planejamento,8) as date, 
					0 as active,
					0 as activeUAT,
					1 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and
					substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
					dt_planejamento <> ''

				union all

				select 
					left(dt_execucao,8) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					1 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

				select 
					left(dt_execucao,8) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					1 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from alm_execucoes WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					-- status <> 'CANCELLED' and
					status in ('PASSED', 'FAILED') and
					dt_execucao <> ''

				union all

				select 
					left((select top 1
						dt_alteracao
					from 
						ALM_Historico_Alteracoes_Campos h
					where 
						h.subprojeto = cts.subprojeto and
						h.entrega = cts.entrega and
						h.tabela = 'TESTCYCL' and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = cts.Evidencia_Validacao_Tecnica
					),8) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					1 as approvedTI,
					0 as approvedUAT
				from 
					alm_cts cts WITH (NOLOCK)
				where
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and 
					Evidencia_Validacao_Tecnica in ('VALIDADO', 'N/A', 'N/A - SOLICITAÇÃO PROJETO')

				union all

				select 
					left((select top 1
						dt_alteracao
					from 
						ALM_Historico_Alteracoes_Campos h
					where 
						h.subprojeto = cts.subprojeto and
						h.entrega = cts.entrega and
						h.tabela = 'TESTCYCL' and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = cts.Evidencia_Validacao_Cliente
					),8) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					1 as approvedUAT
				from 
					alm_cts cts WITH (NOLOCK)
				where
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					UAT = 'SIM' and
					Evidencia_Validacao_Cliente in ('VALIDADO', 'N/A', 'N/A - SOLICITAÇÃO PROJETO')
		        ) Aux
			group by
				date
			order by
				2

            select top 30
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            '@subproject' as subproject,
	            '@delivery' as delivery,
	            t.date,
                t.dateOrder,
	            t.active, 
	            t.activeUAT, 
	            t.planned, 
	            t.realized,
	            t.productivity,
		        (case when t.realized > t.planned then 0 else t.planned - t.realized end) as GAP,
	            t.approvedTI,
	            t.approvedUAT,

	            SUM(t2.active) as activeAcum,
	            SUM(t2.activeUAT) as activeUATAcum,
	            SUM(t2.planned) as plannedAcum,
	            SUM(t2.realized) as realizedAcum,
	            SUM(t2.productivity) as productivityAcum,
		        (case when sum(t2.realized) > sum(t2.planned) then 0 else sum(t2.planned) - sum(t2.realized) end) as GAPAcum,
	            SUM(t2.approvedTI) as approvedTIAcum,
	            SUM(t2.approvedUAT) as approvedUATAcum,

	            round(convert(float, SUM(t2.planned)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percPlanned,

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.planned) <> 0 then SUM(t2.planned) else 1 end) * 100,2) as percRealized,

	            round(convert(float, (case when sum(t2.realized) > sum(t2.planned) then 0 else sum(t2.planned) - sum(t2.realized) end)) / 
									 (case when SUM(t2.planned) <> 0 then SUM(t2.planned) else 1 end) * 100,2) as percGAP,

	            round(convert(float, SUM(t2.approvedTI)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percApprovedTI,

	            round(convert(float, SUM(t2.approvedUAT)) / (case when SUM(t2.activeUAT) <> 0 then SUM(t2.activeUAT) else 1 end) * 100,2) as percApprovedUAT
            from 
	            @t t inner join @t t2 
	              on t.dateOrder >= t2.dateOrder
            group by 
	            t.dateOrder, 
	            t.date,
	            t.active, 
	            t.activeUAT, 
	            t.planned, 
	            t.realized,
	            t.productivity,
	            t.approvedTI,
	            t.approvedUAT
            order by 
	            t.dateOrder desc
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<status> List = Connection.Executar<status>(sql);
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("statusGroupMonth/{subproject}/{delivery}")]
        public List<status> getStatusGroupMonth(string subproject, string delivery)
        {
            string sql = @"
            declare @t table (
	            date varchar(5), 
	            dateOrder varchar(5), 
	            active int, 
	            activeUAT int, 
	            planned int, 
	            realized int,
	            productivity int,
	            approvedTI int,
	            approvedUAT int
            )
            insert into @t (
	            date, 
	            dateOrder,
	            active, 
	            activeUAT, 
	            planned, 
	            realized,
	            productivity,
	            approvedTI,
	            approvedUAT
            )
	        select 
		        date,
		        substring(date,4,2)+substring(date,1,2) as dateOrder,
		        sum(active) as active,
		        sum(activeUAT) as activeUAT,
		        sum(planned) as planned,
		        sum(realized) as realized,
		        sum(productivity) as productivity,
		        sum(approvedTI) as approvedTI,
		        sum(approvedUAT) as approvedUAT
	        from
		        (
				select 
					substring(dt_criacao,4,5) as date, 
					1 as active,
					case when uat = 'SIM' then 1 else 0 end as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and
					dt_criacao <> ''

				union all

				select 
					substring(dt_planejamento,4,5) as date, 
					0 as active,
					0 as activeUAT,
					1 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and
					substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
					dt_planejamento <> ''

				union all

				select 
					substring(dt_execucao,4,5) as date, 
					0 as active,
					0 as activeUAT,
					0 as planned,
					1 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

				select 
					substring(dt_execucao,4,5) as date, 
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					1 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from alm_execucoes WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					-- status <> 'CANCELLED' and
					status in ('PASSED', 'FAILED') and
					dt_execucao <> ''

				union all

				select 
					substring((select top 1
						dt_alteracao
					from 
						ALM_Historico_Alteracoes_Campos h
					where 
						h.subprojeto = cts.subprojeto and
						h.entrega = cts.entrega and
						h.tabela = 'TESTCYCL' and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = cts.Evidencia_Validacao_Tecnica
					),4,5) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					1 as approvedTI,
					0 as approvedUAT
				from 
					alm_cts cts WITH (NOLOCK)
				where
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT <> 'CANCELLED' and 
					Evidencia_Validacao_Tecnica in ('VALIDADO', 'N/A', 'N/A - SOLICITAÇÃO PROJETO')

				union all

				select 
					substring((select top 1
						dt_alteracao
					from 
						ALM_Historico_Alteracoes_Campos h
					where 
						h.subprojeto = cts.subprojeto and
						h.entrega = cts.entrega and
						h.tabela = 'TESTCYCL' and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = cts.Evidencia_Validacao_Cliente
					),4,5) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					0 as productivity,
					0 as approvedTI,
					1 as approvedUAT
				from 
					alm_cts cts WITH (NOLOCK)
				where
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					UAT = 'SIM' and
					Evidencia_Validacao_Cliente in ('VALIDADO', 'N/A', 'N/A - SOLICITAÇÃO PROJETO')
		        ) Aux
			group by
				date
			order by
				2

            select
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            '@subproject' as subproject,
	            '@delivery' as delivery,
	            t.date, 
	            t.dateOrder, 
	            t.active, 
	            t.activeUAT, 
	            t.planned, 
	            t.realized,
	            t.productivity,
		        (case when t.realized > t.planned then 0 else t.planned - t.realized end) as GAP,
	            t.approvedTI,
	            t.approvedUAT,

	            SUM(t2.active) as activeAcum,
	            SUM(t2.activeUAT) as activeUATAcum,
	            SUM(t2.planned) as plannedAcum,
	            SUM(t2.realized) as realizedAcum,
	            SUM(t2.productivity) as productivityAcum,
		        (case when sum(t2.realized) > sum(t2.planned) then 0 else sum(t2.planned) - sum(t2.realized) end) as GAPAcum,
	            SUM(t2.approvedTI) as approvedTIAcum,
	            SUM(t2.approvedUAT) as approvedUATAcum,

	            round(convert(float, SUM(t2.planned)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percPlanned,

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.planned) <> 0 then SUM(t2.planned) else 1 end) * 100,2) as percRealized,

	            round(convert(float, (case when sum(t2.realized) > sum(t2.planned) then 0 else sum(t2.planned) - sum(t2.realized) end)) / 
									 (case when SUM(t2.planned) <> 0 then SUM(t2.planned) else 1 end) * 100,2) as percGAP,

	            round(convert(float, SUM(t2.approvedTI)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percApprovedTI,

	            round(convert(float, SUM(t2.approvedUAT)) / (case when SUM(t2.activeUAT) <> 0 then SUM(t2.activeUAT) else 1 end) * 100,2) as percApprovedUAT
            from 
	            @t t inner join @t t2 
	              on t.dateOrder >= t2.dateOrder
            group by 
	            t.dateOrder, 
	            t.date,
	            t.active, 
	            t.activeUAT, 
	            t.planned, 
	            t.realized,
	            t.productivity,
	            t.approvedTI,
	            t.approvedUAT
            order by 
	            t.dateOrder
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<status> List = Connection.Executar<status>(sql);
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("ctsImpactedXDefects/{subproject}/{delivery}")]
        public List<ctsImpactedXDefects> getCtsImpactedXDefects(string subproject, string delivery)
        {
            string sql = @"
            declare @t table (
	            subproject varchar(30),
	            delivery varchar(30),
	            date varchar(8), 
	            dateOrder varchar(8), 
	            qtyDefectsAmb int, 
	            qtyDefectsCons int, 
	            qtyDefectsTot int,
	            qtyCtsImpacted int
            )

            insert into @t (
	            subproject,
	            delivery,
	            date, 
	            dateOrder,
	            qtyDefectsAmb,
	            qtyDefectsCons,
	            qtyDefectsTot,
	            qtyCtsImpacted
            )            
            select
	            subproject,
	            delivery,
	            date,
	            substring(date,7,2)+substring(date,4,2)+substring(date,1,2) as dateOrder,
	            sum(case when name = 'AMBIENTE' then Qtd_Defeitos else 0 end) as qtyDefectsAmb,
	            sum(case when name = 'CONSTRUÇÃO' then Qtd_Defeitos else 0 end) as qtyDefectsCons,
	            sum(Qtd_Defeitos) as qtyDefectsTot,
	            sum(Qtd_CTs_Impactados) as qtyCtsImpacted
            from
	            (
	            select 
		            subprojeto as subproject,
		            entrega as delivery,
		            substring(dt_inicial,1,8) as date,
		            substring(dt_inicial,7,2) + substring(dt_inicial,4,2) + substring(dt_inicial,1,2) as dateOrder,
		            Origem as name,
		            Qtd_CTs_Impactados,
		            1 as Qtd_Defeitos
	            from 
		            alm_defeitos
	            where
		            subprojeto = '@subproject' and
		            entrega = '@delivery' and
		            status_atual not in ('CLOSED', 'CANCELLED') and
		            Origem in ('AMBIENTE','CONSTRUÇÃO') and
		            dt_inicial <> ''
	            ) aux
            group by 
	            subproject,
	            delivery,
	            date,
	            dateOrder
            order by 
	            dateOrder

            select
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            '@subproject' as subproject,
	            '@delivery' as delivery,
	            t.date,
	            t.qtyDefectsAmb, 
	            t.qtyDefectsCons, 
	            t.qtyDefectsTot, 
	            t.qtyCtsImpacted, 

	            SUM(t2.qtyDefectsAmb) as qtyDefectsAmbAcum,
	            SUM(t2.qtyDefectsCons) as qtyDefectsConsAcum,
	            SUM(t2.qtyDefectsTot) as qtyDefectsTotAcum,
	            SUM(t2.qtyCtsImpacted) as qtyCtsImpactedAcum
            from 
	            @t t 
	            inner join @t t2 
	              on t.dateOrder >= t2.dateOrder
            group by 
	            t.date,
	            t.dateOrder,
	            t.qtyDefectsAmb, 
	            t.qtyDefectsCons, 
	            t.qtyDefectsTot, 
	            t.qtyCtsImpacted
            order by
	            t.dateOrder
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<ctsImpactedXDefects> List = Connection.Executar<ctsImpactedXDefects>(sql);
            Connection.Dispose();

            return List;
        }


        [HttpGet]
        [Route("productivityXDefects/{subproject}/{delivery}")]
        public List<productivityXDefects> getProductivityXDefects(string subproject, string delivery)
        {
            string sql = @"
            declare @t table (
	            date varchar(8), 
	            dateOrder varchar(8), 
	            productivity int,
	            realized int,
	            qtyDefectsAmb int,
	            qtyDefectsCons int,
	            qtyDefectsTot int
            )

            insert into @t (
	            date, 
	            dateOrder,
	            productivity,
	            realized,
	            qtyDefectsAmb,
	            qtyDefectsCons,
	            qtyDefectsTot
            )
	        select 
		        date,
		        substring(date,7,2)+substring(date,4,2)+substring(date,1,2) as dateOrder,
		        sum(productivity) as productivity,
		        sum(realized) as realized,
		        sum(qtyDefectsAmb) as qtyDefectsAmb,
		        sum(qtyDefectsCons) as qtyDefectsCons,
		        sum(qtyDefectsTot) as qtyDefectsTot
	        from
		        (
				select 
					left(dt_execucao,8) as date,
					1 as productivity,
					0 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from alm_execucoes WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					status in ('PASSED', 'FAILED') and
					dt_execucao <> ''

				union all

				select 
					left(dt_execucao,8) as date,
					0 as productivity,
					1 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

	            select 
		            substring(dt_inicial,1,8) as date,
					0 as productivity,
					0 as realized,
					(case when origem = 'AMBIENTE' then 1 else 0 end) as qtyDefectsAmb,
					(case when origem = 'CONSTRUÇÃO' then 1 else 0 end) as qtyDefectsCons,
					1 as qtyDefectsTot
	            from 
		            alm_defeitos
	            where
		            subprojeto = '@subproject' and
		            entrega = '@delivery' and
		            status_atual not in ('CLOSED', 'CANCELLED') and
		            Origem in ('AMBIENTE','CONSTRUÇÃO') and
		            dt_inicial <> ''

		        ) Aux
			group by
				date
			order by
				2

            select
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            '@subproject' as subproject,
	            '@delivery' as delivery,
	            t.date,
	            t.productivity,
	            t.realized,
	            t.qtyDefectsAmb,
	            t.qtyDefectsCons,
	            t.qtyDefectsTot,

	            SUM(t2.qtyDefectsAmb) as qtyDefectsAmbAcum,
	            SUM(t2.qtyDefectsCons) as qtyDefectsConsAcum,
	            SUM(t2.qtyDefectsTot) as qtyDefectsTotAcum
            from 
	            @t t inner join @t t2 
	              on t.dateOrder >= t2.dateOrder
            group by 
	            t.dateOrder, 
	            t.date,
	            t.productivity,
	            t.realized,
	            t.qtyDefectsAmb,
	            t.qtyDefectsCons,
	            t.qtyDefectsTot
            order by 
	            t.dateOrder
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<productivityXDefects> List = Connection.Executar<productivityXDefects>(sql);
            Connection.Dispose();

            return List;
        }

        [HttpGet]
        [Route("productivityXDefectsGroupWeekly/{subproject}/{delivery}")]
        public List<productivityXDefectsGroupWeekly> getProductivityXDefectsGroupWeekly(string subproject, string delivery)
        {
            string sql = @"
            declare @t table (
	            fullWeekNumber varchar(5), 
	            fullWeekNumberOrder varchar(5),
	            weekNumber int,
	            productivity int,
	            realized int,
	            qtyDefectsAmb int,
	            qtyDefectsCons int,
	            qtyDefectsTot int
            )
            insert into @t (
	            fullWeekNumber, 
	            fullWeekNumberOrder, 
	            weekNumber,
	            productivity,
	            realized,
	            qtyDefectsAmb,
	            qtyDefectsCons,
	            qtyDefectsTot
            )
	        select 
		        fullWeekNumber,
		        substring(fullWeekNumber,4,2)+substring(fullWeekNumber,1,2) as fullWeekNumberOrder,
		        convert(int,substring(fullWeekNumber,1,2)) as weekNumber,
		        sum(productivity) as productivity,
		        sum(realized) as realized,
		        sum(qtyDefectsAmb) as qtyDefectsAmb,
		        sum(qtyDefectsCons) as qtyDefectsCons,
		        sum(qtyDefectsTot) as qtyDefectsTot
	        from
		        (
				select 
					right('00' + convert(varchar,datepart(week, convert(datetime, dt_execucao, 5))),2) + '/' + substring(dt_execucao,7,2) as fullWeekNumber,
					1 as productivity,
					0 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from alm_execucoes WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					status in ('PASSED', 'FAILED') and
					dt_execucao <> ''

				union all

				select 
					right('00' + convert(varchar,datepart(week, convert(datetime, dt_execucao, 5))),2) + '/' + substring(dt_execucao,7,2) as fullWeekNumber,
					0 as productivity,
					1 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

	            select 
					right('00' + convert(varchar,datepart(week, convert(datetime, dt_inicial, 5))),2) + '/' + substring(dt_inicial,7,2) as fullWeekNumber,
					0 as productivity,
					0 as realized,
					(case when origem = 'AMBIENTE' then 1 else 0 end) as qtyDefectsAmb,
					(case when origem = 'CONSTRUÇÃO' then 1 else 0 end) as qtyDefectsCons,
					1 as qtyDefectsTot
	            from 
		            alm_defeitos
	            where
		            subprojeto = '@subproject' and
		            entrega = '@delivery' and
		            status_atual not in ('CLOSED', 'CANCELLED') and
		            Origem in ('AMBIENTE','CONSTRUÇÃO') and
		            dt_inicial <> ''

		        ) Aux
			group by
				fullWeekNumber
			order by
				2,3

            declare @t1 table (
	            fullWeekNumber varchar(5), 
	            fullWeekNumberOrder varchar(5),
	            productivity int,
	            realized int,
	            realizedAverage float,
	            qtyDefectsAmb int,
	            qtyDefectsCons int,
	            qtyDefectsTot int
            )
            insert into @t1 (
	            fullWeekNumber, 
	            fullWeekNumberOrder, 
	            productivity,
	            realized,
				realizedAverage,
	            qtyDefectsAmb,
	            qtyDefectsCons,
	            qtyDefectsTot
            )
			select 
				a.fullWeekNumber, 
				a.fullWeekNumberOrder,
				a.productivity,
				a.realized,

				round(
					convert(float,a.realized) / 
					   case 
						  when (b.weekNumber - a.weekNumber) > 0 then (b.weekNumber - a.weekNumber) * 7
						  when (b.weekNumber - a.weekNumber) < 0 then ((b.weekNumber - a.weekNumber) * -1 - 51) * 7
					   end,
				2) as realizedAverage,

				a.qtyDefectsAmb,
				a.qtyDefectsCons,
				a.qtyDefectsTot
			from
				(select ROW_NUMBER() OVER(ORDER BY t1.fullWeekNumberOrder) as id, * from @t t1) a
				left join (select ROW_NUMBER() OVER(ORDER BY t1.fullWeekNumberOrder) as id, * from @t t1) b
				  on b.id = a.id + 1
			order by 
				a.fullWeekNumberOrder

            select
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            '@subproject' as subproject,
	            '@delivery' as delivery,
	            t11.fullWeekNumber,
	            t11.productivity,
	            t11.realized,
				t11.realizedAverage,
	            t11.qtyDefectsAmb,
	            t11.qtyDefectsCons,
	            t11.qtyDefectsTot,

	            SUM(t12.qtyDefectsAmb) as qtyDefectsAmbAcum,
	            SUM(t12.qtyDefectsCons) as qtyDefectsConsAcum,
	            SUM(t12.qtyDefectsTot) as qtyDefectsTotAcum
            from 
	            @t1 t11 inner join @t1 t12 
	              on t11.fullWeekNumberOrder >= t12.fullWeekNumberOrder
            group by 
	            t11.fullWeekNumber,
	            t11.fullWeekNumberOrder, 
	            t11.productivity,
	            t11.realized,
	            t11.realizedAverage,
	            t11.qtyDefectsAmb,
	            t11.qtyDefectsCons,
	            t11.qtyDefectsTot
            order by 
	            t11.fullWeekNumberOrder
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var Connection = new Connection(Bancos.Sgq);
            List<productivityXDefectsGroupWeekly> List = Connection.Executar<productivityXDefectsGroupWeekly>(sql);
            Connection.Dispose();

            return List;
        }


        /*
        [HttpGet]
        [Route("ReincidenciaDefeitos")]
        public List<recurrenceDefects> getRecurrenceDefects()
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
        public List<noPredictionDefects> getnoPredictionDefects()
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
            Connection.Dispose();

            return List;
        }


        /* PEDRO */

        [HttpGet]
        [Route("bptReleases")]
        public List<bptReleases> getReleases()
        {
            string sql = @"
                select distinct 
	                (select nome from SGQ_Meses where id = release_mes) + '-' + convert(varchar,release_ano) as id,
	                (select nome from SGQ_Meses where id = release_mes) + '-' + convert(varchar,release_ano) as name,
	                convert(varchar,release_ano) as year,
	                (select Id from SGQ_Meses where id = release_mes) as month
                from 
	                sgq_releases_entregas
                order by 
                    3 desc, 
                    4 desc
            ";

            var Connection = new Connection(Bancos.Sgq);
            List<bptReleases> ListRelease = Connection.Executar<bptReleases>(sql);
            Connection.Dispose();

            return ListRelease;
        }

        [HttpGet]
        [Route("bptProjects")]
        public List<bptProjects> getBptProjects()
        {
            string sql = @"
                select 'AUTOM_LINK2_ENTREGA' as id, 'MARÇO-2017' as release, 'ESPECIAL' as type, 'AUTOM_LINK2_ENTREGA' as name
                union
                select 'AUTOM_LINK2_1' as id, 'MARÇO-2017' as release, 'ESPECIAL' as type, 'AUTOM_LINK2_1' as name
                union
                select 'AUTOM_LINK2_2' as id, 'MARÇO-2017' as release, 'RELEASE' as type, 'AUTOM_LINK2_2' as name
                union
                select 'AUTOM_LINK2_3' as id, 'MARÇO-2017' as release, 'RELEASE' as type, 'AUTOM_LINK2_3' as name
                union
                select 'AUTOM_LINK2_4' as id, 'JANEIRO-2017' as release, 'RELEASE' as type, 'AUTOM_LINK2_4' as name
                union
                select 'AUTOM_LINK2_5' as id, 'JANEIRO-2017' as release, 'RELEASE' as type, 'AUTOM_LINK2_5' as name
                union
                select 'AUTOM_LINK2_6' as id, 'JANEIRO-2017' as release, 'RELEASE' as type, 'AUTOM_LINK2_6' as name
            ";

            var Connection = new Connection(Bancos.Sgq);
            List<bptProjects> ListProjects = Connection.Executar<bptProjects>(sql);
            Connection.Dispose();

            return ListProjects;
        }

        [HttpGet]
        [Route("bptBpts")]
        public List<bptBpts> getBptBpts()

        {
            string sql = @"
               select distinct
		                b.name as id,
                        b.idBpt,
                        a.release,
                        a.classification, 
                        a.project, 
                        b.name,
                        b.system,
                        b.status
                from

                        (select 'AUTOM_LINK2_ENTREGA' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_ENTREGA' as project
		                 union
		                 select 'AUTOM_LINK2_1' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_1' as project
		                 union
		                 select 'AUTOM_LINK2_2' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_2' as project
		                 union
		                 select 'AUTOM_LINK2_3' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_3' as project
		                 union
		                 select 'AUTOM_LINK2_4' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_4' as project
		                 union
		                 select 'AUTOM_LINK2_5' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_5' as project
		                 union
		                 select 'AUTOM_LINK2_6' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_6' as project) a
                        join
                        (select distinct
				                Id as idBpt, 
                                Subprojeto +'_'+ Entrega as project, 
                                Nome as name, 
                                Sistema as system, 
                                Status_Execucao as status
                         from 
                                BPT_Tests) b
                        on a.project = b.project
                order by
                        a.release desc,
                        a.classification,
                        a.project,
                        b.name                  
            ";

            var Connection = new Connection(Bancos.Sgq);
            List<bptBpts> ListBpts = Connection.Executar<bptBpts>(sql);
            Connection.Dispose();

            return ListBpts;
        }

        [HttpGet]
        [Route("bptValidPlanoEvid")]
        public List<bptValidPlanoEvid> getBptPlanoValidEvid()
        {
            string sql = @"
               select distinct
	                z.id,
	                y.release,
	                y.classification,
	                y.name,
	                z.bpt,
 	                z.plano_val_tecnica,
 	                z.plano_motiv_rej_tec,
 	                z.plano_comentarios_rej_tec,
 	                z.plano_val_cliente,
 	                z.plano_motiv_rej_cliente,
 	                z.plano_comentarios_rej_cliente,
 	                z.evidencia_val_tecnica,
 	                z.evidencia_motiv_rej_tec,
 	                z.evidencia_comentarios_rej_tec,
 	                z.evidencia_val_cliente,
 	                z.evidencia_motiv_rej_cliente,
 	                z.evidencia_comentarios_rej_cliente,
 	                z.components
                from
	                (select 'AUTOM_LINK2_ENTREGA' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_ENTREGA' as name
	                 union
	                 select 'AUTOM_LINK2_1' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_1' as name
	                 union
	                 select 'AUTOM_LINK2_2' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_2' as name
	                 union
	                 select 'AUTOM_LINK2_3' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_3' as name
	                 union
	                 select 'AUTOM_LINK2_4' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_4' as name
	                 union
	                 select 'AUTOM_LINK2_5' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_5' as name
	                 union
	                 select 'AUTOM_LINK2_6' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_6' as name) y
	                join
	                (select distinct
 		                a.subprojeto + '_' + a.entrega id,
 		                a.subprojeto + '_' + a.entrega project,
 		                a.test_id,
 		                e.nome as bpt,
 		                a.plano_val_tecnica,
 		                a.plano_motiv_rej_tec,
 		                a.plano_comentarios_rej_tec,
 		                a.plano_val_cliente,
 		                a.plano_motiv_rej_cliente,
 		                a.plano_comentarios_rej_cliente,
 		                a.evidencia_val_tecnica,
 		                a.evidencia_motiv_rej_tec,
 		                a.evidencia_comentarios_rej_tec,
 		                a.evidencia_val_cliente,
 		                a.evidencia_motiv_rej_cliente,
 		                a.evidencia_comentarios_rej_cliente,
 		                stuff(( select ', ' + convert(varchar,d.nome)
 				                from bpt_tests_to_components c inner join bpt_components d on c.component_id = d.id
 				                where a.test_id = c.test_id and b.test_id = c.test_id for xml path('')),1 ,1, '') components	
	                 from 
 		                ((bpt_test_instances a inner join bpt_tests_cycle b 
 			                on a.test_id = b.test_id and a.test_instance_id = b.id)
 				                inner join bpt_tests e
 					                on a.test_id = e.id)) z
	                on z.id = y.id 		
                order by	
	                y.release desc,
	                z.id                  
                ";

            var Connection = new Connection(Bancos.Sgq);
            List<bptValidPlanoEvid> ListValidPlanoEvid = Connection.Executar<bptValidPlanoEvid>(sql);
            Connection.Dispose();

            return ListValidPlanoEvid;
        }

        [HttpGet]
        [Route("bptCadastroStatus")]
        public List<bptCadastroStatus> getBptCadastroStatus()
        {
            string sql = @"
                select 'AUTOM_LINK2_ENTREGA' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_ENTREGA' as name, 'farol' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_1' as id, 'MARÇO-2017' as release, 'ESPECIAL' as classification, 'AUTOM_LINK2_1' as name, 'farol' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_2' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_2' as name, 'faro' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_3' as id, 'MARÇO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_3' as name, 'faro' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_4' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_4' as name, 'faro' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_5' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_5' as name, 'faro' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores
                union
                select 'AUTOM_LINK2_6' as id, 'JANEIRO-2017' as release, 'RELEASE' as classification, 'AUTOM_LINK2_6' as name, 'faro' as farol, 'causa raiz' as causaraiz, 'plano de ação' as planoacao, 'informativo' as informativo, 'pontos de atenção' as pontoatencao, 'pontos atenção dos indicadores' as pontosatencaoindicadores                 
                ";

            var Connection = new Connection(Bancos.Sgq);
            List<bptCadastroStatus> ListCadastroStatus = Connection.Executar<bptCadastroStatus>(sql);
            Connection.Dispose();

            return ListCadastroStatus;
        }

        /* PEDRO */


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
