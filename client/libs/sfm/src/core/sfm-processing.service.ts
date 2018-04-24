import { Injectable } from '@angular/core';
import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { URLSearchParams, Http } from '@angular/http';
import {Observable} from 'rxjs/Observable';
import { CoreService } from './../core/sfm-core.service';

export enum filterType {
    mo_routing_step = 0 ,
    mo = 1,
    work_center = 2,
    operation = 3,
    sale_order = 4,
    job = 5,
    customer = 6
}

@Injectable()
export class ProcessingsService {

    completeProcessingsList: any[] = [];
    processingsList: any[] = [];

    constructor(private dataService: DataService,
                private coreService: CoreService) {}
    
    getProcessings(worker: number, filter: filterType): Observable<any> {
        
        let p = new URLSearchParams();
        p.set('filter', worker.toString());

        return this.dataService.getData('SFM.SFMProcessingPlanner.Dbl.ProcessingsAssignmentQuery', 'direct', p).map((res: any) => {
            this.completeProcessingsList.push(...res.rows);
            this.FilterData(filter);
            return this.processingsList;
        });
    }
        
    FilterData(type: filterType) {
        switch (type)
        {
            case filterType.customer:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MO_Customer'), []);
                break;
            case filterType.job:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MO_Job'), []);
                break;
            case filterType.sale_order:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MO_InternalOrdNo'), []);
                break;
            case filterType.operation:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MOSteps_Operation'), []);
                break;
            case filterType.work_center:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MOSteps_WC'), []);
                break;
            case filterType.mo:
                this.processingsList = this.completeProcessingsList.reduce(this.Reducer('MA_MO_MONo'), []);
                break;
            default:                
                this.processingsList = this.completeProcessingsList.map(x => Object.assign({}, x));            
        }
    }

    Reducer = field => (newArray: any[], object: any) => {
    
        let elem = newArray.find(x => x[field] === object[field]);
    
        if (elem != undefined)
          elem.val += object.val;
        else
          newArray.push(object);
    
        return newArray;
    };
}
