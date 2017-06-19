import { Inject, forwardRef } from '@angular/core';
import { InfoService } from '../services/info.service';
import { LoginSessionService } from '../services/login-session.service';
export class CoreGuard {
    /**
     * @param {?} loginService
     * @param {?} infoService
     */
    constructor(loginService, infoService) {
        this.loginService = loginService;
        this.infoService = infoService;
    }
    /**
     * @param {?} future
     * @return {?}
     */
    canActivate(future) {
        if (this.infoService.desktop) {
            return true;
        }
        if (this.loginService.isConnected()) {
            return true;
        }
        //se non sono connesso, mi metto da parte l'url, e poi ci andrò non appena effettuata la connessione
        this.loginService.redirectUrl = [];
        future.url.forEach(seg => this.loginService.redirectUrl.push(seg.path));
        return false;
    }
}
/**
 * @nocollapse
 */
CoreGuard.ctorParameters = () => [
    { type: LoginSessionService, decorators: [{ type: Inject, args: [forwardRef(() => LoginSessionService),] },] },
    { type: InfoService, decorators: [{ type: Inject, args: [forwardRef(() => InfoService),] },] },
];
function CoreGuard_tsickle_Closure_declarations() {
    /**
     * @nocollapse
     * @type {?}
     */
    CoreGuard.ctorParameters;
    /** @type {?} */
    CoreGuard.prototype.loginService;
    /** @type {?} */
    CoreGuard.prototype.infoService;
}
