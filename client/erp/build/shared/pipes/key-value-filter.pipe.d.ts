import { PipeTransform } from '@angular/core';
export declare class KeyValueFilterPipe implements PipeTransform {
    transform(value: {
        key;
        value;
    }[], queryString: any): {
        key: any;
        value: any;
    }[];
}
