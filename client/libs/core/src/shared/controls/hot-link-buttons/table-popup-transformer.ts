import { DeferredBuilder } from './../../commons/builder';
import { set, reduce, flow } from 'lodash';
import { Align } from '@progress/kendo-angular-popup';
import { Iterator } from '@progress/kendo-angular-grid/dist/es2015/data/data.iterators';

class Queue<T> {
    private _store: T[] = [];
    push(val: T) { this._store.push(val); }
    pop(): T | undefined { return this._store.shift(); }
}

export class DisplayHelper {
    static readonly backGroundZIndex = '-1';
    static readonly foreGroundZIndex = '10';
    static readonly maxPopupWidth = 800;
    static readonly defaultPopupWidthDelta = 17;

    private static thereIsMoreSpaceToTheLeft(anchorX: number, wholeWidth: number): boolean { return anchorX < (wholeWidth / 2); }
    private static thereIsMoreSpaceToTheTop(anchorY: number, wholeHeight: number): boolean { return anchorY < (wholeHeight / 2); }

    public static getMaxWidth(align: Align, desired: number, windowWidth: number, actualOffset: number): number {
      return align.horizontal === 'left' ?
      (actualOffset + desired) < windowWidth ?
      desired :
      desired - ((actualOffset + desired) - windowWidth + 30) :
      actualOffset - desired > 0 ? 
      desired :
      actualOffset - this.defaultPopupWidthDelta;
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
      return !v ? 0 :
      v.includes(scale) ?
      +v.replace(scale,'') :
      0;
    }

    static getScaledDimension(n: number, scale: string = 'px') : string {
      return JSON.stringify(n) + 'px';
    }

    static getOffsetLeftRec(e: any) : number { return !e ? 0 : e.offsetLeft + this.getOffsetLeftRec(e.offsetParent); }
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
  withRight(value: number): TablePopupTransformer { return this.with('right', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
  withLeft(value: number): TablePopupTransformer { return this.with('left', DisplayHelper.getScaledDimension(value)) as TablePopupTransformer; }
}