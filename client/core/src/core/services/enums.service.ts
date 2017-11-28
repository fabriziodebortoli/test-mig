import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { HttpService } from './http.service';

@Injectable()
export class EnumsService {

    public enumsTable: any;
    constructor(public httpService: HttpService) { }

    getEnumsTable() {
        this.httpService.getEnumsTable().subscribe((json) => {
            this.enumsTable = json.enums;
        });
    }

    async getEnumsTableAsync() {
        if (!this.enumsTable) {
            let result = await this.httpService.getEnumsTable().toPromise();
            this.enumsTable = result.enums;
        }
    }

    getEnumsItem(storedValue: number) {
        if (this.enumsTable === undefined) { return; }
        for (let index = 0; index < this.enumsTable.tags.length; index++) {
            let tag = this.enumsTable.tags[index];
            if (tag !== undefined) {
                for (let j = 0; j < tag.items.length; j++) {
                    if (tag.items[j].stored == storedValue) {
                        return tag.items[j];
                    }
                }
            }
        }
        return undefined;
    }

    getItemFromTagAndValue(tag: string, storedValue: number) {
        if (this.enumsTable === undefined) { return; }
        for (let index = 0; index < this.enumsTable.tags.length; index++) {
            let currentTag = this.enumsTable.tags[index];
            if (currentTag !== undefined && currentTag.value === tag) {

                for (let j = 0; j < currentTag.items.length; j++) {
                    if (currentTag.items[j].stored == storedValue) {
                        return currentTag.items[j];
                    }
                }
            }
        }
        return undefined;
    }

    getItemsFromTag(tag: string) {
        if (this.enumsTable === undefined) { return; }
        for (let index = 0; index < this.enumsTable.tags.length; index++) {
            let currentTag = this.enumsTable.tags[index];
            if (currentTag !== undefined && currentTag.value === tag) {
                return currentTag.items;
            }
        }
        return undefined;
    }
}
