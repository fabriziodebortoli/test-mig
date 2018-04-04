import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpHeaders
} from '@angular/common/http';
import { InfoService } from '@taskbuilder/core';

import { Observable } from 'rxjs/Observable';

@Injectable()
export class HttpSecureInterceptor implements HttpInterceptor {

    private httpHeaders: HttpHeaders;

    constructor(
        private infoService: InfoService
    ) {
        this.httpHeaders = new HttpHeaders().set('Authorization', this.infoService.getAuthorization());
    }
  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {

    // if request isn't against our server, no need to add Auth info
    if ( request.url.indexOf(this.infoService.getBaseUrl()) < 0 ) {
      return next.handle(request);
    }

    // add a custom header
    const customReq = request.clone({
        headers: this.httpHeaders
      });

    return next.handle(customReq);
  }
}
