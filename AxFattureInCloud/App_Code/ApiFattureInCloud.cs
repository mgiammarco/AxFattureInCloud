using System;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;

// purtroppo l'unico modo per fare il refactoring decentemente era andare con il pessimo flusso architetturale.
//	ergo è tutto ancora come prima se non per le api
public static class ApiFattureInCloud
{
	private static string ApiBase => ConfigurationManager.AppSettings["FattureInCloudApiBase"];
	private static string wsUrl => ConfigurationManager.AppSettings["WsUrl"];


	public static HttpResponseMessage GET ( string metodo, string bearer, string companyid = null, string parametri = "" )
	{
		string url = ApiBase + ( string.IsNullOrEmpty(companyid) ? "" : $"c/{companyid}/" ) + metodo + parametri;

		HttpClient client = new HttpClient();
		HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		{
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
		}

        HttpResponseMessage response = client.SendAsync(request).Result;

		MyDbUtility.scriviLog(response.Content?.ReadAsStringAsync().Result);

		if ( !response.IsSuccessStatusCode )
			HttpContext.Current.Response.Write("<p>[" + DateTime.Now + "] - [" + metodo + "] - [" + response.ReasonPhrase + "] - [" + response.RequestMessage + "]</p>");
		
		return response;
	}

	public static HttpResponseMessage POST ( string metodo, string bearer, StringContent body, string companyid = null, bool put = false )
	{
		string url = ApiBase + ( string.IsNullOrEmpty(companyid) ? "" : $"c/{companyid}/" ) + metodo;
			//wsUrl + metodo;
		HttpClient client = new HttpClient();
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		
		HttpResponseMessage response = (put ? client.PutAsync(url,body) : client.PostAsync(url,body)).Result;

		if ( !response.IsSuccessStatusCode )
			HttpContext.Current.Response.Write("<p>[" + DateTime.Now + "] - [" + metodo + "] - [" + response.ReasonPhrase + "] - [" + response.RequestMessage + "]</p>");
	
		return response;
	}


	#region CLIENTI

	private const string CLIENTI_PATH = "entities/clients";
	public static API_AnagraficaListaResponse ClientiLista ( API_AnagraficaListaRequest cliente )
	{

		API_AnagraficaListaResponse result = null;
		StringContent body = cliente.body();
		//POST("clienti/lista", body);
		HttpResponseMessage response = GET(CLIENTI_PATH, cliente.api_key, cliente.api_uid);
		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			result = JsonConvert.DeserializeObject<API_AnagraficaListaResponse>(json);
		}
		else
		{
			//HttpContext.Current.Response.Write("PostFornitore: " + response.ReasonPhrase);
			MyDbUtility.scriviLog("clienti/lista errore:" + response.ReasonPhrase);
		}

		return result;
	}

	public static API_AnagraficaNuovoSingoloResponse ClienteNuovo ( API_AnagraficaNuovoSingoloRequest cliente )
	{
		API_AnagraficaNuovoSingoloResponse  result = null;
		StringContent body = cliente.body();
		HttpResponseMessage response = POST(CLIENTI_PATH, cliente.api_key, body, cliente.api_uid);
		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog("clienti/nuovo = " + cliente.data.name + " " + json);
			result = JsonConvert.DeserializeObject<API_AnagraficaNuovoSingoloResponse>(json);
		}
		else
		{
			MyDbUtility.scriviLog("clienti/nuovo errore:" + response.ReasonPhrase);
			//HttpContext.Current.Response.Write("PostCliente: " + response.ReasonPhrase);
		}

		return result;
	}

	public static API_AnagraficaModificaSingoloResponse ClienteModifica ( API_AnagraficaNuovoSingoloRequest cliente )
	{
		API_AnagraficaModificaSingoloResponse result = null;
		StringContent body = cliente.body();

		HttpResponseMessage response = POST(CLIENTI_PATH + cliente.data.code, cliente.api_key, body, cliente.api_uid, true);

		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog("clienti/modifica = " + cliente.data.name + " " + json);
			result = JsonConvert.DeserializeObject<API_AnagraficaModificaSingoloResponse>(json);
		}
		else
		{
			MyDbUtility.scriviLog("clienti/modifica errore:" + response.ReasonPhrase);
			//HttpContext.Current.Response.Write("PostCliente: " + response.ReasonPhrase);
		}

		return result;
	}

	#endregion


	public static API_DocNuovoResponse FatturaNuovo ( Api_DocNuovoRequest fattura )
	{
		API_DocNuovoResponse result = null;

		StringContent body = fattura.body();
		//POST("fatture/nuovo", body);
		HttpResponseMessage response = POST("issued_documents", fattura.api_key, body, fattura.api_uid);
		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			result = JsonConvert.DeserializeObject<API_DocNuovoResponse>(json);
		}
		else
		{
			MyDbUtility.scriviLog(response.ReasonPhrase);
			//HttpContext.Current.Response.Write("PostCliente: " + response.ReasonPhrase);
		}

		return result;
	}





	public static API_FatturaListaResponse FattureLista ( API_FattureListaRequest fattura )
	{
		API_FatturaListaResponse result = null;
		StringContent body = fattura.body();
		//POST("fatture/lista", body);

		HttpResponseMessage response = GET("issued_documents", fattura.api_key, fattura.api_uid, "?type=invoice");

		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			result = JsonConvert.DeserializeObject<API_FatturaListaResponse>(json);
		}
		else
		{
			//HttpContext.Current.Response.Write("PostFornitore: " + response.ReasonPhrase);
			MyDbUtility.scriviLog("fatture/lista errore:" + response.ReasonPhrase);
		}

		return result;
	}


	public static API_FatturaListaResponse FattureInfo ( API_DocInfoRequest info )
	{
	
		API_FatturaListaResponse result = null;
		StringContent body = info.body();

		//POST("fatture/info", body);
		HttpResponseMessage response = GET("issued_documents", info.api_key, info.api_uid, "?type=invoice&sort=-numeration");

		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			result = JsonConvert.DeserializeObject<API_FatturaListaResponse>(json);
		}
		else
		{
			//HttpContext.Current.Response.Write("PostFornitore: " + response.ReasonPhrase);
			MyDbUtility.scriviLog("fatture/info errore:" + response.ReasonPhrase);
		}
	
		return result;
	}
}
