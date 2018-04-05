import { DeferredBuilder } from './../../commons/builder';
import { set, reduce, flow } from 'lodash';
import { Align } from '@progress/kendo-angular-popup';
import { Iterator } from '@progress/kendo-angular-grid/dist/es2015/data/data.iterators';

class Queue<T> {
    private _store: T[] = [];
    push(val: T) { this._store.push(val); }
    pop(): T | undefined { return this._store.shift(); }
}

// export const createIterator: <T>(e: T) => { next: (value: any) => IteratorResult<T> } = (e) => ({ next: (value: any) => {
//   return {done: true, value: null };
// }});

export class DisplayHelper {
    static backGroundZIndex = '-1';
    static foreGroundZIndex = '10';

    private static thereIsMoreSpaceToTheLeft(anchorX: number, wholeWidth: number): boolean { return anchorX < (wholeWidth / 2); }
    private static thereIsMoreSpaceToTheTop(anchorY: number, wholeHeight: number): boolean { return anchorY < (wholeHeight / 2); }

    public static needsRightMargin(align: Align, e: any): boolean {
      return align.horizontal === 'left' && ((e.offsetLeft + 1000) >= window.innerWidth);
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
        return +v.replace(scale,'');
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
}