import { Maybe } from './monads/maybe';
/**
 * try to execute f and returns the result, or def in case of exception
 * @param f A function returning a T
 * @param def The default value to return in case ov exception
 */
export function tryOrDefault<T>(f: () => T, def?: T): T {
    try {
        return f();
    } catch (e) {
        return def;
    }
}

export function findAnchestorByClass(el: any, cls: string): any {
    if (!el) return null;
    while ((el = el.parentElement) && !el.classList.contains(cls));
    return el;
}

/**
 * Creates an array from an iterable object. returns an empty array if object null or undefined
 * @param arrayLike An array-like object to convert to an array.
 * @param mapfn A mapping function to call on every element of the array.
 */
export function tryArrayFrom<T, U>(arrayLike: ArrayLike<T>): T[]
export function tryArrayFrom<T, U>(arrayLike: ArrayLike<T>, mapfn: (v: T, k: number) => U): U[]
export function tryArrayFrom(arrayLike: any, mapfn?: (v: any, k: number) => any): any {
    if (arrayLike) return mapfn ? Array.from(arrayLike, mapfn) : Array.from(arrayLike);
    return [];
}

/**
 * Creates an array from an iterable object. returns an empty array if object null or undefined
 * @param arrayLike An array-like object to convert to an array.
 * @param mapfn A mapping function to call on every element of the array.
 */
export function arrayFrom<T, U>(arrayLike: ArrayLike<T>): Maybe<T[]>
export function arrayFrom<T, U>(arrayLike: ArrayLike<T>, mapfn: (v: T, k: number) => U): Maybe<U[]>
export function arrayFrom(arrayLike: any, mapfn?: (v: any, k: number) => any): any {
    if (arrayLike) return Maybe.some(mapfn ? Array.from(arrayLike, mapfn) : Array.from(arrayLike));
    return Maybe.none();
}

export function fuzzysearch(needle, haystack) {
    const hlen = haystack.length;
    const nlen = needle.length;
    if (nlen > hlen) {
        return false;
    }
    const pos = new Array(hlen);
    outer: for (let i = 0, j = 0; i < nlen; i++) {
        const nch = needle.charCodeAt(i);
        while (j < hlen) {
            if (haystack.charCodeAt(j++) === nch) {
                pos[j - 1] = true;
                continue outer;
            }
        }
        return false;
    }
    return pos;
}

export module Dom {
    export const targetHasClass = (classes: string) => (e: Event) => tryOrDefault(() => (e.target as any).classList.value.includes(classes));
    export const targetHasAnchestorWithClass = (classes: string) => (e: Event) => findAnchestorByClass(classes)(e.target as Element);
    export const targetHasOrHasAnchestorWithClass = (classes: string) => (e: Event) => targetHasClass(classes)(e) || targetHasAnchestorWithClass(classes)(e);
    export const prevent = (e: Event) => { e.preventDefault(); e.stopPropagation(); };
    export const hasClass = (classes: string) => (e: Element) => tryOrDefault(() => (e.classList as any).value.includes(classes));
    export const findAnchestorByClass = (cls: string) => (el: Element) => {
        if (!el) return null;
        while ((el = el.parentElement) && !hasClass(cls)(el));
        return el;
    }
}

export module Iteration {
    export type IteratorPredicate = () => boolean;
    export type IteratorIncrementer<T> = () => T;
    export const createIterator: <T>(pred: IteratorPredicate, incr: IteratorIncrementer<T>, v: T) => any = 
                (pred, incr, v) => { return { [Symbol.iterator]: function* () { while(pred()) yield incr(); } }; }
    export const createNumberIterator: (n: number) => any = n => {
        let idx = 0;
        createIterator<number>(() => idx < n, () => { idx = idx++; return idx; }, n);
    };
    export const unfold: (n: number) => number[] = n => [...createNumberIterator(n)];
}
