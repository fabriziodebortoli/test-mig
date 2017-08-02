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

import { TaskbuilderCoreModule, TbCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    TaskbuilderCoreModule,
    TbCoreModule.forRoot(),
    TbIconsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

E' inoltre necessario includere il file *m4.css* in *.angular-cli.json*:

```json
"styles": [
    "../node_modules/@taskbuilder/icons/m4.css",
    ...
],
```

Una volta importata la libreria, puoi usare le icone nel modo seguente:

```xml
<!-- You can now use your library component in app.component.html -->
<tb-icon icon="tb-master"></tb-icon>
<tb-icon icon="erp-cash"></tb-icon>
<tb-icon icon="erp-costaccounting"></tb-icon>
<tb-icon icon="erp-summary"></tb-icon>
```

L'elenco completo delle icone finora incluse nella libreria lo trovi a questa [pagina](http://microarea.github.io/)