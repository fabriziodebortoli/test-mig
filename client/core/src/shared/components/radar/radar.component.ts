import { Component } from '@angular/core';

import { Logger } from './../../../core/services/logger.service';
import { DataService } from './../../../core/services/data.service';

import { URLSearchParams } from '@angular/http';
import { NgForm } from "@angular/forms";
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

export const sampleProducts = [
    { "ProductID": 1, "ProductName": "Chai", "QuantityPerUnit": "10 boxes x 20 bags", },
    { "ProductID": 2, "ProductName": "Chang", "QuantityPerUnit": "24 - 12 oz bottles", },
    { "ProductID": 3, "ProductName": "Aniseed Syrup", "QuantityPerUnit": "12 - 550 ml bottles", },
    { "ProductID": 1, "ProductName": "Chai", "QuantityPerUnit": "10 boxes x 20 bags", },
    { "ProductID": 2, "ProductName": "Chang", "QuantityPerUnit": "24 - 12 oz bottles", },
    { "ProductID": 3, "ProductName": "Aniseed Syrup", "QuantityPerUnit": "12 - 550 ml bottles", },
    { "ProductID": 1, "ProductName": "Chai", "QuantityPerUnit": "10 boxes x 20 bags", },
    { "ProductID": 2, "ProductName": "Chang", "QuantityPerUnit": "24 - 12 oz bottles", },
    { "ProductID": 3, "ProductName": "Aniseed Syrup", "QuantityPerUnit": "12 - 550 ml bottles", },
    { "ProductID": 1, "ProductName": "Chai", "QuantityPerUnit": "10 boxes x 20 bags", },
    { "ProductID": 2, "ProductName": "Chang", "QuantityPerUnit": "24 - 12 oz bottles", },
    { "ProductID": 3, "ProductName": "Aniseed Syrup", "QuantityPerUnit": "12 - 550 ml bottles", }
];

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

    public state: string = 'closed';

    public radarData: any[] = sampleProducts;

    public radarColumns: string[] = [
        "ProductID", "ProductName", "QuantityPerUnit"
    ];

    constructor(private dataService: DataService, private logger: Logger) { }

    toggle() {
        this.state = this.state === 'opened' ? 'closed' : 'opened';
    }

    init(radarInfo) {
        this.logger.info('radarInfo', radarInfo);

        this.toggle(); if (this.state === 'closed') return;

        let params: URLSearchParams = new URLSearchParams();
        params.set('query', radarInfo.query);
        params.set('columnInfos', radarInfo.columnInfos);

        this.logger.debug('radar', params);
        this.dataService.getRadarData(params).subscribe((data) => {
            console.log(data)

            this.radarData = data;
        });
    }

}
