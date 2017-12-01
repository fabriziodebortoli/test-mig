import {Router} from '@angular/router';
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
  
  // action panel variabled

  iconSignature: string;
  actionLink: string;
  panelMessageText: string;
  panelActionText: string;
  showPanelButton: boolean;

  constructor(private backendService: BackendService, private router: Router) {
    this.operationResult = new OperationResult();
    this.instancesCount = 0;
    this.iconSignature = '';
    this.actionLink = '';
    this.panelMessageText = '';
    this.panelActionText = '';
    this.showPanelButton = true;
  }

  ngOnInit() {
    
    this.backendService.checkStartup().subscribe(
      res => {
        try
        {
          this.operationResult = res;
          this.instancesCount = res['Content'];

          if (this.instancesCount == 0) {
            this.iconSignature = 'warning';
            this.panelMessageText = "It looks like there are no registered M4 Cloud Instances.";
            this.panelActionText = "Register New Instance";
            this.showPanelButton = true;
            return;
          }

          let countMessage = this.instancesCount > 1 ? " registered instances." : " registered instance."
          this.iconSignature = '';
          this.panelMessageText = this.instancesCount + countMessage;
          this.panelActionText = "Manage";
          this.showPanelButton = false;
        }
        catch (exception)
        {
          alert('An error occurred while boostrapping the system ' + exception);
        }
      },
      err => alert(err)
    )

  }

  goInstanceRegistration() {

    if (this.instancesCount == 0) {
      this.router.navigateByUrl('/instancesRegistration');
    } else {
      this.router.navigateByUrl('/instancesHome');
    }
    
  }

}
