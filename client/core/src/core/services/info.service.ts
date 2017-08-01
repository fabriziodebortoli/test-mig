import { Observable } from 'rxjs';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable } from '@angular/core';

import { HttpService } from './http.service';

@Injectable()
export class InfoService {

    desktop: boolean;
    productInfo: any;
    constructor(private httpService: HttpService) {
    }
    getProductInfo: Observable<any>()       {
        return Observable.create(observer => {
            if (this.productInfo) {
                observer.next(this.productInfo);
                observer.complete();
            }
            else {
                let sub = this.httpService.getProductInfo().subscribe(result => {
                    this.productInfo = result.ProductInfos;
                    if (sub)
                        sub.unsubscribe()
                    observer.next(this.productInfo);
                    observer.complete();
                });
            }
        }
    }
}