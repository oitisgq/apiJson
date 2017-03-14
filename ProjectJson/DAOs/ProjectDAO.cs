using Classes;
using ProjectJson.Models.Project;
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
                ";

            var listProjects = _connection.Executar<Project>(sql);

            return listProjects;
        }

        public Project getProject(string subproject, string delivery)
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
                where 
                    sgq_projetos.subprojeto = '@subproject' and
                    sgq_projetos.entrega = '@delivery'
                ";

            sql = sql.Replace("@subproject", subproject);
            sql = sql.Replace("@delivery", delivery);

            var list = _connection.Executar<Project>(sql);

            return list[0];
        }

        public ProjectFull getProjectFull(string subproject, string delivery)
        {
            var projectFull = new ProjectFull();

            // projectFull.project = this.getProject(subproject, delivery);
            projectFull.defectDensity = this.getDefectsDensityByProject(subproject, delivery);

            projectFull.defectAverangeTime = this.getDefectsAverageTimeByProject(subproject, delivery);
            projectFull.defectAverangeTimeHigh = this.getDefectsAverageTimeByProject(subproject, delivery, "3-HIGH");
            projectFull.defectAverangeTimeGroupSeverity = this.getDefectAverangeTimeGroupSeverityByProject(subproject, delivery);

            projectFull.defectReopened = this.getDefectReopenedByProject(subproject, delivery);
            projectFull.detectableInDev = this.getDetectableInDevByProject(subproject, delivery);

            projectFull.statusLastDays = this.getStatusLastDaysByProject(subproject, delivery);

            projectFull.statusGroupMonth = this.getStatusLastMonthByProject(subproject, delivery);

            projectFull.defectStatus = this.getDefectStatusByProject(subproject, delivery);

            projectFull.defectsGroupOrigin = this.getDefectsGroupOrigin(subproject, delivery);

            return projectFull;
        }

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

            List<Status> List = _connection.Executar<Status>(sql);

            return List;
        }

        public StatusLastDays getStatusLastDaysByProject(string subproject, string delivery)
        {
            List<Status> status30LastDays = this.getStatusGroupDayByProject(subproject, delivery);

            List<Status> status5LastDays;
            if (status30LastDays.Count > 5)
            {
                status5LastDays = status30LastDays.GetRange(0, 4);
            } else
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

    }
}