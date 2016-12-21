# web-server
Questo è il progetto contenitore, che ospita i vari moduli che apportano i controllers per l'interfaccia rest. Ogni sottoprogetto rappresenta un modulo caricato dinamicamente allo startup. Tali moduli vanno elencati nella sezione _Modules_ del file di configurazione appsettings.json
I sottoprogetti sono:

## rs-web:
Gestore del [motore di reportistica](https://github.com/Microarea/Taskbuilder/tree/master/web-server/rs-web)

## tbloader-gate
[Applicazione web](https://github.com/Microarea/Taskbuilder/tree/master/web-server/tbloader-gate)
che fa da tramite verso il processo TBLoader, curandone l'istanziazione e l'esposizione delle API rest verso l'esterno

## task-scheduler 
### (non ancora presente)
Applicazione web per la gestione dei task schedulati

## login-manager
### (non ancora presente)
Gestione degli utenti e delle licenze

## lock-manager
### (non ancora presente)
Gestione dell'accesso concorrente alle risorse di database