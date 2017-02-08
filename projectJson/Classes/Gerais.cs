using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
//using System.Windows.Forms;

namespace Classes
{
    public static class Gerais
    {
        public static void Enviar_Email_Para_Administradores(string Assunto, string Mensagem)
        {
            List<string> To = new List<string>();
            List<string> CC = new List<string>();

            To.Add("joao.frade@oi.net.br");
            To.Add("gustavo.souza@oi.net.br");
            CC.Add("josesilva@oi.net.br");

            Gerais.Enviar_Email(To, CC, Assunto, Mensagem);
        }

        public static void Enviar_Email(
            List<string> Para,
            List<string> CC,
            string Assunto,
            string Mensagem)
        {
            MailMessage objEmail = new MailMessage();
            objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.IsBodyHtml = true;
            objEmail.Priority = MailPriority.Normal;

            foreach (string Linha in Para)
                if (Linha.ToString() != "")
                    objEmail.To.Add(Linha.ToString());

            foreach (string Linha in CC)
                if (Linha.ToString() != "")
                    objEmail.CC.Add(Linha.ToString());

            objEmail.Subject = Assunto;
            objEmail.Body = Mensagem;

            objEmail.From = new MailAddress(Gerais.LerParametro(Lib.SGQ(), "EMail_Aplicativo"));

            SmtpClient objSmtp = new SmtpClient(Gerais.LerParametro(Lib.SGQ(), "Servidor_Smtp"));
            objSmtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            try
            {
                objSmtp.Send(objEmail);
            }
            catch (SmtpException) { }
            objEmail.Dispose();
            objSmtp.Dispose();
        }

        public static void EnviarEmail(
            StringBuilder Para,
            StringBuilder CC,
            string Assunto,
            string Mensagem,
            string De,
            string Usuario,
            string Senha)
        {
            MailMessage objEmail = new MailMessage();
            objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.IsBodyHtml = true;
            objEmail.Priority = MailPriority.Normal;

            foreach (string Linha in Para.ToString().Split(','))
                if (Linha.ToString() != "")
                    objEmail.To.Add(Linha.ToString());

            foreach (string Linha in CC.ToString().Split(','))
                if (Linha.ToString() != "")
                    objEmail.CC.Add(Linha.ToString());


            objEmail.Subject = Assunto;
            objEmail.Body = Mensagem;

            if (De == "")
                objEmail.From = new MailAddress(Gerais.LerParametro(Lib.SGQ(), "EMail_Aplicativo"));
            else
                objEmail.From = new MailAddress(De);


            SmtpClient objSmtp = new SmtpClient(Gerais.LerParametro(Lib.SGQ(), "Servidor_Smtp"));
            if (Usuario == "")
                objSmtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            else
                objSmtp.Credentials = new System.Net.NetworkCredential(Usuario, Senha);

            try
            {
                objSmtp.Send(objEmail);
            }
            catch (SmtpException) { }
            objEmail.Dispose();
            objSmtp.Dispose();
        }

        public static void EnviarEmail_(string Servidor,
                                       string Usuario,
                                       string Senha,
                                       string De,
                                       string Para,
                                       string CC,
                                       string Assunto,
                                       string Mensagem)
        {
            MailMessage objEmail = new MailMessage();
            objEmail.From = new MailAddress(De);
            objEmail.To.Add(Para.ToString());
            objEmail.Priority = MailPriority.Normal;
            objEmail.IsBodyHtml = true;
            objEmail.Subject = Assunto;
            if (CC != "") objEmail.CC.Add(CC);
            //objEmail.CC.Add("renata.pinto@oi.net.br");
            objEmail.CC.Add("joao.frade@oi.net.br");
            objEmail.Body = Mensagem;
            objEmail.SubjectEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            objEmail.BodyEncoding = System.Text.Encoding.GetEncoding("ISO-8859-1");
            SmtpClient objSmtp = new SmtpClient(Servidor);

            if (Usuario != "")
                objSmtp.Credentials = new System.Net.NetworkCredential(Usuario, Senha);
            else
                objSmtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

            try
            {
                objSmtp.Send(objEmail);
            }
            catch (SmtpException) {}
            objEmail.Dispose();
        }

        public static void Enviar_Email_Atualizacao_Tabela(string Assunto, DateTime Dt_Inicio, DateTime Dt_Fim)
        {
            long Tempo = DataEHora.DateDiff(DataEHora.DateInterval.Minute, Dt_Inicio, Dt_Fim);

            string Mensagem =
                @"InÌcio: " + Dt_Inicio.ToString("dd-MM-yyyy HH:mm:ss") +
                @"<br/>Fim: " + Dt_Fim.ToString("dd-MM-yyyy HH:mm:ss") +
                @"<br/>Tempo (min): " + Tempo.ToString();

            Gerais.Enviar_Email(Get_Lista_To_Aviso_Carga(), Get_Lista_CC_Aviso_Carga(), Assunto, Mensagem);
        }
        public static void Enviar_Email_Atualizacao_Projetos(string Assunto, List<string> Lista_Projetos, DateTime Dt_Inicio, DateTime Dt_Fim)
        {
            long Tempo = DataEHora.DateDiff(DataEHora.DateInterval.Minute, Dt_Inicio, Dt_Fim);

            string Mensagem =
                
                @"Projetos: " + Lista_Projetos.Count.ToString() +
                @"<br/>InÌcio: " + Dt_Inicio.ToString("dd-MM-yyyy HH:mm:ss") +
                @"<br/>Fim:   " + Dt_Fim.ToString("dd-MM-yyyy HH:mm:ss") +
                @"<br/>Tempo: " + Tempo.ToString() + " min<br/><br/>";

            foreach (var oProjeto in Lista_Projetos)
                Mensagem = Mensagem + oProjeto.ToString() + "<br/>";

            Gerais.Enviar_Email(Get_Lista_To_Aviso_Carga(), Get_Lista_CC_Aviso_Carga(), Assunto, Mensagem);
        }

        public static List<string> Get_Lista_To_Aviso_Carga()
        {
            List<string> To = new List<string>();
            To.Add("LD-TIExecucaodeTestes@oi.net.br");
            return To;
        }

        public static List<string> Get_Lista_CC_Aviso_Carga()
        {
            List<string> CC = new List<string>();
            CC.Add("joao.frade@oi.net.br");
            CC.Add("gustavo.souza@oi.net.br");
            CC.Add("josesilva@oi.net.br");
            return CC;
        }

        public static void EnviarSMS(string URL_Envio_SMS, string DDD, string Celular, string Mensagem)
        {
            try
            {
                URL_Envio_SMS.Replace("=ddd", "=" + DDD.Trim()).Replace("=celular", "=" + Celular.Trim()).Replace("=texto", Mensagem.Trim());
                Uri objURI = new Uri(@URL_Envio_SMS);
                System.Net.WebRequest objRequest;
                objRequest = System.Net.WebRequest.Create(objURI);
                objRequest.Method = "POST";

                //objRequest.Credentials = new NetworkCredential("R18838","Dell62036203");
                //WebProxy myproxy = new WebProxy(new Uri(@"http://proxy.telemar.corp.net:8350/wsw5.pac"));
                //objRequest.Proxy = myproxy;

                objRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
                objRequest.GetResponse();
            }
            catch
            {
            }
        }

        public static Boolean IsNumeric(string stringToTest)
        {
            int result;
            if (int.TryParse(stringToTest, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string Caminho_App()
        {
            string appName = Environment.CurrentDirectory;
            int h = appName.LastIndexOf("bin");
            string ll = appName.Remove(h).ToString();
            return ll;
        }

        public static void Download_From_BD(HttpResponse oResponse, String Id)
        {
            Connection oConnection = new Connection();
            DataTable oDataTable = oConnection.Get_DataTable(String.Format("select Nome, Tipo, Dados from SGQ_Arquivos where id={0}", Id));
            String Nome = (String)oDataTable.Rows[0]["Nome"];
            String Tipo = (String)oDataTable.Rows[0]["Tipo"];
            Byte[] oBytes = (Byte[])oDataTable.Rows[0]["Dados"];
            oDataTable.Dispose();
            oConnection.Dispose();

            oResponse.Buffer = true;
            oResponse.Charset = "";
            oResponse.Cache.SetCacheability(HttpCacheability.NoCache);
            oResponse.ContentType = Tipo;
            oResponse.AddHeader("content-disposition", "attachment;filename=" + Nome);
            oResponse.BinaryWrite(oBytes);
            oResponse.Flush();
            oResponse.End();
        }


        public static string RetornaPaginaHTML(string url)
        {
            WebRequest objRequest = HttpWebRequest.Create(url);
            objRequest.ContentType = "application/x-www-form-urlencoded";
            //objRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
            objRequest.Credentials = new NetworkCredential("r18838", "Epocler27", "telemar");
            StreamReader sr = new StreamReader(objRequest.GetResponse().GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            return result;
        }

        public static string GetContent(string url)
        {
            string result = "Error communicating with server";
            HttpWebRequest wreq = (HttpWebRequest)System.Net.WebRequest.Create(url);

            //WebProxy wp = new WebProxy(@"http://proxy.telemar:8350/web.pac");
            //wp.Credentials = new NetworkCredential("R18838", "Epocler13", "telemar");
            //wreq.Proxy = wp;
            
            wreq.Method = "GET";
            wreq.Timeout = 999000;
            //wreq.Credentials = new NetworkCredential("r18838", "Epocler13", "telemar");
            wreq.Credentials = System.Net.CredentialCache.DefaultCredentials;
            System.Net.HttpWebResponse wr = (System.Net.HttpWebResponse)wreq.GetResponse();

            if (wr.StatusCode == System.Net.HttpStatusCode.OK)
            {
                System.IO.Stream s = wr.GetResponseStream();
                System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                System.IO.StreamReader readStream = new System.IO.StreamReader(s, enc);
                result = readStream.ReadToEnd();
            }
            return result;
        }

        public static void MontaCondicaoWhere(string NomeCampo, string Operador, string Valor, string OperadorLogico, ref string _Where)
        {
            string Retorno = _Where;

            if ((Operador == "is not null" || Operador == "is null") || (Valor != "" && Valor != "-1"))
            {
                if (_Where.Length > 3)
                    Retorno += " " + OperadorLogico + " ";

                Retorno += " (" + NomeCampo + " " + Operador.Replace("Conteudo", Valor) + ") ";
            }
            _Where = Retorno;
        }

        public static string Pagina(string URL)
        {
            WebClient oWebClient = new WebClient();
            Stream Resposta = oWebClient.OpenRead(URL);
            StreamReader sr = new StreamReader(Resposta, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        public static void MontaCondicaoWhereFaixa(string NomeCampo, string Operador, string ValorDe, string ValorAte, string OperadorLogico, ref string _Where)
        {
            string Retorno = _Where;

            if ((Operador == "is not null" || Operador == "is null") || (ValorDe != "" && ValorDe != "-1"))
            {
                if (_Where.Length > 3)
                    Retorno += " " + OperadorLogico + " ";

                if (!Operador.Contains("between"))
                    Retorno += " (" + NomeCampo + " " + Operador.Replace("Conteudo", ValorDe) + ") ";
                else
                    Retorno += " (" + NomeCampo + " " + Operador.Replace("Conteudo1", ValorDe).Replace("Conteudo2", ValorAte) + ") ";
            }
            _Where = Retorno;
        }

        public static void MontaCondicaoWhereLogico(string NomeCampo, string Operador, int Valor, string OperadorLogico, ref string _Where)
        {
            string Retorno = _Where;

            if ((Operador == "is not null" || Operador == "is null") || (Valor > 0))
            {
                if (_Where.Length > 3)
                    Retorno += " " + OperadorLogico + " ";

                Retorno += " (" + NomeCampo + " " + Operador.Replace("Conteudo", Valor == 1 ? "'true'" : "'false'") + ") ";
            }
            _Where = Retorno;
        }

        public static void MontaCondicaoWhereSubQuery(string NomeCampo, string SubQueryClausulaSelect, string Operador, string Valor, string SubQueryClausulaFrom, string SubQueryCondicao, string OperadorLogico, ref string _Where)
        {
            string Retorno = _Where;

            if ((Operador == "is not null" || Operador == "is null") || (Valor != "" && Valor != "-1"))
            {
                if (_Where.Length > 3)
                    Retorno += " " + OperadorLogico + " ";

                Retorno += " (EXISTS (select " + SubQueryClausulaSelect + " from " + SubQueryClausulaFrom + " where (" + SubQueryCondicao + ") and " + NomeCampo + " " + Operador.Replace("Conteudo", Valor) + ")) ";
            }
            _Where = Retorno;
        }

        public static void MontaCondicaoWhereSubQueryFaixa(string NomeCampo, string Operador, string ValorDe, string ValorAte, string SubQueryNomeTabela, string SubQueryCondicao, string OperadorLogico, ref string _Where)
        {
            string Retorno = _Where;

            if ((Operador == "is not null" || Operador == "is null") || (ValorDe != "" && ValorDe != "-1"))
            {
                if (_Where.Length > 3)
                    Retorno += " " + OperadorLogico + " ";

                if (!Operador.Contains("between"))
                    Retorno += " (EXISTS (select ID from " + SubQueryNomeTabela + " where (" + SubQueryCondicao + ") and " + NomeCampo + " " + Operador.Replace("Conteudo", ValorDe) + ")) ";
                else
                    Retorno += " (EXISTS (select ID from " + SubQueryNomeTabela + " where (" + SubQueryCondicao + ") and " + NomeCampo + " " + Operador.Replace("Conteudo1", ValorDe).Replace("Conteudo2", ValorAte) + ")) ";
            }
            _Where = Retorno;
        }

        public static bool ValidaCPF(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11)
                return false;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static System.Web.UI.Control AcharControle(string idControle, System.Web.UI.Control Container)
        {
            System.Web.UI.Control c = Container.FindControl(idControle);
            if (c != null)
                return c;

            foreach (System.Web.UI.Control item in Container.Controls)
            {
                c = AcharControle(idControle, item);
                if (c != null)
                    return c;
            }
            return null;
        }

        public static void LerDominioEUsuarioLogadoRede(ref string Dominio, ref string UsuarioRede)
        {
            string[] Vetor = null;
            Vetor = HttpContext.Current.User.Identity.Name.Split('\\');

            Dominio = Vetor[0].ToString();
            UsuarioRede = Vetor[1].ToString();
        }

        public static string LerUsuarioLogadoRede()
        {
            string Dominio = "";
            string UsuarioRede = "";
            Gerais.LerDominioEUsuarioLogadoRede(ref Dominio, ref UsuarioRede);
            return UsuarioRede;
        }

        public static void GravaLoginUsuarioLogadoRede(string StringsConexao, string ID)
        {
            string Dominio = "";
            string LoginRede = "";
            LerDominioEUsuarioLogadoRede(ref Dominio, ref LoginRede);
            BD oBD = new BD(StringsConexao);
            oBD.GravarCampo("Atores", "ID=" + ID, "LoginRede", "'" + LoginRede + "'");
            oBD.Close();
        }

        public static string ValidaAtor(string StringsConexao, string Nome, string Senha, ref string ID)
        {
            string retorno = "";
            if (Nome == "") retorno = "Informe o usu·rio";
            else if (Senha == "") retorno = "Informe a senha";
            else
            {
                BD oBD = new BD(StringsConexao);
                oBD.RetornaString("SELECT ID FROM Atores WHERE Login='" + Nome + "' AND Senha='" + Senha + "'", ref ID);
                oBD.Close();
                if (ID == "")
                    retorno = "Usu·rio n„o encontrado";
            }

            //if (retorno == "")
            //    GravaLoginUsuarioLogadoRede(StringsConexao, ID);

            return retorno;
        }

        public static bool ValidaAtorDaRede(string StringsConexao, string LoginRede)
        {
            bool Achou;
            BD oBD = new BD(StringsConexao);
            Achou = oBD.Achou("SELECT ID FROM Atores WHERE LoginRede='" + LoginRede + "'");
            oBD.Close();
            return Achou;
        }

        public static bool ValidaAtorDaRede(string StringsConexao)
        {
            string Dominio = "";
            string LoginRede = "";
            LerDominioEUsuarioLogadoRede(ref Dominio, ref LoginRede);
            return ValidaAtorDaRede(StringsConexao, LoginRede);
        }


        //public static string ValidaAtor(string StringsConexao, string Login, string Senha, ref string ID)
        //{
        //    string retorno = "";
        //    if (Login == "") retorno = "Informe o usu·rio";
        //    else if (Senha == "") retorno = "Informe a senha";
        //    else
        //    {
        //        BD oBD = new BD(StringsConexao);
        //        oBD.RetornaString("SELECT ID FROM Participantes WHERE Login='" + Login + "' AND Senha='" + Senha + "'", ref ID);
        //        oBD.Close();
        //        if (ID == "")
        //            retorno = "Usu·rio n„o encontrado";
        //    }
        //    return retorno;
        //}

        public static string ListaPerfisDeUmaFuncionalidade(string StringsConexao, string IDFuncionalidade)
        {
            string sql =
                "SELECT " +
                    "Papeis.Nome AS Papeis " +
                "FROM " +
                    "PapeisXFuncionalidades LEFT OUTER JOIN Papeis ON PapeisXFuncionalidades.Papel = Papeis.ID " +
                "WHERE " +
                    "PapeisXFuncionalidades.Funcionalidade = " + IDFuncionalidade;

            BD oBD = new BD(StringsConexao);
            ArrayList ArrayPerfis = new ArrayList();
            oBD.RetornaArrayList(sql, ref ArrayPerfis);
            oBD.Close();
            string Lista = "-br-";
            for (int i = 0; i < ArrayPerfis.Count; i++)
            {
                Lista += "-br-" + ArrayPerfis[i].ToString().ToUpper();
            }
            return Lista;
        }

        public static string PapeisParticipante(string StringsConexao, string IDParticipante)
        {
            string sql =
            "SELECT " +
                "Papel " +
            "FROM " +
                "AtoresXPapeis " +
            "WHERE " +
                "Ator = " + IDParticipante;

            BD oBD = new BD(StringsConexao);
            ArrayList Array = new ArrayList();
            oBD.RetornaArrayList(sql, ref Array);
            oBD.Close();
            string Lista = " ";
            for (int i = 0; i < Array.Count; i++)
            {
                Lista += Array[i].ToString().ToUpper() + " , ";
            }
            if (Lista.EndsWith(", "))
                Lista = Lista.Substring(0, Lista.Length - 2);
            else
                Lista = "0";

            return Lista;
        }

        public static bool AcessoAFuncionalidade(string StringsConexao, string IDParticipante, string Funcionalidade)
        {
            bool Retorno = true;
            BD oBD = new BD(StringsConexao);
            String PapeisParticipanteLogado = PapeisParticipante(StringsConexao, IDParticipante);
            if (!PapeisParticipanteLogado.Contains(" 1 "))
            {
                long IDFuncionalidade = oBD.RetornaLong("SELECT ID FROM Funcionalidades WHERE ID = '" + Funcionalidade.ToUpper() + "' and RestricaoAcesso = 'False'");

                if (IDFuncionalidade != 0)
                {
                    string sql = "SELECT " +
                                        "ID " +
                                    "FROM " +
                                        "PapeisXFuncionalidades " +
                                    "WHERE " +
                                        "Papel IN (" + PapeisParticipanteLogado + ") AND " +
                                        "Funcionalidade = " + IDFuncionalidade;

                    Retorno = oBD.Achou(sql);
                }
            }
            oBD.Close();
            return Retorno;
        }




        //public static string ConteudoCampo(string StringsConexao, string Tabela, string NomeCampo, string ID)
        //{
        //    BD oBD = new BD(StringsConexao);
        //    string Conteudo = "";
        //    oBD.RetornaString("SELECT " + NomeCampo + " FROM " + Tabela + " WHERE ID=" + ID, ref Conteudo);
        //    oBD.Close();
        //    return Conteudo;
        //}

        //public static string ConteudoCampo2(string StringsConexao, string Tabela, string NomeCampo, string Condicao)
        //{
        //    BD oBD = new BD(StringsConexao);
        //    string Conteudo = "";
        //    oBD.RetornaString("SELECT " + NomeCampo + " FROM " + Tabela + " WHERE " + Condicao, ref Conteudo);
        //    oBD.Close();
        //    return Conteudo;
        //}

        //public static void ExecutarSql(string StringsConexao, string sql)
        //{
        //    BD oBD = new BD(StringsConexao);
        //    oBD.Executa(sql);
        //    oBD.Close();
        //}

        //public static void GravarCampo(string StringsConexao, string Tabela, string Criterio, string NomeCampo, string NovoConteudo)
        //{
        //    BD oBD = new BD(StringsConexao);
        //    oBD.Executa("UPDATE " + Tabela + " SET " + NomeCampo + " = " + NovoConteudo + " WHERE " + Criterio);
        //    oBD.Close();
        //}

        //public static string LerCampo(string StringsConexao, string Tabela, string Criterio, string NomeCampo)
        //{
        //    string Retorno;
        //    BD oBD = new BD(StringsConexao);
        //    Retorno = oBD.RetornaString("SELECT " + NomeCampo + " FROM " + Tabela + " WHERE " + Criterio);
        //    oBD.Close();
        //    return Retorno;
        //}

        //public static void IncluirRegistro(string StringsConexao, string Tabela, string Campos, string Valores)
        //{
        //    BD oBD = new BD(StringsConexao);
        //    oBD.Executa("INSERT INTO " + Tabela + "(" + Campos + ") VALUES (" + Valores + ")");
        //    oBD.Close();
        //}


        public static string LerStr(ref string Dados, string strPesquisa, ref int PosInicial)
        {
            int PosFinal;
            PosInicial = Dados.IndexOf(strPesquisa, PosInicial) + strPesquisa.Length;
            PosFinal = Dados.IndexOf('\r', PosInicial);
            return Dados.Substring(PosInicial, PosFinal - PosInicial);
        }


        //public static void Teclado(string Conteudo, int tempo)
        //{
        //    SendKeys.SendWait(Conteudo);
        //    Thread.Sleep(tempo);
        //}

        public static void WaitToLoad(string fileName, int milliseconds)
        {
            DateTime dtStart = DateTime.Now;
            TimeSpan ts = new TimeSpan(0);
            while (true)
            {
                try
                {
                    ts = DateTime.Now.Subtract(dtStart);
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    fs.Close();
                    return;
                }
                catch (FileNotFoundException)
                {
                    throw;
                }
                catch (IOException)
                {
                    if (ts.Milliseconds > milliseconds)
                        throw; //new FileNotLoaded(fileName, milliseconds, exc);

                    Thread.Sleep(2000);
                    continue;
                }
            }
        }

        public static string TiraAcento(string palavra)
        {
            string palavraSemAcento = null;
            string caracterComAcento = "·‡„‚‰ÈËÍÎÌÏÓÔÛÚıÙˆ˙˘˚¸Á¡¿√¬ƒ…» ÀÕÃŒœ”“’÷‘⁄Ÿ€‹«";
            string caracterSemAcento = "aaaaaeeeeiiiiooooouuuucAAAAAEEEEIIIIOOOOOUUUUC";

            for (int i = 0; i < palavra.Length; i++)
            {
                if (caracterComAcento.IndexOf(Convert.ToChar(palavra.Substring(i, 1))) >= 0)
                {
                    int car = caracterComAcento.IndexOf(Convert.ToChar(palavra.Substring(i, 1)));
                    palavraSemAcento += caracterSemAcento.Substring(car, 1);
                }
                else
                {
                    palavraSemAcento += palavra.Substring(i, 1);
                }
            }
            return palavraSemAcento;
        }

        public static void GravarParametro(string StringsConexao, string Nome, string Valor)
        {
            BD oBD = new BD(StringsConexao);

            if (!oBD.JaExiste("SGQ_Parametros", "Nome='" + Nome + "'"))
                oBD.GravarCampo("SGQ_Parametros", "Nome='" + Nome + "'", "Valor", "'" + Valor + "'");
            else
                oBD.IncluirRegistro("SGQ_Parametros", "Nome, Valor", "'" + Nome + "', '" + Valor + "'");

            oBD.Close();
        }
        public static string LerParametro(string StringsConexao, string Nome)
        {
            BD oBD = new BD(StringsConexao);
            string Retorno  = oBD.LerCampoCondicao("SGQ_Parametros", "Valor", "Nome='" + Nome + "'");
            oBD.Close();
            return Retorno;
        }

        public static void IncluirRegistro(string StringsConexao, string Tabela, string Campos, string Valores)
        {
            BD oBD = new BD(StringsConexao);
            oBD.IncluirRegistro(Tabela, Campos, Valores);
            oBD.Close();
        }

        public static void GravarCampo(string StringsConexao, string Tabela, string Criterio, string NomeCampo, string NovoConteudo)
        {
            BD oBD = new BD(StringsConexao);
            oBD.GravarCampo(Tabela, Criterio, NomeCampo, NovoConteudo);
            oBD.Close();
        }

        public static string LerCampoCondicao(string StringsConexao, string Tabela, string NomeCampo, string Condicao)
        {
            BD oBD = new BD(StringsConexao);
            string Retorno = oBD.LerCampoCondicao(Tabela, NomeCampo, Condicao);
            oBD.Close();
            return Retorno;
        }
        public static string LerCampoID(string StringsConexao, string Tabela, string NomeCampo, string ID)
        {
            BD oBD = new BD(StringsConexao);
            string Retorno = oBD.LerCampoID(Tabela, NomeCampo, ID);
            oBD.Close();
            return Retorno;
        }

        public static string UltimoID(string StringsConexao, string Tabela)
        {
            BD oBD = new BD(StringsConexao);
            string Retorno = oBD.RetornaString("SELECT Max(ID) FROM " + Tabela);
            oBD.Close();
            return Retorno;
        }

        public static bool ExcluirRegistro(string StringsConexao, string Tabela, string ID)
        {
            BD oBD = new BD(StringsConexao);
            bool Retorno = oBD.Executa("DELETE " + Tabela + " WHERE ID=" + ID);
            oBD.Close();
            return Retorno;
        }

        public static bool AlterarRegistro(string StringsConexao, string Tabela, string ID, string Campos, string Valores)
        {
            BD oBD = new BD(StringsConexao);
            bool Retorno = oBD.AlterarRegistro(Tabela, ID, Campos, Valores);
            oBD.Close();
            return Retorno;
        }

        public static bool ExecutaSql(string StringsConexao, string Sql)
        {
            BD oBD = new BD(StringsConexao);
            bool Retorno = oBD.Executa(Sql);
            oBD.Close();
            return Retorno;
        }

        public static string LerCampoSql(string StringsConexao, string Sql)
        {
            BD oBD = new BD(StringsConexao);
            string Retorno = oBD.RetornaString(Sql);
            oBD.Close();
            return Retorno;
        }

        public static void AnexarArquivo(string StringsConexao, string NomeTabela, string ID, string PastaGravacao, ref FileUpload FileUploadAnexos)
        {
            if (FileUploadAnexos.PostedFile.ContentLength > 0)
            {
                string IDFormatado = ID.PadLeft(10, '0');
                string CaminhoAnexo = Gerais.LerParametro(StringsConexao, "Local_Gravacao_Anexos") + "\\" + PastaGravacao;
                if (!System.IO.Directory.Exists(CaminhoAnexo))
                    System.IO.Directory.CreateDirectory(CaminhoAnexo);

                string StrFileName = FileUploadAnexos.PostedFile.FileName.Substring(FileUploadAnexos.PostedFile.FileName.LastIndexOf("\\") + 1);
                FileUploadAnexos.PostedFile.SaveAs(CaminhoAnexo + "\\" + IDFormatado + "_" + StrFileName);
                IncluirRegistro(StringsConexao, NomeTabela, "Versao,Nome", IDFormatado + ",'" + StrFileName + "'");
            }
        }

        public static bool isInt32(object value)
        {
            bool resposta = true;
            try
            {
                Int32 inteiro = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                resposta = false;
            }

            return resposta;
        }

        public static string SeVazioAltera(string Variavel, string Caracter)
        {
            if (Variavel != "")
                return Variavel;
            else
                return Caracter;
        }
    }
}
