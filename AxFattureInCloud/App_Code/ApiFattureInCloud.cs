﻿using System;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using It.FattureInCloud.Sdk.Model;
using System.Text;

// purtroppo l'unico modo per fare il refactoring decentemente era andare con il pessimo flusso architetturale.
//	ergo è tutto ancora come prima se non per le api
public static class ApiFattureInCloud
{
	public const decimal IVA_MULTIPLIER = 1.22m;

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
        API_AnagraficaListaResponse result = new API_AnagraficaListaResponse();
		List<AnagraficaCliente> lcv = new List<AnagraficaCliente>();
		result.data = lcv;
		StringContent body = cliente.body();
		//POST("clienti/lista", body);
		//TODO siamo sicuri che sia l'unica query di cui abbiamo bisogno?
		HttpResponseMessage response = GET(CLIENTI_PATH, cliente.api_key, cliente.api_uid, 
			$"?q=vat_number%20%3D%20%27{cliente.piva}%27");
		if ( response.IsSuccessStatusCode )
		{
			var risposta = response.Content.ReadAsStringAsync();
			string json = risposta.Result;
			MyDbUtility.scriviLog(json);
			ListClientsResponse newResult = JsonConvert.DeserializeObject<ListClientsResponse>(json);
			
			foreach(var cn in newResult.Data)
			{
				AnagraficaCliente cv = new AnagraficaCliente();
				lcv.Add(cv);
				cv.id = cn.Id.ToString();
				cv.name=cn.Name;
				cv.address_street=cn.AddressStreet;
				cv.address_postal_code = cn.AddressPostalCode;
				cv.address_city= cn.AddressCity;	
				cv.address_province=cn.AddressProvince;
				cv.email=cn.Email;
				cv.phone=cn.Phone;	
				cv.vat_number=cn.VatNumber;
				cv.tax_code=cn.TaxCode;	


			}

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

        ModelClient entity = new ModelClient(
			//TODO attenzione chiedere perché non abbiamo l'informazione se il cliente è company o meno. Lascio company che è il caso più ovvio
			type: ClientType.Company,
			name: cliente.data.name,
		    vatNumber: cliente.data.vat_number,
			taxCode: cliente.data.tax_code,
			addressStreet: cliente.data.address_street,
			addressPostalCode: cliente.data.address_postal_code,
			addressCity: cliente.data.address_city,
			addressProvince: cliente.data.address_province
		);

		//TODO la response la lascio a voi che ho visto che avete fatto un lavoro sofisticato
        API_AnagraficaNuovoSingoloResponse  result = null;
        var dict = new Dictionary<string, object>();
        dict.Add("data", entity); //TODO controllare
        var jsonstr = JsonConvert.SerializeObject(dict);
        StringContent body = new StringContent(jsonstr, Encoding.UTF8, "application/json"); HttpResponseMessage response = POST(CLIENTI_PATH, cliente.api_key, body, cliente.api_uid);
        string json = response.Content.ReadAsStringAsync().Result;


        if (response.IsSuccessStatusCode)
        {
            MyDbUtility.scriviLog(json);
            var resultFromApi = JsonConvert.DeserializeObject<CreateIssuedDocumentResponse>(json);

            result = new API_AnagraficaNuovoSingoloResponse()
            {
                id = resultFromApi.Data.Id.Value.ToString(), //TODO controllare non so se toString sia il meglio
                success = true

            };
            //JsonConvert.DeserializeObject<API_DocNuovoResponse>(json);
        }
        else
        {

            result = new API_AnagraficaNuovoSingoloResponse()
            {
                error = response.StatusCode.ToString(),
                success = false

            };
            MyDbUtility.scriviLog(response.ReasonPhrase);
            //HttpContext.Current.Response.Write("PostCliente: " + response.ReasonPhrase);
        }


        return result;
	}

	public static API_AnagraficaModificaSingoloResponse ClienteModifica ( API_AnagraficaNuovoSingoloRequest cliente )
	{

         ModelClient entity = new ModelClient(
            //TODO attenzione chiedere perché non abbiamo l'informazione se il cliente è company o meno. Lascio company che è il caso più ovvio
            type: ClientType.Company,
            name: cliente.data.name,
            vatNumber: cliente.data.vat_number,
            taxCode: cliente.data.tax_code,
            addressStreet: cliente.data.address_street,
            addressPostalCode: cliente.data.address_postal_code,
            addressCity: cliente.data.address_city,
            addressProvince: cliente.data.address_province
	    );


        API_AnagraficaModificaSingoloResponse result = null;
        var dict = new Dictionary<string, object>();
        dict.Add("data", entity); //TODO controllare
        var jsonstr = JsonConvert.SerializeObject(dict);
        StringContent body = new StringContent(jsonstr, Encoding.UTF8, "application/json");
        //TODO la response la lascio a voi che ho visto che avete fatto un lavoro sofisticato

        HttpResponseMessage response = POST(CLIENTI_PATH + cliente.data.code, cliente.api_key, body, cliente.api_uid, true);

        string json = response.Content.ReadAsStringAsync().Result;


        if (response.IsSuccessStatusCode)
        {
            MyDbUtility.scriviLog(json);
            var resultFromApi = JsonConvert.DeserializeObject<CreateIssuedDocumentResponse>(json);

            result = new API_AnagraficaModificaSingoloResponse()
            {
                //id = resultFromApi.Data.Id.Value,
                success = true

            };
            //JsonConvert.DeserializeObject<API_DocNuovoResponse>(json);
        }
        else
        {

            result = new API_AnagraficaModificaSingoloResponse()
            {
                error = response.StatusCode.ToString(),
                success = false

            };
            MyDbUtility.scriviLog(response.ReasonPhrase);
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
				netPrice: Convert.ToDecimal(la.prezzo_netto),
				grossPrice: Convert.ToDecimal(la.prezzo_netto) * IVA_MULTIPLIER,
				//Convert.ToDecimal(la.prezzo_lordo),
                category: la.Categoria,
                discount: Convert.ToDecimal( la.Sconto),
                qty: Convert.ToDecimal( la.Quantita),
                vat: new VatType(
                    id: Convert.ToInt32(la.cod_iva)
                )
            ));
        }
		
        foreach (var lp in fattura.data.lista_pagamenti)
        {
            paymentsList.Add(new IssuedDocumentPaymentsListItem(
                amount: decimal.Parse(lp.Importo) * IVA_MULTIPLIER,
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
			UseGrossPrices = false
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
