import { InfoService } from './../../core/services/info.service';
import { TbComponentService, TranslationInfo } from './../../core/services/tbcomponent.service';
import { Input, OnInit } from '@angular/core';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId: string = '';

  public dictionaryId = '';
  public installationVersion = '';
  public translations = [];

  constructor(public tbComponentService: TbComponentService) {
    this.dictionaryId = tbComponentService.calculateDictionaryId(this);
  }

  _TB(baseText: string) {
    return this.tbComponentService.translate(this.translations, baseText);
  }
  ngOnInit() {
    let subs = this.tbComponentService.initTranslations(this.dictionaryId).subscribe(ti => {
      if (subs)
        subs.unsubscribe();
      this.translations = ti.translations;
      this.installationVersion = ti.installationVersion;

      if (!this.translations)
        this.readTranslationsFromServer();
    });
  }

  public readTranslationsFromServer() {
    this.tbComponentService.readTranslationsFromServer();
  }
}
