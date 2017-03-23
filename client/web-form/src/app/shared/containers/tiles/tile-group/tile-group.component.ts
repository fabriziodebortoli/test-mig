import { HttpService } from './../../../../core/http.service';
import { Component, OnInit, Input } from '@angular/core';
import { TileManagerComponent } from '../tile-manager/tile-manager.component';
import { TabComponent } from '../../tabs';

enum IconType { MD, TB, IMG };
@Component({
  selector: 'tb-tilegroup',
  templateUrl: './tile-group.component.html',
  styleUrls: ['./tile-group.component.scss']
})
export class TileGroupComponent extends TabComponent implements OnInit {


  @Input() icon: string = '';
  iconType: IconType;
  iconTypes = IconType;
  iconTxt: string;
  constructor(tabs: TileManagerComponent, private httpService: HttpService) {
    super(tabs);
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