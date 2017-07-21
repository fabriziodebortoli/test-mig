import { Component, ViewEncapsulation } from '@angular/core';

import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';

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
    ],
    encapsulation: ViewEncapsulation.None
})
export class RadarComponent {

    public state: string = 'closed';
    private pageSize: number = 7;
    private skip: number = 0;

    private gridView: GridDataResult;
    public radarData: any[];
    public radarColumns: string[] = [];

    constructor(private dataService: DataService, private logger: Logger) { }

    toggle() {
        this.state = this.state === 'opened' ? 'closed' : 'opened';
    }

    init(radarInfo) {
        this.toggle(); if (this.state === 'closed') return;

        this.radarColumns = radarInfo.columnInfos.map(c => c.columnName);

        let params: URLSearchParams = new URLSearchParams();
        params.set('query', radarInfo.query);
        params.set('columnInfos', JSON.stringify(radarInfo.columnInfos));

        this.logger.info('radar', params);
        this.dataService.getRadarData(params).subscribe((data) => {
            console.log(data)

            this.radarData = data.rows;
            this.loadItems();
        });
    }

    protected pageChange(event: PageChangeEvent): void {
        this.skip = event.skip;
        this.loadItems();
    }

    private loadItems(): void {
        this.gridView = {
            data: this.radarData.slice(this.skip, this.skip + this.pageSize),
            total: this.radarData.length
        };
    }

}
