import { Component, OnInit } from '@angular/core';

import { EventDataService } from './../../services/eventdata.service';

@Component({
  selector: 'tb-message-dialog',
  template: "<kendo-dialog title=\"Please confirm\" *ngIf=\"opened\" (close)=\"close('cancel')\"> <p style=\"margin: 30px; text-align: center;\">{{args.text}}</p> <kendo-dialog-actions> <button *ngIf=\"args.no\" kendoButton (click)=\"close('no')\">No</button> <button *ngIf=\"args.yes\" kendoButton (click)=\"close('yes')\" primary=\"true\">Yes</button> <button *ngIf=\"args.cancel\" kendoButton (click)=\"close('cancel')\">Cancel</button> <button *ngIf=\"args.ok\" kendoButton (click)=\"close('ok')\"primary=\"true\">Ok</button> <button *ngIf=\"args.retry\" kendoButton (click)=\"close('retry')\">Retry</button> <button *ngIf=\"args.continue\" kendoButton (click)=\"close('continue')\">Continue</button> <button *ngIf=\"args.abort\" kendoButton (click)=\"close('abort')\">Abort</button> <button *ngIf=\"args.ignore\" kendoButton (click)=\"close('ignore')\">Ignore</button> </kendo-dialog-actions> </kendo-dialog>",
  styles: [""]
})
export class MessageDialogComponent implements OnInit {

  opened = false;
  args: MessageDlgArgs;
  eventData: EventDataService;
  constructor() { }

  ngOnInit() { }

  open(args: MessageDlgArgs, eventData?: EventDataService) {
    this.eventData = eventData;
    this.args = args;
    this.opened = true;
  }

  close(result: string) {
    const res = new MessageDlgResult();
    res[result] = true;
    this.opened = false;
    if (this.eventData) {
      this.eventData.closeMessageDialog.emit(res);
    }
  }

}

export class MessageDlgArgs {
  public cmpId = '';
  public text = '';
  public ok = false;
  public cancel = false;
  public yes = false;
  public no = false;
  public abort = false;
  public ignore = false;
  public retry = false;
  public continue = false;
}

export class MessageDlgResult {

}
