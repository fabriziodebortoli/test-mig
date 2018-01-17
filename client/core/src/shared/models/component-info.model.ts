import { ComponentFactory } from '@angular/core';

export class ComponentInfo {
    factory: ComponentFactory<any>;//usata per creare dinamicamente il componente
    id = '';//id dell'istanza del componente
    parentId = '';//id dell'istanza del componente parent se esiste
    name = '';//nome del componente
    app = ''; //nome applicazione server
    mod = ''; //nome modulo server
    modal = false;//indica se si tratta di una finestra modale
    // document: DocumentService = null;
    document = null;
    tbLoaderDoc = false;//indica se si tratta di un componente che corrisponde ad un documento tbloader
    args: any = {};
}
