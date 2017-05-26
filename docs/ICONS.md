## M4 Icon font

M4 si basa su una libreria di Web Icon Font creata tramite [Fontastic.me](http://fontastic.me/) e [Icons8](https://it.icons8.com/)

Per Mago4 si era scelto di usare le icone attingendo da Icons8, in particolare nell'insieme relativo a IOS (attualmente per es. iOS 10) e tali icone venivano salvate nel colore grigio #333333 (se non vi erano richieste particolari).

Lo schema con le indicazioni per la creazione delle icone di Mago4 era il seguente:

### ICONA (monocromatica, può essere colorata)

| TIPO     | Usata da                 | Dimensione | Anche Bianca | Formato | Overlay                       | Cartella |
| :--      |    --                    |      --    |    --        |     --  |      --                       |  --:     |
|  TILEMNG |Selettore                 | 25         |  Si (NOME_w) |Icons8   |Axialis 24                     | 24x24    |
|  TOOLBAR |Toolbar Bottoni           | 25         |  No          |Icons8   |Axialis 24 anche per le NOME_w | 24x24    |
|  MINI    |Toolbar e Bottoni Bodyedit| 25         |  No          |Icons8   |Axialis 24                     | 24x24    |
|  CONTROL |Control Status Tile       | 20         |  No          |Icons8   |Axialis 20                     | 20x20    |

### GLYPH (multicolore)

| TIPO     | Usata da                 | Dimensione | Bianca       | Formato   | Overlay    | Cartella |
| :--      |    --                    |      --    |    --        |     --    |      --    |  --:     |
| GLYPH    |Cell Bodyedit Tree        | 20         |  No          |Axialis 20 |Axialis 20  | Glyph    |


Attualmente, poichè non era stata tenuta corrispondenza tra i nomi delle icone su Mago4 e quelle Icons8, per ogni icona è necessario:
 * individuare (a tentativi) l'icona su icons8
 * nel caso abbia un overlay vedere tra quelli che propone icons8 quale è quello più adatto.
 * salvare il file svg con il nome del file utilizzato in Mago4 e preceduto da 'erp-' o 'tb-' (es: il file OutsourcedConfirmed.png diventa erp-outsourcedConfirmed.svg).

La versione bianca non viene salvata poiché utilizzando un icon font sarà sufficiente utilizzare la versione base andando, attraverso il css, ad impostare il colore bianco.

Su fontastic il set di riferimento è "Mago Icon Set", mentre il nome dell'Icon Font è "M4".
Per estrarre l'Icon Font è necessario:
* selezionare le icone
* selezionare la tab "publish"
* nel riquadro "Or Install Manually" procedere al download

Per accedere ai siti di Fontastic e di Icons8 sono necessarie delle credenziali che possono essere richieste a Team Sistemi Informativi.




