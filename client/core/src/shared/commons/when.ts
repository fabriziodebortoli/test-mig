import { Observable, pipe } from 'rxjs/Rx';
import { map, filter, withLatestFrom } from 'rxjs/operators';

/**
 * Emits values only when the last emitted value bu enabler is true
 */
export const when = (enabler: Observable<boolean>) => <T>(source: Observable<T>) =>
    source.pipe(withLatestFrom(enabler), filter(t => t[1]), map(t => t[0]));

/**
* Emits values only when the last emitted value by enabler is false
*/
export const whenNot = (enabler: Observable<boolean>) => <T>(source: Observable<T>) =>
    source.pipe(withLatestFrom(enabler), filter(t => !t[1]), map(t => t[0]));

