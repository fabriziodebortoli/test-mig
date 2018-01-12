import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-status-tile',
  templateUrl: './status-tile.component.html',
  styleUrls: ['./status-tile.component.scss']
})
export class StatusTileComponent {

  active: boolean = true;

  @Input() title: string;

  constructor() { }

  ngOnInit() { }
}