import { Inject, forwardRef } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Rx';

import { environment } from './../../environments/environment';

import { LoginService } from './login.service';

export class CoreGuard implements CanActivate {

  constructor(
    @Inject(forwardRef(() => Router)) private router: Router,
    @Inject(forwardRef(() => LoginService)) private loginService: LoginService
  ) { }

  canActivate(): Observable<boolean> | boolean {
    if (!this.loginService.isLogged()) {
      this.router.navigate(['/login']);
      return false;
    }
    return true;
  }

}
