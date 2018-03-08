import { BaseFilterCellComponent, FilterService } from '@progress/kendo-angular-grid';
import { CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { Component, Input } from '@angular/core';
import { State } from '../customisable-grid.component';
import { getEnumValueSiblings } from './U';
import { EnumsService } from '../../../../core/services/enums.service';
import { FilterService as CustomFilterService } from '../../../../core/services/filter.services'

@Component({
    selector: 'tb-enum-filter',
    templateUrl: './enum-filter.component.html',
    styleUrls: ['./enum-filter.component.scss']})
export class EnumFilterComponent extends BaseFilterCellComponent {
    @Input() filter: CompositeFilterDescriptor;
    @Input() state: State; 
    @Input() column : any;
    
    get selectedValue(): any {
        return this.customFilterService.filterBag.get(JSON.stringify(this.column.id))
    }
    textField: string = 'displayString';
    valueField: string = 'code';
    
    public get data(): any {
        let lastData = this.customFilterService.filterBag.get(JSON.stringify(this.column.id)+ '_data');
        if(lastData) return lastData;
        lastData = getEnumValueSiblings(this.column, this.state, this.enumsService);
        this.customFilterService.filterBag.set(JSON.stringify(this.column.id)+ '_data', lastData);
    }

    public get defaultItem(): any {
        return {
            [this.textField]: "*",
            [this.valueField]: null
        };
    }
    
    constructor(filterService: FilterService, 
        private enumsService: EnumsService,
        private customFilterService: CustomFilterService) {
        super(filterService);
    }

    public onChange(value: any): void {
        this.customFilterService.filterBag.set(JSON.stringify(this.column.id), value);
        this.applyFilter(value === null ? 
                    this.removeFilter(this.column.id) : 
                    this.updateFilter({ field: this.column.id, operator: "contains", value: value}));
    }
}