import { DocumentService } from './../../../core/document.service';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-bottom',
  templateUrl: './toolbar-bottom.component.html',
  styleUrls: ['./toolbar-bottom.component.scss']
})
export class ToolbarBottomComponent implements OnInit {

 

  constructor(private document: DocumentService) { }

  ngOnInit() {
  }

}
