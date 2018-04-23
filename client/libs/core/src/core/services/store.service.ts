import { Injectable, OnDestroy } from '@angular/core';
import { Observable, BehaviorSubject, reduce, map, pluck, distinctUntilChanged, of, concat } from './../../rxjs.imports';
import { EventDataService } from './eventdata.service';
import { Logger } from './logger.service';
import { createSelector, createSelectorByMap, createSelectorByPaths } from './../../shared/commons/selector';
import { SelectorMap, Selector } from './../../shared/models/store.models';
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
    let selector: Selector<T, any>;
    if (typeof pathOrMapFn === 'string') {
      mapped$ = map.call(this, selector = createSelectorByPaths(pathOrMapFn, ...paths));
    } else if (typeof pathOrMapFn === 'function') {
      mapped$ = map.call(this, selector = pathOrMapFn);
    } else if (typeof pathOrMapFn === 'object') {
      mapped$ = map.call(this, selector = createSelectorByMap(pathOrMapFn));
    } else {
      throw new TypeError(
        `Unexpected type '${typeof pathOrMapFn}' in select operator,` +
        ` expected 'string' or 'function' or 'SelectorMap'`
      );
    }
    const store = new StoreT<any>(distinctUntilChanged.call(mapped$));
    store['__selector'] = selector;
    return store;
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

  asSelector(): Selector<T, any> {
    return this['__selector'];
  }
}

@Injectable()
export class Store extends StoreT<any> {
  constructor(private eventDataService: EventDataService, private logger: Logger) {
    super(eventDataService.change.startWith('').map(_ => eventDataService.model).publishReplay(1).refCount())
  }
}
