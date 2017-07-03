import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-placeholder',
  template: "<label for=\"{{forCmpID}}\">  {{placeHolder}} </label>",
  styles: [""]
})
export class PlaceholderComponent {

  @Input() placeHolder: string;
  constructor() { }
}
