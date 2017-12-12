import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

import { AccountingHttpService } from './services/accounting/accounting-http.service';
import { CoreHttpService } from './services/core/core-http.service';
import { ManufacturingHttpService } from './services/manufacturing/manufacturing-http.service';
import { LogisticsHttpService } from './services/logistics/logistics-http.service';

export { AccountingHttpService } from './services/accounting/accounting-http.service';
export { CoreHttpService } from './services/core/core-http.service';
export { ManufacturingHttpService } from './services/manufacturing/manufacturing-http.service';
export { LogisticsHttpService } from './services/logistics/logistics-http.service';

export { ClipboardEventHelper, KeyboardEventHelper } from './u/helpers';

export const ERP_SERVICES = [
    AccountingHttpService,
    CoreHttpService,
    ManufacturingHttpService,
    LogisticsHttpService
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
