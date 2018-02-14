import { Injectable } from '@angular/core';

@Injectable()
export class StorageService {
    get(key: string): string {
        return localStorage.getItem(key);
    }
    remove(key: string): void {
        localStorage.removeItem(key);
    }
    set(key: string, data: string): void {
        localStorage.setItem(key, data);
    }
}
