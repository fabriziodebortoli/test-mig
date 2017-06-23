import { ComponentFactory } from '@angular/core';

import { DocumentService } from '@taskbuilder/core';

export class ComponentInfo {
    factory: ComponentFactory<any>;//usata per creare dinamicamente il componente
    id = '';//id dell'istanza del componente
    document: DocumentService = null;
    args: any = {};
}
