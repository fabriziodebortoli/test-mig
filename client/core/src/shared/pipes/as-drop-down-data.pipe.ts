import { Pipe, PipeTransform, NgZone } from '@angular/core';

@Pipe({name: 'asDropDownData'})
export class TbAsDropDownDataPipe implements PipeTransform {

  constructor(private ngZone: NgZone) {
  }

transform(value: {selectionColumn?: string, gridData: {data: any[], total: number, columns: any[]}}): {id: any, displayString: string}[] {
        if (value && value.gridData && value.gridData.data && value.gridData.data.length > 0) {
            let result: {id: any, displayString: string}[] = [];
            result = value.gridData.data.map(x => {
                let idKey = Object.keys(x)[0];
                if (value.selectionColumn) idKey = value.selectionColumn;
                let descriptionKey = 'Description';
                if (!x[descriptionKey]) { descriptionKey = Object.keys(x)[1]; }
                return {id: x[idKey], displayString: x[descriptionKey]};
            });
            return result;
        }
        return null;
    }
}
