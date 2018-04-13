import { Inject, forwardRef, Injectable } from '@angular/core';
import { CanActivate, UrlSegment, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '@taskbuilder/core';
import { Logger } from '@taskbuilder/core';



@Injectable()
export class RsStandaloneGuard implements CanActivate {

    constructor(
        public authService: AuthService,
        public router: Router,
        public logger: Logger,
    ) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        const url: string = state.url;
        this.logger.info('canActivate => Url: ' + url);
        sessionStorage.setItem('authtoken', state.root.queryParams.token);
        this.authService.setRedirectUrl(url);

        return true;
    }

}
