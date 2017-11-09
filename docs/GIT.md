# Bignami comandi GIT
In questo documento trovate i principali comandi per git

* [Clone](#clone)
* [Sincronizzazione](#sincronizzazione)
* [Branch](#branch)

Link utile: [GIT - Guida tascabile](http://rogerdudler.github.io/git-guide/index.it.html)

## Clone
Il primo passaggio è il clone di un repository remoto già esistente.
Questo comando crea una directory con il nome del repository e vi scarica il contenuto remoto

```git clone <url-repository-remoto>```

## Sincronizzazione

### Verificare lo stato del repository
```git status```

### Aggiungere file all'elenco delle modifiche da committare
```git add nomefile```  oppure ```git add .```

### Committare i file in out
```git commit -m "messaggio di commit"```

### Scaricare ultime modifiche dal repository remoto
```git pull```

### Inviare le proprie modifiche al repository remoto
```git push```

## Branch

### Creare nuovo branch
```git checkout -b nomebranch```

### Andare nel branch master
```git checkout master```

### Andare in un branch esistente
```git checkout nomebranch```

### Inviare il branch al repo remoto
```git push origin nomebranch```

### Allineare il branch con il master
```git merge master```

### Allineare il master a un branch esistente
`git checkout master
git merge nomebranch`

### Cancellare branch
```git branch -d nomebranch```

