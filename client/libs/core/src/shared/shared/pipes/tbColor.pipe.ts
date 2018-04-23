import { Pipe, PipeTransform } from '@angular/core';
export function anyToString(value: any) {
    let htmForeColor: string;
        if (typeof value == 'string') {
            htmForeColor = value;
        } else {
            htmForeColor = '#' + (value as number).toString(16);
        }
        return htmForeColor;
};

@Pipe({ name: 'tbColor' })
export class TbColorPipe implements PipeTransform {

    constructor() {
    }

    transform(value: any): string { return anyToString(value); }
}
