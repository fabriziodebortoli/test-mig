import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { CookieService } from 'angular2-cookie/services/cookies.service';
import { OperationResult } from '../../shared/models/operation-result.model';
import { LoginSession } from '../../shared/models/login-session';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';
export declare class LoginSessionService {
    private httpService;
    private socket;
    private cookieService;
    private logger;
    private router;
    defaultUrl: string[];
    connected: boolean;
    errorMessages: string[];
    redirectUrl: string[];
    constructor(httpService: HttpService, socket: WebSocketService, cookieService: CookieService, logger: Logger, router: Router);
    checkIfLogged(): void;
    login(connectionData: LoginSession): Observable<OperationResult>;
    logout(): void;
    isConnected(): boolean;
    setConnected(val: boolean): void;
}
