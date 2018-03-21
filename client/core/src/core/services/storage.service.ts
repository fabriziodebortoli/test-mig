import { Injectable, Injector } from '@angular/core';
import { Logger } from './../../core/services/logger.service';
import { Observable, Subject, Observer } from '../../rxjs.imports';
import {
    StorageOptions, KeyScope, UserScope, DefaultStorageOptions,
    DefaultStorageKeyGenerator, StorageKeyGenerator
} from './storage-key-generator';

@Injectable()
export class StorageService {
    options: StorageOptions = new StorageOptions(DefaultStorageOptions);
    keyGenerator: StorageKeyGenerator;

    constructor(private log: Logger, private injector: Injector) {
        this.options.tryGetCmpInfoFrom(injector);
        this.keyGenerator = new DefaultStorageKeyGenerator(this.options, this.log);
    }

    /**
     * gets value by key from storage. raises exception if not found
     * @param key storage key
     */
    get<T>(key: string): T;
    get(key: string): any {
        return JSON.parse(localStorage.getItem(this.keyGenerator.uniqueKey(key, this.options.scope)));
    }

    /**
     * removes value by key from storage
     * @param key storage key
     * @returns the value, def if key not found
     */
    getOrDefault<T>(key: string, def: T): T {
        return { ...def as any, ...(this.get(key) || this.set(key, def)) };
    }

    /**
     * gets value by key, apply map function, save and returns the resulting value
     * @param key storage key
     * @param def default value to return if key not found
     * @param map function to apply to value
     * @returns the resulting value
     */
    using<T>(key: string, def: T, map: (s: T) => T): T {
        return this.set(key, map(this.getOrDefault<T>(key, def)));
    }

    /**
     * removes value by key from localstorage
     * @param key storage key
     */
    remove(key: string): void {
        let ukey = this.keyGenerator.uniqueKey(key, this.options.scope);
        localStorage.removeItem(ukey);
    }

    /**
     * modify or add value by key in localstorage. logs an error in case of size limit reached or generic
     * @param key storage key
     * @returns the saved value. null in case of error
     */
    set<T>(key: string, value: T): T;
    set(key: string, value: any): any {
        let ukey = this.keyGenerator.uniqueKey(key, this.options.scope);
        try {
            localStorage.setItem(ukey, JSON.stringify(value));
            return value;
        } catch (e) {
            if (e.name === 'QUOTA_EXCEEDED_ERR' || e.name === 'NS_ERROR_DOM_QUOTA_REACHED' ||
                e.name === 'QuotaExceededError' || e.name === 'W3CException_DOM_QUOTA_EXCEEDED_ERR') {
                this.log.error('LocalStorage is full. Error saving: ' + ukey);
            } else {
                this.log.error(`LocalStorage error saving : ${ukey} - ${e.message}`);
            }
            return null;
        }
    }
}

