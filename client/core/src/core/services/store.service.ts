import { Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, reduce, map, pluck, distinctUntilChanged, of, concat } from './../../rxjs.imports';
import { EventDataService } from './eventdata.service';
import { Logger } from './logger.service';
import { createSelector, createSelectorByMap } from './../../shared/commons/selector';
import * as _ from 'lodash';

export interface Action { type: string; }

export abstract class StateObservable extends Observable<any> {}

export const INIT = '@ngrx/store/init' as '@ngrx/store/init';

@Injectable()
export class ActionsSubject extends BehaviorSubject<Action>
  implements OnDestroy {
  constructor() {
    super({ type: INIT });
  }

  next(action: Action): void {
    if (typeof action === 'undefined') {
      throw new TypeError(`Actions must be objects`);
    } else if (typeof action.type === 'undefined') {
      throw new TypeError(`Actions must have a type property`);
    }

    super.next(action);
  }

  complete() { }

  ngOnDestroy() {
    super.complete();
  }
}

export class StoreT<T> extends Observable<T> {
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
    const selectors = paths.map(p => s => _.get(s, p));
    return this.select(createSelector.call(null, selectors, s => s));
  }

  selectByMap<T>(map: T): StoreT<{[P in keyof T]: any}> {
    return this.select(createSelectorByMap(map));
  }

  dispatch<V extends Action>(action: V) {
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
  constructor(private eventDataService: EventDataService, private logger: Logger) {
    super(Observable.of(eventDataService.model).concat(eventDataService.change.map(id => eventDataService.model)));
    this.logger.debug('Store instantiated ' + Math.round(new Date().getTime() / 1000));
  }
}
