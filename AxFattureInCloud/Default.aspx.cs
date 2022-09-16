using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //StringContent stringContent = ApiFattureInCloud.getBodyInfo();
        //var obj =ApiFattureInCloud.PostInfoAccount(stringContent);
        //Response.Write(obj.ToString());

        //new WsFattureInCloud().CercaClientiSenzaFatture(3,2019);

        //Response.Write(new WsFattureInCloud().InviaAnagrafica(1, 9079));

        //new WsFattureInCloud().InviaFattura(1, 7778,1);
        //new WsFattureInCloud().InviaFatturaCorso(1, "7778");

        if (!string.IsNullOrEmpty(Request.QueryString["idcom"]) && !string.IsNullOrEmpty(Request.QueryString["idcli"]))
        {
            int idcom = int.Parse(Request.QueryString["idcom"]);
            int idcli = int.Parse(Request.QueryString["idcli"]);
            Response.Write(new WsFattureInCloud().InviaAnagrafica(idcom, idcli));

        }
        else if (!string.IsNullOrEmpty(Request.QueryString["idcom"]) && !string.IsNullOrEmpty(Request.QueryString["idbil"]))
        {
            int idcom = int.Parse(Request.QueryString["idcom"]);
            int idbil = int.Parse(Request.QueryString["idbil"]);
            //new WsFattureInCloud().InviaFattura(1, 7253);
            //7248
            Response.Write(new WsFattureInCloud().InviaFattura(idcom, idbil,0));

            //Response.Write(new WsFattureInCloud().InviaAnagrafica(1, 6092));
        }
        else if (!string.IsNullOrEmpty(Request.QueryString["idcom"]) && !string.IsNullOrEmpty(Request.QueryString["idcor"]))
        {
            int idcom = int.Parse(Request.QueryString["idcom"]);
            string idcor = Request.QueryString["idcor"];
            Response.Write(new WsFattureInCloud().InviaFatturaCorso(idcom, idcor));
        }
    }
}

