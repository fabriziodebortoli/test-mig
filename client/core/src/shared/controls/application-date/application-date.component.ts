import { Component, OnInit, OnDestroy, Pipe, ViewEncapsulation } from '@angular/core';
import { formatDate } from '@telerik/kendo-intl';

import { Subscription, Subject } from '../../../rxjs.imports';

import { OperationResult } from './../../models/operation-result.model';

import { LocalizationService } from './../../../core/services/localization.service';
import { InfoService } from './../../../core/services/info.service';
import { TaskbuilderService } from './../../../core/services/taskbuilder.service';
import { HttpMenuService } from './../../../menu/services/http-menu.service';

@Component({
    selector: 'tb-application-date',
    templateUrl: './application-date.component.html',
    styleUrls: ['./application-date.component.scss'],
    encapsulation: ViewEncapsulation.None
})
export class ApplicationDateComponent implements OnInit, OnDestroy {
    applicationDate: Date = undefined;
    culture: string = '';
    dateFormat: string = '';
    internalDate: Date = undefined;
    errorMessage: string = "";
    isDesktop: boolean;

    subscriptions: Subscription[] = [];

    public opened: boolean = false;

    constructor(
        public infoService: InfoService,
        public httpMenuService: HttpMenuService,
        public taskbuilderService: TaskbuilderService,
        public localizationService: LocalizationService) {
    }

    ngOnInit() {
        this.subscriptions.push(this.taskbuilderService.tbConnection.subscribe((connected) => {
            if (connected)
                this.getDate();
        }));
        //this.localizationService.localizedElements
        this.isDesktop = this.infoService.isDesktop;
    }

    ngOnDestroy() {
        this.subscriptions.forEach((sub) => sub.unsubscribe());
    }
    getDate() {
        let subs = this.httpMenuService.getApplicationDate().subscribe((res) => {
            if (!res.dateInfo)
                return;

            let d = res.dateInfo.applicationDate.split("T");
            let parts = d[0].split("-");
            this.applicationDate = this.internalDate = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
            this.culture = res.dateInfo.culture;
            this.dateFormat = res.dateInfo.formatDate;

            subs.unsubscribe();
        })
    }

    public handleChange(value: Date) {
        this.internalDate = value;
    }

    public open() {
        this.opened = true;
    }


    public ok() {
        this.errorMessage = "";
        this.httpMenuService.changeApplicationDate(this.internalDate)
            .subscribe((tbRes: OperationResult) => {
                if (!tbRes.error) {
                    this.applicationDate = this.internalDate;
                    this.opened = false;
                }
                else {
                    tbRes.messages.forEach((current) => {
                        this.errorMessage += current.text;
                    })
                }
            });


    }

    public cancel() {
        this.errorMessage = "";
        this.opened = false;
    }

}



