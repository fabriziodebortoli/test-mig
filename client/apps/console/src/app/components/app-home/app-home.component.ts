import {OperationResult} from '../../services/operationResult';
import { Component, OnInit } from '@angular/core';
import { BackendService } from 'app/services/backend.service';
import { Observable } from 'rxjs/Observable';

@Component({
  selector: 'app-app-home',
  templateUrl: './app-home.component.html',
  styleUrls: ['./app-home.component.css']
})
export class AppHomeComponent implements OnInit {

  operationResult: OperationResult;
  instancesCount: number;

  constructor(private backendService: BackendService) {
    this.operationResult = new OperationResult();
  }

  ngOnInit() {
    
    this.backendService.checkStartup().subscribe(
      res => {
        this.operationResult = res;
        this.instancesCount = res['Content'];
      },
      err => alert(err)
    )

  }

}
