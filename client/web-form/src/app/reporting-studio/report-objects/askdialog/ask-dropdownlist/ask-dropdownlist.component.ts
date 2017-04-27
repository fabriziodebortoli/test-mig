import { dropdownlist } from './../../../reporting-studio.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'rs-ask-dropdownlist',
  templateUrl: './ask-dropdownlist.component.html',
  styleUrls: ['./ask-dropdownlist.component.scss']
})
export class AskDropdownlistComponent implements OnInit {

  @Input() dropdownlist : dropdownlist;
  constructor() { }

  ngOnInit() {
   
  }

 

}
