using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System;

public class api_generic_request
{

	public string api_uid { get; set; }
	public string api_key { get; set; }

	public api_generic_request ()
	{

		api_uid = ConfigurationManager.AppSettings["api_uid"];
		api_key = ConfigurationManager.AppSettings["api_key"];
	}

	internal StringContent body ()
	{
		var stringContent = new StringContent(JsonConvert.SerializeObject(this), Encoding.UTF8, "application/json");
		return stringContent;
	}
}

public class api_generic_response
{
	public bool success { get; set; }
	public string error { get; set; }
	public string error_code { get; set; }
	public object error_extra { get; set; }
}

public class API_FattureListaRequest : api_generic_request
{
	public int anno { get; set; }
	public string data_inizio { get; set; }
	public string data_fine { get; set; }
	public string cliente { get; set; }
	public string fornitore { get; set; }
	public int pagina { get; set; }
}

public class API_FatturaListaResponse : api_generic_response
{
	public List<API_DocLight> data { get; set; }//(Array[DocLight]),
	public int current_page { get; set; }//    (integer) : Numero della pagina restituita,
	public int from { get; set; }//(integer): Numero di pagine totali
	public int numero_risultati { get; set; }
	public int risultati_per_pagina { get; set; }
}

public class API_DocInfoRequest : api_generic_request
{
	public int anno { get; set; }
}

public class API_DocLight
{
	public string id { get; set; } //(string): Identificativo univoco del documento (cambia a seguito di una modifica del documento),
	public string token { get; set; }//(string): Identificativo permanente del documento (rimane lo stesso anche a seguito di modiifche),
	public string tipo { get; set; }//(string): Tipologia del documento = ['fatture' o 'proforma' o 'ordini' o 'preventivi' o 'ndc' o 'ddt' o 'rapporti' o 'ordforn'],
	public string id_cliente { get; set; }//(string, opzionale): Identificativo univoco del cliente (se nullo, il cliente non è presente nell'anagrafica) [solo con tipo!="ordforn"],
	public string id_fornitore { get; set; }//(string, opzionale): Identificativo univoco del fornitore (se nullo, il fornitore non è presente nell'anagrafica) [solo con tipo="ordforn"],
	public string nome { get; set; }// (string): Nome o ragione sociale del cliente/fornitore,
	public string numeration { get; set; }//(string): Numero (e serie) del documento,
	public string data { get; set; }//(date): Data di emissione del documento,
	public double importo_netto { get; set; }//(double): Importo netto del documento (competenze),
	public double importo_totale { get; set; }//(double): Importo lordo del documento (totale da pagare),
	public string valuta { get; set; }//(string): Valuta del documento e degli importi indicati,
	public double valuta_cambio { get; set; }//(double): Tasso di cambio EUR/{valuta},
	public string prossima_scadenza { get; set; }//(date, opzionale): [Non presente in preventivi e ddt] Indica la scadenza del prossimo pagamento (vale 00/00/0000 nel caso in cui tutti i pagamenti siano saldati),
	public bool? ddt { get; set; }//(boolean, opzionale): [Solo se tipo!=ndc] Indica la presenza di un DDT incluso nel documento (per i ddt è sempre true),
	public bool? ftacc { get; set; }//(boolean, opzionale): [Solo se tipo=fatture] Indica la presenza di una fattura accompagnatoria inclusa nel documento,
	public string oggetto_visibile { get; set; }//(string, opzionale): [Solo se filtrato per oggetto] Oggetto mostrato sul documento,
	public string oggetto_interno { get; set; }//(string, opzionale): [Solo se filtrato per oggetto] Oggetto (per organizzazione interna),
	public string link_doc { get; set; }//(string, opzionale): [Solo se tipo!=ddt] Link al documento in formato PDF,
	public string ddt_numero { get; set; }//(string, opzionale): Numero del DDT [solo se tipo=ddt],
	public DateTime? ddt_data { get; set; }//(date, opzionale): Data del DDT,
	public bool? bloccato { get; set; }//(boolean, opzionale): [Se presente, vale sempre "true"] Indica che il documento è bloccato e non può essere modificato o eliminato,
	public bool? PA { get; set; }//(boolean, opzionale): [Solo per fatture e ndc elettroniche, vale sempre "true"] Indica che il documento è nel formato FatturaPA,
	public string PA_tipo_cliente { get; set; }//(string, opzionale): [Solo se PA=true] Indica la tipologia del cliente: Pubblica Amministrazione ("PA") oppure privato ("B2B") = ['PA' o 'B2B']
}

public class API_AnagraficaListaRequest : api_generic_request
{
	public string filtro { get; set; }
	public string id { get; set; }
	public string nome { get; set; }
	public string cf { get; set; }
	public string piva { get; set; }
	public int pagina { get; set; }
}

public class API_AnagraficaListaResponse : api_generic_response
{
	public List<AnagraficaCliente> data { get; set; }//(Array[AnagraficaCliente], opzionale) : Lista dei clienti(solo se richiesti i clienti),
}



public class AnagraficaCliente
{
	public string id { get; set; }
	public string code { get; set; }//(string) : Identificativo univoco del cliente,
	public string name { get; set; }// (string): Nome o ragione sociale del cliente,
	//public string referente { get; set; }// (string): Nome referente,
	public string address_street { get; set; }// (string): Indirizzo del cliente,
	public string address_postal_code { get; set; }//(string) : CAP del cliente,
	public string address_city { get; set; }//(string) : Città(comune) del cliente,
	public string address_province { get; set; }// (string): Provincia del cliente,
	//public string address_extra { get; set; }//(string, opzionale) : Note extra sull'indirizzo,
	//public string country { get; set; }//{ get; set; }//(string) : Paese(nazionalità) del cliente,
	public string email { get; set; }//(string): Indirizzo di posta elettronica,
	public string phone { get; set; }// (string): Recapito telefonico,
	//public string paese_iso { get; set; }
//	public string fax { get; set; }// (string): Numero fax,
	public string vat_number { get; set; }// (string): Partita IVA,
	//????
	public string tax_code { get; set; }//(string): Codice fiscale,

	//public int termini_pagamento { get; set; }// (integer): Termini di pagamento predefiniti(giorni a partire dalla data del documento),
	//public bool pagamento_fine_mese { get; set; }//(boolean) : Indica se la scadenza del pagamento deve avvenire alla fine del mese(dopo i giorni specificati in 'termini_pagamento'),
	//public double? val_iva_default { get; set; }//(double) : Valore IVA predefinito,
	//public string desc_iva_default { get; set; }//(string) : Descrizione IVA,
	//public string extra { get; set; }//(string): Informazioni extra sul cliente,

	//public bool PA { get; set; }//(boolean): Indica se il cliente è una pubblica amministrazione,
	//public string PA_codice { get; set; }//(string, opzionale): [Solo se PA=true] Codice pubblica amministrazione
}

public class API_AnagraficaNuovoSingoloRequest : api_generic_request
{
	public AnagraficaCliente data { get; set; }
}

public class API_AnagraficaModificaSingoloRequest : API_AnagraficaNuovoSingoloRequest
{
	public string id { get; set; } //(string) : Nome o ragione sociale del cliente,
}

public class API_DocNuovoResponse : api_generic_response
{
	public int new_id { get; set; }
	public string token { get; set; }//: "1234567890abcdefghijklmnopqrstuv"
}

public class API_AnagraficaNuovoSingoloResponse : api_generic_response
{
	public string id { get; set; }
}

public class API_AnagraficaModificaSingoloResponse : api_generic_response
{

}

public class Api_DocNuovoRequest : api_generic_request
{
	public Doc data { get; set; }
}

public class Doc{

[JsonProperty("id_cliente")]
	public String id_cliente { get; set; }

	[JsonProperty("id_fornitore")]
	public String id_fornitore { get; set; }

	[JsonProperty("nome")]
	public string Nome { get; set; }

	[JsonProperty("indirizzo_via")]
	public string indirizzo_via { get; set; }

	[JsonProperty("indirizzo_cap")]
	public long indirizzo_cap { get; set; }

	[JsonProperty("indirizzo_citta")]
	public string indirizzo_citta { get; set; }

	[JsonProperty("indirizzo_provincia")]
	public string indirizzo_provincia { get; set; }

	[JsonProperty("indirizzo_extra")]
	public string indirizzo_extra { get; set; }

	[JsonProperty("paese")]
	public string paese { get; set; }

	[JsonProperty("paese_iso")]
	public string paese_iso { get; set; }

	[JsonProperty("lingua")]
	public string Lingua { get; set; }

	[JsonProperty("piva")]
	public string Piva { get; set; }

	[JsonProperty("cf")]
	public string Cf { get; set; }

	[JsonProperty("autocompila_anagrafica")]
	public bool autocompila_anagrafica { get; set; }

	[JsonProperty("salva_anagrafica")]
	public bool salva_anagrafica { get; set; }

	[JsonProperty("numero")]
	public string Numero { get; set; }

	[JsonProperty("data")]
	public string Data { get; set; } //19/03/2019

	[JsonProperty("valuta")]
	public string Valuta { get; set; }

	[JsonProperty("valuta_cambio")]
	public long valuta_cambio { get; set; }

	[JsonProperty("prezzi_ivati")]
	public bool prezzi_ivati { get; set; }

	[JsonProperty("rivalsa")]
	public long Rivalsa { get; set; }

	[JsonProperty("cassa")]
	public long Cassa { get; set; }

	[JsonProperty("rit_acconto")]
	public long rit_acconto { get; set; }

	[JsonProperty("imponibile_ritenuta")]
	public long imponibile_ritenuta { get; set; }

	[JsonProperty("rit_altra")]
	public long rit_altra { get; set; }

	[JsonProperty("marca_bollo")]
	public long marca_bollo { get; set; }

	[JsonProperty("oggetto_visibile")]
	public string oggetto_visibile { get; set; }

	[JsonProperty("oggetto_interno")]
	public string oggetto_interno { get; set; }

	[JsonProperty("centro_ricavo")]
	public string centro_ricavo { get; set; }

	[JsonProperty("centro_costo")]
	public string centro_costo { get; set; }

	[JsonProperty("note")]
	public string Note { get; set; }

	[JsonProperty("nascondi_scadenza")]
	public bool nascondi_scadenza { get; set; }

	[JsonProperty("ddt")]
	public bool Ddt { get; set; }

	[JsonProperty("ftacc")]
	public bool Ftacc { get; set; }

	[JsonProperty("id_template")]

	public long id_template { get; set; }

	[JsonProperty("ddt_id_template")]

	public long ddt_id_template { get; set; }

	[JsonProperty("ftacc_id_template")]

	public long ftacc_id_template { get; set; }

	[JsonProperty("mostra_info_pagamento")]
	public bool mostra_info_pagamento { get; set; }

	[JsonProperty("metodo_pagamento")]
	public string metodo_pagamento { get; set; }

	[JsonProperty("metodo_titolo1")]
	public string metodo_titolo1 { get; set; }

	[JsonProperty("metodo_desc1")]
	public string metodo_desc1 { get; set; }

	[JsonProperty("mostra_totali")]
	public string mostra_totali { get; set; }

	[JsonProperty("mostra_bottone_paypal")]
	public bool mostra_bottone_paypal { get; set; }

	[JsonProperty("mostra_bottone_bonifico")]
	public bool mostra_bottone_bonifico { get; set; }

	[JsonProperty("mostra_bottone_notifica")]
	public bool mostra_bottone_notifica { get; set; }

	[JsonProperty("lista_articoli")]
	public List<ListaArticoli> lista_articoli { get; set; }

	[JsonProperty("lista_pagamenti")]
	public List<ListaPagamenti> lista_pagamenti { get; set; }

	[JsonProperty("ddt_numero")]
	public string ddt_numero { get; set; }

	[JsonProperty("ddt_data")]
	public string ddt_data { get; set; }

	[JsonProperty("ddt_colli")]
	public string ddt_colli { get; set; }

	[JsonProperty("ddt_peso")]
	public string ddt_peso { get; set; }

	[JsonProperty("ddt_causale")]
	public string ddt_causale { get; set; }

	[JsonProperty("ddt_luogo")]
	public string ddt_luogo { get; set; }

	[JsonProperty("ddt_trasportatore")]
	public string ddt_trasportatore { get; set; }

	[JsonProperty("ddt_annotazioni")]
	public string ddt_annotazioni { get; set; }

	[JsonProperty("PA")]
	public bool Pa { get; set; }

	[JsonProperty("PA_tipo_cliente")]
	public string PA_tipo_cliente { get; set; }

	[JsonProperty("PA_tipo")]
	public string PA_tipo { get; set; }

	[JsonProperty("PA_numero")]
	public string PA_numero { get; set; }

	[JsonProperty("PA_data")]
	public string PA_data { get; set; }

	[JsonProperty("PA_cup")]
	public string PA_cup { get; set; }

	[JsonProperty("PA_cig")]
	public string PA_cig { get; set; }

	[JsonProperty("PA_codice")]
	public string PA_codice { get; set; }

	[JsonProperty("PA_pec")]
	public string PA_pec { get; set; }

	[JsonProperty("PA_esigibilita")]
	public string PA_esigibilita { get; set; }

	[JsonProperty("PA_modalita_pagamento")]
	public string PA_modalita_pagamento { get; set; }

	[JsonProperty("PA_istituto_credito")]
	public string PA_istituto_credito { get; set; }

	[JsonProperty("PA_iban")]
	public string PA_iban { get; set; }

	[JsonProperty("PA_beneficiario")]
	public string PA_beneficiario { get; set; }

	[JsonProperty("extra_anagrafica")]
	public ExtraAnagrafica extra_anagrafica { get; set; }

	[JsonProperty("split_payment")]
	public bool split_payment { get; set; }
}

public class ExtraAnagrafica
{
	[JsonProperty("mail")]
	public string Mail { get; set; }

	[JsonProperty("tel")]
	public string Tel { get; set; }

	[JsonProperty("fax")]
	public string Fax { get; set; }
}

public class ListaArticoli
{
	[JsonProperty("id")]

	public long Id { get; set; }

	[JsonProperty("codice")]
	public string Codice { get; set; }

	[JsonProperty("nome")]
	public string Nome { get; set; }

	[JsonProperty("um")]
	public string um { get; set; }

	[JsonProperty("quantita")]
	public double Quantita { get; set; }

	[JsonProperty("descrizione")]
	public string Descrizione { get; set; }

	[JsonProperty("categoria")]
	public string Categoria { get; set; }

	[JsonProperty("prezzo_netto")]
	public double prezzo_netto { get; set; }

	[JsonProperty("prezzo_lordo")]
	public double prezzo_lordo { get; set; }

	[JsonProperty("cod_iva")]
	public long cod_iva { get; set; }

	[JsonProperty("tassabile")]
	public bool Tassabile { get; set; }

	[JsonProperty("sconto")]
	public double Sconto { get; set; }

	[JsonProperty("applica_ra_contributi")]
	public bool applica_ra_contributi { get; set; }

	[JsonProperty("ordine")]
	public long Ordine { get; set; }

	[JsonProperty("sconto_rosso")]
	public int sconto_rosso { get; set; }

	[JsonProperty("in_ddt")]
	public bool in_ddt { get; set; }

	[JsonProperty("magazzino")]
	public bool Magazzino { get; set; }
}

public class ListaPagamenti
{
	[JsonProperty("data_scadenza")]
	public string data_scadenza { get; set; }

	[JsonProperty("importo")]
	public string Importo { get; set; }

	[JsonProperty("metodo")]
	public string Metodo { get; set; }

	[JsonProperty("data_saldo")]
	public string data_saldo { get; set; }
}