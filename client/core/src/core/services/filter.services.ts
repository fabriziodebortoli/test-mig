import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';

export type SimpleFilter = { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export type CompositeFilter = { logic?: 'or' | 'and', filter?: Array<CompositeFilter|SimpleFilter> }
export function combineFilters<L, R>(left: Observable<L>, right: Observable<R>): Observable<{left?: L, right?: R}> {
    if (!left && !right)  { return Observable.throw('You must specify at least one argument'); }
    if (!left) { return right.map(x => ({ right: x })); }
    if (!right) { return left.map(x => ({ left: x })); }
    return left.combineLatest(right, (v1, v2) => ({left: v1, right: v2}));
}

function debounceFirst<T>(observable: Observable<T>, dueTime: number): Observable<{ value: T, isFirst: boolean }> {
    return Observable.merge(
        observable.map((value: T) => ({ value, isFirst: true })),
        observable.debounceTime(dueTime).map((value: T) => ({value, isFirst: false}))
    )
    .distinctUntilChanged((a, b) => a.isFirst === b.isFirst);
}

@Injectable()
export class FilterService implements OnDestroy {
    public debounceTime = 200;
    private _debounced = true;
    private filterSubject$: BehaviorSubject<CompositeFilter>;
    private _filter: CompositeFilter;
    public set filter(value: CompositeFilter) {
        this._filter = value;
    }
    public get filter(): CompositeFilter {
        return this._filter;
    }

    private get filterTyping$(): Observable<any> {
        return this.filterSubject$.let(x => debounceFirst<CompositeFilter>(x, this.debounceTime));
    }

    public get filterChanged$(): Observable<CompositeFilter> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.filterTyping$.filter(x => !x.isFirst).map(x => x.value).do(x => 'FILTER CHANGED: ' + JSON.stringify(x));
    }

    public get filterChanging$(): Observable<void> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.filterTyping$.filter(x => x.isFirst).do(x => 'FILTER CHANGED: ' + JSON.stringify(x));
    }

    public configure(debounceTime: number) {
        if (debounceTime >= this.debounceTime) { this.debounceTime = debounceTime; }
        this.filterSubject$ = new BehaviorSubject<CompositeFilter>({});
    }

    public onFilterChanged(value: CompositeFilter) {
        this.filterSubject$.next(value);
    }

    constructor(private ngZone: NgZone) { }

    ngOnDestroy() {
        if (this.filterSubject$) { this.filterSubject$.complete(); }
    }
}
