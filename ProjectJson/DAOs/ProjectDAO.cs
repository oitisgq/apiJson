using Classes;
using ProjectJson.Models;
using ProjectJson.Models.Project;
using System;
using System.Collections;
using System.Collections.Generic;


namespace ProjectJson.DAOs
{
    public class ProjectDAO
    {
        private Connection _connection;

        public ProjectDAO()
        {
            _connection = new Connection(Bancos.Sgq);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public IList<Project> getProjects()
        {
            string sql = @"
                select
	                id,
	                subproject,
	                delivery,
	                subprojectDelivery,
	                name,
	                objective,
	                classification,
	                state,
	                release,
	                GP,
	                N3,
	                UN,
	                trafficLight,
	                rootCause,
	                actionPlan,
	                informative,
	                attentionPoints,
	                attentionPointsOfIndicators,
	                IterationsActive,
	                IterationsSelected,

	                (select 
		                (case when sum(planned) - sum(realized) >= 0 then sum(planned) - sum(realized) else 0 end) as GAP
	                from
		                (
		                select 
			                substring(dt_planejamento,4,5) as date, 
			                1 as planned,
			                0 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT <> 'CANCELLED' and
			                substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
			                dt_planejamento <> ''

		                union all

		                select 
			                substring(dt_execucao,4,5) as date, 
			                0 as planned,
			                1 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT = 'PASSED' and 
			                dt_execucao <> ''
		                ) Aux
	                ) as gap
                from
	                (
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
                        biti_subprojetos.UN as UN,
                        sgq_projetos.Farol as trafficLight,
                        sgq_projetos.Causa_Raiz as rootCause,
                        sgq_projetos.Plano_Acao as actionPlan,
                        sgq_projetos.Informativo as informative,
                        sgq_projetos.Pontos_Atencao as attentionPoints,
                        sgq_projetos.Pontos_Atencao_Indicadores as attentionPointsOfIndicators,
                        sgq_projetos.IterationsActive,
                        sgq_projetos.IterationsSelected
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
                    where 
                        (select Sigla from sgq_meses m where m.id = SGQ_Releases_Entregas.release_mes) + ' ' + convert(varchar, SGQ_Releases_Entregas.release_ano) is not null
	                ) aux
                order by
                    subproject,
                    delivery
                ";

            var listProjects = _connection.Executar<Project>(sql);

            return listProjects;
        }

        public IList<Project> getProjectsByIds(string ids)
        {
            string sql = @"
                select
	                id,
	                subproject,
	                delivery,
	                subprojectDelivery,
	                name,
	                objective,
	                classification,
	                state,
	                release,
	                GP,
	                N3,
	                UN,
	                trafficLight,
	                rootCause,
	                actionPlan,
	                informative,
	                attentionPoints,
	                attentionPointsOfIndicators,
	                IterationsActive,
	                IterationsSelected,

	                (select 
		                (case when sum(planned) - sum(realized) >= 0 then sum(planned) - sum(realized) else 0 end) as GAP
	                from
		                (
		                select 
			                substring(dt_planejamento,4,5) as date, 
			                1 as planned,
			                0 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT <> 'CANCELLED' and
			                substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
			                dt_planejamento <> ''

		                union all

		                select 
			                substring(dt_execucao,4,5) as date, 
			                0 as planned,
			                1 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT = 'PASSED' and 
			                dt_execucao <> ''
		                ) Aux
	                ) as gap
                from
	                (
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
                        biti_subprojetos.UN as UN,
                        sgq_projetos.Farol as trafficLight,
                        sgq_projetos.Causa_Raiz as rootCause,
                        sgq_projetos.Plano_Acao as actionPlan,
                        sgq_projetos.Informativo as informative,
                        sgq_projetos.Pontos_Atencao as attentionPoints,
                        sgq_projetos.Pontos_Atencao_Indicadores as attentionPointsOfIndicators,
                        sgq_projetos.IterationsActive,
                        sgq_projetos.IterationsSelected
                    from 
                        sgq_projetos
                        inner join alm_projetos WITH (NOLOCK)
                        on alm_projetos.subprojeto = sgq_projetos.subprojeto and
                            alm_projetos.entrega = sgq_projetos.entrega
                        left join biti_subprojetos WITH (NOLOCK)
                        on biti_subprojetos.id = sgq_projetos.subprojeto
                            left join SGQ_Releases_Entregas WITH (NOLOCK)
                        on SGQ_Releases_Entregas.subprojeto = sgq_projetos.subprojeto and
                            SGQ_Releases_Entregas.entrega = sgq_projetos.entrega and
                            SGQ_Releases_Entregas.id = (select top 1 re2.id from SGQ_Releases_Entregas re2 
                                                        where re2.subprojeto = SGQ_Releases_Entregas.subprojeto and 
                                                            re2.entrega = SGQ_Releases_Entregas.entrega 
                                                        order by re2.release_ano desc, re2.release_mes desc)
                    where 
                        sgq_projetos.id in (@ids)
	                ) aux
                order by
                    subproject,
                    delivery
                ";

            sql = sql.Replace("@ids", ids);
            var listProjects = _connection.Executar<Project>(sql);

            return listProjects;
        }
        public Project getProject(string subproject, string delivery)
        {
            string sql = @"
                select
	                id,
	                subproject,
	                delivery,
	                subprojectDelivery,
	                name,
	                objective,
	                classification,
	                state,
	                release,
	                GP,
	                N3,
	                UN,
	                trafficLight,
	                rootCause,
	                actionPlan,
	                informative,
	                attentionPoints,
	                attentionPointsOfIndicators,
	                IterationsActive,
	                IterationsSelected,

	                (select 
		                (case when sum(planned) - sum(realized) >= 0 then sum(planned) - sum(realized) else 0 end) as GAP
	                from
		                (
		                select 
			                substring(dt_planejamento,4,5) as date, 
			                1 as planned,
			                0 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT <> 'CANCELLED' and
			                substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
			                dt_planejamento <> ''

		                union all

		                select 
			                substring(dt_execucao,4,5) as date, 
			                0 as planned,
			                1 as realized
		                from ALM_CTs WITH (NOLOCK)
		                where 
			                subprojeto = aux.subproject and
			                entrega = aux.delivery and
			                Status_Exec_CT = 'PASSED' and 
			                dt_execucao <> ''
		                ) Aux
	                ) as gap
                from
	                (
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
                        biti_subprojetos.UN as UN,
                        sgq_projetos.Farol as trafficLight,
                        sgq_projetos.Causa_Raiz as rootCause,
                        sgq_projetos.Plano_Acao as actionPlan,
                        sgq_projetos.Informativo as informative,
                        sgq_projetos.Pontos_Atencao as attentionPoints,
                        sgq_projetos.Pontos_Atencao_Indicadores as attentionPointsOfIndicators,
                        sgq_projetos.IterationsActive,
                        sgq_projetos.IterationsSelected
                    from 
                        sgq_projetos
                        inner join alm_projetos WITH (NOLOCK)
                        on alm_projetos.subprojeto = sgq_projetos.subprojeto and
                            alm_projetos.entrega = sgq_projetos.entrega
                        left join biti_subprojetos WITH (NOLOCK)
                        on biti_subprojetos.id = sgq_projetos.subprojeto
                            left join SGQ_Releases_Entregas WITH (NOLOCK)
                        on SGQ_Releases_Entregas.subprojeto = sgq_projetos.subprojeto and
                            SGQ_Releases_Entregas.entrega = sgq_projetos.entrega and
                            SGQ_Releases_Entregas.id = (select top 1 re2.id from SGQ_Releases_Entregas re2 
                                                        where re2.subprojeto = SGQ_Releases_Entregas.subprojeto and 
                                                            re2.entrega = SGQ_Releases_Entregas.entrega 
                                                        order by re2.release_ano desc, re2.release_mes desc)
                    where 
                        sgq_projetos.subprojeto = '@subproject' and
                        sgq_projetos.entrega = '@delivery'
	                ) aux
                order by
                    subproject,
                    delivery
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<Project>(sql);

            return list[0];
        }

        //public ProjectFull getProjectFull(string subproject, string delivery)
        //{
        //    var projectFull = new ProjectFull();

        //    // projectFull.project = this.getProject(subproject, delivery);
        //    projectFull.defectDensity = this.getDefectsDensityByProject(subproject, delivery);

        //    projectFull.defectAverangeTime = this.getDefectsAverageTimeByProject(subproject, delivery);
        //    projectFull.defectAverangeTimeHigh = this.getDefectsAverageTimeByProject(subproject, delivery, "3-HIGH");
        //    projectFull.defectAverangeTimeGroupSeverity = this.getDefectAverangeTimeGroupSeverityByProject(subproject, delivery);

        //    projectFull.defectReopened = this.getDefectReopenedByProject(subproject, delivery);
        //    projectFull.detectableInDev = this.getDetectableInDevByProject(subproject, delivery);

        //    projectFull.statusLastDays = this.getStatusLastDaysByProject(subproject, delivery);

        //    projectFull.statusGroupMonth = this.getStatusLastMonthByProject(subproject, delivery);

        //    projectFull.defectStatus = this.getDefectStatusByProject(subproject, delivery);

        //    projectFull.defectsGroupOrigin = this.getDefectsGroupOrigin(subproject, delivery);

        //    return projectFull;
        //}

        public DefectDensity getDefectsDensityByProject(string subproject, string delivery)
        {
            string sql = @"
                select
                    sum(qte_defeitos) as QtyDefects,
                    count(*) as QtyCTs,
                    round(convert(float, sum(qte_defeitos)) / (case when count(*) = 0 then 1 else count(*) end) * 100,2) as Density
                from
                    (select
                        cts.subprojeto as subproject,
                        cts.entrega as delivery,
                        substring(cts.dt_execucao, 4, 2) as monthExecution,
                        substring(cts.dt_execucao, 7, 2) as yearExecution,
                        (select count(*)
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
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<DefectDensity>(sql);

            return list[0];
        }

        public DefectAverangeTime getDefectsAverageTimeByProject(string subproject, string delivery)
        {
            string sql = @"
                select 
	                count(*) as qtyDefects,
	                round(sum(Aging),2) as qtyHours,
	                round(sum(Aging) / count(*),2) as AverageHours
                from 
	                alm_defeitos s
                where 
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<DefectAverangeTime>(sql);

            return list[0];
        }
        public DefectAverangeTime getDefectsAverageTimeByProject(string subproject, string delivery, string severity)
        {
            string sql = @"
                select 
	                count(*) as qtyDefects,
	                round(sum(Aging),2) as qtyHours,
	                round(sum(Aging) / count(*),2) as AverageHours
                from 
	                alm_defeitos s
                where 
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
                    severidade = '@severity' and
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@severity", severity);

            var list = _connection.Executar<DefectAverangeTime>(sql);

            return list[0];
        }
        public IList<DefectAverangeTimeGroupSeverity> getDefectAverangeTimeGroupSeverityByProject(string subproject, string delivery)
        {
            string sql = @"
                select 
	                substring(severidade,3,10) as severity,
	                count(*) as qtyDefects,
	                round(sum(Aging),2) as qtyHours,
	                round(sum(Aging) / count(*),2) as AverageHours
                from 
	                alm_defeitos s
                where 
	                --subprojeto = 'PRJ00001149' and
	                --entrega = 'ENTREGA00000425' and
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
	                (ciclo like '%TI%' or ciclo like '%UAT%') and
	                status_atual = 'CLOSED' and
	                dt_final <> ''
                group by
	                severidade
                order by 
	                severidade desc
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<DefectAverangeTimeGroupSeverity>(sql);

            return list;
        }

        public DefectReopened getDefectReopenedByProject(string subproject, string delivery)
        {
            string sql = @"
                select
	                count(*) as qtyTotal,
	                sum(qtd_reopen) as qtyReopened,
	                round(convert(float,sum(qtd_reopen)) / count(*) * 100,2) as percentReopened,
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
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<DefectReopened>(sql);

            return list[0];
        }

        public DetectableInDev getDetectableInDevByProject(string subproject, string delivery)
        {
            string sql = @"
                select 
	                count(*) as qtyTotal,
	                sum(case when Erro_Detectavel_Em_Desenvolvimento = 'SIM' then 1 else 0 end) as qtyDetectableInDev,
                    round(convert(float,sum(case when Erro_Detectavel_Em_Desenvolvimento = 'SIM' then 1 else 0 end)) / count(*) * 100,2) as percentDetectableInDev,
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
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<DetectableInDev>(sql);

            return list[0];
        }

        public List<Status> getStatusGroupDayByProject(string subproject, string delivery)
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
					date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					1 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from
					(select distinct
						ct,
						convert(varchar(8),dateDate, 5) as date
					from
						(select 
							ex.ct,
							min(convert(datetime, ex.dt_execucao,5)) as dateDate
						from 
							alm_cts cts  WITH (NOLOCK)
							inner join alm_execucoes ex  WITH (NOLOCK)
								on ex.subprojeto = cts.subprojeto and
									ex.entrega = cts.entrega and
									ex.ct = cts.ct
						where
					        cts.subprojeto = '@subproject' and
					        cts.entrega = '@delivery' and
							ex.status = 'PASSED'
						group by
							ex.ct
						) aux
					) x

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = 'VALIDADO'
					order by 
						substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
					Evidencia_Validacao_Tecnica = 'VALIDADO'

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = 'VALIDADO'
					order by 
						substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
					Evidencia_Validacao_Cliente = 'VALIDADO'
		        ) Aux
			group by
				date
			order by
				2

            select top 30
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

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percRealized,

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

            List<Status> List = _connection.Executar<Status>(sql);

            return List;
        }

        public StatusLastDays getStatusLastDaysByProject(string subproject, string delivery)
        {
            List<Status> status30LastDays = this.getStatusGroupDayByProject(subproject, delivery);

            if (status30LastDays.Count >= 30)
                status30LastDays = status30LastDays.GetRange(0, 29);

            List<Status> status5LastDays;
            if (status30LastDays.Count > 5)
            {
                status5LastDays = status30LastDays.GetRange(0, 4);
            }
            else
            {
                status5LastDays = status30LastDays;
            }

            status30LastDays.Sort((x, y) => x.dateOrder.CompareTo(y.dateOrder));

            var statusLastDays = new StatusLastDays()
            {
                last05Days = status5LastDays,
                last30Days = status30LastDays
            };
            return statusLastDays;
        }

        public IList<Status> getStatusLastMonthByProject(string subproject, string delivery)
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
	                substring(date,4,5) as date, 
	                0 as active,
	                0 as activeUAT,
	                0 as planned,
	                1 as realized,
	                0 as productivity,
	                0 as approvedTI,
	                0 as approvedUAT
                from
	                (select distinct
		                ct,
		                convert(varchar(8),dateDate, 5) as date
	                from
		                (select 
			                ex.ct,
			                min(convert(datetime, ex.dt_execucao,5)) as dateDate
		                from 
			                alm_cts cts  WITH (NOLOCK)
			                inner join alm_execucoes ex  WITH (NOLOCK)
				                on ex.subprojeto = cts.subprojeto and
					                ex.entrega = cts.entrega and
					                ex.ct = cts.ct
		                where
			                cts.subprojeto = '@subproject' and
			                cts.entrega = '@delivery' and
			                ex.status = 'PASSED'
		                group by
			                ex.ct
		                ) aux
	                ) x

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = 'VALIDADO'
					order by 
						substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
					Evidencia_Validacao_Tecnica = 'VALIDADO'

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = 'VALIDADO'
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
					Evidencia_Validacao_Cliente = 'VALIDADO'
		        ) Aux
			group by
				date
			order by
				2

            select
	            convert(varchar, cast(substring('@subproject',4,8) as int)) + ' ' + convert(varchar,cast(substring('@delivery',8,8) as int)) as project,
	            'subproject' as subproject,
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

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percRealized,

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

            IList<Status> list = _connection.Executar<Status>(sql);

            return list;
        }

        public IList<DefectStatus> getDefectStatusByProject(string subproject, string delivery)
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

            List<DefectStatus> list = _connection.Executar<DefectStatus>(sql);

            return list;
        }

        public IList<DefectStatus> getDefectsGroupOrigin(string subproject, string delivery)
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

            List<DefectStatus> list = _connection.Executar<DefectStatus>(sql);

            return list;
        }

        public IList<CtsImpactedXDefects> getCtsImpactedXDefects(string subproject, string delivery)
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

            List<CtsImpactedXDefects> list = _connection.Executar<CtsImpactedXDefects>(sql);

            return list;
        }

        public IList<DefectsOpen> getDefectsOpenInDevManuf(string subproject, string delivery)
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
	            substring('-  ',2+convert(int,sign(df.aging)),1) + right(convert(varchar, floor(abs(df.aging))), 3) + ':' + right('00' + convert(varchar,round( 60*(abs(df.aging)-floor(abs(df.aging))), 0)), 2) as agingDisplay,
             df.Ping_Pong as pingPong
            from 
	            ALM_Defeitos df WITH (NOLOCK)
            where
	            df.Status_Atual in ('NEW','IN_PROGRESS','PENDENT (PROGRESS)','REOPEN','MIGRATE') and
	            df.subprojeto = '@subproject' and
	            df.entrega = '@delivery'
            order by 
                4
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            List<DefectsOpen> list = _connection.Executar<DefectsOpen>(sql);

            return list;
        }

        public IList<DefectsOpen> getDefectsOpenInTestManuf(string subproject, string delivery)
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
	            substring('-  ',2+convert(int,sign(df.aging)),1) + right(convert(varchar, floor(abs(df.aging))), 3) + ':' + right('00' + convert(varchar,round( 60*(abs(df.aging)-floor(abs(df.aging))), 0)), 2) as agingDisplay,
             df.Ping_Pong as pingPong
            from 
	            ALM_Defeitos df WITH (NOLOCK)
            where
	            df.Status_Atual in ('ON_RETEST','PENDENT (RETEST)','REJECTED') and
	            df.subprojeto = '@subproject' and
	            df.entrega = '@delivery'
            order by 
                4
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            List<DefectsOpen> list = _connection.Executar<DefectsOpen>(sql);

            return list;
        }

        public IList<ProductivityXDefects> getProductivityXDefects(string subproject, string delivery)
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

            List<ProductivityXDefects> list = _connection.Executar<ProductivityXDefects>(sql);

            return list;
        }

        public IList<ProductivityXDefectsGroupWeekly> getProductivityXDefectsGroupWeekly(string subproject, string delivery)
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
                        case when (case 
						      when (b.weekNumber - a.weekNumber) > 0 then (b.weekNumber - a.weekNumber) * 7
						      when (b.weekNumber - a.weekNumber) < 0 then ((b.weekNumber - a.weekNumber) * -1 - 51) * 7
					         end) > 1 
                          then 
                             (case 
						        when (b.weekNumber - a.weekNumber) > 0 then (b.weekNumber - a.weekNumber) * 7
						        when (b.weekNumber - a.weekNumber) < 0 then ((b.weekNumber - a.weekNumber) * -1 - 51) * 7
					         end)
                          else 1
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

            List<ProductivityXDefectsGroupWeekly> list = _connection.Executar<ProductivityXDefectsGroupWeekly>(sql);

            return list;
        }

        // ITERATIONS

        public IList<iteration> getIterations(string subproject, string delivery)
        {
            string sql = @"
                select distinct 
	                iterations as id,
	                iterations as name
                from 
	                alm_cts
                where
	                subprojeto = '@subproject' and
	                entrega = '@delivery' and
                	status_exec_ct <> 'CANCELLED'
                order by 
	                1
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            List<iteration> list = _connection.Executar<iteration>(sql);

            return list;
        }

        public List<string> getIterationsActive(string subproject, string delivery)
        {
            string sql = @"
                select 
                    IterationsActive 
                from 
                    SGQ_Projetos 
                where 
                    subprojeto = '@subproject' and 
                    entrega = '@delivery'
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            string iterations = _connection.Get_String(sql);
            string[] stringSeparators = new string[] { "','" };
            var list = iterations.Split(stringSeparators, StringSplitOptions.None);

            return new List<string>(list);
        }

        public List<string> getIterationsSelected(string subproject, string delivery)
        {
            string sql = @"
                select 
                    IterationsSelected 
                from 
                    SGQ_Projetos 
                where 
                    subprojeto = '@subproject' and 
                    entrega = '@delivery'
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            string iterations = _connection.Get_String(sql);
            string[] stringSeparators = new string[] { "','" };
            var list = iterations.Split(stringSeparators, StringSplitOptions.None);

            return new List<string>(list);
        }

        // --------

        public DefectDensity getDefectsDensityByProjectIterations(string subproject, string delivery, List<string> iterations)
        {
            string sql = @"
                select
                    sum(qte_defeitos) as QtyDefects,
                    count(*) as QtyCTs,
                    round(convert(float, sum(qte_defeitos)) / (case when count(*) = 0 then 1 else count(*) end) * 100,2) as Density
                from
                    (select
                        cts.subprojeto as subproject,
                        cts.entrega as delivery,
                        substring(cts.dt_execucao, 4, 2) as monthExecution,
                        substring(cts.dt_execucao, 7, 2) as yearExecution,
                        (select count(*)
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
                            cts.subprojeto = '@subproject' and
                            cts.entrega = '@delivery' and
                            cts.iterations in (@iterations) and
                            cts.status_exec_ct not in ('CANCELLED', 'NO RUN') and
                            cts.fabrica_desenvolvimento is not null and
                            cts.massa_Teste <> 'SIM' and
                            (cts.ciclo like '%TI%' or cts.ciclo like '%UAT%') and
                            cts.dt_execucao <> ''
                    ) Aux
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            var list = _connection.Executar<DefectDensity>(sql);

            return list[0];
        }

        public DefectAverangeTime getDefectsAverageTimeByProjectIterations(string subproject, string delivery, string severity, List<string> iterations)
        {
            string sql = @"
                select 
	                count(*) as qtyDefects,
	                round(sum(Aging),2) as qtyHours,
	                round(sum(Aging) / count(*),2) as AverageHours
                from 
					            alm_cts cts 
					            inner join alm_defeitos df
						            on df.subprojeto = cts.subprojeto and
                                       df.entrega = cts.entrega and
                                       df.ct = cts.ct
                where 
	                cts.subprojeto = '@subproject' and
	                cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
                    df.severidade = '@severity' and
	                (df.ciclo like '%TI%' or df.ciclo like '%UAT%') and
	                df.status_atual = 'CLOSED' and
	                df.dt_final <> ''
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@severity", severity);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            var list = _connection.Executar<DefectAverangeTime>(sql);

            return list[0];
        }

        public DefectReopened getDefectReopenedByProjectIterations(string subproject, string delivery, List<string> iterations)
        {
            string sql = @"
                select
	                    count(*) as qtyTotal,
	                    sum(qtd_reopen) as qtyReopened,
	                    round(convert(float,sum(qtd_reopen)) / count(*) * 100,2) as percentReopened,
	                    5 as percentReference,
	                    round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
					                alm_cts cts 
					                inner join alm_defeitos df
						                on df.subprojeto = cts.subprojeto and
                                           df.entrega = cts.entrega and
                                           df.ct = cts.ct
                where 
	                    cts.subprojeto = '@subproject' and
	                    cts.entrega = '@delivery' and
                        cts.iterations in (@iterations) and
	                    (df.ciclo like '%TI%' or df.ciclo like '%UAT%') and
	                    df.status_atual = 'CLOSED' and
	                    df.dt_final <> ''
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            var list = _connection.Executar<DefectReopened>(sql);

            return list[0];
        }

        public DetectableInDev getDetectableInDevByProjectIterations(string subproject, string delivery, List<string> iterations)
        {
            string sql = @"
                select 
	                    count(*) as qtyTotal,
	                    sum(case when Erro_Detectavel_Em_Desenvolvimento = 'SIM' then 1 else 0 end) as qtyDetectableInDev,
                        round(convert(float,sum(case when Erro_Detectavel_Em_Desenvolvimento = 'SIM' then 1 else 0 end)) / count(*) * 100,2) as percentDetectableInDev,
	                    5 as percentReference,
	                    round(convert(float,count(*) * 0.05),2) as qtyReference
                from 
					                alm_cts cts 
					                inner join alm_defeitos df
						                on df.subprojeto = cts.subprojeto and
                                           df.entrega = cts.entrega and
                                           df.ct = cts.ct
                where 
	                    cts.subprojeto = '@subproject' and
	                    cts.entrega = '@delivery' and
                        cts.iterations in (@iterations) and
	                    (df.ciclo like '%TI%' or df.ciclo like '%UAT%') and
	                    df.status_atual = 'CLOSED' and 
	                    df.dt_final <> ''
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            var list = _connection.Executar<DetectableInDev>(sql);

            return list[0];
        }

        public List<Status> getStatusGroupDayByProjectIterations(string subproject, string delivery, List<string> iterations)
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
                    iterations in (@iterations) and
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
                    iterations in (@iterations) and
					Status_Exec_CT <> 'CANCELLED' and
					substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
					dt_planejamento <> ''

				union all

				select
					date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					1 as realized,
					0 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from
					(select distinct
						ct,
						convert(varchar(8),dateDate, 5) as date
					from
						(select 
							ex.ct,
							min(convert(datetime, ex.dt_execucao,5)) as dateDate
						from 
							alm_cts cts  WITH (NOLOCK)
							inner join alm_execucoes ex  WITH (NOLOCK)
								on ex.subprojeto = cts.subprojeto and
									ex.entrega = cts.entrega and
									ex.ct = cts.ct
						where
					        cts.subprojeto = '@subproject' and
					        cts.entrega = '@delivery' and
							ex.status = 'PASSED' and
                            cts.iterations in (@iterations)
						group by
							ex.ct
						) aux
					) x

				union all

				select 
					left(ex.dt_execucao,8) as date,
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					1 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from 
					alm_cts cts  WITH (NOLOCK)
					inner join alm_execucoes ex  WITH (NOLOCK)
						on ex.subprojeto = cts.subprojeto and
                           ex.entrega = cts.entrega and
                           ex.ct = cts.ct
				where 
					cts.subprojeto = '@subproject' and
					cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
					ex.status in ('PASSED', 'FAILED') and
					ex.dt_execucao <> ''

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = 'VALIDADO'
                    order by
                        substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
                    iterations in (@iterations) and
					Status_Exec_CT <> 'CANCELLED' and 
					Evidencia_Validacao_Tecnica = 'VALIDADO'

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = 'VALIDADO'
                    order by
                        substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
                    iterations in (@iterations) and
					Status_Exec_CT = 'PASSED' and 
					UAT = 'SIM' and
					Evidencia_Validacao_Cliente = 'VALIDADO'
		        ) Aux
			group by
				date
			order by
				2

            select top 30
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

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percRealized,

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
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<Status> List = _connection.Executar<Status>(sql);

            return List;
        }

        public StatusLastDays getStatusLastDaysByProjectIterations(string subproject, string delivery, List<string> iterations)
        {
            List<Status> status30LastDays = this.getStatusGroupDayByProjectIterations(subproject, delivery, iterations);

            if (status30LastDays.Count >= 30)
                status30LastDays = status30LastDays.GetRange(0, 29);

            List<Status> status5LastDays;
            if (status30LastDays.Count > 5)
            {
                status5LastDays = status30LastDays.GetRange(0, 4);
            }
            else
            {
                status5LastDays = status30LastDays;
            }

            status30LastDays.Sort((x, y) => x.dateOrder.CompareTo(y.dateOrder));

            var statusLastDays = new StatusLastDays()
            {
                last05Days = status5LastDays,
                last30Days = status30LastDays
            };
            return statusLastDays;
        }

        public IList<Status> getStatusGroupMonthByProjectIterations(string subproject, string delivery, List<string> iterations)
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
                    iterations in (@iterations) and
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
                    iterations in (@iterations) and
					Status_Exec_CT <> 'CANCELLED' and
					substring(dt_planejamento,7,2) + substring(dt_planejamento,4,2) + substring(dt_planejamento,1,2) <= convert(varchar(6), getdate(), 12) and
					dt_planejamento <> ''

				union all

                select
	                substring(date,4,5) as date, 
	                0 as active,
	                0 as activeUAT,
	                0 as planned,
	                1 as realized,
	                0 as productivity,
	                0 as approvedTI,
	                0 as approvedUAT
                from
	                (select distinct
		                ct,
		                convert(varchar(8),dateDate, 5) as date
	                from
		                (select 
			                ex.ct,
			                min(convert(datetime, ex.dt_execucao,5)) as dateDate
		                from 
			                alm_cts cts  WITH (NOLOCK)
			                inner join alm_execucoes ex  WITH (NOLOCK)
				                on ex.subprojeto = cts.subprojeto and
					                ex.entrega = cts.entrega and
					                ex.ct = cts.ct
		                where
			                cts.subprojeto = '@subproject' and
			                cts.entrega = '@delivery' and
                            cts.iterations in (@iterations) and
			                ex.status = 'PASSED'
		                group by
			                ex.ct
		                ) aux
	                ) x

				union all

				select 
					substring(ex.dt_execucao,4,5) as date, 
					0 as active,
					0 as activeUAT,
					0 as planned,
					0 as realized,
					1 as productivity,
					0 as approvedTI,
					0 as approvedUAT
				from 
					alm_cts cts  WITH (NOLOCK)
					inner join alm_execucoes ex  WITH (NOLOCK)
						on ex.subprojeto = cts.subprojeto and
                           ex.entrega = cts.entrega and
                           ex.ct = cts.ct
				where 
					cts.subprojeto = '@subproject' and
					cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
					ex.status in ('PASSED', 'FAILED') and
					ex.dt_execucao <> ''

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO TÉCNICA' and
						h.novo_valor = 'VALIDADO'
					order by 
						substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
                    iterations in (@iterations) and
					Status_Exec_CT <> 'CANCELLED' and 
					Evidencia_Validacao_Tecnica = 'VALIDADO'

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
						h.tabela_id =  cts.ct and 
						h.campo = '(EVIDÊNCIA) VALIDAÇÃO CLIENTE' and
						h.novo_valor = 'VALIDADO'
					order by 
						substring(dt_alteracao,7,2)+substring(dt_alteracao,4,2)+substring(dt_alteracao,1,2) desc
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
                    iterations in (@iterations) and
					Status_Exec_CT = 'PASSED' and 
					UAT = 'SIM' and
					Evidencia_Validacao_Cliente = 'VALIDADO'
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

	            round(convert(float, SUM(t2.realized)) / (case when SUM(t2.active) <> 0 then SUM(t2.active) else 1 end) * 100,2) as percRealized,

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
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            IList<Status> list = _connection.Executar<Status>(sql);

            return list;
        }

        public IList<DefectStatus> getDefectStatusByProjectIterations(string subproject, string delivery, List<string> iterations)
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

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
			         where 
                        df.subprojeto = '@subproject' and 
				        df.Entrega = '@delivery' and 
                        cts.iterations in (@iterations) and
				        df.Status_Atual in ('ON_RETEST','PENDENT (RETEST)','REJECTED')
		            ) as qtyDefects,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
                     where 
                        df.subprojeto = '@subproject' and 
                        df.Entrega = '@delivery' and
                        cts.iterations in (@iterations)
                    ) as totalDefects

	            union all

	            select 
		            'Aberto-Fáb.Teste' as name,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
		             where 
                        df.subprojeto = '@subproject' and 
				        df.Entrega = '@delivery' and 
                        cts.iterations in (@iterations) and
				        df.Status_Atual in ('NEW','IN_PROGRESS','PENDENT (PROGRESS)','REOPEN','MIGRATE')
		            ) as qtyDefects,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
                     where 
                        df.subprojeto = '@subproject' and 
                        df.Entrega = '@delivery' and
                        cts.iterations in (@iterations)
                    ) as totalDefects

	            union all

	            select 
		            'Fechado' as name,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
		             where 
                        df.subprojeto = '@subproject' and 
				        df.Entrega = '@delivery' and 
                        cts.iterations in (@iterations) and
				        df.Status_Atual = 'CLOSED'
		            ) as qtyDefects,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
                     where 
                        df.subprojeto = '@subproject' and 
                        df.Entrega = '@delivery' and
                        cts.iterations in (@iterations)
                    ) as totalDefects

	            union all

	            select 
		            'Cancelado' as name,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
		             where 
                        df.subprojeto = '@subproject' and 
				        df.Entrega = '@delivery' and 
                        cts.iterations in (@iterations) and
				        df.Status_Atual = 'CANCELLED'
		            ) as qtyDefects,

		            (select count(*) 
                     from 
					    alm_cts cts WITH (NOLOCK)
					    inner join alm_defeitos df WITH (NOLOCK)
						    on df.subprojeto = cts.subprojeto and
                               df.entrega = cts.entrega and
                               df.ct = cts.ct
                    where 
                        df.subprojeto = '@subproject' and 
                        df.Entrega = '@delivery' and
                        cts.iterations in (@iterations)
                    ) as totalDefects
	            ) aux
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<DefectStatus> list = _connection.Executar<DefectStatus>(sql);

            return list;
        }

        public IList<DefectStatus> getDefectsGroupOriginIterations(string subproject, string delivery, List<string> iterations)
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
					        alm_cts cts 
					        inner join alm_defeitos df
						        on df.subprojeto = cts.subprojeto and
                                   df.entrega = cts.entrega and
                                   df.ct = cts.ct
		                where
			                cts.subprojeto = '@subproject' and
			                cts.entrega = '@delivery' and
                            cts.iterations in (@iterations) and
			                df.Status_Atual = 'CLOSED'
		                ) as totalDefects
	                from 
					    alm_cts cts 
					    inner join alm_defeitos df
						    on df.subprojeto = cts.subprojeto and
                                df.entrega = cts.entrega and
                                df.ct = cts.ct
	                where
		                cts.subprojeto = '@subproject' and
		                cts.entrega = '@delivery' and
                        cts.iterations in (@iterations) and
		                df.Status_Atual = 'CLOSED'
	                group by 
		                Origem
	                ) aux
                order by
	                2 desc
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<DefectStatus> list = _connection.Executar<DefectStatus>(sql);

            return list;
        }

        public IList<CtsImpactedXDefects> getCtsImpactedXDefectsIterations(string subproject, string delivery, List<string> iterations)
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
		            cts.subprojeto as subproject,
		            cts.entrega as delivery,
		            substring(df.dt_inicial,1,8) as date,
		            substring(df.dt_inicial,7,2) + substring(df.dt_inicial,4,2) + substring(df.dt_inicial,1,2) as dateOrder,
		            df.Origem as name,
		            df.Qtd_CTs_Impactados,
		            1 as Qtd_Defeitos
	            from 
					alm_cts cts 
					inner join alm_defeitos df
						on df.subprojeto = cts.subprojeto and
                           df.entrega = cts.entrega and
                           df.ct = cts.ct
	            where
		            cts.subprojeto = '@subproject' and
		            cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
		            df.status_atual not in ('CLOSED', 'CANCELLED') and
		            df.Origem in ('AMBIENTE','CONSTRUÇÃO') and
		            df.dt_inicial <> ''
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
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<CtsImpactedXDefects> list = _connection.Executar<CtsImpactedXDefects>(sql);

            return list;
        }

        public IList<DefectsOpen> getDefectsOpenInDevManufIterations(string subproject, string delivery, List<string> iterations)
        {
            string sql = @"
            select 
	            convert(varchar, cast(substring(cts.Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(cts.Entrega,8,8) as int)) as project,
	            cts.subprojeto as subproject,
	            cts.entrega as delivery,

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
	            substring('-  ',2+convert(int,sign(df.aging)),1) + right(convert(varchar, floor(abs(df.aging))), 3) + ':' + right('00' + convert(varchar,round( 60*(abs(df.aging)-floor(abs(df.aging))), 0)), 2) as agingDisplay,
                df.Ping_Pong as pingPong
            from 
				            alm_cts cts 
				            inner join alm_defeitos df
					            on df.subprojeto = cts.subprojeto and
                                    df.entrega = cts.entrega and
                                    df.ct = cts.ct
            where
	                cts.subprojeto = '@subproject' and
	                cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
	                df.Status_Atual in ('NEW','IN_PROGRESS','PENDENT (PROGRESS)','REOPEN','MIGRATE')
            order by 
                4
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<DefectsOpen> list = _connection.Executar<DefectsOpen>(sql);

            return list;
        }

        public IList<DefectsOpen> getDefectsOpenInTestManufIterations(string subproject, string delivery, List<string> iterations)
        {
            string sql = @"
            select 
	            convert(varchar, cast(substring(cts.Subprojeto,4,8) as int)) + ' ' + convert(varchar,cast(substring(cts.Entrega,8,8) as int)) as project,
	            cts.subprojeto as subproject,
	            cts.entrega as delivery,

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
	            substring('-  ',2+convert(int,sign(df.aging)),1) + right(convert(varchar, floor(abs(df.aging))), 3) + ':' + right('00' + convert(varchar,round( 60*(abs(df.aging)-floor(abs(df.aging))), 0)), 2) as agingDisplay,
                df.Ping_Pong as pingPong
            from 
				            alm_cts cts 
				            inner join alm_defeitos df
					            on df.subprojeto = cts.subprojeto and
                                    df.entrega = cts.entrega and
                                    df.ct = cts.ct
            where
	                df.subprojeto = '@subproject' and
	                df.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
	                df.Status_Atual in ('ON_RETEST','PENDENT (RETEST)','REJECTED')
            order by 
                4
            ";
            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<DefectsOpen> list = _connection.Executar<DefectsOpen>(sql);

            return list;
        }

        public IList<ProductivityXDefects> getProductivityXDefectsIterations(string subproject, string delivery, List<string> iterations)
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
					left(ex.dt_execucao,8) as date,
					1 as productivity,
					0 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from 
					alm_cts cts  WITH (NOLOCK)
					inner join alm_execucoes ex  WITH (NOLOCK)
						on ex.subprojeto = cts.subprojeto and
                           ex.entrega = cts.entrega and
                           ex.ct = cts.ct
				where 
					cts.subprojeto = '@subproject' and
					cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
					ex.status in ('PASSED', 'FAILED') and
					ex.dt_execucao <> ''

				union all

				select 
					left(dt_execucao,8) as date,
					0 as productivity,
					1 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from 
                    ALM_CTs WITH (NOLOCK)
				where 
					subprojeto = '@subproject' and
					entrega = '@delivery' and
                    iterations in (@iterations) and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

	            select 
		            substring(df.dt_inicial,1,8) as date,
					0 as productivity,
					0 as realized,
					(case when df.origem = 'AMBIENTE' then 1 else 0 end) as qtyDefectsAmb,
					(case when df.origem = 'CONSTRUÇÃO' then 1 else 0 end) as qtyDefectsCons,
					1 as qtyDefectsTot
	            from 
					alm_cts cts 
					inner join alm_defeitos df
						on df.subprojeto = cts.subprojeto and
                           df.entrega = cts.entrega and
                           df.ct = cts.ct
	            where
		            cts.subprojeto = '@subproject' and
		            cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
		            df.status_atual not in ('CLOSED', 'CANCELLED') and
		            df.Origem in ('AMBIENTE','CONSTRUÇÃO') and
		            df.dt_inicial <> ''
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
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<ProductivityXDefects> list = _connection.Executar<ProductivityXDefects>(sql);

            return list;
        }

        public IList<ProductivityXDefectsGroupWeekly> getProductivityXDefectsGroupWeeklyIterations(string subproject, string delivery, List<string> iterations)
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
					right('00' + convert(varchar,datepart(week, convert(datetime, ex.dt_execucao, 5))),2) + '/' + substring(ex.dt_execucao,7,2) as fullWeekNumber,
					1 as productivity,
					0 as realized,
					0 as qtyDefectsAmb,
					0 as qtyDefectsCons,
					0 as qtyDefectsTot
				from 
					alm_cts cts  WITH (NOLOCK)
					inner join alm_execucoes ex  WITH (NOLOCK)
						on ex.subprojeto = cts.subprojeto and
                           ex.entrega = cts.entrega and
                           ex.ct = cts.ct
				where 
					cts.subprojeto = '@subproject' and
					cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
					ex.status in ('PASSED', 'FAILED') and
					ex.dt_execucao <> ''

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
                    iterations in (@iterations) and
					Status_Exec_CT = 'PASSED' and 
					dt_execucao <> ''

				union all

	            select 
					right('00' + convert(varchar,datepart(week, convert(datetime, df.dt_inicial, 5))),2) + '/' + substring(df.dt_inicial,7,2) as fullWeekNumber,
					0 as productivity,
					0 as realized,
					(case when origem = 'AMBIENTE' then 1 else 0 end) as qtyDefectsAmb,
					(case when origem = 'CONSTRUÇÃO' then 1 else 0 end) as qtyDefectsCons,
					1 as qtyDefectsTot
	            from 
					alm_cts cts 
					inner join alm_defeitos df
						on df.subprojeto = cts.subprojeto and
                           df.entrega = cts.entrega and
                           df.ct = cts.ct
	            where
		            cts.subprojeto = '@subproject' and
		            cts.entrega = '@delivery' and
                    cts.iterations in (@iterations) and
		            df.status_atual not in ('CLOSED', 'CANCELLED') and
		            df.origem in ('AMBIENTE','CONSTRUÇÃO') and
		            df.dt_inicial <> ''

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
                        case when (case 
						      when (b.weekNumber - a.weekNumber) > 0 then (b.weekNumber - a.weekNumber) * 7
						      when (b.weekNumber - a.weekNumber) < 0 then ((b.weekNumber - a.weekNumber) * -1 - 51) * 7
					         end) > 1 
                          then 
                             (case 
						        when (b.weekNumber - a.weekNumber) > 0 then (b.weekNumber - a.weekNumber) * 7
						        when (b.weekNumber - a.weekNumber) < 0 then ((b.weekNumber - a.weekNumber) * -1 - 51) * 7
					         end)
                          else 1
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
            sql = sql.Replace("@iterations", "'" + string.Join("','", iterations.ToArray()) + "'");

            List<ProductivityXDefectsGroupWeekly> list = _connection.Executar<ProductivityXDefectsGroupWeekly>(sql);

            return list;
        }
    }
}