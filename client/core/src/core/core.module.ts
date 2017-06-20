import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

/**
 * Servizi
 * 
 * Tutti i servizi condivisi TB (http, websocket, 
 */
import {
    Logger
} from './services';

export * from './services/logger.service';

export const TB_SERVICES = [
    Logger
];

@NgModule({
    providers: [TB_SERVICES]
})
export class TbCoreModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: TbCoreModule,
            providers: [TB_SERVICES]
        };
    }
    constructor( @Optional() @SkipSelf() parentModule: TbCoreModule) {
        if (parentModule) {
            throw new Error(
                'TbCoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
