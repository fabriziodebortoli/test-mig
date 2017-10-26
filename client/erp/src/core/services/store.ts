import { Injectable } from '@angular/core';
import { Observable, reduce, map, pluck, distinctUntilChanged } from './../../rxjs.imports';
import { EventDataService } from '@taskbuilder/core';
import { createSelector, createSelectorByMap } from './selector';
import { ActionsSubject } from './actions_subject';
import * as _ from 'lodash';

export interface Action {
  type: string;
}

interface SelectorMap {
  [name: string]: string;
}

export abstract class StateObservable extends Observable<any> {}

class StoreT<T> extends Observable<T> {
  private actionsObserver: ActionsSubject;
  constructor(state$: StateObservable) {
    super();
    this.source = state$;
    this.actionsObserver = new ActionsSubject();
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
    let mapped$: Observable<any>;
    if (typeof pathOrMapFn === 'string') {
      mapped$ = pluck.call(this, pathOrMapFn, ...paths);
    } else if (typeof pathOrMapFn === 'function') {
      mapped$ = map.call(this, pathOrMapFn);
    } else {
      throw new TypeError(
        `Unexpected type '${typeof pathOrMapFn}' in select operator,` +
        ` expected 'string' or 'function'`
      );
    }
    return new StoreT<any>(distinctUntilChanged.call(mapped$));
  }

  selectSlice(...paths: string[]): StoreT<any> {
    return new StoreT(Observable.combineLatest(...paths.map(p => this.select<any>(s => _.get(s, p))))
      .map(res => res.reduce((o, val) => { o[val] = val; return o; }, {})));
  }

  // selectSlice(...paths: string[]): StoreT<any> {
  //   const selectors = paths.map(x => s => _.get(s, paths));
  //   return this.select(createSelector.call(null, selectors, s => s));
  // }

  selectByMap<T>(map: T): StoreT<{[P in keyof T]: any}> {
    return this.select(createSelectorByMap(map));
  }

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
    super(
      eventDataService.change
      .do(c => { console.log('STORE CHANGE'); return c; })
      .map(_ => eventDataService.model));
  }
}
