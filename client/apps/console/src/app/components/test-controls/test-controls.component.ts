import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-controls',
  templateUrl: './test-controls.component.html',
  styleUrls: ['./test-controls.component.css']
})
export class TestControlsComponent implements OnInit {

  // used for checkbox
  chkTest: boolean;

  constructor() { 
    this.chkTest = false;
  }

  ngOnInit() {
  }

}
