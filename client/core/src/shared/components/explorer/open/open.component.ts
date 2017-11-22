import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Response, URLSearchParams, Http } from '@angular/http';
import { Observable, Subscription } from '../../../../rxjs.imports';
import { ErrorObservable } from '../../../../rxjs.imports';

import { DocumentComponent } from './../../document.component';
import { UtilsService } from './../../../../core/services/utils.service';
import { EventDataService } from './../../../../core/services/eventdata.service';
import { ExplorerService } from './../../../../core/services/explorer.service';

import { MenuService } from './../../../../menu/services/menu.service';
import { ImageService } from './../../../../menu/services/image.service';

import { PanelBarExpandMode, PanelBarItemModel } from '@progress/kendo-angular-layout';

@Component({
  selector: 'tb-open',
  templateUrl: './open.component.html',
  styleUrls: ['./open.component.css'],
  providers: [ExplorerService, EventDataService]
})
export class OpenComponent extends DocumentComponent implements OnInit {

  public applications: any;
  public folders: any;
  public files: any;
  // public foldersarray: Array<String> = [];
  public kendoPanelBarExpandMode: any = PanelBarExpandMode.Multiple;

  applicationsSubscription: Subscription;
  folderSubscription: Subscription;
  filesSubscription: Subscription;

  constructor(
    public explorerService: ExplorerService,
    public eventData: EventDataService,
    public imageService: ImageService,
    public menuService: MenuService,
    public utilsService: UtilsService,
    public http: Http, 
    changeDetectorRef: ChangeDetectorRef) {
    super(explorerService, eventData, null, changeDetectorRef);
  }

  ngOnDestroy() {
    this.applicationsSubscription.unsubscribe();
  }

  ngOnInit() {

    this.applicationsSubscription = this.getApplications().subscribe(result => {
      this.applications = result.Applications.Application;
    });

  }

  getApplications() {
    return this.http.get('http://localhost:5000/explorer-open/get-applications/', { withCredentials: true }).map((res: Response) => {

      return res.json();

    }).catch(this.handleError);
  }

  handleError(error: any): ErrorObservable {
    // In a real world app, we might use a remote logging infrastructure
    // We'd also dig deeper into the error to get a better message
    let errMsg = (error.message) ? error.message :
      error.status ? `${error.status} - ${error.statusText}` : 'Server error';
    console.error(errMsg);
    return Observable.throw(errMsg);
  }

  selecteApplication(application) {
    this.folderSubscription = this.callGetFolder(application).subscribe(result => {
      this.folders = result.Folders.Folder;
    });
  }

  callGetFolder(application) {
    let params: URLSearchParams = new URLSearchParams();
    params.set('applicationPath', application);
    return this.http.get('http://localhost:5000/explorer-open/get-folders/' + "kk", { search: params }).map((res: Response) => {
      return res.json();
    }).catch(this.handleError);
  }

  selecteFolder(folder) {
    console.log('sono nella selectFolder');
    this.filesSubscription = this.callGetFolderFiles(folder).subscribe(result => {
      console.log(result);
      this.files = result.Files.File;
      console.log(this.files);
    });
  }

  callGetFolderFiles(folder) {
    let params: URLSearchParams = new URLSearchParams();
    params.set('folderPath', folder);
    return this.http.get('http://localhost:5000/explorer-open/get-folderFiles/' + "kk", { search: params }).map((res: Response) => {
      return res.json();
    }).catch(this.handleError);
  }

}

