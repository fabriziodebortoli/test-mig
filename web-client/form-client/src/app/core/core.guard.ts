import { Inject, forwardRef } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { LoginSessionService } from './login-session.service';

export class CoreGuard implements CanActivate {

    constructor( @Inject(forwardRef(() => Router)) private router: Router, @Inject(forwardRef(() => LoginSessionService)) private loginService: LoginSessionService) { }

    canActivate(): Observable<boolean> | boolean {
        if (!this.loginService.isConnected()) {
            this.router.navigate(['/login']);
            return false;
        }

        return true;

    }

}