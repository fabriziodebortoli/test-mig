import { Injectable } from '@angular/core';
import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { URLSearchParams, Http } from '@angular/http';
import {Observable} from 'rxjs/Observable';

@Injectable()
export class CoreService {

    worker: any;
    workerName: string;
    workerImage: string;

    manufacturingParameters: any;

    constructor(private dataService: DataService) {
    }
    
    getWorker(): Observable<any> {
        let p = new URLSearchParams();
        p.set('filter', localStorage.getItem('_user'));

        return this.dataService.getData('SFM.SFMProcessingPlanner.Dbl.WorkersQuery', 'direct', p).map((res: any) => {
            this.worker = res.rows[0];
            this.setWorkerName();
            this.workerImage = this.worker.RM_Workers_ImagePath;
            return this.worker;
        });
    }

    getManufacturingParameters(): Observable<any> {
        let p = new URLSearchParams();
        p.set('filter', '0');

        return this.dataService.getData('SFM.SFMProcessingPlanner.Dbl.ManufacturingParametersQuery', 'direct', p).map((res: any) => {
            this.manufacturingParameters = res.rows[0];
            return this.manufacturingParameters;
        });
    }

    private setWorkerName()
    {
        if (this.worker.RM_Workers_Name != '')
        {
            if (this.worker.RM_Workers_LastName != '')
                this.workerName = this.worker.RM_Workers_Name + ' ' + this.worker.RM_Workers_LastName;
            else
                this.workerName = this.worker.RM_Workers_Name;
        }
        else
        {
            if (this.worker.RM_Workers_LastName != '')
                this.workerName = this.worker.RM_Workers_LastName;
        }
   }
}
