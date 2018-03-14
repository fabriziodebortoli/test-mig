import { State } from '../customisable-grid.component';
import { EnumsService } from '../../../../core/services/enums.service';

function getEnumTagFromValue(value: any): string {
    return JSON.stringify(Math.floor(value / 65536));
}

function getFirstRowEnumColumnValue(column: any, state: State) : number {
    let firstRow = state.gridData.data[0];
    return (!column || !firstRow) ? null : Number.parseInt(JSON.stringify(firstRow[column.id]));
}

export function getEnumValueSiblings(column: any, state: State, enumsService: EnumsService): { displayString: string, code: string }[] {
    let valueSiblings = enumsService.getItemsFromTag(getEnumTagFromValue(getFirstRowEnumColumnValue(column, state)));
    return !valueSiblings ? null : valueSiblings.map(i => ({displayString: i.name, code: JSON.stringify(i.stored)}));
}

export function getEnumValuesFromTag(tag: string, enumsService: EnumsService): { displayString: string, code: string }[] {
    let valueSiblings = enumsService.getItemsFromTag(tag);
    return !valueSiblings ? null : valueSiblings.map(i => ({displayString: i.name, code: JSON.stringify(i.stored)}));
}