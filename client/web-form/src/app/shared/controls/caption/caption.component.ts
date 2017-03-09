import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-caption',
  templateUrl: './caption.component.html',
  styleUrls: ['./caption.component.scss']
})
export class CaptionComponent {
@Input() captionText: string;

  constructor() { }
}
