import { ModuleWithProviders } from '@angular/core';
import { CoreHttpService } from './services/core/core-http.service';
import { ItemsHttpService } from './services/items/items-http.service';
import { WmsHttpService } from './services/wms/wms-http.service';
export { CoreHttpService } from './services/core/core-http.service';
export { ItemsHttpService } from './services/items/items-http.service';
export { WmsHttpService } from './services/wms/wms-http.service';
export { ClipboardEventHelper, KeyboardEventHelper } from './u/helpers';
export declare const ERP_SERVICES: (typeof CoreHttpService | typeof ItemsHttpService | typeof WmsHttpService)[];
export declare class ERPCoreModule {
    static forRoot(): ModuleWithProviders;
    constructor(parentModule: ERPCoreModule);
}
