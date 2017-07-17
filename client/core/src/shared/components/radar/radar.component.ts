import { Component } from '@angular/core';

import { Logger } from './../../../core/services/logger.service';
import { DataService } from './../../../core/services/data.service';

import { URLSearchParams } from '@angular/http';

@Component({
    selector: 'tb-radar',
    templateUrl: './radar.component.html',
    styleUrls: ['./radar.component.scss']
})
export class RadarComponent {

    radarData: any;

    constructor(private dataService: DataService, private logger: Logger) { }

    getData(query: string) {

        let params: URLSearchParams = new URLSearchParams();
        params.set('query', query);

        this.logger.debug('radar', params);
        this.dataService.getRadarData(params).subscribe((data) => {
            this.radarData = data;
        });
    }

}
