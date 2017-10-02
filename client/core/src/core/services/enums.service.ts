import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { HttpService } from './http.service';

@Injectable()
export class EnumsService {

    public enumsTable: any;
    public getEnumsTableSubscription;

    constructor(public httpService: HttpService) { }

    getEnumsTable() {
        let subs = this.getEnumsTableSubscription = this.httpService.getEnumsTable().subscribe((json) => {
            this.enumsTable = json.enums;
            subs.unsubscribe();
        });
    }

    getEnumsItem(storedValue: string) {

        if (this.enumsTable == undefined)
            return;
        for (let index = 0; index < this.enumsTable.tags.length; index++) {

            let tag = this.enumsTable.tags[index];
            if (tag != undefined) {
                for (let j = 0; j < tag.items.length; j++) {
                    if (tag.items[j].stored == storedValue)
                        return tag.items[j];
                }
            }
        }
        return undefined;

    }

    getItemFromTagAndValue(tag: string, storedValue: string) {

        if (this.enumsTable == undefined)
            return;

        for (let index = 0; index < this.enumsTable.tags.length; index++) {

            let currentTag = this.enumsTable.tags[index];
            if (currentTag != undefined && currentTag.value == tag) {

                for (let j = 0; j < currentTag.items.length; j++) {
                    if (currentTag.items[j].stored == storedValue)
                        return currentTag.items[j];
                }

            }
        }
        return undefined;
    }


    getItemsFromTag(tag: string) {

        if (this.enumsTable == undefined)
            return;
        for (let index = 0; index < this.enumsTable.tags.length; index++) {

            let currentTag = this.enumsTable.tags[index];
            if (currentTag != undefined && currentTag.value == tag) {
                return currentTag.items;
            }
        }
        return undefined;
    }


    dispose() {
        this.getEnumsTableSubscription.unsubscribe();
    }
}