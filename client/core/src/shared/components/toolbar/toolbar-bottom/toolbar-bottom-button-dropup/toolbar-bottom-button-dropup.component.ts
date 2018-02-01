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
  selector: 'tb-toolbar-bottom-button-dropup',
  templateUrl: './toolbar-bottom-button-dropup.component.html',
  styleUrls: ['./toolbar-bottom-button-dropup.component.scss']

})
export class ToolbarBottomButtonDropupComponent extends TbComponent implements OnDestroy {

  anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  anchorAlign2: Align = { horizontal: 'left', vertical: 'top' };
  popupAlign2: Align = { horizontal: 'right', vertical: 'top' };
  public show = false;
    
  @ViewChild('anchor') divFocus: HTMLElement;
  @Input() icon:string;
  @Input() caption:string;

  public viewProductInfo: string;
  private eventDataServiceSubscription;

 constructor(
  public componentService: ComponentService,
  public eventDataService: EventDataService,
  public infoService: InfoService,
  tbComponentService: TbComponentService,
  changeDetectorRef: ChangeDetectorRef
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
       this.outView(null);
  }
  outView(item: ContextMenuItem) {
    if (item !== null && item !== undefined) {
        item.showMySub = false;
    }

    this.show = false;
  }
}