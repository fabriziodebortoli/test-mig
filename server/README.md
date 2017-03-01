# web-server
Questo è il progetto contenitore, che ospita i vari moduli che apportano i controllers per l'interfaccia rest.
Ogni sottoprogetto rappresenta un modulo caricato dinamicamente allo startup.
Tali moduli vanno elencati nella sezione _Modules_ del file di configurazione appsettings.json
I sottoprogetti sono:

## rs-web:
Gestore del [motore di reportistica](https://github.com/Microarea/Taskbuilder/tree/master/server/rs-web)

## data-service:
Gestore del [motore di query](https://github.com/Microarea/Taskbuilder/tree/master/server/data-service)

## tbloader-gate
[Applicazione web](https://github.com/Microarea/Taskbuilder/tree/master/server/tbloader-gate)
che fa da tramite verso il processo TBLoader, curandone l'istanziazione e l'esposizione delle API rest verso l'esterno

## accounting-manager
### [Applicazione web](https://github.com/Microarea/Taskbuilder/tree/master/server/accounting-manager)
Gestione dell'autenticazione degli utenti

## provisioning-server
### [Applicazione web](https://github.com/Microarea/Taskbuilder/tree/master/server/provisioning-server)
Gestione delle licenze ?

## task-scheduler 
### (non ancora presente)
Applicazione web per la gestione dei task schedulati
