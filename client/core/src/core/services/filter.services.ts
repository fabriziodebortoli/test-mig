import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';
import * as _ from 'lodash';

export type SimpleFilter = { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export type CompositeFilter = { logic?: 'or' | 'and', filters?: Array<CompositeFilter|SimpleFilter> }
export function combineFilters<L, R>(left: Observable<L>, right: Observable<R>): Observable<{left?: L, right?: R}> {
    if (!left && !right)  { return Observable.throw('You must specify at least one argument'); }
    if (!left) { return right.map(x => ({ right: x })); }
    if (!right) { return left.map(x => ({ left: x })); }
    return left.combineLatest(right, (v1, v2) => {
        return {left: v1, right: v2}
    });
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
    private _previousFilter: CompositeFilter;
    private _changedField: string | Function = '';
    public set filter(value: CompositeFilter) {
        if (this._filter) { this._previousFilter = _.cloneDeep(this._filter); }
        this._filter = value;

        let diff = this._filter.filters[0];
        if (this._previousFilter) {
            diff = _.differenceWith(this._previousFilter.filters, this._filter.filters, _.isEqual)[0];
            if (diff === [] || !diff) { diff = _.differenceWith(this._filter.filters, this._previousFilter.filters, _.isEqual)[0]; }
        }
        if (diff && diff as SimpleFilter && (diff as SimpleFilter).field) {
            this._changedField = (diff as SimpleFilter).field;
        } else {
            this._changedField = '';
        }
    }
    public get filter(): CompositeFilter {
        return this._filter;
    }

    public get changedField(): string | Function | '' {
        return this._changedField;
    }

    private get filterTyping$(): Observable<any> {
        return this.filterSubject$.let(x => debounceFirst<CompositeFilter>(x, this.debounceTime));
    }

    public get filterChanged$(): Observable<CompositeFilter> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.filterTyping$.filter(x => !x.isFirst).map(x => x.value);
    }

    public get filterChanging$(): Observable<void> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.filterTyping$.filter(x => x.isFirst);
    }

    public start(debounceTime: number) {
        if (debounceTime >= this.debounceTime) { this.debounceTime = debounceTime; }
        this.filterSubject$ = new BehaviorSubject<CompositeFilter>({});
    }

    public stop() {
       this.reset();
    }

    private reset() {
        if(this.filterSubject$)
            this.filterSubject$.complete();
        this.filterSubject$ = null;
        this.debounceTime = 200;
        this._debounced = true;
        this._filter = null;
        this._previousFilter = null;
        this._changedField = '';
    }

    public onFilterChanged(value: CompositeFilter) {
        if (this.filterSubject$) this.filterSubject$.next(value);
    }

    constructor(private ngZone: NgZone) { }

    ngOnDestroy() {
        if (this.filterSubject$) { this.reset(); }
    }
}
