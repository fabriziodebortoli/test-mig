import { LoginSessionService } from './../../../../core/login-session.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-topbar-menu-user',
  templateUrl: './topbar-menu-user.component.html',
  styleUrls: ['./topbar-menu-user.component.css']
})
export class TopbarMenuUserComponent implements OnInit {

  private title: string = "User menu";
data: Array<any> = [{
         actionName: 'Refresh',
    }, {
        actionName: 'Settings',
    }, {
        actionName: 'Help',
    }, {
        actionName: 'Sign Out',
         click: (dataItem) => {
           this.logout();
        }
    }
];


  constructor(private loginSessionService: LoginSessionService) { }

  ngOnInit() {
  }

  logout() {
    this.loginSessionService.logout();
  }
}
