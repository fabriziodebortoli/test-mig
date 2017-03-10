import { Component, OnDestroy, OnInit } from '@angular/core';

import { LoginService } from './core/login.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  constructor(private loginService: LoginService) { }

  ngOnInit() { }

  ngOnDestroy() {
  }
}
