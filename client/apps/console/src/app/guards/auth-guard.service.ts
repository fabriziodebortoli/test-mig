import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { LoginService } from '../services/login.service';
import { OperationResult } from '../services/operationResult';
import { AuthorizationInfo, AuthorizationProperties } from "app/authentication/auth-info";
import { RoleNames, RoleLevels } from "app/authentication/auth-helpers";
import { UrlGuard } from "app/authentication/url-guard";

@Injectable()
export class AuthGuardService implements CanActivate {

  constructor(private router: Router, private loginService: LoginService) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {

    try {
      let authorizationStored = localStorage.getItem('auth-info');

      if (authorizationStored !== null) {
        let authorizationProperties: AuthorizationProperties = JSON.parse(authorizationStored);

        let authorizationInfo: AuthorizationInfo = new AuthorizationInfo(
          authorizationProperties.jwtEncoded,
          authorizationProperties.accountName);

        authorizationInfo.authorizationProperties = authorizationProperties;

        let opRes: OperationResult = UrlGuard.CanNavigate(state.url, authorizationInfo);

        if (!opRes.Result) {
          alert(opRes.Message);
          return false;
        }

        // we ask the loginService to send a message to the appComponent to refresh accountName in the toolbar
        this.loginService.sendMessage(authorizationInfo.authorizationProperties.accountName);
        
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
