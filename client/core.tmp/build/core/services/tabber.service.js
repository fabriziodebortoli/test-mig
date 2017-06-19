/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
export class TabberService {
    constructor() {
        this.tabSelectedSource = new Subject();
        this.tabSelected$ = this.tabSelectedSource.asObservable();
    }
    /**
     * @param {?} index
     * @return {?}
     */
    selectTab(index) {
        this.tabSelectedSource.next(index);
    }
}
TabberService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
TabberService.ctorParameters = () => [];
function TabberService_tsickle_Closure_declarations() {
    /** @type {?} */
    TabberService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TabberService.ctorParameters;
    /** @type {?} */
    TabberService.prototype.tabSelectedSource;
    /** @type {?} */
    TabberService.prototype.tabSelected$;
}
