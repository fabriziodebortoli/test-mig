import { CheckStatus } from './../../../../models/check_status.enum';
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

  @Input() icon: string;
  @Input() caption: string = this._TB('Print');
  private _disabled = false;
  private _checkStatus = CheckStatus.UNDEFINED;
  
  public viewProductInfo: string;
  private eventDataServiceSubscription;

  constructor(
    public componentService: ComponentService,
    public eventData: EventDataService,
    public infoService: InfoService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
    this.enableLocalization();


    this.eventDataServiceSubscription = this.eventData.command.subscribe((args: CommandEventArgs) => {
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
  @Input() public set disabled(value: boolean) {
    this._disabled = value;
  }
  public get disabled(): boolean {
    if (this._disabled) {
      return true;
    }
    if (this.eventData.buttonsState &&
      this.eventData.buttonsState[this.cmpId])
      return !this.eventData.buttonsState[this.cmpId].enabled;
    return false;
  }
  
@Input() public set checkStatus(value: CheckStatus) {
    this._checkStatus = value;
  }

  public get checkStatus(): CheckStatus {
    if (this._checkStatus != CheckStatus.UNDEFINED) {
      return this._checkStatus;
    }
    let status = undefined;
    if (this.eventData.buttonsState &&
      this.eventData.buttonsState[this.cmpId]) {
      status = this.eventData.buttonsState[this.cmpId].checkStatus;
    }
    return status as CheckStatus;
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