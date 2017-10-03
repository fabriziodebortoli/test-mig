import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

/**
 * Servizi
 * 
 * Tutti i servizi condivisi TB (http, websocket, 
 */
import { ERPService } from './services/erp.service';

export { ERPService } from './services/erp.service';

export const ERP_SERVICES = [
    ERPService
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
