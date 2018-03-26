import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';

import { SFMService } from './../sfm.service';

@Component({
    selector: 'tb-sfm-page',
    templateUrl: './sfm-page.component.html',
    styleUrls: ['./sfm-page.component.scss'],
    providers: [SFMService]
})
export class SFMPageComponent extends DocumentComponent implements OnInit {

    @Input() prova: any[] = [];

    WorkerID : number = 1;

    constructor(
        public sfmService: SFMService,
        eventData: EventDataService,
        private dataService: DataService,
        changeDetectorRef: ChangeDetectorRef) {
        super(sfmService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "SFM" } };

        this.FillData();
    }

    async FillData() {
        let p = new URLSearchParams();
        p.set('filter', this.WorkerID.toString());
        let data = await this.dataService.getData('SFM.SFMLabourPlanner.Dbl.LabourAssignmentQuery', 'direct', p).take(1).toPromise();
        if (data !== undefined)
            this.prova.push(...data.rows);
    }

}

@Component({
    template: ''
})
export class SFMPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(SFMPageComponent, resolver, { name: 'sfm' });
    }
} 