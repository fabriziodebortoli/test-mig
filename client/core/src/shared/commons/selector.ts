import { Selector, SelectorMap } from './../models/store.models';
import * as _ from 'lodash';

export type AnyFn = (...args: any[]) => any;
const map = Symbol('map$');
const tag = Symbol('tag$');

export interface MemoizedSelector<State, Result>
  extends Selector<State, Result> {
  release(): void;
  projector: AnyFn;
}

export function memoize(t: AnyFn): { memoized: AnyFn; reset: () => void } {
  let lastArguments: null | IArguments = null;
  let lastResult: any = null;

  function reset() {
    lastArguments = null;
    lastResult = null;
  }

  function memoized(): any {
    if (!lastArguments) {
      lastResult = t.apply(null, arguments);
      lastArguments = arguments;

      return lastResult;
    }
    for (let i = 0; i < arguments.length; i++) {
      if (arguments[i] !== lastArguments[i]) {
        t[tag] = i;
        lastResult = t.apply(null, arguments);
        lastArguments = arguments;

        return lastResult;
      }
    }

    return lastResult;
  }

  return { memoized, reset };
}

export function createSelector<State, Map extends SelectorMap>(map: Map): MemoizedSelector<State, {[P in keyof Map]: any}>;
export function createSelector<State>(...paths: string[]): MemoizedSelector<State, any>;
export function createSelector<State, S1, Result>(
  s1: Selector<State, S1>,
  projector: (S1: S1) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, Result>(
  selectors: [Selector<State, S1>],
  projector: (s1: S1) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  projector: (s1: S1, s2: S2) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, Result>(
  selectors: [Selector<State, S1>, Selector<State, S2>],
  projector: (s1: S1, s2: S2) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  projector: (s1: S1, s2: S2, s3: S3) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, Result>(
  selectors: [Selector<State, S1>, Selector<State, S2>, Selector<State, S3>],
  projector: (s1: S1, s2: S2, s3: S3) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  s4: Selector<State, S4>,
  projector: (s1: S1, s2: S2, s3: S3, s4: S4) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, Result>(
  selectors: [
    Selector<State, S1>,
    Selector<State, S2>,
    Selector<State, S3>,
    Selector<State, S4>
  ],
  projector: (s1: S1, s2: S2, s3: S3, s4: S4) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  s4: Selector<State, S4>,
  s5: Selector<State, S5>,
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, Result>(
  selectors: [
    Selector<State, S1>,
    Selector<State, S2>,
    Selector<State, S3>,
    Selector<State, S4>,
    Selector<State, S5>
  ],
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  s4: Selector<State, S4>,
  s5: Selector<State, S5>,
  s6: Selector<State, S6>,
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5, s6: S6) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, Result>(
  selectors: [
    Selector<State, S1>,
    Selector<State, S2>,
    Selector<State, S3>,
    Selector<State, S4>,
    Selector<State, S5>,
    Selector<State, S6>
  ],
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5, s6: S6) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, S7, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  s4: Selector<State, S4>,
  s5: Selector<State, S5>,
  s6: Selector<State, S6>,
  s7: Selector<State, S7>,
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5, s6: S6, s7: S7) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, S7, Result>(
  selectors: [
    Selector<State, S1>,
    Selector<State, S2>,
    Selector<State, S3>,
    Selector<State, S4>,
    Selector<State, S5>,
    Selector<State, S6>,
    Selector<State, S7>
  ],
  projector: (s1: S1, s2: S2, s3: S3, s4: S4, s5: S5, s6: S6, s7: S7) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, S7, S8, Result>(
  s1: Selector<State, S1>,
  s2: Selector<State, S2>,
  s3: Selector<State, S3>,
  s4: Selector<State, S4>,
  s5: Selector<State, S5>,
  s6: Selector<State, S6>,
  s7: Selector<State, S7>,
  s8: Selector<State, S8>,
  projector: (
    s1: S1,
    s2: S2,
    s3: S3,
    s4: S4,
    s5: S5,
    s6: S6,
    s7: S7,
    s8: S8
  ) => Result
): MemoizedSelector<State, Result>;
export function createSelector<State, S1, S2, S3, S4, S5, S6, S7, S8, Result>(
  selectors: [
    Selector<State, S1>,
    Selector<State, S2>,
    Selector<State, S3>,
    Selector<State, S4>,
    Selector<State, S5>,
    Selector<State, S6>,
    Selector<State, S7>,
    Selector<State, S8>
  ],
  projector: (
    s1: S1,
    s2: S2,
    s3: S3,
    s4: S4,
    s5: S5,
    s6: S6,
    s7: S7,
    s8: S8
  ) => Result
): MemoizedSelector<State, Result>;
export function createSelector(pathOrMapOrSelector: SelectorMap | Selector<any, any> | string | any[], ...input: any[]): Selector<any, any> {
  if (input.length === 0 && typeof pathOrMapOrSelector === 'object')
    return createSelectorByMap(pathOrMapOrSelector as SelectorMap);
  if (typeof pathOrMapOrSelector === 'string')
    return createSelectorByPaths(pathOrMapOrSelector, ...input);

  let args = [pathOrMapOrSelector, ...input];
  if (Array.isArray(args[0])) {
    const [head, ...tail] = args;
    args = [...head, ...tail];
  }

  const selectors = args.slice(0, args.length - 1);
  const projector = args[args.length - 1];

  const memoizedProjector = memoize(function fun(...selectors_: any[]) {
    projector[tag] = fun[tag];
    return projector.apply(null, selectors_);
  });

  // pd: can't memoize state due to mutability
  const memoizedState = function (state: any) {
    return memoizedProjector.memoized.apply(null, selectors.map(fn => fn(state)));
  };

  function release() {
    // memoizedState.reset(); pd: can't memoize state due to mutability
    memoizedProjector.reset();
  }

  return augmentSelector(Object.assign(memoizedState, {
    release,
    selectors: selectors,
    projector: memoizedProjector.memoized
  }));
}

export function createSelectorByMap<Map extends SelectorMap>(selectorMap: Map): Selector<any, {[P in keyof Map]: any}> {
  const props = Object.keys(selectorMap);
  const selectors = props.map(x => s => _.get(s, selectorMap[x]));
  return augmentSelector(createSelector.call(null, selectors,
    function fun(...slices: any[]) {
      return slices.reduce((o, v, i) => ({ ...o, [props[i]]: v }),
        { propertyChangedName: fun.hasOwnProperty(tag) ? props[fun[tag]] : undefined });
    }), selectorMap);
}

export function createSelectorByPaths(...paths: string[]): Selector<any, any> {
  if (paths.length === 1)
    return augmentSelector(createSelector.call(null, s => _.get(s, paths[0]), s => s), pathsToSelectorMap(paths[0]));
  return createSelectorByMap(pathsToSelectorMap(...paths));
}

/**
* Creates a selector containing the new properties obtained nesting the new paths to the base ones.
* The created selector will change on nested properties change, regardless of the base selector properties
* @param baseSelector the base selector: it has to be created by using
* createSelectorByMap or createSelectorByPaths or nestSelector itself
* @param map the selector map
*/
export function nestSelector<T, Map extends SelectorMap>(baseSelector: Selector<T, any>, map: Map): Selector<T, {[P in keyof Map]: any}>;
/**
* Creates a selector containing the new properties obtained nesting the new paths to the base ones.
* The created selector will change on nested properties change, regardless of the base selector properties
* @param baseSelector the base selector: it has to be created by using
* createSelectorByMap or createSelectorByPaths or nestSelector itself
* @param paths the paths (same syntax as lodash get)
*/
export function nestSelector<T, V>(baseSelector: Selector<T, any>, ...paths: string[]): Selector<T, V>;
export function nestSelector(baseSelector: Selector<any, any>,
  selectorMapOrPaths: SelectorMap | string, ...paths: string[]): Selector<any, any> {
  const baseMap = baseSelector[map];
  if (baseMap === undefined)
    throw new Error('only selectors created from a SelectorMap can be extended');
    if (typeof selectorMapOrPaths === 'string') {
      if (paths.length === 0) return createSelectorByPaths(nestPath(baseMap, selectorMapOrPaths));
      return createSelectorByMap(nestMap(baseMap, pathsToSelectorMap(selectorMapOrPaths, ...paths)));
    }
    return createSelectorByMap(nestMap(baseMap, selectorMapOrPaths));
}

function pathsToSelectorMap(...paths: string[]) {
  return paths.map(p => p.includes('.') ? p.slice(p.lastIndexOf('.') + 1) : p)
  .reduce((o, p, i) => ({ ...o, [p]: paths[i] }), { });
}

function pathsToPropNames(paths: string[], propNames: string[] = []) {
  if (propNames.length === paths.length) return propNames;
  return paths.map(p => p.includes('.') ? p.slice(p.lastIndexOf('.') + 1) : p);
}

function nestMap(baseMap, selectorMap) {
  return Object.keys(selectorMap)
    .reduce((o, p: string) => ({ ...o, [p]: nestPath(baseMap, selectorMap[p]) }), {});
}

function nestPath(baseMap, path) {
  const idx = path.indexOf('.');
  if (idx === -1) return baseMap[path];
  return baseMap[path.slice(0, idx)] + path.slice(idx);
}

function augmentSelector(selector, selectorMap?) {
  selector[map] = selectorMap;
  selector.nest = (mapOrPath, ...p) => nestSelector(selector, mapOrPath, ...p);
  selector.asPath = n => selector[map][n];
  selector.asMap = () => selector[map];
  return selector;
}
