import { EventDataService } from './../../../core/eventdata.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-message-dialog',
  templateUrl: './message-dialog.component.html',
  styleUrls: ['./message-dialog.component.scss']
})
export class MessageDialogComponent implements OnInit {

  opened = false;
  args: MessageDlgArgs;
  eventData: EventDataService;
  constructor() { }

  ngOnInit() {
  }
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
