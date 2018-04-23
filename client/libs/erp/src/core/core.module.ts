import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

import { CoreHttpService } from './services/core/core-http.service';
import { ItemsHttpService } from './services/items/items-http.service';
import { WmsHttpService } from './services/wms/wms-http.service';

export { CoreHttpService } from './services/core/core-http.service';
export { ItemsHttpService } from './services/items/items-http.service';
export { WmsHttpService } from './services/wms/wms-http.service';

export { ClipboardEventHelper, KeyboardEventHelper } from './u/helpers';

export const ERP_SERVICES = [
    CoreHttpService,
    ItemsHttpService,
    WmsHttpService
];

@NgModule({
    providers: [ERP_SERVICES]
})
export class ERPCoreModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: ERPCoreModule,
            providers: [ERP_SERVICES]
        };
    }
    constructor( @Optional() @SkipSelf() parentModule: ERPCoreModule) {
        if (parentModule) {
            throw new Error(
                'ERPCoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
