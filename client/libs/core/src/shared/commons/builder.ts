import { URLSearchParams } from '@angular/http';
import { get, set } from 'lodash';

const getString: (value: any) => string = value => (typeof value === 'string') ? value as string : JSON.stringify(value);

export class IfContext<T>  {
    builder: T;
    static New<T, C extends T> (builder: T): IfContext<T> { return new IfContext<T>(builder); }
    constructor(builder: T) {
        this.builder = builder; 
    }
    isDefinedInSource(key: string): IfContext<T> {
        let b = this.builder as any;
        if(!b.skip && get(b.sourceContext, key) === undefined || get(b.sourceContext, key) === null)
            b.skip = true;
        return this;
    }
    isTrue(pred: (builder: T) => boolean) {
        let b = this.builder as any;
        if(!b.skip && !pred(b))
            b.skip = true;
        return this;
    }
    then(): T { return this.builder } 
}

export abstract class Builder<T, U extends Builder<T, U>> {
    protected context: T;
    protected sourceContext: any;
    protected skip: boolean = false;
    protected abstract doContextSet(key: string, value: any);
    public doSourceContextGet: (key: string) => any = (key) => get(this.sourceContext, key);
    protected constructor(context: T,sourceContext: any) { this.context = context; this.sourceContext = sourceContext;}
    if(): IfContext<U> { return IfContext.New(this as any as U); }
    with(key: string, value: any): Builder<T, U> { if (!this.skip) this.doContextSet(key, value); this.skip = false; return this; }
    build(): T { return this.context;}
}

export class URLSearchParamsBuilder extends Builder<URLSearchParams, URLSearchParamsBuilder> {
    static Create(sourceContext: any): URLSearchParamsBuilder { return new URLSearchParamsBuilder(sourceContext); }
    private constructor(sourceContext: any) { super(new URLSearchParams(), sourceContext); }
    protected doContextSet(key: string, value: any) { this.context.set(key, getString(value)); }
    withFilter(value: any) : URLSearchParamsBuilder { return this.with('filter', value) as URLSearchParamsBuilder; }
    withDocumentID(value: any): URLSearchParamsBuilder { return this.with('documentID', value) as URLSearchParamsBuilder; }
    withName(value: any): URLSearchParamsBuilder { return this.with('hklName', value) as URLSearchParamsBuilder; }
    withCustomFilters(value: any): URLSearchParamsBuilder { return this.with('customFilters', value) as URLSearchParamsBuilder; }
    withCustomSort(value: any): URLSearchParamsBuilder { return this.with('customSort', value) as URLSearchParamsBuilder; }
    withDisabled(value: any): URLSearchParamsBuilder { return this.with('disabled', value) as URLSearchParamsBuilder; }
    withPage(value: any): URLSearchParamsBuilder { return this.with('page', value) as URLSearchParamsBuilder; }
    withRowsPerPage(value: any): URLSearchParamsBuilder { return this.with('per_page', value) as URLSearchParamsBuilder; } 
}

class BuilderQueue<T> {
    private _store: T[] = [];
    push(val: T) { this._store.push(val); }
    pop(): T | undefined { return this._store.shift(); }
}

export class DeferredIfContext<T>  {
    builder: T;
    static New<T, C extends T> (builder: T): DeferredIfContext<T> { return new DeferredIfContext<T>(builder); }
    constructor(builder: T) {
        this.builder = builder; 
    }
    isTrue(pred: (builder: T) => boolean) {
        let b = this.builder as any;
        b.push(() => { 
            if(!b.skip && !pred(b))
                b.skip = true;
        });
        return this;
    }
    then(): T { return this.builder; } 
}

export abstract class DeferredBuilder<T, U extends DeferredBuilder<T, U>> {
    protected context: T;
    protected sourceContext: any;
    protected skip: boolean = false;
    private tQueue: BuilderQueue<() => void>;
    protected push(val: () => void): void { this.tQueue.push(val); }
    protected abstract doContextSet(key: string, value: any);
    public doSourceContextGet: (key: string) => any = (key) => get(this.sourceContext, key);
    protected constructor(context: T,sourceContext: any) { 
        this.context = context;
        this.sourceContext = sourceContext;
        this.tQueue = new BuilderQueue<() => void>();
    }
    if(): DeferredIfContext<U> { return DeferredIfContext.New(this as any as U); }
    with(key: string, value: any): DeferredBuilder<T, U> {  
        this.push(() => { 
            if (!this.skip)
                this.doContextSet(key, value);
            this.skip = false;
         });
        return this; 
    }
    build() { 
        let t = this.tQueue.pop();
        while(t) {
            t(); 
            t = this.tQueue.pop();
        }
    }
}
