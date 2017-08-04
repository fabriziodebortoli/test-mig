import { InfoService } from './../../core/services/info.service';
import { ComponentFactory, Inject, forwardRef } from '@angular/core';
import { EventDataService } from './../../core/services/eventdata.service';

import { DocumentService } from '../../core/services/document.service';

export class ComponentInfo {
    factory: ComponentFactory<any>;//usata per creare dinamicamente il componente
    id = '';//id dell'istanza del componente
    parentId = '';//id dell'istanza del componente parent se esiste
    name = '';//nome del componente
    app = ''; //nome applicazione server
    mod = ''; //nome modulo server
    modal = false;//indica se si tratta di una finestra modale
    document: DocumentService = null;
    args: any = {};
}
export class ComponentInfoService {
    public componentInfo: ComponentInfo;
    constructor(
        @Inject(forwardRef(() => InfoService)) public globalInfoService: InfoService)
    { }
    public getComponentId() {
        return this.componentInfo ? this.componentInfo.id : '';
    }
}