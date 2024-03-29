﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

public class AxFattureConnector
{
    public int idCorporate { get; set; }

	public static int iva = int.Parse(ConfigurationManager.AppSettings["iva"]);
    public static double ivaMultiplier = (100 + iva)/100d; // = 1.22

    public static string[] Conf_Payment = ConfigurationManager.AppSettings["PAYMENT"].Split(',');
    public static string[] Conf_Bank = ConfigurationManager.AppSettings["BANK"].Split(',');
    public static string[] Conf_Iban = ConfigurationManager.AppSettings["IBAN"].Split(',');


	public AxFattureConnector(int idCorporate)
    {
	    this.idCorporate = idCorporate;
    }

    private void impostaChiavi(api_generic_request api) {
	    if (this.idCorporate != 0)
	    {
		    api.api_uid = ConfigurationManager.AppSettings[$"api_uid_{this.idCorporate}"];
		    api.api_key = ConfigurationManager.AppSettings[$"api_key_{this.idCorporate}"];
	    }
    }

    public bool trovaAnagrafica(string piva, string cf,out string idCliente)
    {

        API_AnagraficaListaRequest filtro = new API_AnagraficaListaRequest() { piva = piva, cf = cf, pagina = 1 };
        impostaChiavi(filtro);

        API_AnagraficaListaResponse RES = ApiFattureInCloud.ClientiLista(filtro);
        if (RES.success && RES.data.Count > 0)
        {
            //idCliente = RES.data[0].code;
            idCliente = RES.data[0].id;
            return true;
        }
        else
        {
            idCliente = "";
            return false;
        }
    }

    public API_AnagraficaListaResponse listaAnagrafica(string piva, string cf)
    {

        API_AnagraficaListaRequest filtro = new API_AnagraficaListaRequest() { piva = piva, cf = cf, pagina = 1 };
        impostaChiavi(filtro);

        API_AnagraficaListaResponse RES = ApiFattureInCloud.ClientiLista(filtro);
        if (RES.success && RES.data.Count > 0)
        {
            return RES;
            
        }
        else
        {
            return null;
        }
    }

    public bool trovaFatture(int anno,int pagina, out API_FatturaListaResponse fatture) {
        API_FattureListaRequest filtro = new API_FattureListaRequest() { anno = anno, pagina=pagina };
        impostaChiavi(filtro);

        API_FatturaListaResponse RES = ApiFattureInCloud.FattureLista(filtro);
        if (RES.success && RES.data.Count > 0)
        {
            fatture = RES;
            return true;
        }
        else
        {
            fatture = null;
            return false;
        }

    }

    public string EsportaAnagrafica(int idClient)
    {
        using (EntitiesGestisco db = new EntitiesGestisco())
        {

            TCLIENT client = db.TCLIENT.Where(c => c.IDCLIENT == idClient && c.IDCLIENT_CORPORATE == idCorporate).FirstOrDefault();
            return EsportaAnagrafica(client);
        }
    }

    public string EsportaAnagrafica(TCLIENT client)
    {
        if (client != null && (client.VATNUMBER != "" || client.FISCAL_CODE != ""))
        {
            string idAnagrafica = "";
            if (!trovaAnagrafica(client.VATNUMBER.Trim(), client.FISCAL_CODE.Trim(), out idAnagrafica))
            {
                idAnagrafica = inviaAnagrafica(client);
                return idAnagrafica;
            }
            else
            {

                /*ANAGRAFICA GIA ESISTENTE LA AGGIORNO */
                //if (this.idCorporate!=3){SOLO PER AXT/PGA
                aggiornaAnagrafica(idAnagrafica,client);
                //}
                
                return idAnagrafica;
            }
        }
        
        return "";
    }

    public string EsportaAnagraficaCorso(API_AnagraficaNuovoSingoloRequest client)
    {
        if (client != null && (client.data.vat_number != "" || client.data.tax_code != ""))
        {
            string idAnagrafica = "";
            if (!trovaAnagrafica(client.data.vat_number.Trim(), client.data.tax_code.Trim(), out idAnagrafica))
            {
                idAnagrafica = inviaAnagrafica(client);
                return idAnagrafica;
            }
            else
            {

                /*ANAGRAFICA GIA ESISTENTE LA AGGIORNO */
                //if (this.idCorporate!=3){SOLO PER AXT/PGA
                aggiornaAnagraficaCorso(idAnagrafica, client);
                //}

                return idAnagrafica;
            }
        }

        return "";
    }


    public string inviaAnagrafica(API_AnagraficaNuovoSingoloRequest cliente)
    {
        // pec??
        
        impostaChiavi(cliente);
        API_AnagraficaNuovoSingoloResponse clientesalvato = ApiFattureInCloud.ClienteNuovo(cliente);
        return clientesalvato.id;
    }

    public bool modificaAnagrafica(API_AnagraficaModificaSingoloRequest cliente)
    {
        // pec??
        impostaChiavi(cliente);

        API_AnagraficaModificaSingoloResponse clientesalvato = ApiFattureInCloud.ClienteModifica(cliente);
        return clientesalvato.success ;
    }

    private string getEmail(string emailAmm,string email1,string email2) {
        //preleva prima la mail amministrativa, poi la mail normale, altrimenti la secondaria

        if (!string.IsNullOrEmpty(emailAmm)) return emailAmm;
        if (!string.IsNullOrEmpty(email1)) return email1;
        if (!string.IsNullOrEmpty(email2)) return email2;

        return "";
    }

    public string inviaAnagrafica(TCLIENT client)
    {
        
        string email = getEmail(client.EMAIL3,client.EMAIL,client.EMAIL2);

        API_AnagraficaNuovoSingoloRequest cliente = new API_AnagraficaNuovoSingoloRequest()
        {
            data = new AnagraficaCliente()
            {
	            tax_code = (""+client.FISCAL_CODE).Trim(),
	            vat_number = (""+client.VATNUMBER).Trim() ,
	            address_postal_code = client.ZIPCODE,
	            address_city = client.LOCALITY,
	            address_province = (client.TREGISTRY_PROVINCE!=null?client.TREGISTRY_PROVINCE.PROVINCEACRONYM:""),
	            address_street = client.ADDRESS,
	            email = email, /*client.EMAIL,*/
	            name = client.NAME
	            //paese_iso =(client.TREGISTRY_STATE!=null?client.TREGISTRY_STATE.STATECODE:"IT"),
	            //PA = true, //???
	            //PA_codice = client.CODICEFATTURAZIONE,
	            //phone = client.PHONE1,
	            //termini_pagamento = 30 //TODO: termini di pagamento ?
	            , 
	            //extra = client.IDCLIENT.ToString(),
	        }
	    };

        return inviaAnagrafica(cliente);

        //    // pec??
        //    // codice fatturazione? 
        //    impostaChiavi(cliente);
        //    API_AnagraficaNuovoSingoloResponse clientesalvato = ApiFattureInCloud.PostCliente(cliente);
        //    return clientesalvato.id;
        //
    }


    public static string inviaAnagraficaTest()
    {

        API_AnagraficaNuovoSingoloRequest cliente = new API_AnagraficaNuovoSingoloRequest()
        {
			data = new AnagraficaCliente()
			{
	            tax_code = "SMAMRC82T07C573Y",
	            address_postal_code = "47521",
	            address_city= "Forlì",
	            address_province= "FC",
	            address_street = "Via antonio meucci 31",
	            email = "msama@axterisco.it",
	            name = "Marco Sama"
	            //paese_iso = "IT",
	            //PA = false,
	            //phone = "348123456",
	            //termini_pagamento = 30
	        }
        };
        API_AnagraficaNuovoSingoloResponse clientesalvato = ApiFattureInCloud.ClienteNuovo(cliente);
        return clientesalvato.id;
    }

    public string EsportaFattura(int idBill,int iddatifatturazione)
    {
        using (EntitiesGestisco db = new EntitiesGestisco())
        {
            TCLIENT_BILL bill = db.TCLIENT_BILL.Where(b => b.IDBILL == idBill && b.IDBILL_COMPANY == idCorporate).FirstOrDefault();
            TCLIENT client = bill.TCLIENT;

            if (iddatifatturazione> 0){
                //SE PRESENTE, SOSTITUISCO I DATI DI FATTURAZIONE CON QUELLI SPECIFICI DELLA FATTURA
                TCLIENT_DATI_FATTURAZIONE cd = db.TCLIENT_DATI_FATTURAZIONE.Where(w => w.IDDATI_FATTURAZIONE == iddatifatturazione && w.DATI_FATTURAZIONEHIDE==false).FirstOrDefault();
                client.NAME = cd.NAME;
                client.ADDRESS = cd.ADDRESS;
                client.LOCALITY = cd.LOCALITY;
                client.ZIPCODE = cd.ZIPCODE;
                client.IDCLIENT_PROVINCE = cd.IDDATI_FATTURAZIONE_PROVINCE;
                client.VATNUMBER = cd.VATNUMBER;
                client.FISCAL_CODE = cd.FISCAL_CODE;
                client.CODICEFATTURAZIONE = cd.CODICEFATTURAZIONE;
            }

            //SE NON C'E IL CODICE SDI BLOCCO L'INVIO
			if (client == null )return "CLIENTE NON TROVATO" ;
            if (client != null && string.IsNullOrEmpty(client.CODICEFATTURAZIONE)) return "CODICE SDI NON PRESENTE.";
			//if(client.CLIENT_ACCOUNTING(bill.IDBILL_COMPANY).ESENZIONEIVA==null) return "ESENZIONEIVA nullo";
            //string nextBill = ProssimaNumerazioneFattura(bill.IDBILL_YEAR);
            //int iNextBillNumber = 0;

            //if (int.TryParse(nextBill, out iNextBillNumber))
            //{
            //    if (iNextBillNumber != bill.BILL_NUM) return "NUMERO FATTURA NON VALIDO, PREVISTO:" + iNextBillNumber;
            //}
            //else{
            //    return "IMPOSSIBILE VERIFICARE NUMERAZIONE FATTURE";
            //}

            int iva = AxFattureConnector.iva  ;
            if (client.TCLIENT_ACCOUNTING !=null && client.TCLIENT_ACCOUNTING.Count >0 && client.CLIENT_ACCOUNTING(bill.IDBILL_COMPANY).ESENZIONEIVA) iva = 0;

            //richiama stored procedure che calcola i dettagli delle righe della fattura
            List<udfTCLIENT_BILL_YM_Result> bd = db.udfTCLIENT_BILL_YM(bill.IDBILL_COMPANY, 
                                                                        bill.IDBILL_CLIENT, 
                                                                        bill.IDBILL, 
                                                                        bill.IDBILL_MONTH, 
                                                                        bill.IDBILL_YEAR, 
                                                                        iva).ToList();

            //esporta l'anagrafica solo se ha codfisc o piva
            if (client != null && (client.VATNUMBER != "" || client.FISCAL_CODE != ""))
            {
                //esporta solo se non è già presente nel database cloud
                string idAnagrafica = EsportaAnagrafica(client);

                if (idAnagrafica != "")//cliente esportato, esporto la fattura
                {
                    return EsportaFattura(bd, bill, idAnagrafica);
                }
            }
            else {
                return "PARTITA IVA O CODICE FISCALE MANCANTI.";
            }
        }
        return "";
    }

    public string EsportaFattura(List<udfTCLIENT_BILL_YM_Result> billDetails, TCLIENT_BILL billHead, string idAnagrafica)
    {

        TCLIENT_ACCOUNTING CLIENT_ACCOUNTING = billHead.TCLIENT.CLIENT_ACCOUNTING(billHead.IDBILL_COMPANY);
        string iban = (CLIENT_ACCOUNTING!=null?CLIENT_ACCOUNTING.IBAN:"");
        string pa_modalita_pagamento = "";
        string modalita_pagamento = "";
        bool splitpayment = false;
        
        if (string.IsNullOrEmpty(iban)) {
            // non è stato indicato iban quindi metto iban default
            iban = Conf_Iban[this.idCorporate];
        }
        

        //verifico la modalità di pagamento del cliente
        if (billHead.TCLIENT.TCLIENT_ACCOUNTING != null && CLIENT_ACCOUNTING!= null && !string.IsNullOrEmpty(CLIENT_ACCOUNTING.PAYMENT))
        {
            pa_modalita_pagamento = (CLIENT_ACCOUNTING.PAYMENT.ToLower().Contains("bonifico") ? Metodo_Pagamento_Bonifico : Metodo_Pagamento_Riba);
            modalita_pagamento = CLIENT_ACCOUNTING.PAYMENT;
            if (!string.IsNullOrEmpty(CLIENT_ACCOUNTING.SPLIT_PAYMENT)) { splitpayment = true; }
        }
        else {
            //se non presente vado sui valori di default, se il cliente non ha iban indicato: USO BONIFICO; altrimenti RIBA
            pa_modalita_pagamento = (iban != "" ? Metodo_Pagamento_Bonifico : Metodo_Pagamento_Riba);
            modalita_pagamento = "bonifico";
        }

        //modalita_pagamento = "1514954";

        Api_DocNuovoRequest fattura = new Api_DocNuovoRequest() { data = new Doc()
        {
            id_cliente = idAnagrafica,
            autocompila_anagrafica = true,
            Nome = billHead.TCLIENT.NAME,
            prezzi_ivati = false,
            Data = billHead.BILL_DATE.ToString("dd/MM/yyyy"),
            metodo_pagamento= modalita_pagamento,//billHead.TCLIENT.TCLIENT_ACCOUNTING.PAYMENT,
            metodo_titolo1 ="IBAN",
            metodo_desc1=iban,
            mostra_info_pagamento=true,
            Pa = true,
            PA_tipo_cliente = "B2B",
            PA_iban = iban,
            PA_modalita_pagamento= pa_modalita_pagamento,
            Numero = billHead.BILL_NUM.ToString()  ,
            split_payment = splitpayment
        }};


        fattura.data.lista_articoli = new List<ListaArticoli>();
        fattura.data.lista_pagamenti = new List<ListaPagamenti>();

        int nOrd = 0;
        double fattTot = 0;
        foreach (udfTCLIENT_BILL_YM_Result bill in billDetails)
        {
            ListaArticoli art = new ListaArticoli
            {
                Ordine = nOrd++,
                Tassabile = (bill.VAT > 0 ? true : false),
                Codice = bill.IDACTIVITY_TYPE.ToString(),//"", ////TODO: cosa ci mettiamo?
                Quantita = 1,                
                prezzo_netto = (double)bill.DETAIL_PRICE
            };

            fattTot = art.prezzo_netto;
            art.Categoria = bill.IDACTIVITY_TYPE.ToString();
            if (bill.DETAIL.Contains("|"))
            {
                art.Nome = bill.DETAIL.Split('|')[0];
                art.Descrizione= bill.DETAIL.Split('|')[1];
            }
            else {
                art.Nome = bill.DETAIL;
            }
            
            
            //art.Descrizione = ;

            fattura.data.lista_articoli.Add(art);

            //fattura.lista_pagamenti.Add(new ListaPagamenti
            //{
            //    Importo = (long)(bill.DETAIL_PRICE * ivaMultiplier),
            //    Metodo = "not",
            //    data_scadenza = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"),////TODO: cosa ci metto ?
            //    data_saldo = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy") ////TODO:  cosa ci metto?
            //});
        }
        udfTCLIENT_BILL_YM_Result billExtra = billDetails.First();
        if (billExtra.RIMBORSOKM)
        {
            ListaArticoli art = new ListaArticoli
            {
                Ordine = nOrd++,
                Tassabile = true,//(billExtra.VAT > 0 ? true : false),
                Codice = "", ////TODO: cosa ci mettiamo?
                Quantita = 1,
                prezzo_netto = (double)billExtra.VALORE_RIMBORSO,
                Nome = "Rimborso spese di trasferta",
                Descrizione = billExtra.DESCRIZIONE_RIMBORSO
            };
            fattura.data.lista_articoli.Add(art);
            //fattura.lista_pagamenti.Add(new ListaPagamenti
            //{
            //    Importo = (long)(art.prezzo_netto * ivaMultiplier),
            //    Metodo = "not",
            //    data_scadenza = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"),////TODO: cosa ci metto ?
            //    data_saldo = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy") ////TODO:  cosa ci metto?
            //});
            fattTot += art.prezzo_netto;
        }

        if (billExtra.SPESEINCASSO.HasValue && billExtra.SPESEINCASSO.Value) 
        {
            ListaArticoli art = new ListaArticoli
            {
                Ordine = nOrd++,
                Tassabile = (billExtra.VAT > 0 ? true : false),
                Codice = "", ////TODO: cosa ci mettiamo?
                Quantita = 1,
                prezzo_netto = (double)billExtra.SPESEINCASSOVAL,
                Nome = "Spese di Incasso"

            };
            fattura.data.lista_articoli.Add(art);

            //fattura.lista_pagamenti.Add(new ListaPagamenti
            //{
            //    Importo = (long)(art.prezzo_netto * ivaMultiplier),
            //    Metodo = "not",
            //    data_scadenza = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"),////TODO: cosa ci metto ?
            //    data_saldo = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy") ////TODO:  cosa ci metto?
            //});
            fattTot += art.prezzo_netto;
        }


        var importTOT = (billExtra.SPESEINCASSO ?? false) ?
            ((decimal)billHead.BILL_IMPORT + billExtra.SPESEINCASSOVAL) : (decimal)billHead.BILL_IMPORT;
        fattura.data.lista_pagamenti.Add(new ListaPagamenti
        {
            Importo = importTOT.ToString(),
            Metodo = "not",
            data_scadenza = billHead.BILL_DATE.ToString("dd/MM/yyyy"),////TODO: cosa ci metto ?
            data_saldo = billHead.BILL_DATE.ToString("dd/MM/yyyy") ////TODO:  cosa ci metto?
        });



        //}
        //fattura.lista_articoli;
        //fattura.lista_pagamenti 
        impostaChiavi(fattura);

        API_DocNuovoResponse newdoc = ApiFattureInCloud.FatturaNuovo(fattura);
        if (newdoc.success)
        {
            return newdoc.new_id.ToString();
        }
        else
        {
            return newdoc.error ;
        }
        //return newdoc.new_id;
    }


    public string EsportaFatturaCorso(string BillingList)
    {
        string idAnagrafica = "";
        string iban="";
        string modalita_pagamento="";
        string clientName="";
        bool isPrivato = false;

        using (EntitiesGestisco db = new EntitiesGestisco())
        {
            //richiama stored procedure che trova le righe da fatturare
            List<udfTcourseEditionBilling_Result> bd = db.udfTcourseEditionBilling(BillingList).ToList();
            if (bd.Count == 0) return "";

            udfTcourseEditionBilling_Result bill_course_head = bd.FirstOrDefault();

            string nextBill = ProssimaNumerazioneFattura(bill_course_head.BILL_DATE_YEAR);
            int iNextBillNumber = 0;

            if (int.TryParse(nextBill, out iNextBillNumber))
            {
                if (iNextBillNumber < bill_course_head.BILLING_NUM) return "NUMERO FATTURA NON VALIDO, PREVISTO:" + iNextBillNumber;
            }
            else{
                return "IMPOSSIBILE VERIFICARE NUMERAZIONE FATTURE";
            }

            if (bill_course_head.IDCLIENT.HasValue) {
                //cliente consularea

                //esporta solo se non è già presente nel database cloud
                TCLIENT client = db.TCLIENT.Where(c => c.IDCLIENT == bill_course_head.IDCLIENT.Value).FirstOrDefault();
                clientName = client.NAME;

                //SE NON C'E IL CODICE SDI BLOCCO L'INVIO
                if (client != null && (!string.IsNullOrEmpty(client.VATNUMBER)) && string.IsNullOrEmpty(client.CODICEFATTURAZIONE)) return "CODICE SDI NON PRESENTE.";

                TCLIENT_ACCOUNTING CLIENT_ACCOUNTING = client.CLIENT_ACCOUNTING(this.idCorporate);
                iban = (CLIENT_ACCOUNTING != null ? CLIENT_ACCOUNTING.IBAN :"");
                idAnagrafica = EsportaAnagrafica(client);
            }else{
                //cliente corso
                var q = (from tm in db.TCOURSE_MEMBER join
                        tmc in db.TCOURSE_EDITION_MEMBER on tm.IDMEMBER equals tmc.IDMEMBER_LINK
                        where tm.IDMEMBER == bill_course_head.IDBILLING_MEMBER  && tmc.IDEDITION_LINK== bill_course_head.IDBILLING_EDITION 
                        select new {MEMBERNAME=tm.LASTNAME + " " + tm.FIRSTNAME,tm.ADDRESS,
                                    tm.ZIPCODE ,tm.LOCALITY ,tm.TREGISTRY_PROVINCE.PROVINCEACRONYM,
                                    tm.FISCALCODE,tm.VAT ,tm.PAYMENT, tm.BANK, tm.IBAN, tmc.HOURNUM,tm.EMAIL ,tm.TELEPHONE 
                                    }).FirstOrDefault();

                if (!string.IsNullOrEmpty(q.VAT) || !string.IsNullOrEmpty(q.FISCALCODE)) {
                    
                    API_AnagraficaNuovoSingoloRequest cliente = new API_AnagraficaNuovoSingoloRequest()
                    {
                        data = new AnagraficaCliente()
                    
                        {
                            tax_code = q.FISCALCODE,
                            vat_number = q.VAT,
                            address_postal_code = q.ZIPCODE,
                            address_city= q.LOCALITY,
                            address_province= (q.PROVINCEACRONYM != null ? q.PROVINCEACRONYM : ""),
                            address_street = q.ADDRESS,
                            email = q.EMAIL,
                            name = q.MEMBERNAME
                            //paese_iso = "IT",
                            //PA = true, //???
                            //PA_codice = "",
                            //phone = q.TELEPHONE,
                            //termini_pagamento = 30 //TODO: termini di pagamento ?
                                                   //extra = client.IDCLIENT.ToString(),
                        }
	                        };

                    // per i privati, il codice SDI è sempre "0000000"
                    if (string.IsNullOrEmpty(q.VAT))
                    {
                        //cliente.data.PA_codice = "0000000";
                        modalita_pagamento = "Contanti";
                        isPrivato = true;
                    }
                    else {
                        modalita_pagamento = q.PAYMENT;
                    }

                    clientName = q.MEMBERNAME;
                    iban = q.IBAN;

                    idAnagrafica = EsportaAnagraficaCorso(cliente); //inviaAnagrafica(cliente);
                    
                }
            }

            if (idAnagrafica != "") //cliente esportato, esporto la fattura
            {
                return EsportaFatturaCorso(bd, idAnagrafica,iban,modalita_pagamento,clientName,isPrivato);
            }
        }
        return "";
    }

    public string EsportaFatturaCorso(List<udfTcourseEditionBilling_Result> billList, string idAnagrafica, string iban,
                                    string modalita_pagamento, string ClientName, bool isPrivato)
    {

        using (EntitiesGestisco db = new EntitiesGestisco())
        {
            udfTcourseEditionBilling_Result bill_head = billList.FirstOrDefault();


            string titolo_pag="IBAN";
            string pa_modalita_pagamento = "";
            //string modalita_pagamento = "";
            if (string.IsNullOrEmpty(iban))
            {
                // non è stato indicato iban quindi metto iban default
                iban = Conf_Iban[this.idCorporate];
            }
            if (!isPrivato) { 
                //verifico la modalità di pagamento del cliente
                if (!string.IsNullOrEmpty(modalita_pagamento))
                {
                    pa_modalita_pagamento = (modalita_pagamento.ToLower().Contains("bonifico") ? Metodo_Pagamento_Bonifico : Metodo_Pagamento_Riba);
                }
                else
                {
                    //se non presente vado sui valori di default, se il cliente non ha iban indicato: USO BONIFICO; altrimenti RIBA
                    pa_modalita_pagamento = (iban != "" ? Metodo_Pagamento_Bonifico : Metodo_Pagamento_Riba);
                    modalita_pagamento = "Bonifico";
                }
            }else{
                // per il privato pagamento sempre saldato in contanti
                pa_modalita_pagamento = Metodo_Pagamento_Contanti;
                modalita_pagamento = "PAGATO";
                iban = "";
                titolo_pag = "";
            }
            Api_DocNuovoRequest fattura = new Api_DocNuovoRequest(){ data = new Doc()
            {
                id_cliente = idAnagrafica,
                autocompila_anagrafica = true,
                Nome = ClientName,
                prezzi_ivati = false,
                Data = bill_head.BILL_DATE,
                metodo_pagamento = modalita_pagamento,//billHead.TCLIENT.TCLIENT_ACCOUNTING.PAYMENT,
                metodo_titolo1 = titolo_pag,
                metodo_desc1 = iban,
                mostra_info_pagamento = true,
                Pa = true,
                PA_tipo_cliente = "B2B",
                PA_iban = iban,
                PA_modalita_pagamento = pa_modalita_pagamento,
                Numero = bill_head.BILLING_NUM.ToString()
            }};


            fattura.data.lista_articoli = new List<ListaArticoli>();
            fattura.data.lista_pagamenti = new List<ListaPagamenti>();

            int nOrd = 0;



            foreach (udfTcourseEditionBilling_Result bill in billList)
            {
                string strDesc = "";

                int iNumeroOre = 0;

                var res = (from tcm in db.TCOURSE_MEMBER join tcem in db.TCOURSE_EDITION_MEMBER on tcm.IDMEMBER equals tcem.IDMEMBER_LINK
                           where (tcm.IDMEMBER == bill.IDBILLING_MEMBER.Value && tcem.IDEDITION_LINK == bill.IDBILLING_EDITION)
                           select new {tcem.HOURNUM}
                           ).FirstOrDefault();

                if (res != null && res.HOURNUM.HasValue)
                {
                    iNumeroOre = res.HOURNUM.Value;
                }
                else {
                    iNumeroOre = bill.HOURNUMTOTAL.Value;
                }

                ListaArticoli art = new ListaArticoli
                {
                    Ordine = nOrd++,
                    Tassabile = (bill.BILLING_VAT > 0 ? true : false),
                    Codice = "", ////TODO: cosa ci mettiamo?
                    Quantita = 1,
                    prezzo_netto = bill.BILLING_PRICE.Value
                };


                // todo: categoria per i corsi?
                //art.Categoria = bill.IDACTIVITY_TYPE.ToString();

                art.Nome = bill.COURSE;

                if (bill.MINDATE.HasValue)
                {

                    if (bill.MINDATE.Value != bill.MAXDATE.Value && bill.MINDATE.Value.Month == bill.MAXDATE.Value.Month && bill.MINDATE.Value.Year == bill.MAXDATE.Value.Year)
                    {
                        strDesc = FirstCharToUpper(bill.MINDATE.Value.ToString("dd", System.Globalization.CultureInfo.CreateSpecificCulture("it")));
                        strDesc += " - "  + FirstCharToUpper(bill.MAXDATE.Value.ToString("dd", System.Globalization.CultureInfo.CreateSpecificCulture("it")));
                        strDesc +=" " + FirstCharToUpper(bill.MINDATE.Value.ToString("MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("it")));
                    }
                    else {
                        strDesc = FirstCharToUpper(bill.MINDATE.Value.ToString("dd MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("it")));
                    }

                    if (bill.MINDATE.Value.Year != bill.MAXDATE.Value.Year) strDesc += "  " + bill.MINDATE.Value.Year;
                    if (bill.MINDATE.Value.Month != bill.MAXDATE.Value.Month) strDesc += " / " + FirstCharToUpper(bill.MAXDATE.Value.ToString("dd MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("it"))); ;
                    strDesc += " " + bill.MAXDATE.Value.Year + " - Totale " + iNumeroOre + " ore";
                }

                art.Descrizione = strDesc + " - Corso di formazione n." + bill.EDITION_NUM.ToString();
                fattura.data.lista_articoli.Add(art);
            }

            fattura.data.lista_pagamenti.Add(new ListaPagamenti
            {
                Importo = "auto",
                Metodo = "not",
                data_scadenza = bill_head.BILL_DATE,////TODO: cosa ci metto ?
                data_saldo = bill_head.BILL_DATE, ////TODO:  cosa ci metto?,
            });
            if (isPrivato) fattura.data.lista_pagamenti[0].Metodo = "Contanti";


            //}
            //fattura.lista_articoli;
            //fattura.lista_pagamenti 
            impostaChiavi(fattura);

            API_DocNuovoResponse newdoc = ApiFattureInCloud.FatturaNuovo(fattura);
            if (newdoc.success)
            {
                return newdoc.new_id.ToString();
            }
            else
            {
                return newdoc.error;
            }
            //return newdoc.new_id;
        }
    }

    public bool aggiornaAnagrafica(string id, TCLIENT client)
    {
        return true; //TODO temporaneamente disabilitato, da verificare successivamente

        string email = getEmail(client.EMAIL3, client.EMAIL, client.EMAIL2);

        API_AnagraficaModificaSingoloRequest cliente = new API_AnagraficaModificaSingoloRequest()
        {
            data = new AnagraficaCliente()
        
        {
            code = id,
            tax_code =("" + client.FISCAL_CODE).Trim(),
            vat_number = ("" + client.VATNUMBER).Trim(),
            address_postal_code = client.ZIPCODE,
            address_city= client.LOCALITY,
            address_province= (client.TREGISTRY_PROVINCE != null ? client.TREGISTRY_PROVINCE.PROVINCEACRONYM : ""),
            address_street = client.ADDRESS,
            email = email,
            name = client.NAME
            //paese_iso = (client.TREGISTRY_STATE != null ? client.TREGISTRY_STATE.STATECODE : "IT"),
            //PA = true, //???
            //PA_codice = client.CODICEFATTURAZIONE,
            //phone = client.PHONE1,
            //termini_pagamento = 30 //TODO: termini di pagamento ?

            //extra = client.IDCLIENT.ToString(),
        }

        };

        return modificaAnagrafica(cliente);

        //    // pec??
        //    // codice fatturazione? 
        //    impostaChiavi(cliente);
        //    API_AnagraficaNuovoSingoloResponse clientesalvato = ApiFattureInCloud.PostCliente(cliente);
        //    return clientesalvato.id;
        //
    }

    public bool aggiornaAnagraficaCorso(string id, API_AnagraficaNuovoSingoloRequest client)
    {
        string email = client.data.email;

        API_AnagraficaModificaSingoloRequest cliente = new API_AnagraficaModificaSingoloRequest()
        {
            data = new AnagraficaCliente()
            {
	            code = id,
	            tax_code = ("" + client.data.tax_code).Trim(),
	            vat_number = ("" + client.data.vat_number).Trim(),
	            address_postal_code = client.data.address_postal_code ,
	            address_city= client.data.address_city ,
	            address_province= (client.data.address_province != null ? client.data.address_province: ""),
	            address_street = client.data.address_street,
	            email = email,
	            name = client.data.name
	            //paese_iso = (client.data.paese_iso != null ? client.data.paese_iso: "IT"),
	            //PA = true, //???
	            //PA_codice = client.data.PA_codice,
	            //phone = client.data.phone,
	            //termini_pagamento = 30 //TODO: termini di pagamento ?

	            //extra = client.IDCLIENT.ToString(),
            }
        }
        ;

        return modificaAnagrafica(cliente);

        //    // pec??
        //    // codice fatturazione? 
        //    impostaChiavi(cliente);
        //    API_AnagraficaNuovoSingoloResponse clientesalvato = ApiFattureInCloud.PostCliente(cliente);
        //    return clientesalvato.id;
        //
    }


    const string Metodo_Pagamento_Contanti = "MP01";
    const string Metodo_Pagamento_Bonifico = "MP05";
    const string Metodo_Pagamento_Riba = "MP12";

    /*
 * 
MP01 contanti
MP02 assegno
MP03 assegno circolare
MP04 contanti presso Tesoreria
MP05 bonifico
MP06 vaglia cambiario
MP07 bollettino bancario
MP08 carta di credito
MP09 RID
MP10 RID utenze
MP11 RID veloce
MP12 Riba
MP13 MAV
MP14 quietanza erario stato
MP15 giroconto su conti di contabilità speciale
* 
*/

    //public string EsportaFattura(TCLIENT_BILL bill,string idAnagrafica)
    //{
    //    Api_DocNuovoRequest fattura = new Api_DocNuovoRequest()
    //    {
    //        id_cliente= idAnagrafica,
    //        autocompila_anagrafica = true,
    //        Nome = bill.TCLIENT.NAME ,
    //        prezzi_ivati = false,
    //        Data = bill.BILL_DATE.ToString("dd/MM/yyyy"),
    //    };
    //    fattura.lista_articoli = new List<ListaArticoli>();
    //    fattura.lista_pagamenti = new List<ListaPagamenti>();
    //    int nOrd = 0;

    //    foreach (var det in bill.TCLIENT_BILL_DETAIL )
    //    {
    //        ListaArticoli art = new ListaArticoli
    //        {
    //            Ordine = nOrd++,
    //            Tassabile = true,
    //            Codice = "", ////TODO: cosa ci mettiamo?
    //            Quantita = 1,
    //            prezzo_netto = (long)det.DETAIL_PRICE
    //        };

    //        //
    //        if(det.ACTIVITY_BILLING != null) { // FATTURA LEGATA AD ATTIVITA'
    //            if (det.ACTIVITY_BILLING.ACTIVITY.ACTIVITY_TYPE == "hour") {
    //                //ATTIVITA' A CONSUNTIVO

    //            } else {

    //            }
    //            art.Categoria = det.ACTIVITY_BILLING.ACTIVITY.IDACTIVITY.ToString();
    //            art.Descrizione = det.ACTIVITY_BILLING.ACTIVITY.DESCRIPTION;
    //            if (det.ACTIVITY_BILLING.ACTIVITY_BILLING_STEP != null) {
    //                art.Descrizione += " " + det.ACTIVITY_BILLING.ACTIVITY_BILLING_STEP.STEP ;
    //            }
    //        }
    //        else if(det.DOMAIN_SERVICE!=null)
    //        {
    //            art.Categoria = det.DOMAIN_SERVICE.IDSERVICE_DOMAIN.ToString() + det.DOMAIN_SERVICE.IDSERVICE_TYPE.ToString() ;
    //            art.Descrizione = det.DOMAIN_SERVICE.DOMAIN_SERVICE_TYPE.TYPE;
    //        }

    //        //art.Categoria = categoria;// "803002",////TODO:  cosa ci mettiamo?

    //        fattura.lista_articoli.Add(art);

    //        fattura.lista_pagamenti.Add(new ListaPagamenti {
    //            Importo = (long) (det.DETAIL_PRICE * ivaMultiplier ), Metodo = "not",//TODO: DOVE TROVO L'IVA
    //            data_scadenza = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"),////TODO: cosa ci metto ?
    //            data_saldo = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy") ////TODO:  cosa ci metto?
    //        } );
    //}
    ////fattura.lista_articoli;
    ////fattura.lista_pagamenti 

    //API_DocNuovoResponse newdoc = ApiFattureInCloud.PostFattura(fattura);
    //    if (newdoc.success) {
    //        return newdoc.new_id.ToString();
    //    } else {
    //        return ""; }
    //    //return newdoc.new_id;
    //}


    private string FirstCharToUpper(string input)
    {
        return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
    }

    public string ProssimaNumerazioneFattura(int anno)
    {
        API_DocInfoRequest info = new API_DocInfoRequest() { anno = anno };
        impostaChiavi(info);

        var retInfo = ApiFattureInCloud.FattureInfo(info);
        
        if (retInfo.success)
        {
            //return retInfo.info.numerazioni.FirstOrDefault<KeyValuePair<string,string>>().Value;
            return retInfo.data.First().numeration;
        }
        else
        {
            return retInfo.error;
        }
    }

        public static string inviaFatturaTest()
    {

        Api_DocNuovoRequest fattura = new Api_DocNuovoRequest(){ data = new Doc()
        {
            autocompila_anagrafica = true,
            Pa = true,
            PA_codice = "AXT1234",
            PA_tipo_cliente = "B2B",
            /*id_cliente= 23152623,*/
            salva_anagrafica = false,
            Nome = "Axterisco",
            prezzi_ivati = false,
            Piva = "03152720409",
            Data = DateTime.Now.ToString("dd/MM/yyyy"),
            lista_articoli = new List<ListaArticoli>  {new ListaArticoli {Ordine=1, Categoria="803002",Tassabile=true, Codice="20", Descrizione="Sito Web",Quantita=1,prezzo_netto=1000 },
                 new ListaArticoli {Ordine=2, Categoria = "803003", Codice="30", Descrizione = "Corso Web",Tassabile=true,Quantita=1,prezzo_netto =600}}
                ,
            lista_pagamenti = new List<ListaPagamenti> {{ new ListaPagamenti { Importo = "auto", Metodo = "not", data_scadenza = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"), data_saldo = DateTime.Now.AddDays(30).ToString("dd/MM/yyyy")}}}
        }};

        API_DocNuovoResponse newdoc = ApiFattureInCloud.FatturaNuovo(fattura);
        return newdoc.token;
    }

}

