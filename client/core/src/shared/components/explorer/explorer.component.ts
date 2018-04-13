import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  HostListener,
  Input,
  trigger,
  state,
  style,
  transition,
  animate,
  Output,
  EventEmitter
} from '@angular/core';
import { EventDataService } from '../../../core/services/eventdata.service';
import { ComponentMediator } from '../../../core/services/component-mediator.service';
import { StorageService } from '../../../core/services/storage.service';
import { ControlComponent } from '../../../shared/controls/control.component';
import { ExplorerEventHandler } from './explorer.event-handler';
import {
  ExplorerService,
  Item,
  ObjType,
  rootItem
} from '../../../core/services/explorer.service';
import { Observable, BehaviorSubject, Subject } from '../../../rxjs.imports';
import { Maybe } from '../../commons/monads/maybe';
import { fuzzysearch } from '../../commons/u';
import { get, cloneDeep } from 'lodash';

const objTypeToFileExtension = {
  [ObjType.Report]: ['.wrm'],
  [ObjType.Image]: ['.png', '.jpg', '.jpeg', '.tif', '.tiff']
};

export class ExplorerOptions {
  objType: ObjType = ObjType.Document;
  startNamespace?: string;
  upload?: boolean;
  uploadUrl?: string;
  initialNamespace?: string;
  uploadRestrictions?: {
    maxFileSize?: number;
    allowedExtensions?: string[];
  };
  constructor(opt?: Partial<ExplorerOptions>) {
    Object.assign(this, cloneDeep(opt));
  }
}

export class ExplorerItem {
  constructor(public name: string, public namespace: string) {}
}

export class ExplorerResult {
  constructor(public items?: ExplorerItem[]) {}
}

export const ViewStates = { opened: 'opened', closed: 'closed' };

@Component({
  selector: 'tb-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
  providers: [ComponentMediator, StorageService],
  animations: [
    trigger('shrinkOut', [
      state(ViewStates.opened, style({ height: '*' })),
      state(ViewStates.closed, style({ height: 0, overflow: 'hidden' })),
      transition(
        ViewStates.opened + ' <=> ' + ViewStates.closed,
        animate('150ms ease-in-out')
      )
    ])
  ]
})
export class ExplorerComponent extends ControlComponent implements OnInit {
  private _search: string;
  private _currentItem: Item;
  private _options = new ExplorerOptions();
  @Input()
  set options(value: ExplorerOptions) {
    this._options = value;
  }
  get options(): ExplorerOptions {
    if (!this._options.uploadUrl)
      this._options.uploadUrl =
        this.explorer.info.getBaseUrl() + '/tbfs-service/UploadObject';
    if (!this._options.uploadRestrictions)
      this._options.uploadRestrictions = {
        maxFileSize: 4194304,
        allowedExtensions: objTypeToFileExtension[this._options.objType]
      };
    return this._options;
  }
  @Output() selectionChanged = new EventEmitter<ExplorerItem>();
  @Output() stateChanged = new EventEmitter<string>();
  @Output() itemClick = new EventEmitter<Item>();
  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('menuPopup', { read: ElementRef })
  public menuPopup: ElementRef;
  @ViewChild('searchPopup', { read: ElementRef })
  public findPopup: ElementRef;
  @ViewChild('uploader') public uploader: any;
  get filteredItems(): Item[] {
    // TODOPD: memoize
    return (
      (this.items &&
        this.items.filter(
          i =>
            (i['match'] = fuzzysearch(
              this.search.toLowerCase(),
              i.name.toLowerCase()
            ))
        )) ||
      []
    );
  }
  get search() {
    return this._search;
  }
  set search(v: string) {
    this._search = v;
    this.selectedItem = null;
  }
  get currentItem(): Item {
    return this._currentItem;
  }
  items: Item[];
  viewState = ViewStates.closed;
  selectedItem: Item;
  currentPath: string[];
  path: Item[] = [rootItem];
  showMenu = false;
  showSearch = false;
  baseUsers = ['All', localStorage.getItem('_user')];
  otherUsers = [];
  isCustom = false;
  saveHeaders;
  get users() {
    return [...this.baseUsers, ...this.otherUsers];
  }

  constructor(
    public m: ComponentMediator,
    public explorer: ExplorerService,
    private elRef: ElementRef
  ) {
    super(m.layout, m.tbComponent, m.changeDetectorRef);
    ExplorerEventHandler.handle(this);
  }

  async ngOnInit() {
    this.saveHeaders = this.explorer.getHeaders();
    this.updateItemsInside(rootItem);
  }

  breadClick(item: Item) {
    this.updateItemsInside(item);
  }

  _itemClick(item: Item) {
    this.updateItemsInside(item);
    this.itemClick.emit(item);
  }

  customLevelChanged() {
    this.updateItemsInside(this.currentItem);
  }

  refresh() {
    this.updateItemsInside(this.currentItem);
  }

  close() {}

  select(item: Item) {
    this.selectionChanged.emit(new ExplorerItem(item.name, item.namespace));
  }

  async updateItemsInside(item: Item): Promise<void> {
    if (item.level === 3) return;
    (await this.getItemsInside(item)).map(items => {
      this.items = items;
      this.path = this.getPathFor(item);
      this._currentItem = item;
      this.search = '';
      this.viewState = item.level === 2 ? ViewStates.opened : ViewStates.closed;
      this.clearUploadList();
    });
  }

  async getItemsInside(item: Item) {
    switch (item.level) {
      case 0:
        return this.explorer.GetApplications();
      case 1:
        return this.explorer.GetModules(item);
      case 2:
        return this.explorer.GetObjs(this.path[1], item, this.options.objType);
    }
  }

  getPathFor(item: Item) {
    const res = [];
    while (item) {
      res.push(item);
      item = item.parent;
    }
    return res.reverse();
  }

  @HostListener('document:click', ['$event'])
  documentClick(event: any): void {
    if (
      this.showMenu &&
      !this.anchor.nativeElement.contains(event.target) &&
      !this.menuPopup.nativeElement.contains(event.target)
    )
      this.showMenu = false;
    if (
      this.showSearch &&
      !this.anchor.nativeElement.contains(event.target) &&
      !this.findPopup.nativeElement.contains(event.target)
    )
      this.toggleSearch();
  }

  async toggleSearch(force?: boolean, startingText?: string) {
    if (this.showMenu) this.toggleMenu();
    if (!this.showSearch && typeof startingText !== 'undefined')
      this.search += startingText;
    this.showSearch = force || !this.showSearch;
    await Observable.timer(0).toPromise();
    this.focus();
  }

  focus() {
    if (this.showSearch) {
      const el = this.elRef.nativeElement.querySelector(
        'kendo-popup [kendotextbox]'
      );
      if (el) el.focus();
    } else this.elRef.nativeElement.querySelector('.main').focus();
  }

  toggleMenu(force?: boolean) {
    if (this.showSearch) this.toggleSearch();
    this.showMenu = force || !this.showMenu;
  }

  clearUploadList() {
    this.uploader && this.uploader.fileList && this.uploader.fileList.clear();
  }

  uploadEventHandler(e) {
    e.data = { currentNamespace: this.currentItem.namespace };
  }

  successEventHandler(e) {
    this.refresh();
  }

  errorEventHandler(e) {}
}
