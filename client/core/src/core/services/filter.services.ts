import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone, ElementRef } from '@angular/core';
import { debounceFirst } from './../../shared/commons/debounceFirst';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import * as _ from 'lodash';

export type SimpleFilter = { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export type CompositeFilter = { logic?: 'or' | 'and', filters?: Array<CompositeFilter | SimpleFilter> }

export function combineFiltersMap<T, T2, R>(left: Observable<T>, right: Observable<T2>, project: (v1: T, v2: T2) => R): Observable<R> {
    if (!left && !right) { return Observable.throw('You must specify at least one argument'); }
    return left.combineLatest(right, project);
}

export function combineFilters<L, R>(left: Observable<L>, right: Observable<R>): Observable<{ left?: L, right?: R }> {
    if (!left && !right) { return Observable.throw('You must specify at least one argument'); }
    if (!left) { return right.map(x => ({ right: x })); }
    if (!right) { return left.map(x => ({ left: x })); }
    return left.combineLatest(right, (v1, v2) => {
        return { left: v1, right: v2 }
    });
}

@Injectable()
export class FilterService implements OnDestroy {
    public debounceTime = 200;
    public filtersContainerRef: ElementRef;
    public lastChangedFilterIdx: number = 0;
    private _debounced = true;
    private filterSubject$: BehaviorSubject<CompositeFilter>;
    private _filter: CompositeFilter;
    private _previousFilter: CompositeFilter;
    private _changedField: string | Function = '';
    public set filter(value: CompositeFilter) {
        if (this._filter) { this._previousFilter = this._filter; }
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
        return this.filterSubject$.pipe(debounceFirst(this.debounceTime));
    }

    public get filterChanged$(): Observable<CompositeFilter> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configured. Must call start(...).'); }
        return this.filterTyping$.filter(x => !x.isFirst).map(x => x.value);
    }

    public get filterChanging$(): Observable<void> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configured. Must call start(...).'); }
        return this.filterTyping$.filter(x => x.isFirst);
    }

    public start(debounceTime: number, filterContainer?: ElementRef) {
        if (debounceTime >= this.debounceTime) { this.debounceTime = debounceTime; }
        this.filterSubject$ = new BehaviorSubject<CompositeFilter>({});
        this.filtersContainerRef = filterContainer;
    }

    public stop() {
        this.reset();
    }

    private reset() {
        if (this.filterSubject$)
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

    public sortChanged$: Observable<SortDescriptor[]> = new BehaviorSubject<SortDescriptor[]>([]);

    public set sort(descriptors: SortDescriptor[]) {
        (this.sortChanged$ as BehaviorSubject<SortDescriptor[]>).next(descriptors);
    }

    public get sort(): SortDescriptor[] {
        return (this.sortChanged$ as BehaviorSubject<SortDescriptor[]>).getValue();
    }

    public resetFocus() {
        this.setFocus('[kendofilterinput]', this.lastChangedFilterIdx);
    }
    
    private setFocus(selector: string, index: number) {
        setTimeout(() => {
            const filters = this.filtersContainerRef.nativeElement.querySelectorAll(selector);
            filters && filters[index] && filters[index].focus();
        }, 100);
    }
}
