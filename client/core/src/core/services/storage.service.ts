import { Injectable } from '@angular/core';
import { Logger } from './../../core/services/logger.service';
import { Observable, Subject, Observer } from '../../rxjs.imports';

export enum KeyScope { Component, Document, Global };
export enum ChangeAction { set, remove };
export type StorageOptions = { scope?: KeyScope, componentInfo?: { cmpId?: string, type: string, app?: string, mod?: string } };
export type Change = { action: ChangeAction; key: string };

@Injectable()
export class StorageService {
    options: StorageOptions = { scope: KeyScope.Component };

    constructor(private log: Logger) { }

    get<T>(key: string): T;
    get(key: string): any {
        return JSON.parse(localStorage.getItem(this.uniqueKey(key, this.options.scope)));
    }

    getOrDefault<T>(key: string, def: T): T {
        return { ...def as any, ...(this.get(key) || this.set(key, def)) };
    }

    using<T>(key: string, def: T, map: (s: T) => T): T {
        return this.set(key, map(this.getOrDefault<T>(key, def)));
    }

    remove(key: string): void {
        let ukey = this.uniqueKey(key, this.options.scope);
        localStorage.removeItem(ukey);
    }

    set<T>(key: string, data: T): T;
    set(key: string, data: any): any {
        let ukey = this.uniqueKey(key, this.options.scope);
        try {
            localStorage.setItem(ukey, JSON.stringify(data));
        } catch (e) {
            if (e.name === 'QUOTA_EXCEEDED_ERR' || e.name === 'NS_ERROR_DOM_QUOTA_REACHED' ||
                e.name === 'QuotaExceededError' || e.name === 'W3CException_DOM_QUOTA_EXCEEDED_ERR') {
                this.log.error('LocalStorage is full. Error saving: ' + ukey);
            } else {
                this.log.error(`LocalStorage error saving : ${ukey} - ${e.message}`);
            }
        }
        return data;
    }

    private uniqueKey = (key, per) => this.uniqueKeyPer[per](key);

    // tslint:disable-next-line:member-ordering
    private uniqueKeyPer = {
        [KeyScope.Component]: key => [
            'storage',
            localStorage.getItem('_user'),
            localStorage.getItem('_company'),
            this.options.componentInfo.app,
            this.options.componentInfo.mod,
            this.options.componentInfo.type,
            this.options.componentInfo.cmpId,
            key].filter(x => x).join('_'),
        [KeyScope.Document]: key => '',
        [KeyScope.Global]: key => ''
    };
}
