import { Observable, pipe } from 'rxjs/Rx';
import { merge, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import 'rxjs/add/observable/merge';

/**
 * Immediatly emits a value from an Observable and a second one only after a
 * particular time span has passed without another source emission.
 * @param {number} dueTime The timeout duration in milliseconds (or the time
 * unit determined internally by the optional `scheduler`) for the window of
 * time required to wait for emission silence before emitting the most recent
 * source value.
 * @see {@link debounce}
 * @see {@link debounceTime}
 */
export const debounceFirst = (dueTime: number) => <T>(source: Observable<T>) => (
    Observable.merge(
        source.map((value: T) => ({ value, isFirst: true })),
        source.debounceTime(dueTime).map((value: T) => ({ value, isFirst: false }))
    ).pipe(distinctUntilChanged((a, b) => a.isFirst === b.isFirst))
);

