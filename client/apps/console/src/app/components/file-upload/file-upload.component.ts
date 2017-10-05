import { Component, OnInit, ViewChild } from '@angular/core';
import { UploadFileService } from 'app/services/upload-file.service';


@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent implements OnInit {

  @ViewChild("fileInput") fileInput;

  constructor(private uploadService: UploadFileService) { }

  ngOnInit() {
  }

  addFile(): void {
    let fi = this.fileInput.nativeElement;
    if (fi.files && fi.files[0]) {
        let fileToUpload = fi.files[0];
        this.uploadService
            .upload(fileToUpload)
            .subscribe(res => {
                console.log(res);
            });
    }
  }  

}
