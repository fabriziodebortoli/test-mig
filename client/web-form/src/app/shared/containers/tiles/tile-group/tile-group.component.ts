import { Component, Input } from '@angular/core';

import { HttpService } from './../../../../core/http.service';

enum IconType { MD, TB, IMG };

@Component({
  selector: 'tb-tilegroup',
  templateUrl: './tile-group.component.html',
  styleUrls: ['./tile-group.component.scss']
})
export class TileGroupComponent {

  @Input() title: string;

  active: boolean;

  @Input() icon: string = '';
  iconType: IconType;
  iconTypes = IconType;
  iconTxt: string;

  constructor(private httpService: HttpService) {
  }

  ngOnInit() {
    this.checkIcon();
  }

  checkIcon() {
    if (this.icon.startsWith("md-")) {
      this.iconType = IconType.MD;
      this.iconTxt = this.icon.slice(3);
    } else if (this.icon.startsWith("tb-")) {
      this.iconType = IconType.TB;
      this.iconTxt = this.icon.slice(3);
    } else {
      this.iconType = IconType.IMG;
      this.iconTxt = this.httpService.getDocumentBaseUrl() + 'getImage/?src=' + this.icon;
    }
  }
}