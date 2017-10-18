import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { ERPService } from './services/erp.service';
export { ERPService } from './services/erp.service';
import { ErpHttpService } from './services/erp-http.service';
export { ErpHttpService } from './services/erp-http.service';

export const ERP_SERVICES = [
    ERPService,
    ErpHttpService
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
