import { CanActivate, ActivatedRouteSnapshot } from '@angular/router';
import { InfoService } from '../services/info.service';
import { LoginSessionService } from '../services/login-session.service';
export declare class CoreGuard implements CanActivate {
    private loginService;
    private infoService;
    constructor(loginService: LoginSessionService, infoService: InfoService);
    canActivate(future: ActivatedRouteSnapshot): boolean;
}
