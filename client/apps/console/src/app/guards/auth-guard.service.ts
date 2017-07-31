import {OperationResult} from '../services/operationResult';
import { CanActivate, CanActivateChild, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { RoleNames, RoleLevels } from "app/authentication/auth-helpers";
import { UrlGuard } from "app/authentication/url-guard";

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

        let opRes: OperationResult;

        if (state.url == '/instancesHome') {
          opRes = UrlGuard.CanNavigateURL(state.url, authorizationInfo);
          if (opRes.Result){
            return true;
          } else {
            alert(opRes.Message);
            this.router.navigateByUrl('/');
            return false;
          }
        }

        // checking generic "provisioning" level

        opRes = UrlGuard.CanNavigateLevel(RoleLevels.Subscription, authorizationInfo);

        if (!opRes.Result) {
          alert(opRes.Message);
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
