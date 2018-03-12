import { Logger } from './../../../../../core/services/logger.service';
import { Component, OnInit, Input, ChangeDetectorRef, OnDestroy, ViewChild, ViewEncapsulation, HostListener, ElementRef } from '@angular/core';
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
  public show = false;
    
  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

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

public toggle(show?: boolean): void {
  this.show = show !== undefined ? show : !this.show;
  
}
@HostListener('document:click', ['$event'])
public documentClick(event: any): void {
    if (!this.contains(event.target)) {
      this.toggle(false);
    }
}

@HostListener('keydown', ['$event'])
public keydown(event: any): void {
    if (event.keyCode === 27) {
        this.toggle(false);
    }
}

private contains(target: any): boolean {
  return this.anchor.nativeElement.contains(target) ||
      (this.popup ? this.popup.nativeElement.contains(target) : false);
}

}
