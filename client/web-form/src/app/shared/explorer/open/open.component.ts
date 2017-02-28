import { HttpService } from './../../../core/http.service';
import { UtilsService } from './../../../core/utils.service';
import { Observable } from 'rxjs';
import { ExplorerService } from './../../../core/explorer.service';
import { ImageService } from '../../../menu/services/image.service';
import { Component, OnInit } from '@angular/core';
import { MenuService } from './../../../menu/services/menu.service';
import { Response } from '@angular/http';
import { OperationResult } from 'tb-core';

@Component({
  selector: 'tb-open',
  templateUrl: './open.component.html',
  styleUrls: ['./open.component.css']
})
export class OpenComponent implements OnInit {

  constructor(private explorerService: ExplorerService,
    private imageService: ImageService,
    private menuService: MenuService,
    private utilsService: UtilsService,
    private httpService: HttpService) {

    httpService.postData('http://localhost:5000/explorer-open/get-applications', {}).map((res: Response) => {
      return this.createOperationResult(res);
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

  createOperationResult(res: Response): OperationResult {
    console.log('dddddddddddddd');
    let jObject = res.ok ? res.json() : null;
    let ok = jObject && jObject.success === true;
    let messages = jObject ? jObject.messages : [];
    return new OperationResult(!ok, messages);
  }

  public applications: any;
  public menu: any;

  ngOnInit() {


    this.menu = this.menuService.applicationMenu;
    this.applications = this.utilsService.toArray(this.menu.Application);

  }

  selecteApplication(application) {
    this.explorerService.setSelectedApplication(application)
  }

}
