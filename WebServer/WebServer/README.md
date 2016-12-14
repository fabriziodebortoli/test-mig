# WebServer
Questo è il progetto contenitore, che ospita i vari moduli che apportano i controllers per l'interfaccia rest. Ogni sottoprogetto rappresenta un modulo caricato dinamicamente allo startup.
I sottoprogetti sono:

## RSWeb:
Gestore del motore di reportistica

## TbLoaderGate
Applicazione web che fa da tramite per il processo TBLoader, curandone l'istanziazione e l'esposizione delle API rest verso l'esterno

## Task Scheduler 
### (non ancora presente)
Applicazione web per la gestione dei task schedulati

## LoginManager
### (non ancora presente)
Gestione degli utenti e delle licenze

## LockManager
### (non ancora presente)
Gestione dell'accesso concorrente alle risorse di database