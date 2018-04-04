import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-status-tile',
  templateUrl: './status-tile.component.html',
  styleUrls: ['./status-tile.component.scss']
})
export class StatusTileComponent {


  active: boolean = true;

  @Input() title: string;
  @Input() clickable: boolean;
  @Input() visible: boolean;
  @Input() backgroundColor: string;

  constructor() { }

  ngOnInit() { }
}