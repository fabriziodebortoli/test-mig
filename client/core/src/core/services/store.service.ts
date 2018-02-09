import { Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, reduce, map, pluck, distinctUntilChanged, of, concat } from './../../rxjs.imports';
import { EventDataService } from './eventdata.service';
import { Logger } from './logger.service';
import { createSelector, createSelectorByMap, createSelectorByPaths } from './../../shared/commons/selector';
import { SelectorMap } from './../../shared/models/store.models';
import * as _ from 'lodash';

export interface Action { type: string; }
export abstract class StateObservable extends Observable<any> { }
export const INIT = '@ngrx/store/init' as '@ngrx/store/init';

@Injectable()
export class ActionsSubject extends BehaviorSubject<Action> implements OnDestroy {
  constructor() { super({ type: INIT }); }

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

  select<K>(map: (state: T) => K): StoreT<K>;
  select<M extends SelectorMap>(map: M): StoreT<{[P in keyof M]: any}>;
  select(...paths: string[]): StoreT<any>;
  select(
    pathOrMapFn: ((state: T) => any) | SelectorMap | string,
    ...paths: string[]
  ): StoreT<any> {
    let mapped$: Observable<any>;
    if (typeof pathOrMapFn === 'string') {
      mapped$ = map.call(this, createSelectorByPaths(pathOrMapFn, ...paths));
    } else if (typeof pathOrMapFn === 'function') {
      mapped$ = map.call(this, pathOrMapFn);
    } else if (typeof pathOrMapFn === 'object') {
      mapped$ = map.call(this, createSelectorByMap(pathOrMapFn));
    } else {
      throw new TypeError(
        `Unexpected type '${typeof pathOrMapFn}' in select operator,` +
        ` expected 'string' or 'function' or 'SelectorMap'`
      );
    }
    return new StoreT<any>(distinctUntilChanged.call(mapped$));
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
    super(Observable.of(eventDataService.model).concat(eventDataService.change.map(_ => eventDataService.model)));
  }
}
