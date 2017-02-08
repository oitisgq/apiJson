using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Classes
{
    public class Connection : IDisposable
    {
        public SqlConnection connection;

        private string ConnectionString;

        public Bancos Banco { get; set; }

        public Connection(Bancos banco = Bancos.Sgq)
        {
            this.Banco = banco;

            if (this.Banco == Bancos.Sgq)
                this.ConnectionString = ConfigurationManager.ConnectionStrings["connectionStringSgq"].ConnectionString;
            else
                this.ConnectionString = ConfigurationManager.ConnectionStrings["connectionStringBiti"].ConnectionString;

            this.connection = new SqlConnection(this.ConnectionString);
            this.connection.Open();
        }
        public List<T> Executar<T>(string sql)
        {
            SqlDataReader DataReader = this.Get_DataReader(sql);

            List<T> Lista = this.DataReaderMapToList<T>(DataReader);
            DataReader.Dispose();

            return Lista;
        }
        public int Executar(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return 0;

            var oCommand = new SqlCommand(sql);
            int retorno = Executar(oCommand);
            oCommand.Dispose();

            return retorno;
        }

        public int Executar(SqlCommand cmd)
        {
            int retorno = 0;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 0;
            try
            {
                if (this.connection.State != ConnectionState.Open)
                    this.connection.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception oEX)
            {
                retorno = -1;
                Gerais.Enviar_Email_Para_Administradores("SGQ - Executar - EXCEPTION - ", oEX.ToString().Replace("\n", "<br />") + "<br /><br />" + cmd.CommandText.Replace("\n", "<br />"));
            }

            return retorno;
        }
        public void Executar(SqlDataReader DataReader_Comandos, int Qte_Comandos)
        {
            StringBuilder Conjunto_Comandos = new StringBuilder();
            int cont = 0;

            while (DataReader_Comandos.Read())
            {
                Conjunto_Comandos.Append(DataReader_Comandos["Conteudo"].ToString());
                cont = cont + 1;
                if (cont >= Qte_Comandos)
                {
                    this.Executar(Conjunto_Comandos.ToString());
                    Conjunto_Comandos.Clear();
                    cont = 0;
                }
            }
            if (cont > 0)
                Executar(Conjunto_Comandos.ToString());
        }
        //public void Executar(ref OracleDataReader DataReader_Comandos, int Qte_Comandos)
        //{
        //    if (DataReader_Comandos == null || DataReader_Comandos.HasRows == false)
        //        return;

        //    if (Qte_Comandos <= 1)
        //    {
        //        while (DataReader_Comandos.Read())
        //        {
        //            try
        //            {
        //                this.Executar(DataReader_Comandos["Conteudo"].ToString());
        //            }
        //            catch (Exception oEX)
        //            {
        //                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - Executar ", oEX.ToString().Replace("\n", "<br />"));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        StringBuilder Conjunto_Comandos = new StringBuilder();
        //        int cont = 0;
        //        while (DataReader_Comandos.Read())
        //        {
        //            Conjunto_Comandos.Append(DataReader_Comandos["Conteudo"].ToString());
        //            cont = cont + 1;
        //            if (cont >= Qte_Comandos)
        //            {
        //                this.Executar(Conjunto_Comandos.ToString());
        //                Conjunto_Comandos.Clear();
        //                cont = 0;
        //            }
        //        }
        //        if (cont > 0)
        //            Executar(Conjunto_Comandos.ToString());
        //    }
        //}
        public void Executar(List<Comando> List_Comandos, int Qte_Comandos)
        {
            StringBuilder Conjunto_Comandos = new StringBuilder();
            int cont = 0;
            foreach (Comando oComando in List_Comandos)
            {
                if (oComando.Conteudo == null)
                   continue;

                Conjunto_Comandos.Append(oComando.Conteudo);

                cont = cont + 1;
                if (cont >= Qte_Comandos)
                {
                    this.Executar(Conjunto_Comandos.ToString());
                    Conjunto_Comandos.Clear();
                    cont = 0;
                }
            }

            if (cont > 0)
                Executar(Conjunto_Comandos.ToString());
        }
        public string Get_String(string sql)
        {
            var Retorno = "";
            var oSqlCommand = new SqlCommand(sql, connection);
            SqlDataReader oSqlDataReader = oSqlCommand.ExecuteReader();

            if (oSqlDataReader.Read())
                Retorno = oSqlDataReader[0].ToString();

            oSqlDataReader.Dispose();
            oSqlCommand.Dispose();
            return Retorno;
        }
        public string Get_String_Por_Id(string Tabela, string Campo, string Id)
        {
            var Retorno = "";
            var oSqlCommand = new SqlCommand("select " + Campo + " from " + Tabela + " where Id=" + Id, connection);
            SqlDataReader oSqlDataReader = oSqlCommand.ExecuteReader();

            if (oSqlDataReader.Read())
                Retorno = oSqlDataReader[0].ToString();

            oSqlDataReader.Dispose();
            oSqlCommand.Dispose();
            return Retorno;
        }
        public SqlDataReader Get_DataReader(string sql)
        {
            var oSqlCommand = new SqlCommand(sql, this.connection);
            oSqlCommand.CommandTimeout = 300;
            SqlDataReader DataReader = oSqlCommand.ExecuteReader();
            oSqlCommand.Dispose();
            return DataReader;
        }

        public DataTable Get_DataTable(String sql)
        {
            var oCommand = new SqlCommand(sql);
            DataTable oDataTable = Get_DataTable(oCommand);
            oCommand.Dispose();

            return oDataTable;
        }
        public DataTable Get_DataTable(SqlCommand cmd)
        {
            cmd.CommandType = CommandType.Text;
            cmd.Connection = this.connection;

            SqlDataAdapter oDataAdapter = new SqlDataAdapter();
            oDataAdapter.SelectCommand = cmd;

            DataTable oDataTable = new DataTable();
            try
            {
                if (this.connection.State != ConnectionState.Open)
                    this.connection.Open();

                oDataAdapter.Fill(oDataTable);
                return oDataTable;
            }
            catch
            {
                return null;
            }
            finally
            {
                oDataAdapter.Dispose();
            }
        }

        public int Set_Campos_Por_Id(string Tabela, string Id, string Campos, string Valores)
        {
            string[] vCampos = Campos.Split(',');
            string[] vValores = Valores.Split(',');
            string StringtSet = "";
            for (int i = 0; i < vCampos.GetLength(0); i++)
            {
                StringtSet += vCampos[i] + "=" + vValores[i] + ", ";
            }
            int TamanhoStringtSet = StringtSet.Length;
            if (TamanhoStringtSet > 0)
                StringtSet = StringtSet.Substring(0, TamanhoStringtSet - 2);

            return this.Executar("update " + Tabela + " set " + StringtSet + " where Id=" + Id);
        }
        public void Dispose()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }
        private List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();

                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }

    public class BD
    {
        private string _StringConexao = "";
        private SqlConnection _Conexao = null;

        public BD(string StringConexao)
        {
            _StringConexao = StringConexao;
            Open();
        }

        public SqlConnection GetConexao()
        {
            return _Conexao;
        }

        //private static string GetStringConexao()
        //{
        //    //return ConfigurationManager.AppSettings.Get("ConnectionStrings");
        //    //return "Description=BDGestaoRH;DRIVER=SQL Server;SERVER=ESCRITORIO;APP=Microsoft Office 2003;WSID=ESCRITORIO;DATABASE=BDGestaoRH;Network=DBMSLPCN;Trusted_Connection=Yes";
        //    //return "Data Source=ESCRITORIO;Initial Catalog=BDGestaoRH;Integrated Security=True; Provider=.NET Framework Data Provider for SQL Serve";
        //    //return "Provider=SQLNCLI.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BD;Data Source=(local)";
        //    //return @"Data Source=(local)\SQLEXPRESS;Initial Catalog=BDGestaoTestes;Integrated Security=True";
        //}

        public void Open()
        {
            if (_Conexao != null)
                if (_Conexao.State == ConnectionState.Open) return;

            _Conexao = new SqlConnection(_StringConexao);
            try
            {
                _Conexao.Open();
            }
            catch (Exception oEX)
            {
                throw new Exception(oEX.Message);
            }
        }

        public void Close()
        {
            if (_Conexao.State == ConnectionState.Open)
                _Conexao.Close();
        }

        private void VerificarConexao(IDbCommand cmd)
        {
            if ((_Conexao == null))
            {
                _Conexao = new SqlConnection(_StringConexao);
                _Conexao.Open();
            }
            else if ((_Conexao.State != ConnectionState.Open))
            {
                _Conexao.Open();
            }
            cmd.Connection = _Conexao;
            return;
        }

        public SqlCommand CriarComando(string cmdText, CommandType cmdType, params IDbDataParameter[] parameters)
        {

            SqlCommand cmd = new SqlCommand(cmdText);

            cmd.CommandType = cmdType;

            if ((parameters != null))
            {
                foreach (SqlParameter param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }
            VerificarConexao(cmd);
            cmd.CommandTimeout = 1000;
            return cmd;
        }

        public int ExecuteNonQuery(string cmdText, CommandType cmdType, params IDbDataParameter[] parameters)
        {
            int Retorno = 0;
            IDbCommand cmd = CriarComando(cmdText, cmdType, parameters);
            try
            {
                Retorno = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Retorno = -1;
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - ExecuteNonQuery ", ex.ToString().Replace("\n", "<br />") + "<br /><br />" + cmdText.Replace("\n", "<br />"));
            }
            finally
            {
                cmd.Dispose();
                this.Close();
            }
            return Retorno;
        }
        public int ExecuteNonQuery(string cmdText, CommandType cmdType)
        {
            return ExecuteNonQuery(cmdText, cmdType, null);
        }

        public int Executar(string cmdText)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, null);
        }
        public void RetornaHashtable(ref SqlCommand cmd, ref Hashtable oHashtable)
        {
            this.Open();
            cmd.Connection = _Conexao;

            SqlDataReader dr;
            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    oHashtable.Add(dr[0].ToString(), dr[1].ToString());
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - RetornaHashtable " + cmd.CommandText, ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                this.Close();
            }
        }

        public void RetornaHashtable(string sql, ref Hashtable oHashtable)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaHashtable(ref cmd, ref oHashtable);
        }

        public void RetornaArrayList(ref SqlCommand cmd, ref ArrayList Array)
        {
            this.Open();
            cmd.Connection = _Conexao;
            SqlDataReader dr;
            try
            {
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Array.Add(dr[0].ToString());
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - RetornaArrayList " + cmd.CommandText, ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                this.Close();
            }
        }

        public void RetornaArrayList(string sql, ref ArrayList Array)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaArrayList(ref cmd, ref Array);
        }

        public void RetornaString(ref SqlCommand cmd, ref string returnString)
        {
            this.Open();
            cmd.Connection = _Conexao;
            SqlDataReader dr;
            try
            {
                dr = cmd.ExecuteReader();
                if (dr.Read())
                    returnString = dr[0].ToString();

                dr.Close();
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - RetornaString: ", "SQL:\n\n" + cmd.CommandText + "\n\n\nMensagem:\n\n" + ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                this.Close();
            }
        }

        public void RetornaString(string sql, ref string returnString)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaString(ref cmd, ref returnString);
        }

        public string RetornaString(string sql)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;

            string VarString = "";

            RetornaString(ref cmd, ref VarString);
            if (!VarString.Equals(""))
                return VarString;
            else
                return "";
        }

        public void RetornaLong(ref SqlCommand cmd, ref long VarLong)
        {
            string VarString = "";
            RetornaString(ref cmd, ref VarString);
            if (!VarString.Equals(""))
                VarLong = Convert.ToInt64(VarString);
        }

        public void RetornaLong(string sql, ref long VarLong)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaLong(ref cmd, ref VarLong);
        }

        public long RetornaLong(string sql)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;

            string VarString = "";

            RetornaString(ref cmd, ref VarString);
            if (!VarString.Equals(""))
                return Convert.ToInt64(VarString);
            else
                return 0;
        }

        public void RetornaDataTable(ref SqlCommand cmd, ref DataTable returnDataTable)
        {
            this.Open();
            cmd.Connection = _Conexao;
            SqlDataAdapter da = new SqlDataAdapter(cmd);

            try
            {
                da.Fill(returnDataTable);
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - RetornaDataTable " + cmd.CommandText, ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                da.Dispose();
                this.Close();
            }
        }

        public void RetornaDataTable(string sql, ref DataTable returnDataTable)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaDataTable(ref cmd, ref returnDataTable);
        }

        public void RetornaDataSet(ref SqlCommand cmd, ref DataSet returnDataSet, string nameTableDataSet)
        {
            this.Open();
            cmd.Connection = _Conexao;

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            try
            {
                da.Fill(returnDataSet, nameTableDataSet);
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.ToString());
                Gerais.Enviar_Email_Para_Administradores("SGQ - ALM - EXCEPTION - RetornaDataSet " + cmd.CommandText, ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                da.Dispose();
                this.Close();
            }
        }

        public void RetornaDataSet(string sql, ref DataSet returnDataSet, string nameTableDataSet)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            RetornaDataSet(ref cmd, ref returnDataSet, nameTableDataSet);
        }

        public bool Executa(string sql)
        {
            return ExecuteNonQuery(sql, CommandType.Text) != -1;
        }

        public bool IncluirRegistro(string Tabela, string Campos, string Valores)
        {
            return this.Executa("INSERT INTO " + Tabela + "(" + Campos + ") VALUES (" + Valores + ")");
        }

        public bool AlterarRegistro(string Tabela, string ID, string Campos, string Valores)
        {
            string[] vCampos = Campos.Split(',');
            string[] vValores = Valores.Split(',');
            string StringtSet = "";
            for (int i = 0; i < vCampos.GetLength(0); i++)
            {
                StringtSet += vCampos[i] + "=" + vValores[i] + ", ";
            }
            int TamanhoStringtSet = StringtSet.Length;
            if (TamanhoStringtSet > 0)
                StringtSet = StringtSet.Substring(0, TamanhoStringtSet - 2);

            return this.Executa("UPDATE " + Tabela + " SET " + StringtSet + " WHERE ID=" + ID);
        }

        public bool ExcluirRegistro(string Tabela, string ID)
        {
            return this.Executa("DELETE " + Tabela + " WHERE ID=" + ID);
        }

        public string LerCampoID(string Tabela, string NomeCampo, string ID)
        {
            return this.RetornaString("SELECT " + NomeCampo + " FROM " + Tabela + " WHERE ID=" + ID);
        }

        public string LerCampoCondicao(string Tabela, string NomeCampo, string Condicao)
        {
            return this.RetornaString("SELECT " + NomeCampo + " FROM " + Tabela + " WHERE " + Condicao);
        }

        public void GravarCampo(string Tabela, string Criterio, string NomeCampo, string NovoConteudo)
        {
            this.Executa("UPDATE " + Tabela + " SET " + NomeCampo + " = " + NovoConteudo + " WHERE " + Criterio);
        }

        public string UltimoID(string Tabela)
        {
            return this.RetornaString("SELECT Max(ID) FROM " + Tabela);
        }

        public string ID(string Tabela, string Condicao, string IDRegAtual)
        {
            string Sql = "SELECT ID FROM " + Tabela + " WHERE " + Condicao;

            if (IDRegAtual != "0")
                Sql += " and ID <> " + IDRegAtual;

            return this.RetornaString(Sql);
        }

        public string ID(string Tabela, string Condicao)
        {
            return ID(Tabela, Condicao, "0");
        }

        public bool Achou(string sql)
        {
            string Retorno = "";
            RetornaString(sql, ref Retorno);
            return Retorno != "" ? true : false;
        }

        public bool JaExiste(string Tabela, string Condicao, string IDRegAtual)
        {
            return ID(Tabela, Condicao, IDRegAtual) != "" ? false : true;
        }

        public bool JaExiste(string Tabela, string Condicao)
        {
            return ID(Tabela, Condicao, "0") != "" ? false : true;
        }
    }
}
