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

    async getWorker() {
        if (this.worker === undefined)
            return await this.GetWorker().take(1).toPromise();
        else
            return this.worker;
    }
    
    private GetWorker(): Observable<any> {
        let p = new URLSearchParams();
        p.set('filter', localStorage.getItem('_user'));

        return this.dataService.getData('SFM.SFMProcessingsAssignment.Dbl.WorkersQuery', 'direct', p).map((res: any) => {
            this.worker = res.rows[0];
            this.setWorkerName();
            this.workerImage = this.worker.RM_Workers_ImagePath;
            return this.worker;
        });
    }

    async getManufacturingParameters() {
        if (this.manufacturingParameters === undefined)
            return await this.GetManufacturingParameters().take(1).toPromise();
        else
            return this.manufacturingParameters;
    }

    GetManufacturingParameters(): Observable<any> {
        let p = new URLSearchParams();
        p.set('filter', '0');

        return this.dataService.getData('SFM.SFMProcessingsAssignment.Dbl.ManufacturingParametersQuery', 'direct', p).map((res: any) => {
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
