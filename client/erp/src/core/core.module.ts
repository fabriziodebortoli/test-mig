import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';
import { ERPService, Store } from './services';
export { ERPService, Store } from './services';

export const ERP_SERVICES = [
    ERPService,
    Store
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
