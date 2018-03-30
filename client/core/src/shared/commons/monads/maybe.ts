import { get } from 'lodash';

export class Maybe<T> {
    static some<T>(value: T) {
        if (typeof value === 'undefined' || value === null) throw Error('Provided value must not be empty');
        return new Maybe(value);
    }

    static none<T>() {
        return new Maybe<T>(null);
    }

    static from<T>(value: T) {
        return value ? Maybe.some(value) : Maybe.none<T>();
    }

    static get<T>(value: T, path: string, defaultValue?: T) {
        return Maybe.from(typeof defaultValue !== 'undefined' ?
            get(value, path, defaultValue) : get(value, path));
    }

    private constructor(private value: T | null) { }

    getOrDefault(defaultValue: T) {
        return this.value === null ? defaultValue : this.value;
    }

    map<R>(f: (wrapped: T) => R): Maybe<R> {
        return this.value === null ? Maybe.none<R>() : Maybe.from(f(this.value));
    }

    mapOr<R>(f: (wrapped: T) => R, fElse: () => R): Maybe<R> {
        return this.value === null ? Maybe.from(fElse()) : Maybe.from(f(this.value));
    }

    flatMap<R>(f: (wrapped: T) => Maybe<R>): Maybe<R> {
        return this.value === null ? Maybe.none<R>() : f(this.value);
    }

    flatMapOr<R>(f: (wrapped: T) => Maybe<R>, fElse: () => Maybe<R>): Maybe<R> {
        return this.value === null ? fElse() : f(this.value);
    }
}