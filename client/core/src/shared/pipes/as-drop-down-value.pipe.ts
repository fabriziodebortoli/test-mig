import { Pipe, PipeTransform, NgZone } from '@angular/core';
import * as _ from 'lodash';

@Pipe({name: 'asDropDownValue'})
export class TbAsDropDownValuePipe implements PipeTransform {

  constructor(private ngZone: NgZone) {
  }

transform(value: any, sourceIdField: string = 'value', destIdField: string = 'id', destDescrField: string = 'displayString'): any {
        let result = {};
        if (value && sourceIdField && destIdField && _.get(value, sourceIdField)) {
            _.set(result, destIdField, _.get(value, sourceIdField));
            _.set(result, destDescrField, '');
        }
        else {
            _.set(result, destIdField, '');
            _.set(result, destDescrField, '');
        }
        return result;
    }
}
