import { Component } from '@angular/core';

import { Logger } from './../../../core/services/logger.service';
import { DataService } from './../../../core/services/data.service';

@Component({
    selector: 'tb-radar',
    templateUrl: './radar.component.html',
    styleUrls: ['./radar.component.scss']
})
export class RadarComponent {

    radarData: any;

    metadata: any = {

    }

    constructor(private dataService: DataService, private logger: Logger) { }

    go() {
        this.logger.info("GO");
        this.getData();
    }

    getData() {
        this.logger.debug('radar', {
            metadata: this.metadata
        })
        this.dataService.getRadarData('test', {}).subscribe((data) => {
            this.radarData = data;
        });
    }

}
