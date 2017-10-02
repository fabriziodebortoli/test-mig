import { InfoService } from './../../core/services/info.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Input, OnInit } from '@angular/core';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId: string = '';

  public dictionaryId = '';
  public translations = [];
  public culture = '';
  public installationVersion = '';

  constructor(public tbComponentService: TbComponentService) { }

  _TB(baseText: string) {
    let target = baseText;
    this.translations.some(t => {
      if (t.base == baseText) {
        target = t.target;
        return true;
      }
      return false;
    });
    return target;
  }

  ngOnInit() {
    if (this.dictionaryId) {
      let sub = this.tbComponentService.infoService.getProductInfo().subscribe((productInfo: any) => {
        this.installationVersion = productInfo.installationVersion;
        if (sub)
          sub.unsubscribe();
        this.readTranslations();
      });
    }
  }

  public readTranslations() {
    let item = localStorage.getItem(this.dictionaryId);
    let found = false;
    if (item) {
      try {
        let jItem = JSON.parse(item);

        if (jItem.installationVersion === this.installationVersion) {
          this.translations = jItem.translations;
          found = true;
        }
      }
      catch (ex) {
        console.log(ex);
      }
    }
    if (!found) {
      this.readTranslationsFromServer();
    }
  }

  public readTranslationsFromServer() {

  }
}
