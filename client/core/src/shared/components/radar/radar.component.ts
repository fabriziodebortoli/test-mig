import { EventDataService } from './../../../core/services/eventdata.service';
import { Component, ViewEncapsulation } from '@angular/core';

import { GridDataResult, PageChangeEvent, SelectionEvent } from '@progress/kendo-angular-grid';
import { SortDescriptor, orderBy } from '@progress/kendo-data-query';

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
    private sort: SortDescriptor[] = [];
    private gridView: GridDataResult;
    public radarData: any[];
    public radarColumns: string[] = [];

    constructor(private dataService: DataService, private logger: Logger, private eventData: EventDataService) { }

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
            this.load();
        });
    }

    protected pageChange(event: PageChangeEvent): void {
        this.skip = event.skip;
        this.load();
    }

    protected sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        this.load();
    }

    protected selectionChange(event: SelectionEvent) {
        if (event.selected) {
            console.log('rowSelected:', this.radarData[event.index]);
            this.onRadarRecordSelected(this.radarData[event.index]);
        }
    }

    protected onRowDoubleClick(event: SelectionEvent) {
        if (event.selected) {
            console.log('rowDoubleClick:', this.radarData[event.index]);
            this.onRadarRecordSelected(this.radarData[event.index]);
        }
    }

    private load(): void {
        this.gridView = {
            data: orderBy(this.radarData.slice(this.skip, this.skip + this.pageSize), this.sort),
            total: this.radarData.length
        };
    }

    onRadarRecordSelected(row: any) {
        let tbGuid = row["TBGuid"];
        this.eventData.radarRecordSelected.emit(tbGuid);
    }
}
