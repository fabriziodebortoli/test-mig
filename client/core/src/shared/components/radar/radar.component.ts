import { EnumsService } from './../../../core/services/enums.service';
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
    public pageSize: number = 7;
    public skip: number = 0;
    public sort: SortDescriptor[] = [];
    public gridView: GridDataResult;
    public radarData: any[];
    public columnInfos: any[] = [];

    constructor(
        public dataService: DataService,
        public logger: Logger,
        public eventData: EventDataService,
        public enumsService: EnumsService) {

    }

    toggle() {
        this.state = this.state === 'opened' ? 'closed' : 'opened';
    }

    init(radarInfo) {
        this.toggle(); if (this.state === 'closed') return;
        /*dataType*/

        this.columnInfos = radarInfo.columnInfos
        // this.columnInfos = radarInfo.columnInfos.slice(0, 10);

        let params: URLSearchParams = new URLSearchParams();
        params.set('query', radarInfo.query);
        params.set('columnInfos', JSON.stringify(this.columnInfos));

        this.logger.info('radar', params);
        this.dataService.getRadarData(params).subscribe((data) => {
            this.radarData = data.rows;
            this.load();
        });
    }

    public editHandler({ sender, rowIndex, dataItem }) {
        this.logger.info('dataItem', dataItem);
    }

    public pageChange(event: PageChangeEvent): void {
        this.skip = event.skip;
        this.load();
    }

    public sortChange(sort: SortDescriptor[]): void {
        this.sort = sort;
        this.load();
    }

    public selectionChange(event: SelectionEvent) {
        if (event.selected) {
            console.log('rowSelected:', this.radarData[event.index]);
            this.onRadarRecordSelected(this.radarData[event.index]);
        }
    }

    public edit(event: SelectionEvent) {
        this.logger.debug('editClick')
        if (event.selected) {
            console.log('rowDoubleClick:', this.radarData[event.index]);
            this.onRadarRecordSelected(this.radarData[event.index]);
        }
    }

    public load(): void {
        this.gridView = {
            data: orderBy(this.radarData.slice(this.skip, this.skip + this.pageSize), this.sort),
            total: this.radarData.length
        };
    }

    onRadarRecordSelected(row: any) {
        let tbGuid = row["TBGuid"];
        this.eventData.radarRecordSelected.emit(tbGuid);
    }

    //TODOLUCA....caching...non chiamare tutte le volte la tabella di enumerativi
    getEnumValue(dataItem: any, column: any): string {
        let value = dataItem[column.columnName];
        let tag = column.enumTag;
        return this.enumsService.getItemFromTagAndValue(tag, value).name;
    }
}
