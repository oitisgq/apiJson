using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections;
using System.Web.Configuration;
using System.IO;
using System.Text;
using Classes;


public static class Lib
{
    public static string SGQ()
    {
        if (Convert.ToBoolean(WebConfigurationManager.AppSettings["Producao"]))
            return WebConfigurationManager.ConnectionStrings["connectionStringSgq"].ToString();
        else
            return WebConfigurationManager.ConnectionStrings["connectionStringSgqDev"].ToString();
    }

    public static string BITI()
    {
        return @"Data Source=10.32.204.66\dbinst1,1440;Initial Catalog=BDGestaoTestes;Persist Security Info=True; Timeout=1000; User ID=SGTBD0;Password=GT_prd@teste";
    }

    public static string QC11()
    {
        return "Data Source=btdf5377.brasiltelecom.com.br:1530/QC11PRD1;Persist Security Info=True;User ID=QC11PRD1;Password=t1l1m5r#;Unicode=True";
    }

    public static string QC11SP()
    {
        //return "Data Source=btdf5377.brasiltelecom.com.br:1530/QC11PRD1;Persist Security Info=True;User ID=QC11PRD1;Password=tstqc11;Unicode=True";
        //return "Data Source=btdf5377.brasiltelecom.com.br:1530/QC11PRD1;Persist Security Info=True;User ID=OI273945;Password=1#alm110$d2;Unicode=True";
        return "Data Source=btdf5377.brasiltelecom.com.br:1530/QC11PRD1;Persist Security Info=True;User ID=OI273945;Password=t1l1m5r#;Unicode=True";

    }

    public static string STI()
    {
        return "Data Source=svrscn01:1529/stiweb;Persist Security Info=True;User ID=STICONSULTA;Password=STICONSULTA;Unicode=True";
    }

    public static string SistemasImpactados(string IDDemanda)
    {
        string Sql = "select (select RTRIM(LTRIM(Nome)) as Nome from Sistemas where Sistemas.ID = DemandasXSistemas.Sistema) as SistemaNome from DemandasXSistemas where Demanda = " + IDDemanda + " order by SistemaNome";

        BD oBD = new BD(Lib.SGQ());
        ArrayList ArraySistemas = new ArrayList();
        oBD.RetornaArrayList(Sql, ref ArraySistemas);
        oBD.Close();

        string ListaSistemas = "";

        for (int i = 0; i < ArraySistemas.Count; i++)
        {
            ListaSistemas = ListaSistemas + "'" + ArraySistemas[i].ToString() + "', ";
        }
        return ListaSistemas + "";
    }

    public static string ResponsaveisTecnicos(string IDDemanda)
    {
        string Sql = "select (select RTRIM(LTRIM(Nome)) as Nome from LideresTecnicos where LideresTecnicos.ID = DemandasXSistemas.LiderTecnico) as LiderTecnicoNome from DemandasXSistemas where Demanda = " + IDDemanda + " order by LiderTecnicoNome";

        BD oBD = new BD(Lib.SGQ());
        ArrayList ArrayLideresTecnicos = new ArrayList();
        oBD.RetornaArrayList(Sql, ref ArrayLideresTecnicos);
        oBD.Close();

        string ListaLideresTecnicos = "";

        for (int i = 0; i < ArrayLideresTecnicos.Count; i++)
        {
            ListaLideresTecnicos = ListaLideresTecnicos + "'" + ArrayLideresTecnicos[i].ToString() + "', ";
        }
        return ListaLideresTecnicos + "";
    }

    public static string FTs(string IDDemanda)
    {
        string Sql = "select (select RTRIM(LTRIM(Nome)) as Nome from Empresas where Empresas.ID = DemandasXFTsContratadas.FT) as FTNome from DemandasXFTsContratadas where DemandasXFTsContratadas.Demanda=" + IDDemanda + " order by FTNome";

        BD oBD = new BD(Lib.SGQ());
        ArrayList ArrayFTs = new ArrayList();
        oBD.RetornaArrayList(Sql, ref ArrayFTs);
        oBD.Close();

        string ListaFTs = "";

        for (int i = 0; i < ArrayFTs.Count; i++)
        {
            ListaFTs = ListaFTs + "'" + ArrayFTs[i].ToString() + "', ";
        }
        return ListaFTs + "";
    }

    //public static string AutenticaUsuario()
    //{
    //    if (Session["IDAtor"] == null)
    //    {
    //        Session["IDAtor"] = Gerais.LerCampoCondicao(Lib.SGQ(), "Atores", "ID", "LoginRede='" + Gerais.LerUsuarioLogadoRede() + "'");
    //        if (Session["IDAtor"].ToString() == "") Server.Transfer("Login.aspx");
    //    }   
    //}
}
