import { Component, OnInit, OnDestroy, Pipe, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { formatDate } from '@telerik/kendo-intl';

import { TbComponent, InfoService, TaskBuilderService, DiagnosticService, TbComponentService, OperationResult } from '@taskbuilder/core';

import { HttpMenuService } from './../../services/http-menu.service';

@Component({
    selector: 'tb-application-date',
    templateUrl: './application-date.component.html',
    styleUrls: ['./application-date.component.scss']
})
export class ApplicationDateComponent extends TbComponent implements OnInit, OnDestroy {
    applicationDate: Date = undefined;
    culture: string = '';
    dateFormat: string = '';
    internalDate: Date = undefined;
    isDesktop: boolean;

    subscriptions: Subscription[] = [];

    public opened: boolean = false;

    constructor(
        public infoService: InfoService,
        public httpMenuService: HttpMenuService,
        public taskbuilderService: TaskBuilderService,
        public diagnosticService: DiagnosticService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);

        this.enableLocalization();
    }

    ngOnInit() {
        super.ngOnInit();
        this.subscriptions.push(this.taskbuilderService.tbConnection.subscribe((connected) => {
            if (connected)
                this.getDate();
        }));
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
        
        this.httpMenuService.changeApplicationDate(this.internalDate)
            .subscribe((tbRes: OperationResult) => {
                if (!tbRes.error) {
                    this.applicationDate = this.internalDate;
                    this.opened = false;
                    this.infoService.setApplicationDate(this.internalDate);
                }
                this.diagnosticService.showDiagnostic(tbRes.messages);
            });


    }

    public cancel() {
        this.opened = false;
    }

}



