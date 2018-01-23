import { CommandEventArgs } from './../../models/eventargs.model';
import { Component, Input, ViewChild, OnInit, ChangeDetectorRef } from '@angular/core';

import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

import { EventDataService } from './../../../core/services/eventdata.service';
import { WebSocketService } from './../../../core/services/websocket.service';
import { ContextMenuDirective } from './../../directives/context-menu.directive';
import { ContextMenuItem } from './../../models/context-menu-item.model';
import { TbComponent } from './../../../shared/components/tb.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { Logger } from './../../../core/services/logger.service';

@Component({
  selector: 'tb-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrls: ['./context-menu.component.scss']
})
export class ContextMenuComponent  extends TbComponent {
  anchorAlign: Align = { horizontal: 'left', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  public collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  show = false;
  isMouseDown = false;
  contextMenuBinding: ContextMenuItem[];
  currentItem: ContextMenuItem;

  @Input() fontIcon = 'tb-menu2';
  @Input() contextMenu: ContextMenuItem[];
  @Input() popupClass = 'content popup';
  @ViewChild('anchor') divFocus: HTMLElement;



  constructor(
    public webSocketService: WebSocketService, 
    public eventDataService: EventDataService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    public logger: Logger
) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();
    // SCENARIO 1: RIEMPIRE DA SERVER
    // this.webSocketService.contextMenu.subscribe((result) => {
    //   this.contextMenu = result.contextMenu;
    // });

    // SCENARIO 2: RIEMPITO DA HTML
    this.contextMenu = new Array<ContextMenuItem>();

  }

  onOpen() {
  }
  onTranslationsReady(){
    super.onTranslationsReady();
    // this.contextMenu.splice(0, this.contextMenu.length);
    // const subItems_bis = new Array<ContextMenuItem>();
    // const item4 = new ContextMenuItem(this._TB('only this disable unchecked'), 'Id4', false, false);
    // subItems_bis.push(item4);

    // const subItems = new Array<ContextMenuItem>();
    // const item1 = new ContextMenuItem(this._TB('disabled unchecked'), 'Id1', false, false);
    // const item5 = new ContextMenuItem(this._TB('enabled checked'), 'Id5', true, true);
    // const item2 = new ContextMenuItem(this._TB('has one sub item'), 'Id2', true, false, subItems_bis);
    // subItems.push(item1, item5);

    // const item3 = new ContextMenuItem(this._TB('has 2 sub items'), 'Id3', true, false, subItems);
    // this.contextMenu.push(item1, item2, item5, item3);
  }

  ngOnInit() {
    super.ngOnInit();
  }

  public doCommand(menuItem: any) {
    if (!menuItem) {
      this.logger.debug('NOT doCommand for ContextMenu!'); 
      return;
    }
    if (this.hasSubItems(menuItem)) {
      return;
    }

    if (typeof menuItem.fnc === 'function')
      menuItem.fnc();
    else
      this.eventDataService.raiseCommand('', menuItem.id);

    this.onToggle();
  }

  ///////////////////////////////////////////////////////////////////////////////////

  public onToggle(): void {
    this.logger.debug(this.show);
    this.show = !this.show;
    if (!this.show && this.currentItem !== null && this.currentItem !== undefined) {
      this.currentItem.showMySub = false;
    }
  }

  public closePopupIf(): void {
    if (this.isMouseDown) {
      this.isMouseDown = false;
      document.getElementById('anchor').focus();
      return;
    }
    this.outView(this.currentItem);
  }

  setMouseDown() {
    this.isMouseDown = true;
  }

  hasSubItems(item: ContextMenuItem) {
    const y = item.subItems;
    return y !== null && y.length > 0;
  }

  openSubItems(open: boolean, item: ContextMenuItem) {
    if (!this.hasSubItems(item) || item === null || item === undefined) {
      return;
    }
    item.showMySub = open;
    this.currentItem = item;
  }

  outView(item: ContextMenuItem) {
    if (item !== null && item !== undefined) {
      item.showMySub = false;
    }

    this.show = false;
    this.currentItem = null;
    this.isMouseDown = false;
  }

}
