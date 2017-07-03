import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
export class LoginSessionService {
    /**
     * @param {?} httpService
     * @param {?} socket
     * @param {?} cookieService
     * @param {?} logger
     * @param {?} router
     */
    constructor(httpService, socket, cookieService, logger, router) {
        this.httpService = httpService;
        this.socket = socket;
        this.cookieService = cookieService;
        this.logger = logger;
        this.router = router;
        this.defaultUrl = ['home'];
        this.connected = false;
        this.errorMessages = [];
        this.redirectUrl = this.defaultUrl;
        const subs = this.socket.close.subscribe(() => {
            subs.unsubscribe();
            this.setConnected(false);
        });
        this.checkIfLogged();
    }
    /**
     * @return {?}
     */
    checkIfLogged() {
        const /** @type {?} */ subs = this.httpService.isLogged().subscribe(ret => {
            if (!ret) {
                this.setConnected(false);
            }
            else {
                this.logger.debug('Just logged in');
                this.setConnected(true);
                this.socket.wsConnect();
            }
            subs.unsubscribe();
        }, error => {
            this.errorMessages = [error];
            this.logger.error('isLogged HTTP error: ' + error);
            subs.unsubscribe();
            this.setConnected(false);
        });
    }
    /**
     * @param {?} connectionData
     * @return {?}
     */
    login(connectionData) {
        return Observable.create(observer => {
            const /** @type {?} */ subs = this.httpService.login(connectionData).subscribe(result => {
                this.setConnected(!result.error);
                this.errorMessages = result.messages;
                if (this.connected) {
                    this.socket.wsConnect();
                }
                observer.next(result);
                observer.complete();
                subs.unsubscribe();
            }, error => {
                this.logger.error('login HTTP error: ' + error);
                this.errorMessages = [error];
                observer.error(error);
                observer.complete();
                subs.unsubscribe();
            });
        });
    }
    /**
     * @return {?}
     */
    logout() {
        const /** @type {?} */ subscription = this.httpService.logout().subscribe(loggedOut => {
            this.logger.debug('logout returns: ' + loggedOut);
            this.setConnected(!loggedOut);
            this.socket.wsClose();
            this.cookieService.remove('authtoken');
            subscription.unsubscribe();
        }, error => {
            this.logger.error('logout HTTP error: ' + error);
            subscription.unsubscribe();
        });
    }
    /**
     * @return {?}
     */
    isConnected() {
        return this.connected;
    }
    /**
     * @param {?} val
     * @return {?}
     */
    setConnected(val) {
        this.connected = val;
        let /** @type {?} */ url = this.connected ? this.redirectUrl : ['login'];
        if (url.length === 0) {
            url = this.defaultUrl;
        }
        this.router.navigate(url, { skipLocationChange: false, replaceUrl: false });
    }
}
LoginSessionService.decorators = [
    { type: Injectable },
];
/**
 * @nocollapse
 */
LoginSessionService.ctorParameters = () => [
    { type: HttpService, },
    { type: WebSocketService, },
    { type: CookieService, },
    { type: Logger, },
    { type: Router, },
];
function LoginSessionService_tsickle_Closure_declarations() {
    /** @type {?} */
    LoginSessionService.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    LoginSessionService.ctorParameters;
    /** @type {?} */
    LoginSessionService.prototype.defaultUrl;
    /** @type {?} */
    LoginSessionService.prototype.connected;
    /** @type {?} */
    LoginSessionService.prototype.errorMessages;
    /** @type {?} */
    LoginSessionService.prototype.redirectUrl;
    /** @type {?} */
    LoginSessionService.prototype.httpService;
    /** @type {?} */
    LoginSessionService.prototype.socket;
    /** @type {?} */
    LoginSessionService.prototype.cookieService;
    /** @type {?} */
    LoginSessionService.prototype.logger;
    /** @type {?} */
    LoginSessionService.prototype.router;
}
