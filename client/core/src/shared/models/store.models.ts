export interface Action {
  type: string;
}

export type TypeId<T> = () => T;

export type InitialState<T> = Partial<T> | TypeId<Partial<T>> | void;

export interface ActionReducer<T, V extends Action> {
  (state: T | undefined, action: V): T;
}

export type ActionReducerMap<T, V extends Action> = {
  [p in keyof T]: ActionReducer<T[p], V>
};

export interface ActionReducerFactory<T, V extends Action> {
  (
    reducerMap: ActionReducerMap<T, V>,
    initialState?: InitialState<T>
  ): ActionReducer<T, V>;
}

export type MetaReducer<T, V extends Action> = (
  reducer: ActionReducer<T, V>
) => ActionReducer<T, V>;

export interface StoreFeature<T, V extends Action> {
  key: string;
  reducers: ActionReducerMap<T, V> | ActionReducer<T, V>;
  reducerFactory: ActionReducerFactory<T, V>;
  initialState?: InitialState<T>;
  metaReducers?: MetaReducer<T, V>[];
}

export interface SelectorMap {
  [name: string]: string
}

export interface Selector<T, V> {
  (state: T): V;
  /**
  * Creates a selector containing the new properties obtained nesting the new paths to the current ones.
  * The created selector will change on nested properties change, regardless of the base selector properties
  * @param map the selector map
  */
  nest?<Map extends SelectorMap>(map: Map): Selector<T, {[P in keyof Map]: any}>;
  /**
  * Creates a selector containing the new properties obtained nesting the new paths to the current ones.
  * The created selector will change on nested properties change, regardless of the base selector properties
  * @param paths the paths (same syntax as lodash get)
  */
  nest?<K>(...paths: string[]): Selector<T, K>;
  nest?(selectorMapOrPaths: SelectorMap | string, ...paths: string[]): Selector<any, any>;
}
