import { UrlService } from './url.service';
import { EventManagerService } from './../../menu/services/event-manager.service';
import { MenuService } from './../../menu/services/menu.service';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable } from 'rxjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

import { OperationResult, LoginSession } from '../../shared/models';

import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { Logger } from './logger.service';

@Injectable()
export class TaskbuilderService {
    defaultUrl = ['home'];
    connected = false;
    errorMessages: string[] = [];
    redirectUrl = this.defaultUrl;

    constructor(private httpService: HttpService,
        private socket: WebSocketService,
        private cookieService: CookieService,
        private logger: Logger,
        private router: Router,
        private menuService: MenuService,
        private urlService: UrlService
    ) {

        // lettura file di configurazione backend urls
        urlService.init().subscribe();

        // const subs = this.socket.close.subscribe(() => {
        //     this.openTbConnection(true);
        // });
        // socket.loginSessionService = this;
    }

    // quando fare openTBconnection??
    // openTbConnection(retry: boolean = false) {
    //     const subs = this.openTbConnectionAsync(retry).subscribe(ret => { 
    //         subs.unsubscribe() 
    //     });
    // }

    openTbConnection(): Observable<boolean> {

        let authtoken = this.cookieService.get('authtoken');
        this.logger.log("openTbConnection...", authtoken);
        return this.httpService.openTBConnection({ authtoken: authtoken }).map(
            (tbRes: OperationResult) => {
                this.logger.log("openTBConnection result", tbRes);
                if (tbRes.error) {
                    this.logger.log("openTBConnection OperationResult Error - Reconnecting...");
                    // il TB c'è ma non riesce a collegare
                    // TODO riconnettere
                    setTimeout(function () {
                        this.openTbConnection();
                    }, 2000);
                    return false;
                }
                // Connesso al TB, ci colleghiamo al WS
                this.logger.info("TbLoader Connected...")
                this.socket.wsConnect();
                return true;

            },
            (error) => {
                this.logger.log("openTBConnection error", error);
                // TODO riconnettere
                // setTimeout(function () {
                //     this.openTbConnection(true);
                // }, 5000);
                return false;
            });

    }

}