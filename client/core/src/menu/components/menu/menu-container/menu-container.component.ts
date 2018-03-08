import { Component, Input, OnInit, OnDestroy, ViewChild, ViewEncapsulation, AfterViewInit, AfterContentInit, ViewContainerRef, ChangeDetectionStrategy, Output, EventEmitter, ChangeDetectorRef, HostListener, ElementRef } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";
import { Subscription } from "rxjs/Rx";

import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { SettingsService } from './../../../../core/services/settings.service';
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
      transition('expanded <=> collapsed', animate('400ms ease')),
    ])
  ]
})

export class MenuContainerComponent extends TbComponent implements AfterContentInit, OnDestroy {

  public subscriptions: Subscription[] = [];
  public selectedGroupChangedSubscription;
  public tiles: any[];

  public selectorCollapsed: string = localStorage.getItem('menuSelectorCollapsed') ? localStorage.getItem('menuSelectorCollapsed') : 'expanded';
  public appActive: any; 
  public groupActive: any; 
  @Output() itemSelected: EventEmitter<any> = new EventEmitter();

  @ViewChild('tabber') tabber;

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
  private contains(target: any): boolean {
    return (this.hiddenTilesAnchor ? this.hiddenTilesAnchor.nativeElement.contains(target) : false) ||
      (this.hiddenTilesPopup ? this.hiddenTilesPopup.nativeElement.contains(target) : false);
  }

  // ---------------------------------------------------------------------------------------
  closeHiddenTilesPopup() {
    this.showHiddenTilesPopup = false;
  }

  // ---------------------------------------------------------------------------------------
  toggleHiddenTilesPopup(){
    this.showHiddenTilesPopup = !this.showHiddenTilesPopup;
  }

  constructor(
    public menuService: MenuService,
    public utilsService: UtilsService,
    public settingsService: SettingsService,
    public imageService: ImageService,
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
  }

  getSelectorIcon() {
    return this.selectorCollapsed ? 'tb-circledrightfilled' : 'tb-gobackfilled';
  }

  toggleSelector() {
    this.selectorCollapsed = this.selectorCollapsed === 'expanded' ? 'collapsed' : 'expanded';
    localStorage.setItem('menuSelectorCollapsed', this.selectorCollapsed);
  }

  refreshLayout() {
  }

  ngAfterContentInit() {
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
  }

  getTiles() {
    if (this.menuService.selectedMenu) {
      let array = this.menuService.selectedMenu.Menu;
      let newArray = [];
      if (array) {
        for (let i = 0; i < array.length; i++) {

          let element =array[i];
          let env = element.environment ? element.environment.toLowerCase() : '';
          let show = env == '' || (this.tbComponentService.infoService.isDesktop && env == 'desktop') || (!this.tbComponentService.infoService.isDesktop && env == 'web')
          if (this.tileIsVisible(element) && !element.hiddenTile && show )
            newArray.push(element);
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