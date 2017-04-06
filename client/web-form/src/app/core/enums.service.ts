import { HttpService } from './http.service';
import { URLSearchParams, Http, Response } from '@angular/http';
import { Injectable } from '@angular/core';

@Injectable()
export class EnumsService {

    private enumsTable: any;
    private getEnumsTableSubscription
    constructor(private httpService: HttpService) {

    }

    getEnumsTable() {
        this.getEnumsTableSubscription = this.httpService.getEnumsTable().subscribe((json) => {
            this.enumsTable = json.enums;
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
