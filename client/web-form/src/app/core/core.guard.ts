import { Inject, forwardRef } from '@angular/core';
import { CanActivate, UrlSegment, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { environment } from './../../environments/environment';

import { InfoService } from './info.service';
import { LoginSessionService } from './login-session.service';

export class CoreGuard implements CanActivate {

    constructor(
        @Inject(forwardRef(() => LoginSessionService)) private loginService: LoginSessionService,
        @Inject(forwardRef(() => InfoService)) private infoService: InfoService
    ) {

    }

    canActivate(future: ActivatedRouteSnapshot): boolean {
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