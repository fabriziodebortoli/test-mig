import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-controls',
  templateUrl: './test-controls.component.html',
  styleUrls: ['./test-controls.component.css']
})
export class TestControlsComponent implements OnInit {

  //--------------------------------------------------------------------------------
  // checkbox auxiliary variables

  chkTest: boolean;

  //--------------------------------------------------------------------------------
  // dropdown auxiliary variables

  artWorks: Array<{name:string, value:string}> = [
    { name: 'Monna Lisa', value:'leo-monna' },
    { name: 'The Virgin of The Rocks', value:'leo-vrgocks' },
    { name: 'Lady with an Ermine', value:'leo-ladyermine' },
    { name: 'The Last Supper', value:'leo-lastsupp' }
  ];

  selectedItem: string = 'leo-monna';

  constructor() { 
    this.chkTest = false;
  }

  ngOnInit() {
  }

}
