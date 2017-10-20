import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, pluck, distinctUntilChanged } from './../../rxjs.imports';
import { EventDataService } from '@taskbuilder/core';
import * as _ from 'lodash';

export interface Action {
  type: string;
}

class Dispatcher extends BehaviorSubject<any> { }

class StoreT<T> extends Observable<T> {
  private actionsObserver = new Dispatcher('');
  constructor(private stateContainer: { model: T, change: Observable<any> }) {
    super();
    this.stateContainer.change
      .subscribe(s => this.actionsObserver.next(this.stateContainer.model));
  }

  select<K>(mapFn: (state: T) => K): StoreT<K>;
  select<a extends keyof T>(key: a): StoreT<T[a]>;
  select<a extends keyof T, b extends keyof T[a]>(
    key1: a,
    key2: b
  ): StoreT<T[a][b]>;
  select<a extends keyof T, b extends keyof T[a], c extends keyof T[a][b]>(
    key1: a,
    key2: b,
    key3: c
  ): StoreT<T[a][b][c]>;
  select<
    a extends keyof T,
    b extends keyof T[a],
    c extends keyof T[a][b],
    d extends keyof T[a][b][c]
    >(key1: a, key2: b, key3: c, key4: d): StoreT<T[a][b][c][d]>;
  select<
    a extends keyof T,
    b extends keyof T[a],
    c extends keyof T[a][b],
    d extends keyof T[a][b][c],
    e extends keyof T[a][b][c][d]
    >(key1: a, key2: b, key3: c, key4: d, key5: e): StoreT<T[a][b][c][d][e]>;
  select<
    a extends keyof T,
    b extends keyof T[a],
    c extends keyof T[a][b],
    d extends keyof T[a][b][c],
    e extends keyof T[a][b][c][d],
    f extends keyof T[a][b][c][d][e]
    >(
    key1: a,
    key2: b,
    key3: c,
    key4: d,
    key5: e,
    key6: f
    ): StoreT<T[a][b][c][d][e][f]>;
  select(
    pathOrMapFn: ((state: T) => any) | string,
    ...paths: string[]
  ): StoreT<any> {
    let mapped$: StoreT<any>;
    if (typeof pathOrMapFn === 'string') {
      mapped$ = pluck.call(this.actionsObserver, pathOrMapFn, ...paths);
    } else if (typeof pathOrMapFn === 'function') {
      mapped$ = map.call(this.actionsObserver, pathOrMapFn);
    } else {
      throw new TypeError(
        `Unexpected type '${typeof pathOrMapFn}' in select operator,` +
        ` expected 'string' or 'function'`
      );
    }
    return distinctUntilChanged.call(mapped$);
  }

  selectSlices(...paths: string[]): Observable<any> {
    return Observable.combineLatest(
      ...paths.map(x => this.select<any>(s => _.get(s, x)))
    ).map(res => res.reduce((o, val) => { o[val] = val; return o; }, {}));
  }

  selectBySlicer(slicer: {}): Observable<any> {
    return Observable.combineLatest(
      ...Object.keys(slicer)
        .map(x => this.select<any>(s => _.get(s, slicer[x])))
    ).map(res => res.reduce((o, val, i) => { o[Object.keys(slicer)[i]] = val; return o; }, {}));
  }

  // selectBySlicerT<
  //   a extends keyof T
  // >(slicer: T): Observable<T> {
  //   return Observable.combineLatest(
  //     ...Object.keys(slicer)
  //       .map(x => this.select<any>(s => _.get(s, slicer[x])))
  //   ).map(res => res.reduce((o, val, i) => { o[Object.keys(slicer)[i]] = val; return o; }, {}));
  // }

  dispatch<V extends Action = Action>(action: V) {
    this.actionsObserver.next(action);
  }

  next(action: Action) {
    this.actionsObserver.next(action);
  }

  error(err: any) {
    this.actionsObserver.error(err);
  }

  complete() {
    this.actionsObserver.complete();
  }
}

@Injectable()
export class Store extends StoreT<any> {
  constructor(private eventDataService: EventDataService) {
    super(eventDataService);
  }
}
