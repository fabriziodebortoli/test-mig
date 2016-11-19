import { TbComponent } from './tb.component';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-document',
  template: `
  `,
  styles: []
})
export class DocumentComponent extends TbComponent implements OnInit {
title: string;
  constructor() { 
    super();
  }

  ngOnInit() {
  }

}
