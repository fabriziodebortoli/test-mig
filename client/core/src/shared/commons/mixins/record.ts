/**
 * The Record mixin adds immutability and *with* construct to an object
 * @example
 * @Withable
 * class Sample1 implements Withable<Sample1> {
 *     with: (w: Partial<Sample1>) => Sample1;
 *     name = 'sample1'
 * }
 *
 * class Sample2 extends Withable(class {
 *     name = 'sample2'
 * }) {}
 *
 * const Sample3 = Withable(class {
 *     name = 'sample3'
 * })
 * @author pd
 */

type Constructor<T = {}> = new (...args: any[]) => T;

export interface Withable<T> {
    with: (w: Partial<T> | ((s: T) => Partial<T>)) => T;
}

export function Record<T extends object, U extends Constructor>(Base: Constructor<T> & U) {
    return class W extends (Base as U) {
        static new(a?: Partial<T>): W & T {
            if (a) return new W().with(a);
            return new W() as any;
        }

        constructor(...args) {
            super(...args);
        }

        with(a: (Partial<T> | ((s: T) => Partial<T>))): W & T {
            if (typeof a === 'function')
                a = a(this as any);
            return Object.assign(new W(), this, a) as any;
        }
    }
}
