/**
 * The Record mixin adds immutability and *with* construct to an object
 * @example
 * @Withable
 * class Sample1 implements Record<Sample1> {
 *     with: (w: Partial<Sample1>) => Sample1;
 *     name = 'sample1'
 * }
 *
 * class Sample2 extends Record(class {
 *     name = 'sample2'
 * }) {}
 *
 * const Sample3 = Record(class {
 *     name = 'sample3'
 * })
 * @author pd
 */

type Constructor<T = {}> = new (...args: any[]) => T;

export interface Record<T> {
    with: (w: Partial<T> | ((s: T) => Partial<T>)) => Readonly<T>;
}

export function Record<T extends object, C extends Constructor>(Base: Constructor<T> & C) {
    return class Rec extends (Base as C) {
        static readonly base = Base;
        static new(a?: Partial<T>): Readonly<Rec & T> {
            if (a) return new Rec().with(a);
            return new Rec() as Readonly<Rec & T>;
        }

        private constructor(...args) {
            super(...args);
        }

        with(a: (Partial<T> | ((s: T) => Partial<T>))): Readonly<Rec & T> {
            if (typeof a === 'function')
                a = a(this as any);
            return Object.assign(new Rec(), this, a) as any;
        }
    }
}
