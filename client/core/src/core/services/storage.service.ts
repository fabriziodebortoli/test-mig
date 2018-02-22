import { Injectable } from '@angular/core';

export type StorageOptions = { scope?: KeyScope, componentInfo?: { cmpId?: string, type: string, app?: string, mod?: string } };
export enum KeyScope { Component, Document, Global };

@Injectable()
export class StorageService {
    private storageOpt: StorageOptions = { scope: KeyScope.Component };

    get options(): StorageOptions { return this.storageOpt; }
    set options(opt: StorageOptions) {
        this.storageOpt = { ...this.storageOpt, ...opt };
    }

    get<T>(key: string): T;
    get(key: string): any {
        return JSON.parse(localStorage.getItem(this.uniqueKey(key, this.storageOpt.scope)));
    }

    getOrDefault<T>(key: string, def: T): T {
        return { ...def as any, ...(this.get(key) || this.set(key, def)) };
    }

    using<T>(key: string, def: T, map: (s: T) => T): T {
        return this.set(key, map(this.getOrDefault<T>(key, def)));
    }

    remove(key: string): void {
        localStorage.removeItem(this.uniqueKey(key, this.storageOpt.scope));
    }

    set<T>(key: string, data: T): T;
    set(key: string, data: any): any {
        localStorage.setItem(this.uniqueKey(key, this.storageOpt.scope), JSON.stringify(data));
        return data;
    }

    private uniqueKey = (key, per) => this.uniqueKeyPer[per](key);

    // tslint:disable-next-line:member-ordering
    private uniqueKeyPer = {
        [KeyScope.Component]: key => [
            localStorage.getItem('_user'),
            localStorage.getItem('_company'),
            this.storageOpt.componentInfo.app,
            this.storageOpt.componentInfo.mod,
            this.storageOpt.componentInfo.type,
            this.storageOpt.componentInfo.cmpId,
            key].filter(x => x).join('_'),
        [KeyScope.Document]: key => '',
        [KeyScope.Global]: key => ''
    };
}
