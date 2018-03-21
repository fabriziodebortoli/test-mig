import { Injectable } from '@angular/core';
import { Http, Headers, Response, URLSearchParams } from '@angular/http';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpProgressEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { EventDataService } from './eventdata.service';
import { InfoService } from './info.service';
import { Subject, Observable, BehaviorSubject, of, concat } from '../../rxjs.imports';
import { delay } from 'rxjs/operators/delay';
import { get } from 'lodash';

export enum ObjType { Document, Report, File, Image, Profile }
export interface Item { namespace?: string; name?: string; level: number; parent?: Item; }
const augmentItem = (partial: Partial<Item>) => item => {
    item = { ...item, ...partial }; item.name = item.name || item.title; return item;
};

@Injectable()
export class ExplorerService {
    private _waiting$ = new BehaviorSubject(false);

    constructor(private info: InfoService, private http: Http) { }

    get waiting$() { return this._waiting$.asObservable(); }

    async GetApplications(): Promise<Item[]> {
        return this.tryGetMap('GettAllApplications', r => r.applications.map(augmentItem({ level: 1 })));
    }

    async GetModules(app: Item): Promise<Item[]> {
        return this.tryGetMap({ method: 'GetAllModulesByApplication', params: { 'appName': app.name.toLowerCase() } },
            r => r.modules.map(augmentItem({ parent: app, level: 2 })));
    }

    async GetObjs(app: Item, module: Item, type: ObjType): Promise<Item[]> {
        return this.tryGetMap({
            method: 'GetAllObjectsBytype', params: {
                'appName': app.name.toLowerCase(),
                'modulesName': module.name.toLowerCase(),
                'objType': type
            }
        }, r => r.objects
            .map(augmentItem({ parent: module, level: 3 })));
    }

    async GetObjsByNamespace(namespace: string, type: ObjType): Promise<Item[]> {
        return [];
    }

    async GetByUser(type: ObjType, namespace: string, username: string): Promise<Item[]> {
        return [];
    }

    async tryGetMap(method: string | { method: string, params?: { [n: string]: any } }, map?: (p: any) => any, ): Promise<any> {
        const search = new URLSearchParams();
        if (typeof method !== 'string') {
            const m = method as any;
            m.params && Object.keys(m.params).forEach(k => search.set(k, m.params[k]));
            method = m.method;
        }
        const headers = new Headers();
        headers.append('Authorization', this.info.getAuthorization());
        const url = this.info.getBaseUrl() + '/tbfs-service/' + method;
        this._waiting$.next(true);
        let res = await this.http.get(url, { headers, search, withCredentials: true })
            .map(r => r && r.ok && r.text() ? r.json() : null)
            .toPromise();
        this._waiting$.next(false);
        if (res && map) res = map(res);
        return res;
    }
}

@Injectable()
export class UploadInterceptor implements HttpInterceptor {
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.url === 'mockSaveUrl') {
            const events: Observable<HttpEvent<any>>[] = [0, 30, 60, 100].map((x) => of(<HttpProgressEvent>{
                type: HttpEventType.UploadProgress,
                loaded: x,
                total: 100
            }).pipe(delay(1000)));
            const success = of(new HttpResponse({ status: 200 })).pipe(delay(1000));
            events.push(success);
            return Observable.concat(...events);
        }
        if (req.url === 'mockRemoveUrl') {
            return of(new HttpResponse({ status: 200 }));
        }
        return next.handle(req);
    }
}
