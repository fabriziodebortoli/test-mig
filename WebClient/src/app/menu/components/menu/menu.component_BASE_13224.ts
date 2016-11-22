import { UtilsService } from './../../../core';
import { DocumentInfo } from './../../../shared';
import { HttpService } from './../../../core/';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css']
})
export class MenuComponent implements OnInit {

  constructor(private httpService: HttpService, private utilService: UtilsService) { }

  ngOnInit() {
  }

  runDocument(ns: string) {
    this.httpService.runObject(new DocumentInfo(0, ns, this.utilService.generateGUID()))
    .subscribe(result => {
      console.log(result);
    });
  }
}
