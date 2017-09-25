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
    value: string;
    constructor(private httpMenuService: HttpMenuService, private taskbuilderService: TaskbuilderService) {
    }

    ngOnInit() {

        // this.taskbuilderService.connected.subscribe(() => {
        //     this.httpMenuService.getApplicationDate().subscribe((res) => {

        //         //this.value = res;
        //         console.log(res);
        //     })
        // }
        // )

    }

}



