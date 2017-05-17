import { Observable } from 'rxjs/Rx';
import { ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss']
})
export class AskHotlinkComponent implements DoCheck {

  @ViewChild(ComboBoxComponent)
  private acComp: ComboBoxComponent;

  @Input() hotlink: hotlink;
  differ: any;

  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

  public valueNormalizer = (text: Observable<string>) => text.map((text: string) => {
    return {
      description: '',
      code: text
    }
  });

  ngDoCheck() {
    let changes = this.differ.diff(this.hotlink);
    if (changes && this.hotlink.values && this.hotlink.values.rows) {
      let vals: any[] = [];
      for (let i = 0; i < this.hotlink.values.rows.length; i++) {
        let k = this.hotlink.values.rows[i];
        let text = '';
        let value = '';
        for (let key in k) {
          if (k.hasOwnProperty(key)) {
            let element = k[key];
            if (value === '') {
              value = element;
            }
            text += ' ' + element;
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
