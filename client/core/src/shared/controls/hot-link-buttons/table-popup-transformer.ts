import { Builder } from './../../commons/builder';
import { set } from 'lodash';
import { Align } from '@progress/kendo-angular-popup';

class Queue<T> {
    private _store: T[] = [];
    push(val: T) { this._store.push(val); }
    pop(): T | undefined { return this._store.shift(); }
  }

export class DisplayHelper {
    static backGroundZIndex = '-1';
    static foreGroundZIndex = '10';

    static getPopupAlign(anchor: any): Align {
        let Horizontal = 'left';
        let Vertical = 'top';
    
        let height = window.innerHeight;
        let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
        if (height - bottomPoint <= 300) {
          Vertical = 'bottom';
        }
    
        let width = window.innerWidth;
        let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
        if (rightPoint > (width *0.45)) {
          Horizontal = 'right';
        }
        return {
          horizontal: Horizontal,
          vertical: Vertical
        } as Align;
    }

    public static getAnchorAlign(anchor: any): Align {
        let Horizontal = 'left';
        let Vertical = 'bottom';
    
        let height = window.innerHeight;
        let bottomPoint = anchor.offsetTop + anchor.offsetHeight;
        if (height - bottomPoint <= 300) {
          Vertical = 'top';
        }
    
        let width = window.innerWidth;
        let rightPoint = anchor.offsetLeft + anchor.offsetWidth;
        if (rightPoint > (width *0.45)) {
          Horizontal = 'right';
        }
    
        return {
          horizontal: Horizontal,
          vertical: Vertical
        } as Align;
      }

    static getDimension(v: string, scale: string = 'px') : number {
      if(v.includes(scale))
        return  +v.replace(scale,'');
      return 0;
    }

    static getScaledDimension(n: number, scale: string = 'px') : string {
      return JSON.stringify(n) + 'px';
    }
}

export class TablePopupTransformer extends Builder< HTMLElement, TablePopupTransformer> {
    static On(element: HTMLElement, align?: Align): TablePopupTransformer { return new TablePopupTransformer(element, align); }
    private tQueue: Queue<() => void>;
    private constructor(element: HTMLElement, align: Align) { super(element, align); this.tQueue = new Queue<() => void>() }
    protected doContextSet(key: string, value: any) { set(this.context.style, key, value); }
    withBackGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.backGroundZIndex); }
    withForeGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.foreGroundZIndex); }
    withZIndex(value: string): TablePopupTransformer { this.tQueue.push(() => { if(this.context) this.with('zIndex', value); }); return this; }
    withMaxWidth(value: number) : TablePopupTransformer { this.tQueue.push(() => { if(this.context) this.with('maxWidth', DisplayHelper.getScaledDimension(value)); }); return this; }
    withMinWidth(value: number) : TablePopupTransformer { this.tQueue.push(() => { if(this.context) this.with('minWidth', DisplayHelper.getScaledDimension(value)); }); return this; }
    withAddRight(value: number) : TablePopupTransformer { this.tQueue.push(() => { if(this.context) this.with('right', DisplayHelper.getScaledDimension(value)); }); return this; }
    withAddLeft(value: number) : TablePopupTransformer { this.tQueue.push(() => { if(this.context) this.with('left', DisplayHelper.getScaledDimension(value)); }); return this; }
    withAutoFitColumn() : TablePopupTransformer { 
      this.tQueue.push(() => {
        if(!this.context) return;
        let autofitColumnsBtn = ((this.context
          .getElementsByClassName('tb-autofit-columns') as HTMLCollection).item(0) as HTMLElement);
        if (autofitColumnsBtn) autofitColumnsBtn.click(); 
      }); return this; }
    withShowFilters(): TablePopupTransformer {
      this.tQueue.push(() => {
        if(!this.context) return;
        let toggleFiltersBtn = ((this.context.getElementsByClassName('tb-toggle-filters') as HTMLCollection).item(0) as HTMLElement);
        if(!toggleFiltersBtn) return;
        toggleFiltersBtn.click();
      }); return this;
    }
      
    transform() {let t = this.tQueue.pop();
        while(t) {
            console.log('PRE ' + this.context.style.zIndex);
            console.log(t);
            t(); 
            console.log('POST ' + this.context.style.zIndex);
            t = this.tQueue.pop(); }
    }
}