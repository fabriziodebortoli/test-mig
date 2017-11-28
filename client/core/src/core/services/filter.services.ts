import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';

export type SimpleFilter = { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export type CompositeFilter = { logic?: 'or' | 'and', filter?: Array<CompositeFilter|SimpleFilter> }
export function combineFilters<L, R>(left: Observable<L>, right: Observable<R>): Observable<{left: L, right: R}> {
    return left.combineLatest(right).map(x => ({ left: x[0], right: x[1] }));
}

@Injectable()
export class FilterService implements OnDestroy {
    private debounced = true;
    private filterSubject$: BehaviorSubject<CompositeFilter> = new BehaviorSubject<CompositeFilter>({});
    private filterTypingSubject$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    private _filter: CompositeFilter;
    public set filter(value: CompositeFilter) {
        this._filter = value;
        this.filterSubject$.next(value);
    }
    public get filter(): CompositeFilter {
        return this._filter;
    }

    public get filter$(): Observable<CompositeFilter> {
        return this.ngZone.runOutsideAngular(() => this.filterSubject$.asObservable().debounceTime(200));
    }
    public get filterTyping$(): Observable<boolean> { return this.filterTypingSubject$.asObservable(); }

    constructor(private ngZone: NgZone) {
        this.filter$.subscribe(x => {
            this.debounced = true;
            this.filterTypingSubject$.next(false);
        });
        this.filterSubject$.asObservable().subscribe(x => {
            if (this.debounced) { this.filterTypingSubject$.next(true); }
            this.debounced = false;
        });
    }

    ngOnDestroy() {
        this.filterTypingSubject$.complete();
        this.filterSubject$.complete();
    }
}
