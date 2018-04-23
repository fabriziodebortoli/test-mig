import { Injectable } from '@angular/core';
import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { URLSearchParams, Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class MessagesService {

    messagesList: any[] = [];

    WorkerID: number = 47;

    constructor(private dataService: DataService) { }

    getMessages(): Observable<any> {

        let p = new URLSearchParams();
        p.set('filter', this.WorkerID.toString());

        return this.dataService.getData('SFM.SFMProcessingPlanner.Dbl.WorkerMessagesQuery', 'direct', p).map((res: any) => {
            this.messagesList.push(...res.rows);
            return this.messagesList;
        });
    }
}
