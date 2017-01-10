import { Inject, forwardRef } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { LoginSessionService } from './';

export class CoreGuard implements CanActivate {

    constructor(private loginService: LoginSessionService) { }

    canActivate(): Observable<boolean> | boolean {
        return this.loginService.isConnected();
    }

}