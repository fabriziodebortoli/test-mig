import { Injectable } from '@angular/core';
import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { URLSearchParams, Http } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { CoreService } from './../core/sfm-core.service';

@Injectable()
export class MessagesService {

    messagesList: any[] = [];

    constructor(private dataService: DataService,
                private coreService: CoreService) { }

    getMessages(worker: number): Observable<any> {

        let p = new URLSearchParams();
        p.set('filter', worker.toString());

        return this.dataService.getData('SFM.SFMProcessingPlanner.Dbl.WorkerMessagesQuery', 'direct', p).map((res: any) => {
            this.messagesList.push(...res.rows);
            return this.messagesList;
        });
    }
}
