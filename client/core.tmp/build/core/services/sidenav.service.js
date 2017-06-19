/**
 * La sidenav e' comandata da HomeComponent, il quale risulta sottoscritto all'observable sidenavOpened$
 * Ad ogni component che vuole aprire/chiudere la sidenav basta richiamare il metodo toggleSidenav di questo service
 * (es: home-sidenav.component)
 */
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
export class SidenavService {
    constructor() {
        this.sidenavOpenedSource = new Subject();
        this.sidenavOpened$ = this.sidenavOpenedSource.asObservable();
    }
    /**
     * @return {?}
     */
    toggleSidenav() {
        this.sidenavOpenedSource.next();
    }
}
SidenavService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
SidenavService.ctorParameters = () => [];
function SidenavService_tsickle_Closure_declarations() {
    /** @type {?} */
    SidenavService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    SidenavService.ctorParameters;
    /** @type {?} */
    SidenavService.prototype.sidenavOpenedSource;
    /** @type {?} */
    SidenavService.prototype.sidenavOpened$;
}
