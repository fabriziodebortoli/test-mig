import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { DocumentService } from './../../../core/services/document.service';
import { EnumsService } from './../../../core/services/enums.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { LayoutService } from './../../../core/services/layout.service';
import { TbHotLinkBaseComponent } from './../hot-link-base/tb-hot-link-base.component';
import { HttpService } from './../../../core/services/http.service';
import { OnDestroy, OnInit, Component, Input, ViewContainerRef, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { URLSearchParams } from '@angular/http';
import { PageChangeEvent } from '@progress/kendo-angular-grid';
import { FilterDescriptor, CompositeFilterDescriptor } from '@progress/kendo-data-query';
import { PopupService, PopupSettings, PopupRef } from '@progress/kendo-angular-popup';
import { BehaviorSubject, Subscription, Observable } from '../../../rxjs.imports';
import { PaginatorService, ServerNeededParams } from '../../../core/services/paginator.service';
import { FilterService, combineFilters, combineFiltersMap } from '../../../core/services/filter.services';
import { HyperLinkService, HyperLinkInfo} from '../../../core/services/hyperlink.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { HlComponent, HotLinkState } from './../hot-link-base/hotLinkTypes';
import * as _ from 'lodash';

declare var document:any;

@Component({
  selector: 'tb-hotlink-combo',
  templateUrl: './tb-hot-link-combo.component.html',
  styleUrls: ['./tb-hot-link-combo.component.scss'],
  providers: [PaginatorService, FilterService, HyperLinkService]
})
export class TbHotlinkComboComponent extends TbHotLinkBaseComponent implements OnDestroy, OnInit, AfterViewInit {
  
  constructor(layoutService: LayoutService,
    protected httpService: HttpService,
    protected documentService: DocumentService,
    protected enumService: EnumsService,
    protected changeDetectorRef: ChangeDetectorRef,
    protected eventDataService: EventDataService,
    protected paginator: PaginatorService,
    protected filterer: FilterService,
    protected hyperLinkService: HyperLinkService,
    protected optionsPopupService: PopupService,
    protected tablePopupService: PopupService,
    protected vcr: ViewContainerRef
  ) {
    super(layoutService, documentService, changeDetectorRef, paginator, filterer, hyperLinkService, eventDataService);
  }

  private dropDownOpened = false;
  openDropDown() {
    this.start();
    this.dropDownOpened = true;
  }

  closeDropDown() {
    this.stop();
    this.dropDownOpened = false;
  }

  protected start() {
    this.defaultPageCounter = 0;
    this.filterer.start(200);
    this.paginator.start(1, this.pageSize,
      Observable
      .combineLatest(this.slice$, this.filterer.filterChanged$, this.filterer.sortChanged$,
        (slice, filter, sort) => ({ model: slice, customFilters: filter, customSort: sort})),
      (pageNumber, serverPageSize, args) => {
        let ns = this.hotLinkInfo.namespace;
        if (!ns && args.model.selector && args.model.selector !== '') { 
          ns = args.model.items[args.model.selector].namespace;
        }
        this.currentHotLinkNamespace = ns;
        let p: URLSearchParams = new URLSearchParams();
        p.set('filter', JSON.stringify(args.model.value));
        p.set('documentID', (this.tbComponentService as DocumentService).mainCmpId);
        p.set('hklName', this.hotLinkInfo.name);
        if (args.customFilters && args.customFilters.logic && args.customFilters.filters && args.customFilters.field)
          p.set('customFilters', JSON.stringify(args.customFilters));
        if (args.customSort)
          p.set('customSort', JSON.stringify(args.customSort));
        p.set('disabled', '0');
        p.set('page', JSON.stringify(pageNumber + 1));
        p.set('per_page', JSON.stringify(serverPageSize));
        return this.httpService.getHotlinkData(ns, this.state.selectionType,  p);
      });

    this.paginator.clientData.subscribe((d) => {
        this.state = {...this.state, selectionColumn: d.key, gridData: { data: d.rows, total: d.total, columns: d.columns} };        
    });

    this.filterer.filterChanged$.filter(x => x.logic !== undefined)
      .subscribe(x => { 
      if (this.modelComponent && this.modelComponent.model) {
          this.modelComponent.model.value = _.get(x, 'filters[0].value');
          this.emitModelChange();
      }
    });
  }

  private _filter: CompositeFilterDescriptor;
    public get filter(): CompositeFilterDescriptor {
        return this._filter;
    }

    public set filter(value: CompositeFilterDescriptor) {
        this._filter = _.cloneDeep(value);
        this.filterer.filter = _.cloneDeep(value);
        this.filterer.onFilterChanged(value);
    }
    
    protected async pageChange(event: PageChangeEvent) {
        await this.paginator.pageChange(event.skip, event.take);
    }
    
    protected async nextDefaultPage() {
        this.defaultPageCounter++;
        await this.paginator.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
    }
    
    protected async prevDefaultPage() {
        this.defaultPageCounter--;
        await this.paginator.pageChange(this.defaultPageCounter * this.pageSize, this.pageSize);
    }
    
    protected async firstDefaultPage() {
        this.defaultPageCounter = 0;
        await this.paginator.pageChange(0, this.pageSize);
    }

  public onFilterChange(filter: string): void {
    if (this.dropDownOpened) {
      if(filter === '' || !filter) this.filter = {logic: 'and', filters: []};
      else this.filter = {logic: 'and', filters: [{field: this.state.selectionColumn, operator: 'contains', value: filter}]};
    } 
  }

  selectionChanged(value: any) {
    _.set(this.eventDataService.model, this.hotLinkInfo.name + '.Description.value', _.get(value, 'displayString'));
    if (this.modelComponent && this.modelComponent.model) {
      this.modelComponent.model.value =  _.get(value, 'id');
      this.emitModelChange();
    }
  }

  private _cbInfo: {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}
  get comboBoxInfo (): {element: HTMLElement, clickSubscription?: Subscription, initInfo: { color: string, textDecoration: string, cursor: string, pointerEvents: string }}  {
    if(this._cbInfo) return this._cbInfo;

    let cb: HTMLElement;
    cb = (this.vcr.element.nativeElement.parentNode.getElementsByClassName('k-searchbar') as HTMLCollection).item(0) as HTMLElement;
    if(!cb) return undefined;
    let oldColor = cb.style.color;
    let oldDecoration = cb.style.textDecoration;
    let oldCursor = cb.style.cursor;
    let oldPointerEvents = cb.style.pointerEvents;
    this._cbInfo = { 
        element: cb,
        initInfo: {
          color: oldColor,
          textDecoration: oldDecoration,
          cursor: oldCursor,
          pointerEvents: oldPointerEvents
        }
    };
    return this._cbInfo;
  }

  ngAfterViewInit(): void {
    if (this.comboBoxInfo){
        this.slice$.pipe(untilDestroy(this)).subscribe(x => {
          if(!x.enabled && x.value) { 
            this.comboBoxInfo.element.style.textDecoration = 'underline'; 
            this.comboBoxInfo.element.style.color = 'blue';
            this.comboBoxInfo.element.style.cursor = 'pointer';
            this.comboBoxInfo.element.style.pointerEvents = 'all';
            this.comboBoxInfo.clickSubscription = Observable.fromEvent(document, 'click', { capture: false })
              .filter(e => this.comboBoxInfo &&  (e as any) && (e as any).target === this.comboBoxInfo.element)
              .subscribe(e => this.hyperLinkService.follow({ns: this.hotLinkInfo.name, cmpId: this.documentService.mainCmpId }));
          } else {
            this.comboBoxInfo.element.style.textDecoration = this.comboBoxInfo.initInfo.textDecoration;
            this.comboBoxInfo.element.style.color = this.comboBoxInfo.initInfo.color;
            this.comboBoxInfo.element.style.cursor = this.comboBoxInfo.initInfo.cursor;
            this.comboBoxInfo.element.style.pointerEvents = this.comboBoxInfo.initInfo.pointerEvents;
            if(this.comboBoxInfo.clickSubscription)
              this.comboBoxInfo.clickSubscription.unsubscribe();
          } 
        });
      }
  }

  ngOnInit() {
    this.state = {...this.state, selectionType: 'combo'};
  }

  ngOnDestroy() {
  }
}