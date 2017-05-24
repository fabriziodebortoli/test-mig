import { LoginSessionService } from './../../../../core/login-session.service';
import { Component, OnInit } from '@angular/core';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { MenuItem } from './../../../context-menu/menu-item.model';

@Component({
  selector: 'tb-topbar-menu-user',
  templateUrl: './topbar-menu-user.component.html',
  styleUrls: ['./topbar-menu-user.component.css']
})
export class TopbarMenuUserComponent implements OnInit {
 private title: string = "User menu";
 anchorAlign: Align = { horizontal: 'right', vertical: 'bottom' };
  popupAlign: Align = { horizontal: 'right', vertical: 'top' };
  private collision: Collision = { horizontal: 'flip', vertical: 'fit' };
  contextMenu: MenuItem[] = new Array<MenuItem>();
  private show = false;


  constructor(private loginSessionService: LoginSessionService) { 
    const item1 = new MenuItem('Refresh', 'idRefreshButton', false, false);
    const item2 = new MenuItem('Settings', 'idSettingsButton', false,  false);
    const item3 = new MenuItem('Help', 'idHelpButton', false,  false);
    const item4 = new MenuItem( 'Sign Out', 'idSignOutButton', false,  false);
   this.contextMenu.push(item1, item2, item3, item4);
    }

  ngOnInit() {
  }

  chooseAction(buttonName: string){
switch (buttonName) {
  case 'idSignOutButton':
    return this.logout();
  default:
    break;
    }
  }

  logout() {
    this.loginSessionService.logout();
  }
    public closePopup(): void {
      this.show = false;
    }
}
