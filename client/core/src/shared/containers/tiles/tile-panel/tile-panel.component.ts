
import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'tb-tile-panel',
    templateUrl: './tile-panel.component.html',
    styleUrls: ['./tile-panel.component.scss']
})
export class TilePanelComponent implements OnInit {

    public _showAsTile: boolean = true;
    @Input() title: string = '';
    public _isCollapsed: boolean = false;
    public _isCollapsible: boolean = true;
    

    @Input()
    set isCollapsed(value: boolean) {
      this._isCollapsed = value;
    }
  
    get isCollapsed(): boolean {
      return this._isCollapsed;
    }
  
    @Input()
    set isCollapsible(value: boolean) {
      this._isCollapsible = value;
    }
  
    get isCollapsible(): boolean {
      return this._isCollapsible;
    }


    @Input()
    set showAsTile(value: boolean) {
        this._showAsTile = value;
    }

    get showAsTile(): boolean {
        return this._showAsTile;
    }

    constructor() { }

    ngOnInit() {
    }

    toggleCollapse(event: MouseEvent): void {
        if (!this.isCollapsible)
            return;

        // event.preventDefault();
        this.isCollapsed = !this.isCollapsed;
    }
}
