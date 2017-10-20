import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { CookieModule, CookieService } from 'ngx-cookie';
import { ERPService } from './services/erp.service';
export { ERPService } from './services/erp.service';
import { ErpHttpService } from './services/erp-http.service';
export { ErpHttpService } from './services/erp-http.service';
import { Store } from './services/store';
export { Store } from './services/store';


export const ERP_SERVICES = [
    ERPService,
    ErpHttpService,
    Store
];

@NgModule({
    imports: [CookieModule.forChild()],
    providers: [ERP_SERVICES, CookieService]
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
