import { OperationResult } from './../../models/operation-result.model';
import { Subscription } from 'rxjs/Subscription';
import { Subject } from 'rxjs/Subject';
import { TaskbuilderService } from './../../../core/services/taskbuilder.service';
import { HttpMenuService } from './../../../menu/services/http-menu.service';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
    selector: 'tb-application-date',
    templateUrl: './application-date.component.html',
    styleUrls: ['./application-date.component.scss']
})
export class ApplicationDateComponent implements OnInit, OnDestroy {
    applicationDate: Date = undefined;
    culture: string = '';
    dateFormat: string = '';
    internalDate: Date = undefined;
    errorMessage: string = "";

    subscriptions: Subscription[] = [];

    public opened: boolean = false;

    constructor(public httpMenuService: HttpMenuService, public taskbuilderService: TaskbuilderService) {
    }

    ngOnInit() {
        this.subscriptions.push(this.taskbuilderService.tbConnection.subscribe((connected) => {
            if (connected)
                this.getDate();
        }));
        //this.localizationService.localizedElements
    }

    ngOnDestroy() {
        this.subscriptions.forEach((sub) => sub.unsubscribe());
    }
    getDate() {
        this.httpMenuService.getApplicationDate().subscribe((res) => {
            console.log("dateInfo", res.dateInfo);
            if (!res.dateInfo)
                return;

            let d = res.dateInfo.applicationDate.split("T");
            let parts = d[0].split("-");
            this.applicationDate = this.internalDate = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
            this.culture = res.dateInfo.culture;
            this.dateFormat = res.dateInfo.dateFormat;
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
                    this.errorMessage;
                }
            });


    }

    public cancel() {
        this.errorMessage = "";
        this.opened = false;
    }

}



