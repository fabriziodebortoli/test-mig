import { CanActivate, CanActivateChild, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { RoleNames } from "app/authentication/auth-helpers";

@Injectable()
export class AuthGuardService implements CanActivate {

  constructor(private router: Router) { }

  canActivate() {
    try {
      let authorizationStored = localStorage.getItem('auth-info');

      if (authorizationStored !== null) {
        let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

        let authorizationInfo: AuthorizationInfo = new AuthorizationInfo(
          authorizationProperties.jwtEncoded,
          authorizationProperties.accountName);
      
        authorizationInfo.authorizationProperties = authorizationProperties;

        if (!authorizationInfo.HasRole(RoleNames.ProvisioningAdmin)) {
          alert(RoleNames.ProvisioningAdmin.toString() + ' role missing');
          return false;
        }

        return true;
      }
    }
    catch (exc)
    {
      alert('An exception occurred ' + exc);
      return false;      
    }

    // not logged in so redirect to login page with the return url and return false
    this.router.navigate(['/loginComponent']);
    return false;
  }

  canActivateChild() {
    return false;
  }  
}
