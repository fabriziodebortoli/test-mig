import { Subject } from 'rxjs/Subject';
import { TaskbuilderService } from './../../../core/services/taskbuilder.service';
import { HttpMenuService } from './../../../menu/services/http-menu.service';
import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'tb-application-date',
    templateUrl: './application-date.component.html',
    styleUrls: ['./application-date.component.scss']
})
export class ApplicationDateComponent implements OnInit {
    applicationDate: Date = undefined;
    culture: string = '';
    dateFormat: string = '';
    internalDate: Date = undefined;

    constructor(private httpMenuService: HttpMenuService, private taskbuilderService: TaskbuilderService) {
    }

    ngOnInit() {

        this.taskbuilderService.connected.subscribe(() => {
            this.getDate();
        });

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

    onBlur() {
        this.httpMenuService.changeApplicationDate(this.internalDate).subscribe((res) => { });

        //$http.post( 'changeApplicationDate/?day=' + day + '&month=' + month + '&year=' + year)
        console.log(this.internalDate);
    }

    public handleChange(value: Date) {
        this.internalDate = value;
        // this.model.birthDate = this.intl.formatDate(value, 'yyyy-MM-dd'); //update the JSON birthDate string date
        // this.output = JSON.stringify(this.model);
    }

}



