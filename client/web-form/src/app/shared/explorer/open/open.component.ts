import { Component, OnInit } from '@angular/core';
import { Response, Http } from '@angular/http';
import { Observable, Subscription } from 'rxjs';

import { OperationResult } from 'tb-core';

import { HttpService } from './../../../core/http.service';
import { UtilsService } from './../../../core/utils.service';
// import { ExplorerService } from './../../../core/explorer.service';
import { ImageService } from '../../../menu/services/image.service';
import { MenuService } from './../../../menu/services/menu.service';

@Component({
  selector: 'tb-open',
  templateUrl: './open.component.html',
  styleUrls: ['./open.component.css']
})
export class OpenComponent implements OnInit {

  public applications: any = {};
  public menu: any;

  applicationsSubscription: Subscription;

  constructor(
    // private explorerService: ExplorerService,
    private imageService: ImageService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private http: Http) {

  }

  ngOnDestroy() {
    this.applicationsSubscription.unsubscribe();
  }

  ngOnInit() {

    this.applicationsSubscription = this.getApplications().subscribe(result => {
      let obj = JSON.parse(result);
      console.log(obj);
      this.applications = result;
      console.log(this.applications);
    });


  }

  getApplications() {
    return this.http.get('http://localhost:5000/explorer-open/get-applications/', { withCredentials: true }).map((res: Response) => {

      return res.json();

    }).catch(this.handleError);
  }

  protected handleError(error: any) {
    // In a real world app, we might use a remote logging infrastructure
    // We'd also dig deeper into the error to get a better message
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error(errMsg);
    return Observable.throw(errMsg);
  }





  selecteApplication(application) {
    // this.explorerService.setSelectedApplication(application);
  }

}
