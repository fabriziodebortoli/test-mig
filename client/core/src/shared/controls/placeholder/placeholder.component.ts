import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-placeholder',
  templateUrl: './placeholder.component.html',
  styleUrls: ['./placeholder.component.scss']
})
export class PlaceholderComponent {

  @Input() placeHolder: string;
  @Input() forCmpID: string;
  constructor() { }
}
