using System;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using It.FattureInCloud.Sdk.Model;
using System.Net;
using System.Data.Entity.Infrastructure;
using System.Text;

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
			HttpContext.Current.Response.Write("<p>[" + DateTime.Now + "] - [" + metodo + "] - [" + response.ReasonPhrase + "] - [" + response.Content.ReadAsStringAsync().Result + "]</p>");
	
		return response;
	}


	#region CLIENTI

	private const string CLIENTI_PATH = "entities/clients";
	public static API_AnagraficaListaResponse ClientiLista ( API_AnagraficaListaRequest cliente )
	{

		API_AnagraficaListaResponse result = null;
		StringContent body = cliente.body();
		//POST("clienti/lista", body);
		HttpResponseMessage response = GET(CLIENTI_PATH, cliente.api_key, cliente.api_uid, 
			$"?q=vat_number%20%3D%20%27{cliente.piva}%27");
		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			result = JsonConvert.DeserializeObject<API_AnagraficaListaResponse>(json);
			result.success = true;
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

		Entity entity = new Entity(
		   id: int.Parse( fattura.data.id_cliente ),
		   name: fattura.data.Nome,
		   vatNumber: fattura.data.Piva,
		   taxCode: fattura.data.Cf,
		   addressStreet: fattura.data.indirizzo_via,
		   addressPostalCode: fattura.data.indirizzo_cap.ToString(),
		   addressCity: fattura.data.indirizzo_citta,
		   addressProvince: fattura.data.indirizzo_provincia,
		   country: "Italia"
	   );


		var itemList = new List<IssuedDocumentItemsListItem>();
        var paymentsList = new List<IssuedDocumentPaymentsListItem>();

        foreach (var la in fattura.data.lista_articoli)
        {
            itemList.Add(new IssuedDocumentItemsListItem(
                productId: (int?)la.Id, // Nella logica precedente è un Long, quando nel tipo nuovo è un int, dipende dai valori, ma è rischioso a causa di overflow
                code: la.Codice,
                name: la.Nome,
                //netPrice: ,
				grossPrice: Convert.ToDecimal(la.prezzo_netto),
                category: la.Categoria,
                discount: Convert.ToDecimal( la.Sconto),
                qty: Convert.ToDecimal( la.Quantita)
                //vat: new VatType(
                //    id: Convert.ToInt32(la.cod_iva)
                //)
            ));
        }
		
        foreach (var lp in fattura.data.lista_pagamenti)
        {
            paymentsList.Add(new IssuedDocumentPaymentsListItem(
                amount: decimal.Parse(lp.Importo),
                dueDate: DateTime.Parse(lp.data_scadenza),
                paidDate: DateTime.Parse(lp.data_saldo),
                status: IssuedDocumentStatus.NotPaid
			));
                // List your payment accounts: https://github.com/fattureincloud/fattureincloud-csharp-sdk/blob/master/docs/InfoApi.md#listPaymentAccounts
                //paymentAccount: new PaymentAccount(id: 110)));
        }
		
		

		IssuedDocument invoice = new IssuedDocument(
			type: IssuedDocumentType.Invoice,
			entity: entity,
			date: DateTime.Parse( fattura.data.Data),
			number: int.Parse(fattura.data.Numero),
			numeration: "/fatt",
			subject: fattura.data.oggetto_interno,
			visibleSubject: fattura.data.oggetto_visibile,
			currency: new Currency(
				id: "EUR"
			),
			language: new Language(
				code: "it",
				name: "italiano"
			),
			itemsList: itemList,
			paymentsList: paymentsList,
			// Here we set e_invoice and ei_data
			eInvoice: true,
			eiData: new IssuedDocumentEiData(
				paymentMethod: "MP05"
			)
		)
		{
			UseGrossPrices = true
		};

		var dict = new Dictionary<string, object>();
		dict.Add("data", invoice);
		var jsonstr = JsonConvert.SerializeObject(dict);
        StringContent body = new StringContent(jsonstr, Encoding.UTF8, "application/json");


		//POST("fatture/nuovo", body);
		HttpResponseMessage response = POST("issued_documents", fattura.api_key, body, fattura.api_uid);
		string json  = response.Content.ReadAsStringAsync().Result;

        
		if (response.IsSuccessStatusCode)
		{
			MyDbUtility.scriviLog(json);
			var resultFromApi = JsonConvert.DeserializeObject<CreateIssuedDocumentResponse>(json);

			result = new API_DocNuovoResponse()
			{
				new_id = resultFromApi.Data.Id.Value,
				success = true
				 
			};
			//JsonConvert.DeserializeObject<API_DocNuovoResponse>(json);
		}
		else
		{

			result = new API_DocNuovoResponse()
			{
				error = response.StatusCode.ToString(),
				success = false
                
			};
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
