import { Component, OnInit, ViewChild, ElementRef, HostListener, Input } from '@angular/core';
import { EventDataService } from '../../../core/services/eventdata.service';
import { ComponentMediator } from '../../../core/services/component-mediator.service';
import { StorageService } from '../../../core/services/storage.service';
import { ControlComponent } from '../../../shared/controls/control.component';
import ExplorerEventHandler from './explorer.event-handler';
import { ExplorerService, Item, ObjType, UploadInterceptor } from '../../../core/services/explorer.service';
import { Observable, BehaviorSubject } from '../../../rxjs.imports';
import { get } from 'lodash';

const rootItem: Item = { name: 'Explorer', namespace: '', level: 0 };

@Component({
  selector: 'tb-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
  providers: [ComponentMediator, StorageService]
})
export class ExplorerComponent extends ControlComponent implements OnInit {
  @Input() saveUrl = 'mockInsertUrl';
  @Input() removeUrl = 'mockRemoveUrl';
  @Input() objectType: ObjType = ObjType.Document;
  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('menuPopup', { read: ElementRef }) public menuPopup: ElementRef;
  @ViewChild('searchPopup', { read: ElementRef }) public findPopup: ElementRef;
  items: Item[];
  get filteredItems(): Item[] {
    return this.items.filter(i => i.name.toLowerCase().includes(this.search.toLowerCase()));
  }
  search: string;
  private _currentItem: Item;
  get currentItem(): Item { return this._currentItem; }
  currentPath: string[];
  path: Item[] = [rootItem];
  showMenu = false;
  showSearch = false;
  baseUsers = ['All', localStorage.getItem('_user')];
  otherUsers = [];
  isCustom = false;
  get users() { return [...this.baseUsers, ...this.otherUsers]; }

  constructor(public m: ComponentMediator, public explorer: ExplorerService) {
    super(m.layout, m.tbComponent, m.changeDetectorRef);
    ExplorerEventHandler.handle(this);
  }

  async ngOnInit() { this.updateItems(rootItem); }
  breadClick(item: Item) { this.updateItems(item); }
  itemClick(item: Item) { this.updateItems(item); }
  close() { }
  customLevelChanged() { this.updateItems(this.currentItem); }
  refresh() { this.updateItems(this.currentItem); }

  async updateItems(item: Item) {
    switch (item.level) {
      case 1:
        this.items = await this.explorer.GetModules(item);
        this.path = [rootItem, item];
        this._currentItem = item;
        this.search = '';
        break;
      case 2:
        this.items = await this.explorer.GetObjs(this.path[1], item, this.objectType);
        this.path = [rootItem, item.parent, item];
        this._currentItem = item;
        this.search = '';
        break;
      case 3: return;
      default:
        this.items = await this.explorer.GetApplications();
        this.path = [rootItem];
        this._currentItem = rootItem;
        this.search = '';
    }
  }

  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (this.showMenu && !this.anchor.nativeElement.contains(event.target) &&
      !this.menuPopup.nativeElement.contains(event.target))
      this.showMenu = false;
    if (this.showSearch && !this.anchor.nativeElement.contains(event.target) &&
      !this.findPopup.nativeElement.contains(event.target))
      this.toggleSearch();
  }

  public toggleSearch() {
    if (this.showMenu) this.toggleMenu();
    this.showSearch = !this.showSearch;
  }

  public toggleMenu() {
    if (this.showSearch) this.toggleSearch();
    this.showMenu = !this.showMenu;
  }
}
