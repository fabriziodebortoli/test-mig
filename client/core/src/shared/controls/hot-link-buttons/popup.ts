import { Align } from '@progress/kendo-angular-popup';
import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';
import { untilDestroy } from '../../commons/untilDestroy';
import { Observable, BehaviorSubject } from '../../../rxjs.imports';
import { PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { Builder } from './../../commons/builder';
import { set } from 'lodash';
import { TablePopupTransformer, DisplayHelper } from './table-popup-transformer';


export type OpenCloseTableFunction = () => void;
export type OnTablePopupOpen = () => void;

export class TbHotlinkButtonsPopupHandler {
  static Attach(hlb: TbHotlinkButtonsComponent, onTablePopupOpen: OnTablePopupOpen): TbHotlinkButtonsPopupHandler { 
    return new TbHotlinkButtonsPopupHandler(hlb, onTablePopupOpen);
  }
  
  optionsPopupRef: PopupRef;
  tablePopupRef: PopupRef;
  isTablePopupVisible = false;
  declareTablePopupVisible: () => void = () => { this.isTablePopupVisible = true; } 
  declareTablePopupHidden: () => void = () => { this.isTablePopupVisible = false; } 
  isOptionsPopupVisible = false;
  declareOptionsPopupVisible: () => void = () => { this.isOptionsPopupVisible = true; } 
  declareOptionsPopupHidden: () => void = () => { this.isOptionsPopupVisible = false; } 
  optionOffset: {top: number, left: number};

  private showTableSubj$ = new BehaviorSubject(false);
  private showOptionsSubj$ = new BehaviorSubject(false);
  get showTable$(): Observable<boolean> { return this.showTableSubj$.distinctUntilChanged().filter(hasToOpen => hasToOpen).map(_ => true); }
  get hideTable$(): Observable<boolean> { return this.showTableSubj$.distinctUntilChanged().filter(hasToOpen => !hasToOpen).map(_ => false); }
  get showOptions$() { return this.showOptionsSubj$.filter(hasToOpen => hasToOpen).map(_ => true); }
  get hideOptions$() { return this.showOptionsSubj$.filter(hasToOpen => !hasToOpen).map(_ => false); }
  
  closeOptions() { this.showOptionsSubj$.next(false); } 
  openOptions() { this.showOptionsSubj$.next(true); }
  closeTable: OpenCloseTableFunction;
  openTable: OpenCloseTableFunction = () => this.showTableSubj$.next(true);
  closePopups() { this.closeOptions(); this.closeTable(); }

  readonly setPopupElementInBackground: (align: Align) => void; 
  readonly setPopupElementInForeground: () => void;
  readonly showGridPupup: () => void;
  readonly getHotLinkElement: () => HTMLElement;
  get optionsPopupStyle(): any { return { 'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)' }; }

  private constructor (hlb: any, onTablePopupOpen: OnTablePopupOpen) {
    this.getHotLinkElement = () => (hlb.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
    this.closeTable = () => { this.closeOptions(); this.showTableSubj$.next(false); hlb.stop(); }
    this.setPopupElementInBackground = (align: Align) => TablePopupTransformer.On(this.tablePopupRef.popupElement, align)
    .withBackGroundZIndex()
      .transform();
    this.setPopupElementInForeground = () => TablePopupTransformer.On(this.tablePopupRef.popupElement)
      .withForeGroundZIndex().transform();
    this.showGridPupup = () => {
      this.setPopupElementInForeground();
      this.declareTablePopupVisible();
    };

    this.showTable$.pipe(untilDestroy(hlb)).subscribe(_ => {
        let popupAlign = DisplayHelper.getPopupAlign(hlb.hotLinkButtonTemplate.nativeElement);
        let anchorAlign = DisplayHelper.getAnchorAlign(hlb.hotLinkButtonTemplate.nativeElement);
        this.tablePopupRef = hlb.tablePopupService.open({
            anchor: hlb.hotLinkButtonTemplate.nativeElement,
            content: hlb.tableTemplate,
            popupAlign: popupAlign,
            anchorAlign: anchorAlign,
            popupClass: 'tb-hotlink-tablePopup',
            appendTo: hlb.vcr});
        console.log(this.tablePopupRef.popupElement.style);
        this.setPopupElementInBackground(popupAlign);
        console.log(this.tablePopupRef.popupElement.style);
        this.tablePopupRef.popupOpen.asObservable().subscribe(_ => onTablePopupOpen());
    });
      
    this.hideTable$.pipe(untilDestroy(hlb)).subscribe(_ => {
      if (this.tablePopupRef) {
          this.tablePopupRef.close();
          this.tablePopupRef = null;
      }
      this.getHotLinkElement().focus();
      this.declareTablePopupHidden();
      hlb.restoreHotLinkIconToDefault();
    });
      
    this.showOptions$.pipe(untilDestroy(hlb)).subscribe(_ => {
      this.optionsPopupRef = hlb.optionsPopupService.open({
        content: hlb.optionsTemplate,
        appendTo: hlb.vcr,
        offset: this.optionOffset
      });
      this.optionsPopupRef.popupOpen.asObservable()
      .do(_ => this.declareOptionsPopupVisible())
      .subscribe(_ => hlb.loadOptions() );
    });
        
    this.hideOptions$.pipe(untilDestroy(hlb)).subscribe(_ => {
        if (this.optionsPopupRef) { this.optionsPopupRef.close(); }
        this.optionsPopupRef = null;
        this.declareOptionsPopupHidden();
    });
  }
}



