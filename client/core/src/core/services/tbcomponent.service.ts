import { TbComponentServiceParams } from './tbcomponent.service.params';
import { HttpService } from './http.service';
import { Injectable } from '@angular/core';

import { InfoService } from './info.service';
import { Logger } from './logger.service';
import { EventDataService } from './eventdata.service';
import { Observable } from '../../rxjs.imports';

@Injectable()
export class TbComponentService {
    public logger: Logger;
    public infoService: InfoService;
    public httpService: HttpService;
    public dictionaryId = '';
    public installationVersion = '';
    public translations = [];
    constructor(params: TbComponentServiceParams
    ) {
        this.logger = params.logger;
        this.infoService = params.infoService;
        this.httpService = params.httpService;
        let subs = this.initTranslations(this.dictionaryId).subscribe(ti => {
            if (subs)
                subs.unsubscribe();
            this.translations = ti.translations;
            this.installationVersion = ti.installationVersion;

            if (!this.translations) {
                let subs = this.readTranslationsFromServer(this.dictionaryId).subscribe(tn => {
                    if (subs)
                        subs.unsubscribe();
                    this.translations = tn;
                    this.saveTranslations(this.dictionaryId, this.translations);
                });
            }
        });
    }
    public _TB(baseText: string) {
        return this.translate(this.translations, baseText);
      }
    public readTranslationsFromServer(dictionaryId: string): Observable<Array<any>> {
        return this.httpService.getTranslations('TbCardContentComponent', this.infoService.culture.value);
    }
    public calculateDictionaryId(obj: Object) {
        let dictionaryId = '';
        let needSeparator = false;
        while ((obj = Object.getPrototypeOf(obj)) != Object.prototype) {
            if (needSeparator)
                dictionaryId += '.';
            else
                needSeparator = true;
            dictionaryId += obj.constructor.name;
        }
        return dictionaryId;
    }
    public saveTranslations(dictionaryId: string, translations: any[]) {
        let jItem = { translations: translations, installationVersion: this.installationVersion };
        localStorage.setItem(dictionaryId, JSON.stringify(jItem));
    }
    public readTranslations(dictionaryId: string, installationVersion: string) {
        let item = localStorage.getItem(dictionaryId);

        if (item) {
            try {
                let jItem = JSON.parse(item);

                if (jItem.installationVersion === installationVersion) {
                    return jItem.translations;
                }
            }
            catch (ex) {
                console.log(ex);
            }
        }
        return null;
    }
    public initTranslations(dictionaryId: string): Observable<TranslationInfo> {
        return Observable.create(observer => {
            let sub = this.infoService.getProductInfo().subscribe((productInfo: any) => {
                let ti = new TranslationInfo();
                if (sub)
                    sub.unsubscribe();
                ti.translations = this.readTranslations(dictionaryId, productInfo.installationVersion);
                ti.installationVersion = productInfo.installationVersion;

                observer.next(ti);
                observer.complete();
            });

        });
    }

    public translate(translations: Array<any>, baseText: string) {
        let target = baseText;
        if (translations) {
            translations.some(t => {
                if (t.b === baseText) {
                    target = t.t;
                    return true;
                }
                return false;
            });
        }
        return target;
    }

}

export class TranslationInfo {
    public installationVersion = '';
    public translations = [];
}