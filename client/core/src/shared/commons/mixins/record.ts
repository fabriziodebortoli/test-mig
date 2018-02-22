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
    with: (w: Partial<T> | ((s: T) => Partial<T>)) => T;
}

export function Record<T extends object, U extends Constructor>(Base: Constructor<T> & U) {
    return class Record extends (Base as U) {
        static new(a?: Partial<T>): Record & T {
            if (a) return new Record().with(a);
            return new Record() as any;
        }

        constructor(...args) {
            super(...args);
        }

        with(a: (Partial<T> | ((s: T) => Partial<T>))): W & T {
            if (typeof a === 'function')
                a = a(this as any);
            return Object.assign(new Record(), this, a) as any;
        }
    }
}
