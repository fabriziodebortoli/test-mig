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

    @Input() processingsList: any[] = [];
    @Input() messagesList: any[] = [];
    
    WorkerID : number = 47;

    constructor(
        public sfmService: SFMService,
        eventData: EventDataService,
        private dataService: DataService,
        changeDetectorRef: ChangeDetectorRef) {
        super(sfmService, eventData, null, changeDetectorRef);
    }

    async ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "Manufacturing Data Collection" } };

        await this.FillData();
        
        for (let elem of this.processingsList) {
            if (elem.MA_MOSteps_ProductionQty > 0)
                elem["SF_LabourAssignment_Percentage"] = elem.MA_MOSteps_ProducedQty * 100 / elem.MA_MOSteps_ProductionQty;
            else
                elem["SF_LabourAssignment_Percentage"] = 0;
        }

        for (let elem of this.messagesList) {

            if (elem.SF_LabourMessages_MessageType == '2044788736')
                elem["SF_LabourMessages_Header"] = "Hint";
            else if (elem.SF_LabourMessages_MessageType == '2044788737')
                elem["SF_LabourMessages_Header"] = "Warning";
            else if (elem.SF_LabourMessages_MessageType == '2044788738')
                elem["SF_LabourMessages_Header"] = "Error";

            if (elem.SF_LabourMessages_MessageDate != '1799-12-31')
                elem["SF_LabourMessages_Header"] += " " + elem.SF_LabourMessages_MessageDate;

            if (elem.SF_LabourMessages_WorkerID == '0')
                elem["SF_LabourMessages_To"] = "For all workers";
            else
                elem["SF_LabourMessages_To"] = "For worker " + this.WorkerID;
            
            if (elem.SF_LabourMessages_Expire == '1')
                if (elem.SF_LabourMessages_ExpirationDate == '1799-12-31')
                    elem["SF_LabourMessages_ExpireMessage"] = "Expire on exit";
                else
                    elem["SF_LabourMessages_ExpireMessage"] = "Expire on" + elem.SF_LabourMessages_ExpirationDate;
            else
                elem["SF_LabourMessages_ExpireMessage"] = "";
        }
    }

    getPercentage(elem: any) {
        let perc: number = 76;
        return perc;
     }	
      
    async FillData() {
        let p1 = new URLSearchParams();
        p1.set('filter', this.WorkerID.toString());
        let data1 = await this.dataService.getData('SFM.SFMLabourPlanner.Dbl.LabourAssignmentQuery', 'direct', p1).take(1).toPromise();
        if (data1 !== undefined)
            this.processingsList.push(...data1.rows);

        let p2 = new URLSearchParams();
        p2.set('filter', this.WorkerID.toString());
        let data2 = await this.dataService.getData('SFM.SFMLabourPlanner.Dbl.LabourMessagesQuery', 'direct', p2).take(1).toPromise();
        if (data2 !== undefined)
            this.messagesList.push(...data2.rows);            
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