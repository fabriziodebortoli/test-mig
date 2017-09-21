import { Inject, forwardRef } from '@angular/core';
import { CanActivate, UrlSegment, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { InfoService } from './../services/info.service';
import { LoginSessionService } from './../services/login-session.service';

export class CoreGuard implements CanActivate {

    constructor(
        @Inject(forwardRef(() => LoginSessionService)) private loginService: LoginSessionService,
        @Inject(forwardRef(() => InfoService)) private infoService: InfoService
    ) {

    }

    canActivate(future: ActivatedRouteSnapshot): boolean {

        if (this.loginService.isConnected()) {
            return true;
        }

        if (this.infoService.desktop) {
            // aggiungo parametro a cookie
            // verifica token => NETCore service dedicato?? (altro metodo di loginManager)
            // redirect a login
            // return true;
        }

        //se non sono connesso, mi metto da parte l'url, e poi ci andrò non appena effettuata la connessione
        this.loginService.redirectUrl = [];
        future.url.forEach(seg => this.loginService.redirectUrl.push(seg.path));
        //return false;

        // redirect to login e togliere da loginsessionservice
    }
}