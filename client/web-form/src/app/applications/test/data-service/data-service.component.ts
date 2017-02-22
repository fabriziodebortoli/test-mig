import { HttpService } from './../../../core/http.service';
import { Response } from '@angular/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'tb-data-service',
  templateUrl: './data-service.component.html',
  styleUrls: ['./data-service.component.css']
})
export class DataServiceComponent implements OnInit {

 constructor(private httpService: HttpService) { 
     httpService.postData(httpService.getBaseUrl() + 'ds/data-service', {}).map((res: Response) => {
                return res.ok && res.json().success === true;
     }); 
  }

  ngOnInit() {
  }

}
