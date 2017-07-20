import { Component } from '@angular/core';

import { Logger } from './../../../core/services/logger.service';
import { DataService } from './../../../core/services/data.service';

import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

@Component({
    selector: 'tb-radar',
    templateUrl: './radar.component.html',
    styleUrls: ['./radar.component.scss'],
    animations: [
        trigger('shrinkOut', [
            state('opened', style({ height: '*' })),
            state('closed', style({ height: 0, overflow: 'hidden' })),
            transition('opened <=> closed', animate('250ms ease-in-out')),
        ])
    ]
})
export class RadarComponent {

    public radarData: any[] = [];
    private state: string = 'closed';
    private query: string;

    public columns: string[] = [
        "CompanyName", "ContactName", "ContactTitle"
    ];

    constructor(private dataService: DataService, private logger: Logger) {
        this.query = 'select * from belincheneso';
        //this.radarData = [{ "Id": "ALFKI", "CompanyName": "Alfreds Futterkiste", "ContactName": "Maria Anders", "ContactTitle": "Sales Representative", "City": "Berlin" }, { "Id": "ANATR", "CompanyName": "Ana Trujillo Emparedados y helados", "ContactName": "Ana Trujillo", "ContactTitle": "Owner", "City": "México D.F." }];
    }

    toggle() {
        this.state = this.state === 'opened' ? 'closed' : 'opened';
    }

    onSubmit(form: NgForm) {
        console.log('submitted', form.value);
    }

    init(radarInfos) {

        let params: URLSearchParams = new URLSearchParams();
        params.set('query', this.query);

        this.logger.debug('radar', params);
        this.dataService.getRadarData(params).subscribe((data) => {
            this.radarData = data;
        });
    }

}
