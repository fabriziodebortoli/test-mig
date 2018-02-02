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
  @ViewChild('anchor') divFocus: HTMLElement;

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

ngOnDestroy() {
  this.eventDataServiceSubscription.unsubscribe();
}

onOpen() {
}

public onToggle(): void {
  this.show = !this.show;
}
public closePopupIf(): void {
  } 
}