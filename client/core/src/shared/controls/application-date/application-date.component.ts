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
    
    subscriptions: Subscription[] = [];

    public opened: boolean = false;

    constructor(private httpMenuService: HttpMenuService, private taskbuilderService: TaskbuilderService) {
    }

    ngOnInit() {
        this.subscriptions.push(this.taskbuilderService.connected.subscribe(() => {
            this.getDate();
        }));
        //this.localizationService.localizedElements
    }

    ngOnDestroy() {
        this.subscriptions.forEach((sub) => sub.unsubscribe());
    }
    getDate() {
        this.httpMenuService.getApplicationDate().subscribe((res) => {
            if (!res.dateInfo)
                return;

            let d = res.dateInfo.applicationDate.split("T");
            let parts = d[0].split("-");
            this.applicationDate = new Date(Number(parts[0]), Number(parts[1]) - 1, Number(parts[2]));
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
        this.httpMenuService.changeApplicationDate(this.internalDate).subscribe((res) => {
            console.log("internal date", this.internalDate);
            this.applicationDate = this.internalDate;
            this.opened = false;
        });
    }

    public cancel() {
        this.opened = false;
    }

}



