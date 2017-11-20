import { Pipe, PipeTransform, NgZone } from '@angular/core';
import { EnumsService } from './../../core/services/enums.service';

@Pipe({name: 'tbEnums'})
export class TbEnumsPipe implements PipeTransform {

  constructor(private enumService: EnumsService,
             private ngZone: NgZone) {
  }

  async transform(value: string, type: string): Promise<string> {
        if (type === 'Enum') {
            await this.ngZone.runOutsideAngular(() => this.enumService.getEnumsTableAsync());
            let res = this.enumService.getEnumsItem(value);
            if (res) { return res.name; }
        }
        return value;
    }
}
