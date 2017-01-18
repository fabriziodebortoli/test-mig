import { DocumentService } from 'tb-core';
import { Document } from './../../models/document.model';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})

export class ToolbarTopComponent implements OnInit {

  constructor(private document: DocumentService) {

  }

  ngOnInit() {
  }



}
