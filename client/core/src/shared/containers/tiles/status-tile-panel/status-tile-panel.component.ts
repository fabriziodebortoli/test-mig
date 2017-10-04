import { Component, Input } from '@angular/core';

@Component({
  selector: 'tb-status-tile-panel',
  templateUrl: './status-tile-panel.component.html',
  styleUrls: ['./status-tile-panel.component.scss']
})
export class StatusTilePanelComponent {

  active: boolean = true;

  @Input() title: string;

  constructor() { }

  ngOnInit() { }
}