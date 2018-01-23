import { Logger } from './../../../../../core/services/logger.service';
import { Component, OnInit, Input, ChangeDetectorRef, OnDestroy, ViewChild, ViewEncapsulation } from '@angular/core';
import { CommandEventArgs } from './../../../../models/eventargs.model';
import { EventDataService } from './../../../../../core/services/eventdata.service';
import { ComponentService } from './../../../../../core/services/component.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';
import { TbComponent } from './../../../../../shared/components/tb.component';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';



@Component({
  selector: 'tb-toolbar-top-button-dropdown',
  templateUrl: './toolbar-top-button-dropdown.component.html',
  styleUrls: ['./toolbar-top-button-dropdown.component.scss']

})
export class ToolbarTopButtonDrodownComponent extends TbComponent implements OnDestroy {

  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  public show = false;
  currentItem: ContextMenuItem;
  public fontIcon: string;
  
  @ViewChild('anchor') divFocus: HTMLElement;


  public menuElements: ContextMenuItem[] = new Array<ContextMenuItem>();
  public viewProductInfo: string;
  private eventDataServiceSubscription;

 constructor(
  public componentService: ComponentService,
  public eventDataService: EventDataService,
  public infoService: InfoService,
  tbComponentService: TbComponentService,
  changeDetectorRef: ChangeDetectorRef,
  public logger: Logger
) {
   super(tbComponentService, changeDetectorRef);
  this.enableLocalization();


  this.eventDataServiceSubscription = this.eventDataService.command.subscribe((args: CommandEventArgs) => {
      switch (args.commandId) {
          default:
              break;
      }
  }); 
}
 
 onTranslationsReady() {
  super.onTranslationsReady();
  this.menuElements.splice(0, this.menuElements.length);
  const item1 = new ContextMenuItem(this._TB('Query'), 'idQueryButton', true, false);
  const item2 = new ContextMenuItem(this._TB('Share'), 'iShareButton', true, false);
  const item3 = new ContextMenuItem(this._TB('Refresh'), 'idRefreshButton', true, false);
  const item4 = new ContextMenuItem(this._TB('Inspect'), 'idInspectButton', true, false);
  const item5 = new ContextMenuItem(this._TB('Customize'), 'idCustomizeButton', true, false);
  this.menuElements.push(item1, item2, item3, item4,item5);


  
}
ngOnDestroy() {
  this.eventDataServiceSubscription.unsubscribe();
}

onOpen() {
}
public doCommand(menuItem: any) {
  if (!menuItem) {
      return;
  }
 this.logger.debug(menuItem.id + " clicked!");
  this.eventDataService.raiseCommand('', menuItem.id);
  this.onToggle();
}
public onToggle(): void {
  this.show = !this.show;
  if (!this.show && this.currentItem !== null && this.currentItem !== undefined) {
      this.currentItem.showMySub = false;
  }
}
public closePopupIf(): void {
   this.outView(this.currentItem);
}
outView(item: ContextMenuItem) {
  if (item !== null && item !== undefined) {
      item.showMySub = false;
  }

  this.show = false;
  this.currentItem = null;
}

public getCorrectIcon(menuItem: any)
{
  switch(menuItem.id)
  {
    case 'idQueryButton':
      return 'tb-query';

    case 'iShareButton':
    return 'tb-socialshare';

    case 'idRefreshButton':
    return 'tb-refresh';

    case 'idInspectButton':
    return 'tb-inspect';

    case 'idCustomizeButton':
    return 'tb-customize';

    default:
      break;
  }
}
}