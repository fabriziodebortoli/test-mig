import { Pipe, PipeTransform } from '@angular/core';


@Pipe({ name: 'toUpper' })

export class TbToUpper implements PipeTransform {

    transform(value: any, isuppercase: boolean): string {
        if (isuppercase) {
            return value.toUpperCase();
        } else {
            return value;
        }
    }
}
