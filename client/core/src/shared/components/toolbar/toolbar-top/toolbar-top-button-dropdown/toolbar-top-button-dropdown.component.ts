import { CheckStatus } from './../../../../models/check_status.enum';
import { Logger } from './../../../../../core/services/logger.service';
import { Component, OnInit, Input, ChangeDetectorRef, OnDestroy, ViewChild, ViewEncapsulation, HostListener, ElementRef } from '@angular/core';
import { CommandEventArgs } from './../../../../models/eventargs.model';
import { EventDataService } from './../../../../../core/services/eventdata.service';
import { ComponentService } from './../../../../../core/services/component.service';
import { ContextMenuItem } from './../../../../models/context-menu-item.model';
import { TbComponent } from './../../../../../shared/components/tb.component';
import { TbComponentService } from './../../../../../core/services/tbcomponent.service';
import { InfoService } from './../../../../../core/services/info.service';

@Component({
  selector: 'tb-toolbar-top-button-dropdown',
  templateUrl: './toolbar-top-button-dropdown.component.html',
  styleUrls: ['./toolbar-top-button-dropdown.component.scss']

})
export class ToolbarTopButtonDrodownComponent extends TbComponent implements OnDestroy {
  public show = false;

  @ViewChild('anchor') public anchor: ElementRef;
  @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

  @Input() icon = 'tb-menu2';
  @Input() cmpId = '_BUTTONGROUPADVANCED';
  @Input() caption = '';
  @Input() click: () => boolean = () => true;
  public viewProductInfo: string;
  private eventDataServiceSubscription;
  private _disabled = false;
  private _checkStatus = CheckStatus.UNDEFINED;
  constructor(
    public componentService: ComponentService,
    public eventData: EventDataService,
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
    return status ? status : CheckStatus.UNDEFINED;
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
