import { Observable } from 'rxjs';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable } from '@angular/core';

import { HttpService } from './http.service';

@Injectable()
export class InfoService {

    desktop: boolean;
    productInfo: any;
    dictionaries: any;
    culture = { enabled: true, value: '' };
    cultureId = '__current_culture__';

    constructor(private httpService: HttpService) {
        this.culture.value = localStorage.getItem(this.cultureId);
    }
    saveCulture() {
        localStorage.setItem(this.cultureId, this.culture.value);

    }
    public getProductInfo(): Observable<any> {
        return Observable.create(observer => {
            if (this.productInfo) {
                observer.next(this.productInfo);
                observer.complete();
            }
            else {
                let sub = this.httpService.getProductInfo().subscribe(result => {
                    this.productInfo = result.ProductInfos;
                    if (sub)
                        sub.unsubscribe();
                    observer.next(this.productInfo);
                    observer.complete();
                });
            }
        });
    }

    public getDictionaries(): Observable<any> {
        return Observable.create(observer => {
            if (this.dictionaries) {
                observer.next(this.dictionaries);
                observer.complete();
            }
            else {
                let sub = this.httpService.getDictionaries().subscribe(result => {
                    this.dictionaries = result.dictionaries;
                    if (sub)
                        sub.unsubscribe();
                    observer.next(this.dictionaries);
                    observer.complete();
                });
            }
        });
    }
}