import { Pipe, PipeTransform } from '@angular/core';
import { FormattersService } from './../../core/services/formatters.service';
import { formatStringDate } from '../controls/date-input/u';

@Pipe({name: 'tbDate'})
export class TbDatePipe implements PipeTransform {

  constructor(private formatterService: FormattersService) { }

  transform(value: string, type: string): string {
        return type.includes('DateTime') ? formatStringDate(value, this.formatterService.getFormatter('Date'), type) : value;   
    }
}