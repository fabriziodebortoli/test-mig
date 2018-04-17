import { Align } from '@progress/kendo-angular-popup';
import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';
import { untilDestroy } from '../../commons/untilDestroy';
import { Observable, BehaviorSubject } from '../../../rxjs.imports';
import { PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { Builder } from './../../commons/builder';
import { set } from 'lodash';
import { TablePopupTransformer, DisplayHelper } from './table-popup-transformer';


export type SearchOrExitFunction = () => void;
export type OnSearch = () => void;

export class TbHotlinkButtonsPopupHandler {
  static Attach(hlb: TbHotlinkButtonsComponent, onSearch: OnSearch): TbHotlinkButtonsPopupHandler { 
    return new TbHotlinkButtonsPopupHandler(hlb, onSearch);
  }
  
  optionsPopupRef: PopupRef;
  tablePopupRef: PopupRef;
  isTablePopupVisible = true;
  declareTablePopupVisible: () => void = () => { this.isTablePopupVisible = true; } 
  declareTablePopupHidden: () => void = () => { this.isTablePopupVisible = false; } 
  isOptionsPopupVisible = false;
  declareOptionsPopupVisible: () => void = () => { this.isOptionsPopupVisible = true; } 
  declareOptionsPopupHidden: () => void = () => { this.isOptionsPopupVisible = false; } 
  optionOffset: {top: number, left: number};
  getTableHeight: () => number;

  private searchOrExitSubj$ = new BehaviorSubject(false);
  private showOptionsSubj$ = new BehaviorSubject(false);
  get search$(): Observable<boolean> { return this.searchOrExitSubj$.distinctUntilChanged().filter(hasToOpen => hasToOpen).map(_ => true); }
  get exit$(): Observable<boolean> { return this.searchOrExitSubj$.distinctUntilChanged().filter(hasToOpen => !hasToOpen).map(_ => false); }
  get showOptions$() { return this.showOptionsSubj$.filter(hasToOpen => hasToOpen).map(_ => true); }
  get hideOptions$() { return this.showOptionsSubj$.filter(hasToOpen => !hasToOpen).map(_ => false); }
  
  closeOptions() { this.showOptionsSubj$.next(false); } 
  openOptions() { this.showOptionsSubj$.next(true); }
  onHklExit: SearchOrExitFunction;
  onSearch: SearchOrExitFunction = () => this.searchOrExitSubj$.next(true);
  closePopups() { this.closeOptions(); this.onHklExit(); }

  readonly resizeTablePopup: () => void;
  readonly showGridPupup: () => void;
  readonly getHotLinkElement: () => HTMLElement;
  get optionsPopupStyle(): any { return { 'background': 'whitesmoke', 'border': '1px solid rgba(0,0,0,.05)' }; }
  tableHeight: number;
  private constructor (hlb: any, onSearch: OnSearch) {
    this.tableHeight = hlb.state.rows.lenght * 23;
    this.getHotLinkElement = () => (hlb.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
    this.onHklExit = () => { this.closeOptions(); this.searchOrExitSubj$.next(false); hlb.stop(); }
    this.resizeTablePopup = () => TablePopupTransformer.On(this.tablePopupRef.popupElement)
      .withMaxWidth(DisplayHelper.getMaxWidth(DisplayHelper.maxPopupWidth, DisplayHelper.getOffsetLeftRec(hlb.hotLinkButtonTemplate.nativeElement)))
      .build();
    this.showGridPupup = () => {
      let anchorAlign = DisplayHelper.getAnchorAlign(hlb.hotLinkButtonTemplate.nativeElement);
        let popupAlign = DisplayHelper.getPopupAlign(anchorAlign);
        this.tablePopupRef = hlb.tablePopupService.open({
            anchor: hlb.hotLinkButtonTemplate.nativeElement,
            content: hlb.tableTemplate,
            popupAlign: popupAlign,
            anchorAlign: anchorAlign,
            popupClass: 'tb-hotlink-tablePopup',
            appendTo: hlb.vcr});
      this.resizeTablePopup();
      this.declareTablePopupVisible();
    };

    this.search$.pipe(untilDestroy(hlb)).subscribe(_ => onSearch());
      
    this.exit$.pipe(untilDestroy(hlb)).subscribe(_ => {
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



