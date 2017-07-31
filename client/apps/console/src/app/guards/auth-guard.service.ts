import { CanActivate, CanActivateChild, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { RoleNames, RoleLevels } from "app/authentication/auth-helpers";

@Injectable()
export class AuthGuardService implements CanActivate {

  constructor(private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {

    if (state.url == '/logout') {
      if (localStorage.length == 0) {
        alert('You are already not logged!');
        return;
      }

      localStorage.removeItem('auth-info');
      this.router.navigateByUrl('/appHome');
      alert('You are now logged out');
      return;
    }

    try {
      let authorizationStored = localStorage.getItem('auth-info');

      if (authorizationStored !== null) {
        let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

        let authorizationInfo: AuthorizationInfo = new AuthorizationInfo(
          authorizationProperties.jwtEncoded,
          authorizationProperties.accountName);

        authorizationInfo.authorizationProperties = authorizationProperties;

        if (state.url == '/instancesHome') {
          if (!authorizationInfo.VerifyRole(RoleNames.Admin, RoleLevels.Instance, "*")) {
            // user has no roles to navigate the requested url sending him back to home component
            alert(RoleNames.Admin + ' role missing');
            this.router.navigateByUrl('/');
            return false;
          } else {
            return true;
          }
        }        

        if (!authorizationInfo.VerifyRoleLevel(RoleNames.Admin, RoleLevels.Subscription)) {
          // user has no roles to navigate the requested url sending him back to home component
          alert(RoleNames.Admin + ' role missing');
          this.router.navigateByUrl('/');
          return false;
        } 

        return true;
      }
    }
    catch (exc) {
      alert('An exception occurred ' + exc);
      return false;
    }

    // not logged in so redirect to login page with the return url and return false
    this.router.navigate(['/loginComponent'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  canActivateChild() {
    return false;
  }
}
