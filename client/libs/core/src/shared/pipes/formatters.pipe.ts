import { Pipe, PipeTransform, NgZone } from '@angular/core';
import { FormattersService } from './../../core/services/formatters.service';

@Pipe({ name: 'tbFormatters' })
export class TbFormattersPipe implements PipeTransform {

    constructor(private formatterService: FormattersService,
        private ngZone: NgZone) {
    }

    async transform(value: string, type: string): Promise<string> {
        if (type === 'Formatter') {
            await this.ngZone.runOutsideAngular(() => this.formatterService.getFormattersTableAsync());
            let res = null; //TODO GIANLUCA this.formatterService.getFormattersItem(value);
            if (res) { return res.name; }
        }
        return value;
    }
}
