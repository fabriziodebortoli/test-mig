import { Observable, BehaviorSubject } from '../../rxjs.imports';
import { Injectable, OnDestroy, NgZone } from '@angular/core';

export type SimpleFilter = { field?: string | Function; operator: string | Function; value?: any; ignoreCase?: boolean }
export type CompositeFilter = { logic?: 'or' | 'and', filter?: Array<CompositeFilter|SimpleFilter> }
export function combineFilters<L, R>(left: Observable<L>, right: Observable<R>): Observable<{left?: L, right?: R}> {
    if (!left && !right)  { return Observable.throw('You must specify at least one argument'); }
    if (!left) { return right.map(x => ({ right: x })); }
    if (!right) { return left.map(x => ({ left: x })); }
    return left.combineLatest(right).map(x => ({ left: x[0], right: x[1] }));
}

@Injectable()
export class FilterService implements OnDestroy {
    public debounceTime = 200;
    private _debounced = true;
    private filterSubject$: BehaviorSubject<CompositeFilter>;
    private filterTypingSubject$: BehaviorSubject<boolean>;
    private _filter: CompositeFilter;
    public set filter(value: CompositeFilter) {
        this._filter = value;
    }
    public get filter(): CompositeFilter {
        return this._filter;
    }

    public get filter$(): Observable<CompositeFilter> {
        if (!this.filterSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.ngZone.runOutsideAngular(() => this.filterSubject$.asObservable()
            .filter(x => JSON.stringify(x) !== JSON.stringify({})).debounceTime(this.debounceTime));
    }

    public get filterTyping$(): Observable<boolean> {
        if (!this.filterTypingSubject$) { Observable.throw('Filter Service not correctly configure. Must call configure(...).'); }
        return this.filterTypingSubject$.asObservable();
    }

    public configure(debounceTime: number) {
        if (debounceTime >= this.debounceTime) { this.debounceTime = debounceTime; }
        this.filterSubject$ = new BehaviorSubject<CompositeFilter>({});
        this.filterTypingSubject$  = new BehaviorSubject<boolean>(false);

        this.filter$.subscribe(x => {
            this._debounced = true;
            this.filterTypingSubject$.next(false);
        });
        this.filterSubject$.asObservable().subscribe(x => {
            if (this._debounced) { this.filterTypingSubject$.next(true); }
            this._debounced = false;
        });
    }

    public filterChanged(value: CompositeFilter) {
        this.filterSubject$.next(value);
    }

    constructor(private ngZone: NgZone) { }

    ngOnDestroy() {
        this.filterTypingSubject$.complete();
        this.filterSubject$.complete();
    }
}
