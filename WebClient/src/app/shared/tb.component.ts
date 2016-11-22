import { Component, OnInit } from '@angular/core';

@Component({
})
export abstract class TbComponent implements OnInit {
  title: string = '';
  id: string = '';
  constructor() { }

  ngOnInit() {
  }

}
