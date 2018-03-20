import { BaseFilterCellComponent, FilterService } from '@progress/kendo-angular-grid';
import { CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Component, Input } from '@angular/core';
import { State } from '../customisable-grid.component';
import { getEnumValuesFromTag } from './U';
import { EnumsService } from '../../../../core/services/enums.service';
import { FilterService as CustomFilterService } from '../../../../core/services/filter.service'

@Component({
    selector: 'tb-enum-filter',
    templateUrl: './enum-filter.component.html',
    styleUrls: ['./enum-filter.component.scss']})
export class EnumFilterComponent extends BaseFilterCellComponent {
    textField: string = 'displayString';
    valueField: string = 'code';
    @Input() filter: CompositeFilterDescriptor;
    @Input() column : any;
    @Input() state: State; 
    private get filterId(): string { return JSON.stringify(this.column.id); }
    
    get isFilterActive(): boolean {
        return this.customFilterService.filterBag.get(this.filterId + '_isFilterActive') === true;
    }

    set isFilterActive(value: boolean) {
        this.customFilterService.filterBag.set(this.filterId + '_isFilterActive', value);
    }
    
    get selectedValue(): any {
        return this.customFilterService.filterBag.get(this.filterId);
    }

    public get clearFilterStyle(): any {
        return { visibility: this.isFilterActive ? 'visible' : 'hidden', };
    }
    
    public get data(): any {
        let lastData = this.customFilterService.filterBag.get(this.filterId + '_data');
        if(lastData) return lastData;
        lastData = getEnumValuesFromTag(this.column.enumTag, this.enumsService);
        this.customFilterService.filterBag.set(this.filterId + '_data', lastData);
    }

    public get defaultItem(): any {
        return {
            [this.textField]: "(All)",
            [this.valueField]: null
        };
    }
    
    constructor(filterService: FilterService, 
        private enumsService: EnumsService,
        private customFilterService: CustomFilterService) {
        super(filterService);
    }

    clearFilter(columnId: any) {
        this.applyFilter(this.removeFilter(this.filterId));
        this.isFilterActive = false;
    }

    doFilter(columnId: any, value: any) {
        this.applyFilter(this.updateFilter({ field: columnId, operator: "contains", value: value}));
        this.isFilterActive = true;
    }

    public onChange(value: any): void {
        this.customFilterService.filterBag.set(this.filterId, value);
        value === null ? this.clearFilter(this.filterId) : this.doFilter(this.filterId, value);
    }
}