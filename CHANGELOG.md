## Cambiamenti
* aggiunta una soluzione e modificate le opzioni per poter avviare il progetto
* aggiornate dipendenze per poter compilare il progetto con visual studio 2022
* tradotta gran parte delle api in inglese 

### TODO
* riscrivere da 0 per usare un framework moderno, testabile, ed una architettura semplificata.

## Note
* __⚠ importantissimo richio di sicurezza__ rigenerare o elminare tutte le api_key dato che sono salvate nel file web.config e sono visibili al mondo intero, essendo il repo pubblico
* questo progetto ha accumulato un enorme debito tecnico, una versione ottimale va riscritta da 0
	* __l'immensa confusione e ingarbugliatezza della codebase ha impedito di aggiornare in maniera ottimale le API__
* WebForms. Si consiglia di migrare a ASP.NET.
	* per quanto ancora supportata come parte di NET Framework, è una tecnologia obsoleta. 
	* Se si cercava di avere un applicazione che servisse delle WebAPI, WebForms non è la tecnologia adatta per questo tipo di applicazione. 
* il database è inutilmente sovraingegnerizzato
	* la versione di EntityFramework usata è obsoleta, come parte della migrazione si consiglia di passare a tecnologie meno dipendenti da Visual Studio
	* l'utilizzo dei texttemplate per generare il codice del database è una configurazione obsoleta
* manca uno spazio dei nomi per il codice
* manca uno spazio dei nomi per il servizio (ancora http://tempuri.org)


_~ Comparin Jacopo per Giammar_
