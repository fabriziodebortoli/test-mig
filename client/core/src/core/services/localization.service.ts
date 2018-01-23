import { TbComponentServiceParams } from './tbcomponent.service.params';
import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

import { InfoService } from './info.service';
import { EventDataService } from './eventdata.service';
import { Observable } from '../../rxjs.imports';

@Injectable()
export class LocalizationService {

    public dictionaryId = '';
    public translations = [];
    constructor(
        public infoService: InfoService,
        public httpService: HttpService
    ) {
        this.dictionaryId = this.calculateDictionaryId(this);
        const ids = this.dictionaryId.split('.');
        let count = ids.length;
        ids.forEach(id => {
            let subs = this.readFromLocal(id).subscribe(tn => {
                if (subs) {
                    subs.unsubscribe();
                }
                if (tn) {
                    this.translations = this.translations.concat(tn);
                } else {
                    subs = this.readTranslationsFromServer(id).subscribe(
                        tn => {
                            if (subs) {
                                subs.unsubscribe();
                            }
                            if (!tn)
                                tn = [];

                            this.translations = this.translations.concat(tn);
                            if (--count == 0)
                                this.onTranslationsReady();
                            this.saveToLocal(id, tn);
                        }, err => {
                            if (subs) {
                                subs.unsubscribe();
                            }
                            if (--count == 0)
                                this.onTranslationsReady();
                            //dictionary file may not exist on server
                            if (err && err.status === 404) {
                                this.saveToLocal(id, []);
                            }
                        });
                }
            });
        });
    }
    protected onTranslationsReady() {

    }
    public _TB(baseText: string, ...args: any[]) {
        return this.translate(this.translations, baseText, args);
    }
    public readTranslationsFromServer(dictionaryId: string): Observable<Array<any>> {
        return this.httpService.getTranslations(dictionaryId, this.infoService.culture.value);
    }
    public calculateDictionaryId(obj: Object) {
        let dictionaryId = '';
        let needSeparator = false;
        while ((obj = Object.getPrototypeOf(obj)) !== Object.prototype) {
            if (needSeparator) {
                dictionaryId += '.';
            } else {
                needSeparator = true;
            }

            dictionaryId += obj.constructor.name;
        }
        return dictionaryId;
    }
    public saveToLocal(dictionaryId: string, translations: any[]) {
        const sub = this.infoService.getProductInfo(false).subscribe((productInfo: any) => {
            if (sub) {
                sub.unsubscribe();
            }

            const jItem = { tn: translations, v: productInfo.installationVersion };
            localStorage.setItem(this.getStorageId(dictionaryId), JSON.stringify(jItem));
        });
    }
    private getStorageId(dictionaryId: string) {
        return this.infoService.culture.value + '-' + dictionaryId;
    }
    public readFromLocal(dictionaryId: string): Observable<any> {
        return Observable.create(observer => {
            const sub = this.infoService.getProductInfo(false).subscribe((productInfo: any) => {
                if (sub) {
                    sub.unsubscribe();
                }
                let tn = null;
                const item = localStorage.getItem(this.getStorageId(dictionaryId));
                if (item) {
                    try {
                        const jItem = JSON.parse(item);

                        if (jItem.v === productInfo.installationVersion) {
                            tn = jItem.tn;
                        }
                    } catch (ex) {
                        console.log(ex);
                    }
                }

                observer.next(tn);
                observer.complete();
            });

        });
    }

    public translate(translations: Array<any>, baseText: string, args: any[]) {
        let target = baseText;
        if (translations && translations.length) {
            translations.some(t => {
                if (t.b === baseText) {
                    target = t.t;
                    return true;
                }
                return false;
            });
        }

        if (args) {
            target = target.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] != 'undefined'
                    ? args[number]
                    : match
                    ;
            });
        }
        return target;
    }

}
