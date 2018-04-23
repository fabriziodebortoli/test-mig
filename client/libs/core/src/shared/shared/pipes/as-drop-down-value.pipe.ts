import { Pipe, PipeTransform, NgZone } from '@angular/core';
import { ComboBoxData } from './../models/combo-box-data';
import * as _ from 'lodash';

@Pipe({name: 'asDropDownValue'})
export class TbAsDropDownValuePipe implements PipeTransform {

  constructor(private ngZone: NgZone) {
  }

transform(value: any, sourceIdField: string = 'value'): any {
        let result = new ComboBoxData({id: '', displayString: ''});
        if (value && sourceIdField && _.get(value, sourceIdField)) {
            result.id = _.get(value, sourceIdField);
            result.displayString = '';
        }
        return result;
    }
}
