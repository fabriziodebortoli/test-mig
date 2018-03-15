import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone, ElementRef } from '@angular/core';
import { SortDescriptor, orderBy, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { debounceFirst } from './../../shared/commons/debounceFirst';
import * as _ from 'lodash';

export class SimpleFilter { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export class CompositeFilter { logic?: 'or' | 'and'; filters?: Array<CompositeFilter | SimpleFilter> }

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

    private _filterOperatorsMap = {
        'eq': 'IsEqualTo',
        'neq': 'IsNotEqualTo',
        'gte': 'GreaterOrEqualTo',
        'gt': 'GreaterTo',
        'lte': 'LowerOrEqualTo',
        'lt': 'LowerTo',
        'isnull': 'IsEmpty'
    }

    private convertOperator: (string) => string = (op) => !(op as String) || !this._filterOperatorsMap[op] ? op :  this._filterOperatorsMap[op];

    public filterBag: Map<string, any> = new Map<string, any>();

    public debounceTime = 200;
    public filtersContainerRef: ElementRef;
    public lastChangedFilterIdx: number = 0;
    private _debounced = true;
    private filterSubject$ = new BehaviorSubject<CompositeFilter>({});
    private _filter: CompositeFilter;
    private _previousFilter: CompositeFilter;
    private _changedField: string | Function = '';
    public set filter(value: CompositeFilter) {
        if (this._filter) { this._previousFilter = this._filter; }
        this._filter = value;
        let diff = _.get(this._filter, 'filters[0]');
        if (this._previousFilter) {
            diff = _.differenceWith(this._previousFilter.filters, _.get(this._filter, 'filters'), _.isEqual)[0];
            if (diff === [] || !diff) { diff = _.differenceWith(_.get(this._filter, 'filters'), this._previousFilter.filters, _.isEqual)[0]; }
        }
        if (diff && diff as SimpleFilter && (diff as SimpleFilter).field) {
            this._changedField = (diff as SimpleFilter).field;
        } else {
            this._changedField = '';
        }
        this.storeFocus();
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
        return this.filterTyping$.filter(x => !x.isFirst).map(x => x.value);
    }

    public get filterChanging$(): Observable<void> {
        return this.filterTyping$.filter(x => x.isFirst);
    }

    public start(debounceTime: number, filterContainer?: ElementRef) {
        if (debounceTime >= this.debounceTime) { this.debounceTime = debounceTime; }
        this.filtersContainerRef = filterContainer;
    }

    public stop() {
        this.reset();
    }

    private reset() {
        this.filterSubject$.complete();
        this.filterSubject$ = new BehaviorSubject<CompositeFilter>({});
        this.debounceTime = 200;
        this._debounced = true;
        this._filter = null;
        this._previousFilter = null;
        this._changedField = '';
        this.filterBag.clear();
    }

    public onFilterChanged(value: CompositeFilter) {
        let formattedValue = this.format(value);
        if (this.filterSubject$) this.filterSubject$.next(formattedValue);
    }

    private format(value: CompositeFilter): CompositeFilter {
        return !value || !value.filters || value.filters.length === 0 ? value : { ...value, filters: value.filters.map(f => (f instanceof SimpleFilter) ? this.formatSimple(f as SimpleFilter) : this.format(f as CompositeFilter)) };
    }

    private formatSimple(value: SimpleFilter) : SimpleFilter {
        const isFilterEmpty = !value || ((value.value === undefined || value.value === undefined) && !value.operator);
        const emptyFilter = (value ? {...value, value: '' } : value);
        return  isFilterEmpty ? emptyFilter
            : (value.value instanceof Date) ? { ...value, value: value.value.toISOString(), operator: this.convertOperator(value.operator) } 
            // Currently server crashes if Boolean values are not managed this way...
            : ((typeof(value.value) === "boolean") ? {...value, value: value.value === true ? '1' : '0', operator: this.convertOperator(value.operator)}
            : {...value, operator: this.convertOperator(value.operator)} );
    }

    constructor(private ngZone: NgZone) { }

    ngOnDestroy() {
        this.reset();
    }

    public sortChanged$: Observable<SortDescriptor[]> = new BehaviorSubject<SortDescriptor[]>([]);

    public set sort(descriptors: SortDescriptor[]) {
        (this.sortChanged$ as BehaviorSubject<SortDescriptor[]>).next(descriptors);
    }

    public get sort(): SortDescriptor[] {
        return (this.sortChanged$ as BehaviorSubject<SortDescriptor[]>).getValue();
    }

    public storeFocus() {        
        let filtersContainerRef = this.filtersContainerRef;
        if(!filtersContainerRef)
        {
            return;
        }
        this.lastChangedFilterIdx =
            Array.from(filtersContainerRef.nativeElement.querySelectorAll('[kendofilterinput]'))
                .findIndex(e => e === document.activeElement);
    }

    public restoreFocus() {
        let lastChangedFilterIdx = this.lastChangedFilterIdx ? this.lastChangedFilterIdx : -1;
        this.setFocus('[kendofilterinput]', lastChangedFilterIdx);
    }

    private setFocus(selector: string, index: number) {
        setTimeout(() => {
            const filters = this.filtersContainerRef.nativeElement.querySelectorAll(selector);
            filters && filters[index] && filters[index].focus();
        }, 100);
    }
}