import { ComponentFactory } from '@angular/core';
export class ComponentInfo {
    factory: ComponentFactory<any>;//usata per creare dinamicamente il componente
    id: string;//id dell'istanza del componente
}
