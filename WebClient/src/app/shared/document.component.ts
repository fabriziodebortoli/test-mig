import { DocumentService } from 'tb-core';
import { TbComponent } from './tb.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-document',
  template: '',
  styles: []
})
export abstract class DocumentComponent extends TbComponent implements OnInit {
title: string;
  constructor(private documentService: DocumentService) { 
    super();
  }

  ngOnInit() {
  }

}
