# How to create or update font icon file

Dop aver creato la nuova folder m4.zip contentente il nuovi file di font (.eot, .svg, .ttf, .woff) da Fontastic o Icon8 i passi da seguire per ricreare il nuovo file di icone sono i seguenti:

 - estrarre i file del .zip in una folder locale per esempio in C:\icons\m4

 - copiare i nuovi file (.eot, .svg, .ttf, .woff) che trovate dentro la folder **m4** dentro la folder **.\Web\client\icons\src\fonts**

 - aggiornare il file **Web\client\icons\src\fonts\m4.css** sostituendolo con il file **styles.css** che si trova dentro la folder **m4** 

 > **Importante :**
    > nel file **m4.css** va mantenuta la sezione **@font-face { ... *omissis* ... }**

 - occorre creare il repository **ng-icons** che è su github in locale per es nella folder **@TaskBuilder**

 - Dopo aver creato il repository nella folder **@TaskBuilder\ng-icons\docs** si deve modificare il file **index.html** in 
 **\Standard\Web\client\icons\src\docs** il contenuto del file **\m4\icons-reference.html**

> **Importante:**
    > nel file **Standard\Web\client\icons\src\docs\index.html** dopo aver copiato il contenuto del file  **m4\icons-reference.html** va sostituito questo link  `<link rel="stylesheet" href="styles.css">` con questo `<link rel='stylesheet\' href='./fonts/m4.css'>`

 - bisogna modificare i files **Web\client\icons\package.json** e **@TaskBuilder\ng-icons\packag.json** aggiornando in entrambi il numero di versione
    > **"version": "......."**
    
 - nella folder **Web\client\icons** eseguire i seguenti comandi :
   * > *npm i*
   * > *npm run build*

    l'ultimo comando crea una folder **Web\client\icons\dist** il cui contenuto va copiato nella folder **@TaskBuilder\ng-icons**
 
 - A questo punto per rendere permanenti le modifiche occorre fare la
   commit prima e poi la push sia sul repository
   **@TaskBuilder\ng-icons** che sul repository **\Web\client**

 > **Importante :**
    > prima di copiare il contenuto della folder **dist** occorre sincronizzare sia il repository **@TaskBuilder\ng-icons** che il repository  **\Web\client**


