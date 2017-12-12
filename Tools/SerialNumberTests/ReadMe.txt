Questa applicazione serve per eseguire test di non regressione sui metodi che tornano il numero di cal.
Quindi nella cartella file ci sono vari licensed.config, ognuno elenca diversi serial number con diversi significati con diversi moduli.
Il nome del file deve essere seguito da una formula del tipo .{0}_{1}_{2}_{3}_{4}_TEST_{5}
Dove i primi 4 sono il numero di cal che ci si aspetta di ottenere con quei serial number 
nell'ordine: named, concurrent (sempre zero in 2x), easylook, magicdocument, magicLink.
L'ultimo è l'elenco degli shortname dei seriali utilizzati, per capire cosa c'è dentro senza aprirli.
il sistema cancella il licensed.config della custom, ne copia uno a uno quelli della cartella, 
rinominandoli correttamente e esegue una init dell'activation obj, 
se poi il risultato della getcalnumber è uguale a quello che ci aspettiamo e che abbiamo indicato sul file 
allora il test è positivo altrimenti è negativo.
Se errori nell'output viene indicato tutto.
Tasto destro per test su singolo file.