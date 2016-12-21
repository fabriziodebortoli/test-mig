# tbloader-gate

Applicazione web che espone le funzionalità di TBLoader attraverso API REST.

L'applicazione riceve chiamate da client remoti e, previo l'istanziazione di un processo 
TBLoader (che viene istanziato indirettamente dal servizio TBLoaderService, il quale comunica col gate ricevendo chiamate
sulla porta 11000), fa da tramite con quest'ultimo inoltrando ad esso tutte le chiamate http e websocket.

# Istruzioni per l'uso in sviluppo
* Compilare l'applicazione angular \Taskbuilder\web-client\form-client; per fare questo posizionarsi nella cartella e digitare l'istruzione  _ng build -w_
* Compilare TaskBuilder ed ERP
* Lanciare manualmente il servizio \standard\taskbuilder\TaskBuilderNet\Microarea.TaskBuilderNet.TBLoaderService\bin\Debug\TBLoaderService.exe
* posizionarsi dal prompt dei comandi in \web-server\tbloader-gate e digitare il comando _dotnet run_
* accedere col browser all'indirizzo http://localhost:5000