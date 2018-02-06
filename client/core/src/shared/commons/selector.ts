import { Selector } from './../models/store.models';
import * as _ from 'lodash';

export type AnyFn = (...args: any[]) => any;

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
        t['tag'] = i;
        lastResult = t.apply(null, arguments);
        lastArguments = arguments;

        return lastResult;
      }
    }

    return lastResult;
  }

  return { memoized, reset };
}

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
export function createSelector(...input: any[]): Selector<any, any> {
  let args = input;
  if (Array.isArray(args[0])) {
    const [head, ...tail] = args;
    args = [...head, ...tail];
  }

  const selectors = args.slice(0, args.length - 1);
  const projector = args[args.length - 1];

  const memoizedProjector = memoize(function fun(...selectors: any[]) {
    projector['tag'] = fun['tag'];
    return projector.apply(null, selectors);
  });

  // pd: can't memoize state due to mutability
  const memoizedState = function (state: any) {
    const args = selectors.map(fn => fn(state));

    return memoizedProjector.memoized.apply(null, args);
  };

  function release() {
    // memoizedState.reset();
    memoizedProjector.reset();
  }

  return Object.assign(memoizedState, {
    release,
    projector: memoizedProjector.memoized,
  });
}

export function createSelectorByMap(selectorMap): Selector<any, any> {
  const props = Object.keys(selectorMap);
  const selectors = props.map(x => s => _.get(s, selectorMap[x]));
  const sel = createSelector.call(null, selectors,
    function fun(...slices: any[]) {
      return slices.reduce((o, val, i) => { o[props[i]] = val; return o; },
        { propertyChangedName: fun.hasOwnProperty('tag') ? props[fun['tag']] : undefined });
    });
  sel.map = selectorMap;
  return sel;
}

export function createSelectorByPaths(paths: string[], propNames: string[] = []): Selector<any, any> {
  if (propNames.length !== paths.length)
    propNames = paths.map(p => p.includes('.') ? p.slice(p.lastIndexOf('.') + 1) : p);
  return createSelectorByMap(propNames.reduce((o, p, i) => { o[p] = paths[i]; return o; }, {}));
}

export function extendSelectorByMap(selector: Selector<any, any>, selectorMap): Selector<any, any> {
  const baseMap = (selector as any).map;
  if (baseMap === undefined)
    throw new Error('only selectors created by createSelectorByMap or createSelectorByPaths can be extended');
  return createSelectorByMap(extendMap(baseMap, selectorMap));
}

function extendMap(baseMap, selectorMap) {
  const props = Object.keys(selectorMap);
  const baseProps = Object.keys(baseMap);
  const paths = props.map(p => selectorMap[p].split('.', 2));
  return props.reduce((o, p, i) => { o[p] = baseMap[paths[i][0]] + '.' + paths[i][1]; return o; }, {});
}

