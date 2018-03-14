import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation, AfterViewInit, AfterContentInit, ViewContainerRef, ChangeDetectionStrategy, Output, EventEmitter, ChangeDetectorRef, HostListener, ElementRef } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { Subscription } from "rxjs/Rx";

import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { ComponentService } from './../../../../core/services/component.service';
import { SettingsService } from './../../../../core/services/settings.service';
import { SidenavService } from './../../../../core/services/sidenav.service';
import { UtilsService } from './../../../../core/services/utils.service';
import { ImageService } from './../../../services/image.service';
import { MenuService } from './../../../services/menu.service';

import { TbComponent } from '../../../../shared/components/tb.component';

@Component({
  selector: 'tb-menu-container',
  templateUrl: './menu-container.component.html',
  styleUrls: ['./menu-container.component.scss'],
  animations: [
    trigger('collapsing', [
      state('expanded', style({ width: '260px', overflow: 'hidden' })),
      state('collapsed', style({ width: '40px', overflow: 'hidden' })),
      transition('expanded <=> collapsed', animate('200ms ease')),
    ])
  ]
})

export class MenuContainerComponent extends TbComponent implements AfterContentInit, OnDestroy {

  public subscriptions: Subscription[] = [];
  public selectedGroupChangedSubscription;
  public tiles: any[] = [];

  public tileClass: string = "tile-1";

  public selectorCollapsed: string = localStorage.getItem('menuSelectorCollapsed') ? localStorage.getItem('menuSelectorCollapsed') : 'expanded';
  public appActive: any;
  public groupActive: any;
  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  @ViewChild('tabber') tabber;
  @ViewChild('menuContent') menuContent;

  public showHiddenTilesPopup: boolean = false;
  @ViewChild('hiddenTilesAnchor') public hiddenTilesAnchor: ElementRef;
  @ViewChild('hiddenTilesPopup', { read: ElementRef }) public hiddenTilesPopup: ElementRef;

  // ---------------------------------------------------------------------------------------
  @HostListener('keydown', ['$event'])
  public keydown(event: any): void {
    if (event.keyCode === 27) {
      this.closeHiddenTilesPopup();
    }
  }

  // ---------------------------------------------------------------------------------------
  @HostListener('document:click', ['$event'])
  public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      this.closeHiddenTilesPopup();
    }
  }

  // ---------------------------------------------------------------------------------------
  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.calcTileClass();
  }
  /** 
   * Modifica la css class delle tile del menu in base alla larghezza del contenitore del menu.
   * Viene richiamata sul resize della finestra, ogni volta che viene collassato/espanso il selettore di app e gruppi,
   * ogni volta che viene espansa/collassata oppure pinnata/unpinnata la sidenav.
   * Il timeout di 250ms è necessario perché la dimensione del contenitore può essere calcolata solo alla fine dell'animazione di collapsing.
  */
  calcTileClass(time: number = 250) {
    setTimeout(() => {
      let menuContentWidth = this.menuContent ? this.menuContent.nativeElement.offsetWidth : 1024;
      //console.log("menuContentWidth", menuContentWidth)

      if (menuContentWidth > 768 && menuContentWidth <= 1200)
        this.tileClass = "tile-2";
      else if (menuContentWidth > 1200 && menuContentWidth <= 1440)
        this.tileClass = "tile-3";
      else if (menuContentWidth >= 1440 && this.tiles.length > 3)
        this.tileClass = "tile-4";
      else if (menuContentWidth >= 1440 && this.tiles.length < 4)
        this.tileClass = "tile-3";
      else
        this.tileClass = "tile-1";
    }, time);
  }

  // ---------------------------------------------------------------------------------------
  private contains(target: any): boolean {
    return (this.hiddenTilesAnchor ? this.hiddenTilesAnchor.nativeElement.contains(target) : false) ||
      (this.hiddenTilesPopup ? this.hiddenTilesPopup.nativeElement.contains(target) : false);
  }

  // ---------------------------------------------------------------------------------------
  closeHiddenTilesPopup() {
    this.showHiddenTilesPopup = false;
  }

  // ---------------------------------------------------------------------------------------
  toggleHiddenTilesPopup() {
    this.showHiddenTilesPopup = !this.showHiddenTilesPopup;
  }

  constructor(
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public imageService: ImageService,
    public sidenavService: SidenavService,
    public componentService: ComponentService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);

    this.subscriptions.push(this.menuService.menuActivated.subscribe(() => {
      this.tiles = this.getTiles();
      this.changeTabWhenMenuChanges();
      this.refreshLayout();
    }));

    this.subscriptions.push(this.menuService.selectedMenuChanged.subscribe(() => {
      this.tiles = this.getTiles();
      this.changeTabWhenMenuChanges();
    }));

    this.subscriptions.push(this.menuService.selectedGroupChanged.subscribe(() => {
      this.initTab();
    }));

    this.subscriptions.push(this.sidenavService.sidenavOpened$.subscribe((o) => this.calcTileClass()));
    this.subscriptions.push(this.sidenavService.sidenavPinned$.subscribe((o) => this.calcTileClass()));
    this.subscriptions.push(this.componentService.componentInfoRemoved.subscribe((o) => this.calcTileClass()));
  }

  getSelectorIcon() {
    return this.selectorCollapsed ? 'tb-circledrightfilled' : 'tb-gobackfilled';
  }

  toggleSelector() {
    this.selectorCollapsed = this.selectorCollapsed === 'expanded' ? 'collapsed' : 'expanded';
    localStorage.setItem('menuSelectorCollapsed', this.selectorCollapsed);
    this.calcTileClass();
  }

  refreshLayout() {
  }

  ngAfterContentInit() {
    this.calcTileClass()
  }

  initTab() {

    if (this.menuService.selectedGroup == undefined) {
      return;
    }

    let found = false;
    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      for (let i = 0; i < tempMenuArray.length; i++) {
        if (tempMenuArray[i].name.toLowerCase() == this.settingsService.LastMenuName.toLowerCase()) {
          this.menuService.setSelectedMenu(tempMenuArray[i]);
          return;
        }
      }
    }

    if (!found) {
      this.menuService.setSelectedMenu(tempMenuArray[0]);
    }

  }

  isSelected(tab) {
    if (this.menuService.selectedMenu == undefined || tab == undefined)
      return false;
    return tab.title == this.menuService.selectedMenu.title;
  }

  changeTabWhenMenuChanges() {
    if (this.menuService.selectedMenu == undefined)
      return;

    let idx = this.findTabIndexByMenu();
    this.tabber.selectTab(idx);
  }

  findTabIndexByMenu(): number {
    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      for (let i = 0; i < tempMenuArray.length; i++) {
        if (tempMenuArray[i].title == this.menuService.selectedMenu.title)
          return i;
      }
    }
    return -1;
  }

  ngOnDestroy() {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }

  changeTabByIndex(event) {
    let index = event.index;
    if (index < 0 || this.menuService.selectedGroup == undefined)
      return;

    let tempMenuArray = this.menuService.selectedGroup.Menu;
    if (tempMenuArray) {
      let tab = tempMenuArray[index];
      if (tab != undefined) {
        this.menuService.setSelectedMenu(tab);
      }
    }

    this.calcTileClass(0);
  }

  getTiles() {
    if (this.menuService.selectedMenu) {
      let array = this.menuService.selectedMenu.Menu;
      let newArray = [];
      if (array) {
        for (let i = 0; i < array.length; i++) {
          if (this.tileIsVisible(array[i]) && !array[i].hiddenTile)
            newArray.push(array[i]);
        }
      }

      // //aggiunto per menù a tre livelli
      // let olstyleMenu = this.menuService.selectedGroup.Menu;
      // for (let i = 0; i < olstyleMenu.length; i++) {
      //   if (this.tileIsVisible(olstyleMenu[i]) && !olstyleMenu[i].hiddenTile)
      //     newArray.push(olstyleMenu[i]);
      // }
      return newArray;
    }

  }

  //---------------------------------------------------------------------------------------------
  ifTileHasObjects(tile) {
    if (tile == undefined || tile.Object == undefined)
      return false;

    return tile.Object.length > 0
  }

  tileIsVisible(tile) {
    return this.ifTileHasObjects(tile);
  }

  selectApplication(application) {
    this.appActive = application;
    this.menuService.setSelectedApplication(application)
  }

  selectGroup(group) {
    this.groupActive = group;
    this.menuService.setSelectedGroup(group);
    this.itemSelected.emit();
  }
}