import { Pipe, PipeTransform, NgZone } from '@angular/core';

@Pipe({name: 'asDropDownData'})
export class TbAsDropDownDataPipe implements PipeTransform {

  constructor(private ngZone: NgZone) {
  }

transform(value: {key?: string, data: any[], total: number, columns: any[]}): {id: any, displayString: string}[] {
        if (value && value.data && value.data.length > 0) {
            let result: {id: any, displayString: string}[] = [];
            result = value.data.map(x => {
                let idKey = Object.keys(x)[0];
                if (value.key) idKey = value.key;
                let descriptionKey = 'Description';
                if (!x[descriptionKey]) { descriptionKey = Object.keys(x)[1]; }
                return {id: x[idKey], displayString: x[descriptionKey]};
            });
            return result;
        }
        return null;
    }
}
