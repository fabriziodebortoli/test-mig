import { InfoService } from './../../core/services/info.service';
import { TbComponentService, TranslationInfo } from './../../core/services/tbcomponent.service';
import { Input, OnInit } from '@angular/core';
import { Observable } from '../../rxjs.imports';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId = '';

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
    const ids = this.dictionaryId.split('.');
    ids.forEach(id=>{
      const subs = this.tbComponentService.readFromLocal(id).subscribe(ti => {
        if (subs) {
          subs.unsubscribe();
        }
        this.installationVersion = ti.installationVersion;
        
        if (ti.translations) {
          this.translations = this.translations.concat(ti.translations);
        } else {
          this.readTranslationsFromServer(id);
        }
      });
    });
    
  }

  public readTranslationsFromServer(dictionaryId: string) {
    const subs = this.tbComponentService.readTranslationsFromServer(dictionaryId).subscribe(tn => {
      if (subs) {
        subs.unsubscribe();
      }
      if (tn){
        this.translations = this.translations.concat(tn);
      }
      this.translations = tn;
      this.tbComponentService.saveToLocal(this.dictionaryId, this.translations);
    });
  }
}
