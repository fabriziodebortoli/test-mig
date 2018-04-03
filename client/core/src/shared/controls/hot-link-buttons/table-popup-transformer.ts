import { DeferredBuilder } from './../../commons/builder';
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

    private static thereIsMoreSpaceToTheLeft(anchorX: number, wholeWidth: number): boolean { return anchorX < (wholeWidth / 2); }
    private static thereIsMoreSpaceToTheTop(anchorY: number, wholeHeight: number): boolean { return anchorY < (wholeHeight / 2); }
    
    public static needsRightMargin(align: Align, e: any): boolean {
      return align.horizontal === 'left' && ((DisplayHelper.getDimension(e.style.left) + DisplayHelper.getDimension(e.style.maxWidth)) >= window.innerWidth);
    }

    public static getAnchorAlign(anchor: any): Align {
      return {
        horizontal: DisplayHelper.thereIsMoreSpaceToTheLeft(anchor.offsetLeft + anchor.offsetWidth, window.innerWidth) ? 
          'left' : 'right',
        vertical: DisplayHelper.thereIsMoreSpaceToTheTop(window.innerHeight - (anchor.offsetTop + anchor.offsetHeight), window.innerHeight) ? 
          'top' : 'bottom'
      } as Align;
    }

    static getPopupAlign(anchorAlign: Align): Align {
        return {
          horizontal: anchorAlign.horizontal,
          vertical: anchorAlign.vertical === 'top' ? 'bottom' : 'top'
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

export class TablePopupTransformer extends DeferredBuilder< HTMLElement, TablePopupTransformer> {
  static On(element: HTMLElement): TablePopupTransformer { return new TablePopupTransformer(element); }
  public get popupElement(): HTMLElement {
    return this.context;
  }
  private constructor(element: HTMLElement) { super(element, undefined); }
  protected doContextSet(key: string, value: any) { set(this.context.style, key, value); }
  withBackGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.backGroundZIndex); }
  withForeGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.foreGroundZIndex); }
  withZIndex(value: string): TablePopupTransformer { return this.with('zIndex', value) as TablePopupTransformer; }
  withMaxWidth(value: number): TablePopupTransformer { return this.with('maxWidth', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withMinWidth(value: number): TablePopupTransformer { return this.with('minWidth', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withMaxHeight(value: number): TablePopupTransformer { return this.with('maxHeight', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withRight(value: number): TablePopupTransformer { return this.with('right', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withLeft(value: number): TablePopupTransformer { return this.with('left', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withAutoFitColumn(): TablePopupTransformer { 
    this.push(() => {
      if(!this.skip){
        let autofitColumnsBtn = ((this.context
          .getElementsByClassName('tb-autofit-columns') as HTMLCollection).item(0) as HTMLElement);
        if (autofitColumnsBtn) autofitColumnsBtn.click(); 
      }
      this.skip = false;
    }); return this; }
  withShowFilters(): TablePopupTransformer {
    this.push(() => {
      if(!this.skip){
        let toggleFiltersBtn = ((this.context.getElementsByClassName('tb-toggle-filters') as HTMLCollection).item(0) as HTMLElement);
        if(!toggleFiltersBtn) return;
        toggleFiltersBtn.click();  
      }
      this.skip = false;
    }); return this;
  }
}

// export class TablePopupTransformer extends Builder< HTMLElement, TablePopupTransformer> {
//     static On(element: HTMLElement, align?: Align): TablePopupTransformer { return new TablePopupTransformer(element, align); }
//     private tQueue: Queue<() => void>;
//     public get popupElement(): HTMLElement {
//       return this.context;
//     }
//     private constructor(element: HTMLElement, align: Align) { super(element, align); this.tQueue = new Queue<() => void>() }
//     protected doContextSet(key: string, value: any) { set(this.context.style, key, value); }
//     protected push(val: () => void): void { if(!this.skip) this.tQueue.push(val); this.skip = false;  }
//     withBackGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.backGroundZIndex); }
//     withForeGroundZIndex(): TablePopupTransformer { return this.withZIndex(DisplayHelper.foreGroundZIndex); }
//     withZIndex(value: string): TablePopupTransformer { this.push(() => { if(this.context) this.with('zIndex', value); }); return this; }
//     withMaxWidth(value: number): TablePopupTransformer { this.push(() => { if(this.context) this.with('maxWidth', DisplayHelper.getScaledDimension(value)); }); return this; }
//     withMinWidth(value: number): TablePopupTransformer { this.push(() => { if(this.context) this.with('minWidth', DisplayHelper.getScaledDimension(value)); }); return this; }
//     withMaxHeight(value: number): TablePopupTransformer { this.push(() => { if(this.context) this.with('maxHeight', DisplayHelper.getScaledDimension(value)); }); return this; }
//     withRight(value: number): TablePopupTransformer { this.push(() => { if(this.context) this.with('right', DisplayHelper.getScaledDimension(value)); }); return this; }
//     withLeft(value: number): TablePopupTransformer { this.push(() => { if(this.context) this.with('left', DisplayHelper.getScaledDimension(value)); }); return this; }
//     withAutoFitColumn(): TablePopupTransformer { 
//       this.push(() => {
//         if(!this.context) return;
//         let autofitColumnsBtn = ((this.context
//           .getElementsByClassName('tb-autofit-columns') as HTMLCollection).item(0) as HTMLElement);
//         if (autofitColumnsBtn) autofitColumnsBtn.click(); 
//       }); return this; }
//     withShowFilters(): TablePopupTransformer {
//       this.push(() => {
//         if(!this.context) return;
//         let toggleFiltersBtn = ((this.context.getElementsByClassName('tb-toggle-filters') as HTMLCollection).item(0) as HTMLElement);
//         if(!toggleFiltersBtn) return;
//         toggleFiltersBtn.click();
//       }); return this;
//     }
      
//     transform() {let t = this.tQueue.pop();
//         while(t) { t(); t = this.tQueue.pop(); }
//     }
// }