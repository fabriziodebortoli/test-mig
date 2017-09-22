import { Inject, forwardRef, Injectable } from '@angular/core';
import { CanActivate, UrlSegment, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { Logger } from './../services/logger.service';
import { AuthService } from './../services/auth.service';
import { InfoService } from './../services/info.service';
import { LoginSessionService } from './../services/login-session.service';

@Injectable()
export class CoreGuard implements CanActivate {

    constructor(
        private authService: AuthService,
        private router: Router,
        private logger: Logger,
        private loginService: LoginSessionService,
        private infoService: InfoService
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        let url: string = state.url;
        this.logger.info('canActivate => Url: ' + url);

        return this.authService.isLogged().map(isLogged => {
            if (isLogged) {
                if (url === this.authService.getLoginUrl()) {
                    this.router.navigate([this.authService.getDefaultUrl()]);
                    return false;
                }
                return true;
            } else {
                this.authService.setRedirectUrl(url);
                this.router.navigate([this.authService.getLoginUrl()]);
                return false;
            }
        }).catch(() => {
            this.authService.setRedirectUrl(url);
            this.router.navigate([this.authService.getLoginUrl()]);
            return Observable.of(false);
        });









        // if (this.authService.isUserLoggedIn()) {
        //     return true;
        // }
        // this.authService.setRedirectUrl(url);
        // this.router.navigate([this.authService.getLoginUrl()]);
        // return false;
    }


    // canActivate(future: ActivatedRouteSnapshot): boolean {

    //     if (this.loginService.isConnected()) {
    //         return true;
    //     }

    //     if (this.infoService.desktop) {
    //         // aggiungo parametro a cookie
    //         // verifica token => NETCore service dedicato?? (altro metodo di loginManager)
    //         // redirect a login
    //         // return true;
    //     }

    //     //se non sono connesso, mi metto da parte l'url, e poi ci andrò non appena effettuata la connessione
    //     this.loginService.redirectUrl = [];
    //     future.url.forEach(seg => this.loginService.redirectUrl.push(seg.path));
    //     //return false;

    //     // redirect to login e togliere da loginsessionservice
    // }
}