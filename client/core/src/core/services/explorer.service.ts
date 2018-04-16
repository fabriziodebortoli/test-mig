import { Injectable } from '@angular/core';
import { Http, Headers, Response, URLSearchParams } from '@angular/http';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpProgressEvent,
  HttpEventType,
  HttpResponse
} from '@angular/common/http';
import { EventDataService } from './eventdata.service';
import { InfoService } from './info.service';
import {
  Subject,
  Observable,
  BehaviorSubject,
  of,
  concat
} from '../../rxjs.imports';
import { delay } from 'rxjs/operators/delay';
import { Maybe } from '../../shared/commons/monads/maybe';
import { get } from 'lodash';

export enum ObjType {
  Document,
  Report,
  File,
  Image
}
export interface Item {
  namespace?: string;
  name?: string;
  level: number;
  parent?: Item;
  icon?: string;
  type?: ObjType;
}
export const rootItem: Item = { name: 'Root', namespace: '', level: 0 };

@Injectable()
export class ExplorerService {
  private _waiting$ = new BehaviorSubject(false);

  constructor(public info: InfoService, private http: Http) {}

  get waiting$() {
    return this._waiting$.asObservable();
  }

  async GetApplications(): Promise<Maybe<Item[]>> {
    return this.tryGetMap('GettAllApplications', r =>
      r.applications.map(augmentItem({ level: 1, parent: rootItem }))
    );
  }

  async GetModules(app: Item): Promise<Maybe<Item[]>> {
    return this.tryGetMap(
      {
        method: 'GetAllModulesByApplication',
        params: { appName: app.name.toLowerCase() }
      },
      r => r.modules.map(augmentItem({ parent: app, level: 2 }))
    );
  }

  async GetObjs(
    app: Item,
    module: Item,
    type: ObjType
  ): Promise<Maybe<Item[]>> {
    return this.tryGetMap(
      {
        method: 'GetAllObjectsBytype',
        params: {
          appName: app.name.toLowerCase(),
          modulesName: module.name.toLowerCase(),
          objType: type
        }
      },
      r => r.objects.map(augmentItem({ parent: module, level: 3 }, type))
    );
  }
  async ExistsObject(
    objnameSpace: string,
    user: string,
    companyName: string,
    culture: string
  ): Promise<boolean> {
    return (await this.tryGetMap({
      method: 'ExistObject',
      params: { objnameSpace, user, companyName, culture }
    })).getOrDefault(false);
  }

  async GetDescription(
    objnameSpace: string,
    user: string,
    companyName: string,
    culture: string
  ): Promise<string> {
    return (await this.tryGetMap({
      method: 'GetDescription',
      params: { objnameSpace, user, companyName, culture }
    })).getOrDefault(false);
  }


  async GetObjsByNamespace(namespace: string, type: ObjType): Promise<Item[]> {
    return [];
  }

  async GetByUser(
    type: ObjType,
    namespace: string,
    username: string
  ): Promise<Item[]> {
    return [];
  }

  getHeaders() {
    const headers = new Headers();
    headers.append('Authorization', this.info.getAuthorization());
    return headers;
  }

  // async tryGetMap<T>(method: string | { method: string; params?: { [n: string]: any } }, map?: (p: T) => T): Promise<Maybe<T>>
  async tryGetMap(
    method: string | { method: string; params?: { [n: string]: any } },
    map?: (p: any) => any
  ): Promise<Maybe<any>> {
    const search = new URLSearchParams();
    if (typeof method !== 'string') {
      const m = method as any;
      m.params &&
        Object.keys(m.params).forEach(k => search.set(k, m.params[k]));
      method = m.method;
    }
    const url = this.info.getBaseUrl() + '/tbfs-service/' + method;
    this._waiting$.next(true);
    let res = await this.http
      .get(url, { headers: this.getHeaders(), search, withCredentials: true })
      .map(r => (r && r.ok && r.text() ? r.json() : null))
      .toPromise();
    this._waiting$.next(false);
    if (res && map) res = map(res);
    return Maybe.from(res);
  }
}

const augmentItem = (partial: Partial<Item>, type?: ObjType) => item => {
  const humanizeName = (name: string) => {
    let idx = -1;
    if ((idx = name.lastIndexOf('/')) !== -1) return name.slice(idx + 1);
    if ((idx = name.lastIndexOf('\\')) !== -1) return name.slice(idx + 1);
    return name;
  };
  const typeToIcon = (t?: ObjType) =>
    typeof t === 'undefined'
      ? 'tb-open'
      : {
          [ObjType.Document]: 'erp-document',
          [ObjType.File]: 'erp-documenttextnote',
          [ObjType.Report]: 'tb-report',
          [ObjType.Image]: 'tb-picture'
        }[t];
  item = { ...item, ...partial };
  item.name = humanizeName(item.name || item.title);
  item.namespace = item.namespace || item.NameSpace;
  item.type = type;
  item.icon = typeToIcon(type);
  return item;
};
