import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'keyValueFilter'
})
export class KeyValueFilterPipe implements PipeTransform {
    transform(value: { key, value }[], queryString) {
        if (value === null) {
            return null;
        }

        if (queryString !== undefined) {
            return value.filter(item => (item.key.toLowerCase().indexOf(queryString.toLowerCase()) !== -1) ||
                (item.value.toLowerCase().indexOf(queryString.toLowerCase()) !== -1));
        } else {
            return value;
        }
    }
}
