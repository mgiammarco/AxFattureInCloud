using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// Descrizione di riepilogo per AxFAttureInCloud
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente. 
// [System.Web.Script.Services.ScriptService]
public class WsFattureInCloud : System.Web.Services.WebService
{

    public WsFattureInCloud()
    {
        //Rimuovere il commento dalla riga seguente se si utilizzano componenti progettati 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }


    /// <summary>
    /// Esportazione dell'anagrafica gestisco su fatture in cloud
    /// </summary>
    /// <param name="idCompany">id azienda</param>
    /// <param name="idClient">codice cliente</param>
    /// <returns>IdAnagrafica</returns>
    [WebMethod]
    public string InviaAnagrafica(int idCompany,int idClient)
    {
        MyDbUtility.scriviLog("InviaAnagrafica = idCompany:" + idCompany + "|idBill:" + idClient);
        return new AxFattureConnector(idCompany).EsportaAnagrafica(idClient);
    }

    /// <summary>
    /// Esportazione fattura gestisco
    /// </summary>
    /// <param name="idCompany"></param>
    /// <param name="idBill"></param>
    /// <returns></returns>
    [WebMethod]
    public string InviaFattura(int idCompany, int idBill,int iddatifatturazione)
    {
        MyDbUtility.scriviLog("InviaFattura = idCompany:" + idCompany + "|idBill:" + idBill + "|iddatifatturazione:"+ iddatifatturazione);
        String strReturn="";
        try
        {
            strReturn = new AxFattureConnector(idCompany).EsportaFattura(idBill, iddatifatturazione);
        }
        catch (Exception ex)
        {

            MyDbUtility.scriviLog("Errore InviaFattura = idCompany:" + idCompany + "|idBill:" + idBill +"|" +ex.Message + "|" + ex.StackTrace);
        }
        
        MyDbUtility.scriviLog(strReturn);
        return strReturn;
        //return AxFattureConnector.inviaFatturaTest();
    }

    /// <summary>
    /// Esportazione fattura multicorso
    /// </summary>
    /// <param name="idCompany"></param>
    /// <param name="idBill_corsi">elenco (stringa csv) degli idbill da esportare</param>
    /// <returns></returns>
    [WebMethod]
    public string InviaFatturaCorso(int idCompany, string idBill_corsi)
    {
        MyDbUtility.scriviLog("InviaFatturaCorso = idCompany:" + idCompany + "|idBill_corsi:" + idBill_corsi);
        string strReturn = new AxFattureConnector(idCompany).EsportaFatturaCorso(idBill_corsi);
        MyDbUtility.scriviLog(strReturn);
        return strReturn;
        //return AxFattureConnector.inviaFatturaTest();
    }

    [WebMethod]
    public string CercaClientiSenzaFatture(int idCompany, int anno)
    {
        MyDbUtility.scriviLog("CercaClientiSenzaFatture");

        var fc = new AxFattureConnector(idCompany);
        API_AnagraficaListaResponse clienti=   fc.listaAnagrafica("", "");

        API_FatturaListaResponse fatture;
        fc.trovaFatture(anno,1,out fatture);
        if(fatture.lista_documenti.Count>0 && fatture.numero_pagine > fatture.pagina_corrente)
        {
            API_FatturaListaResponse fatture2;
            fc.trovaFatture(anno, 2, out fatture2);
            fatture.lista_documenti.AddRange(fatture2.lista_documenti);
        }

        List<AnagraficaCliente> clientiSenzaFatture = new List<AnagraficaCliente>() ;
        if (clienti.lista_clienti.Count>0 && fatture.lista_documenti.Count > 0) {
            foreach (var item in clienti.lista_clienti)
            {
                if (!fatture.lista_documenti.Any(w => w.id_cliente == item.id)) {
                    clientiSenzaFatture.Add(item);
                }
            }
        }

        string output = JsonConvert.SerializeObject(clientiSenzaFatture);

        return output;
        //return new AxFattureConnector(idCompany).EsportaFatturaCorso(idBill_corsi);
        //return AxFattureConnector.inviaFatturaTest();
    }

}
