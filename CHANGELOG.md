## Cambiamenti
* aggiunta una soluzione e modificate le opzioni per poter avviare il progetto
* aggiornate dipendenze per poter compilare il progetto con visual studio 2022
* aggiunta configurazione per un file secrets.json nella root del repository perché WebForms non supporta i secret store

### TODO
* migrare a v2 delle api di fatture in cloud

## Note
* __importantissimo richio di sicurezza__ rigenerare o elminare tutte le api_key dato che sono salvate nel file web.config e sono visibili al mondo intero, essendo il repo pubblico
* questo progetto ha accumulato un enorme debito tecnico, è molto probabile che una versione più stabile sia da rifare da 0
* WebForms, per quanto ancora supportata come parte di NET Framework, è una tecnologia obsoleta. Si consiglia di migrare a ASP.NET
* la versione di EntityFramework usata è obsoleta, come parte della migrazione si consiglia di passare a tecnologie meno dipendenti da Visual Studio
* l'utilizzo dei texttemplate per generare il codice del database è una configurazione obsoleta
* manca uno spazio dei nomi per il codice
* manca uno spazio dei nomi per il servizio (tempuri.org)


~ Comparin Jacopo per Giammar
