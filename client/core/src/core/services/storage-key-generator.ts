import { Logger } from './../../core/services/logger.service';
import { Injector, ElementRef } from '@angular/core';
import { ComponentInfoService } from '../core.module';
import { get, cloneDeep } from 'lodash';

export enum UserScope { Current, All };
export enum KeyScope { Component, Document, Global };
export type StorageScope = { key: KeyScope, user: UserScope };

export class StorageOptions {
    scope: StorageScope
    componentInfo: { cmpId?: string, type?: string, app?: string, mod?: string }
    component: any;

    constructor(opt?: Partial<StorageOptions>) {
        Object.assign(this, cloneDeep(opt));
    }

    /** mutates object */
    withType(t: string) { this.componentInfo.type = t; return this; }
    /** mutates object */
    withApp(a: string) { this.componentInfo.app = a; return this; }
    /** mutates object */
    withModule(m: string) { this.componentInfo.mod = m; return this; }
    /** mutates object */
    withCmpId(c: string) { this.componentInfo.cmpId = c; return this; }
    /** 
     * tries to fill ComponentInfo from a provided Injector
     * @return mutated object 
     */
    tryGetCmpInfoFrom(i: Injector) {
        if (!i) return;
        let s = i.get(ComponentInfoService);
        if (s) {
            this.componentInfo.app = get(s, 'componentInfo.app', this.componentInfo.app);
            this.componentInfo.mod = get(s, 'componentInfo.mod', this.componentInfo.mod);
        }
        let el = i.get(ElementRef);
        if (el) {
            this.componentInfo.type = el.nativeElement &&
                (el.nativeElement.localName || el.nativeElement.nodeName || el.nativeElement.tagName);
        }
        if (get(i, 'elDef.element.attrs[0][1]') === 'cmpId')
            this.componentInfo.cmpId = get(i, 'elDef.element.attrs[0][2]');
    }
};

export const DefaultStorageOptions = new StorageOptions({ scope: { key: KeyScope.Component, user: UserScope.Current }, componentInfo: {} });

export interface StorageKeyGenerator {
    /**
     * Generates a unique key representing a given key in a given scope
     * @param key starting key
     * @param per the scope
     */
    uniqueKey(key: string, per: StorageScope);
}

export class DefaultStorageKeyGenerator implements StorageKeyGenerator {
    constructor(private options: StorageOptions) { }

    public uniqueKey = (key: string, per: StorageScope) => this.uniqueKeyPer[per.key](key, per.user);

    private globalKeyPart = ['storage'];
    private userKeyPart = userScope =>
        userScope === UserScope.Current ? [localStorage.getItem('_user'), localStorage.getItem('_company')] : [];
    private docKeyPart = () => [this.options.componentInfo.app, this.options.componentInfo.mod].filter(x => x);
    private cmpKeyPart = () => [this.options.componentInfo.type, this.options.componentInfo.cmpId].filter(x => x);

    // tslint:disable-next-line:member-ordering
    private uniqueKeyPer = {
        [KeyScope.Global]: (key, us) => [
            ...this.globalKeyPart,
            key
        ].join('_'),
        [KeyScope.Document]: (key, us) => [
            ...this.globalKeyPart,
            ...this.userKeyPart(us),
            ...this.docKeyPart(),
            key
        ].join('_'),
        [KeyScope.Component]: (key, us) => [
            ...this.globalKeyPart,
            ...this.userKeyPart(us),
            ...this.docKeyPart(),
            ...this.cmpKeyPart(),
            key
        ].join('_')
    };
}

