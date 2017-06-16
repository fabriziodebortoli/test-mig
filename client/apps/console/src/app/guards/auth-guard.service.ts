import { CanActivate, CanActivateChild, Router } from '@angular/router';

import { Injectable } from '@angular/core';

@Injectable()
export class AuthGuardService implements CanActivate {

  constructor(private router: Router) { }

  canActivate() {

    if (localStorage.getItem('jwt-token')) {
        // logged in so return true
        return true;
    }

    // not logged in so redirect to login page with the return url and return false
    this.router.navigate(['/loginComponent']);
    return false;
  }

  canActivateChild() {
    return false;
  }  
}
