import { Component, Input } from '@angular/core';
import { HttpService } from './../../../core/http.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-bool-edit',
  templateUrl: './bool-edit.component.html',
  styleUrls: ['./bool-edit.component.scss']
})
export class BoolEditComponent extends ControlComponent {

  constructor(private httpService: HttpService) {
    super();
  }
}
