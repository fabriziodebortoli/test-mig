import { Router } from '@angular/router';
import { LoginSessionService } from 'tb-core';


import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  constructor(
    private loginSession: LoginSessionService,
    private router: Router) {

  }
  ngOnInit() {
   
  }

}
