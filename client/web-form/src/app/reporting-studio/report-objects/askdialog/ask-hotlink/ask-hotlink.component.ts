<<<<<<< HEAD
import { AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
=======
import { Observable } from 'rxjs/Rx';
import { ComboBoxComponent } from '@progress/kendo-angular-dropdowns';
>>>>>>> master
import { ReportingStudioService } from './../../../reporting-studio.service';
import { hotlink, CommandType } from './../../../reporting-studio.model';
import { Component, Input, DoCheck, KeyValueDiffers, Output, EventEmitter, ViewChild } from '@angular/core';

@Component({
  selector: 'rs-ask-hotlink',
  templateUrl: './ask-hotlink.component.html',
  styleUrls: ['./ask-hotlink.component.scss']
})
export class AskHotlinkComponent implements DoCheck {

<<<<<<< HEAD
  @ViewChild(AutoCompleteComponent)
  private acComp: AutoCompleteComponent;

  @Input() hotlink: hotlink;
  value: string = '';
  differ: any;
 


=======
  @ViewChild(ComboBoxComponent)
  private acComp: ComboBoxComponent;

  @Input() hotlink: hotlink;
  differ: any;
>>>>>>> master

  constructor(private rsService: ReportingStudioService, private differs: KeyValueDiffers) {

    this.differ = differs.find({}).create(null);
  }

<<<<<<< HEAD
  ngDoCheck() {
    let changes = this.differ.diff(this.hotlink);
    if (changes && this.hotlink.values.length !== 0) {
=======
  public valueNormalizer = (text: Observable<string>) => text.map((text: string) => {
    return {
      description: text,
      code: text
    }
  });

  ngDoCheck() {
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
>>>>>>> master
      this.acComp.toggle(true);
    }
  }

  OnOpen(event: any) {
<<<<<<< HEAD
    if (this.hotlink.values.length === 0) {
=======
    if (!this.hotlink.values) {
>>>>>>> master
      event.preventDefault();
    }
  }

  onButtonClick() {
    let msg = {
      ns: this.hotlink.ns,
<<<<<<< HEAD
      filter: this.value,
=======
      filter: this.hotlink.value.code,
>>>>>>> master
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
