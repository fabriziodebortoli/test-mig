import { Inject, forwardRef } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';
import { environment } from './../../environments/environment';
import { LoginSessionService } from './login-session.service';

export class CoreGuard implements CanActivate {

    constructor( @Inject(forwardRef(() => Router)) private router: Router, @Inject(forwardRef(() => LoginSessionService)) private loginService: LoginSessionService) { }

    canActivate(): Observable<boolean> | boolean {
        if (environment.desktop){
            return true;
        }
        
        if (!this.loginService.isConnected()) {
            this.router.navigate(['/login']);
            return false;
        }

        return true;

    }

}