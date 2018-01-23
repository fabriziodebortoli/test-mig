import { Inject, forwardRef, Injectable } from '@angular/core';
import { CanActivate, UrlSegment, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from '../../rxjs.imports';

import { Logger } from './../services/logger.service';
import { AuthService } from './../services/auth.service';
import { InfoService } from './../services/info.service';

@Injectable()
export class CoreGuard implements CanActivate {

    constructor(
        public authService: AuthService,
        public router: Router,
        public logger: Logger,
        public infoService: InfoService
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        let url: string = state.url;
        // this.logger.info('canActivate => Url: ' + url);

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
    }

}