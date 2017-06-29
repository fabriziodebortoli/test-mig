import { Component, OnInit, ViewEncapsulation } from '@angular/core';

import { LoginSessionService } from './../../../../core/services/login-session.service';

@Component({
  selector: 'tb-topbar-menu',
  templateUrl: './topbar-menu.component.html',
  styleUrls: ['./topbar-menu.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopbarMenuComponent implements OnInit {

  constructor(private loginSession: LoginSessionService) { }

  ngOnInit() {
  }

}
