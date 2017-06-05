import { Observable } from 'rxjs/Rx';
import { ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlinkOld.component.html',
  styleUrls: ['./ask-hotlinkOld.component.scss']
})
export class AskHotlinkOldComponent implements DoCheck {

  @ViewChild(ComboBoxComponent)
  private acComp: ComboBoxComponent;

  @Input() hotlink: hotlink;
  differ: any;

  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

  public valueNormalizer = (text: Observable<string>) => text.map((text: string) => {
    return {
      description: text,
      code: text
    }
  });

  ngDoCheck() {
    if (this.hotlink === undefined) {
      return;
    }
    let changes = this.differ.diff(this.hotlink);
    if (changes && this.hotlink.values && this.hotlink.values.rows) {
      let vals: any[] = [];
      let keyValue = this.hotlink.values.key;
      for (let i = 0; i < this.hotlink.values.rows.length; i++) {
        let k = this.hotlink.values.rows[i];
        let text = '';
        const value = k[keyValue];
        for (let key in k) {
          if (k.hasOwnProperty(key)) {
            text += ' ' + k[key];
          }
        }
        let obj = {
          description: text,
          code: value
        }
        vals.push(obj);
      }

      this.hotlink.values = vals;
      this.acComp.toggle(true);
    }
  }

  OnOpen(event: any) {
    if (!this.hotlink.values) {
      event.preventDefault();
    }
  }

  onButtonClick() {
    let msg = {
      ns: this.hotlink.ns,
      filter: this.hotlink.value.code,
      name: this.hotlink.name
    };

    let message = {
      commandType: CommandType.HOTLINK,
      message: JSON.stringify(msg),
      page: this.hotlink.id
    };

    this.rsService.doSend(JSON.stringify(message));
  }

}
