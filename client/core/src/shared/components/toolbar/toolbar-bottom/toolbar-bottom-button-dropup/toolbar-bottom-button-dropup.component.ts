import { Component, OnInit, Input, ChangeDetectorRef, HostListener, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommandEventArgs } from './../../../../models/eventargs.model';
import { EventDataService } from './../../../../../core/services/eventdata.service';
import { ComponentService } from './../../../../../core/services/component.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';
import { TbComponent } from './../../../../../shared/components/tb.component';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';



@Component({
  selector: 'tb-toolbar-bottom-button-dropup',
  templateUrl: './toolbar-bottom-button-dropup.component.html',
  styleUrls: ['./toolbar-bottom-button-dropup.component.scss']

})
export class ToolbarBottomButtonDropupComponent extends TbComponent implements OnDestroy {

  public show = false;
    
  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

  @Input() icon:string;
  @Input() caption:string = this._TB('Print');

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
  ngOnDestroy() {
    this.eventDataServiceSubscription.unsubscribe();
  }

  public toggle(show?: boolean): void {
    this.show = show !== undefined ? show : !this.show;
}
}