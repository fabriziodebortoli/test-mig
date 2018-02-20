import { Injectable } from '@angular/core';

@Injectable()
export class StorageService {
    get<T>(key: string): T;
    get<T>(key: string): any {
        return JSON.parse(localStorage.getItem(key)) as T;
    }
    remove(key: string): void {
        localStorage.removeItem(key);
    }
    set(key: string, data: any): void {
        localStorage.setItem(key, JSON.stringify(data));
    }
}
