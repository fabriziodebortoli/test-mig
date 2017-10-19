# Icon Font Library for TaskbuilderWeb App

Libreria che replica le immagini di Mago Desktop in Icon Font in modo da migliorare la velocità di caricamento delle pagine, la scalabilità e le capacità di personalizzazione.

Un icon font è composto da un set predefinito di icone ‘mappate’ ognuna con un carattere. Essendo vettoriale, permette di ridimensionare o cambiare il colore di un'icona semplicemente via CSS.

## Installazione

Per installare questa libreria:

```bash
$ npm install https://github.com/Microarea/ng-icons.git --save
```

## Utilizzo

Dopo aver installato la libreria, puoi importarla nella tua applicazione Angular all'interno di AppModule:

```typescript
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';

import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    TbIconsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

E' inoltre necessario includere il file *m4.css* in *.angular-cli.json*:

```json
"assets": [
    { "glob": "**/*", "input": "../node_modules/@taskbuilder/icons/fonts/", "output": "./assets/m4" },
    ...
],
```

e aggiungere l'import in *index.html*:

```html
<link href="assets/m4/m4.css" rel="stylesheet">
```

Una volta importata la libreria, puoi usare le icone nel modo seguente:

```xml
<!-- You can now use your library component in app.component.html -->
<m4-icon icon="tb-master"></m4-icon>
<m4-icon icon="erp-cash"></m4-icon>
<m4-icon icon="erp-costaccounting"></m4-icon>
<m4-icon icon="erp-summary"></m4-icon>
```

L'elenco completo delle icone finora incluse nella libreria lo trovi a questa [pagina](https://microarea.github.io/ng-icons/)

Le istruzioni per creare e aggiungere nuove icone all'icon font le trovi a questa [pagina](https://github.com/Microarea/Taskbuilder/blob/08aaf9d4df9a73d2e5fa391205d32f2572dd8509/docs/ICONS.md)

Le istruzioni per aggiornare il file delle icon font le trovi a questa [pagina](https://github.com/Microarea/Taskbuilder/blob/master/client/icons/BUILD.MD)