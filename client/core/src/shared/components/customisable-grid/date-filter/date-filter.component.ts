import { BaseFilterCellComponent, FilterService, DateFilterCellComponent  } from '@progress/kendo-angular-grid';
import { CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Component, Input, ViewChild, AfterContentInit } from '@angular/core';
import { State } from '../customisable-grid.component';
import { EnumsService } from '../../../../core/services/enums.service';
import { FilterService as CustomFilterService, SimpleFilter } from '../../../../core/services/filter.service';
import { FormattersService } from '../../../../core/services/formatters.service';
import { getDateFormatByFormatter, NullDate } from '../../../controls/date-input/u';

@Component({
    selector: 'tb-date-filter',
    templateUrl: './date-filter.component.html',
    styleUrls: ['./date-filter.component.scss']})
export class DateFilterComponent extends BaseFilterCellComponent implements AfterContentInit {
    @Input() column : any;
    @Input() kendoColumn: any;
    @Input() filter: CompositeFilterDescriptor ;
    @Input() get format(): any { return getDateFormatByFormatter(this.formatterService.getFormatter('Date')); }
    private get filterId(): string { return JSON.stringify(this.column.id); }

    constructor(filterService: FilterService, 
        private enumsService: EnumsService,
        private customFilterService: CustomFilterService,
        private formatterService: FormattersService) {
        super(filterService);
    }

    public get defaultItem(): any { return NullDate; }
}