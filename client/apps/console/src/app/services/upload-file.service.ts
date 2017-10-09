import { Injectable } from '@angular/core';
import { Http, RequestOptions, Headers, Response } from '@angular/http';

@Injectable()
export class UploadFileService {

  constructor(private http: Http) { }

  public upload(fileToUpload: any) {
    let input = new FormData();
    input.append("file", fileToUpload);

    return this.http
        .post("http://localhost:10344/api/tbfs/init", input);
  }

}
