import { InfoService } from './../../core/services/info.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Input, OnInit } from '@angular/core';
import { Observable } from '../../rxjs.imports';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId = '';

  public dictionaryId = '';
  public translations = [];

  constructor(public tbComponentService: TbComponentService) {
    this.dictionaryId = tbComponentService.calculateDictionaryId(this);
  }

  _TB(baseText: string) {
    return this.tbComponentService.translate(this.translations, baseText);
  }
  ngOnInit() {
    const ids = this.dictionaryId.split('.');
    ids.forEach(id => {
      const subs = this.tbComponentService.readFromLocal(id).subscribe(tn => {
        if (subs) {
          subs.unsubscribe();
        }

        if (tn) {
          this.translations = this.translations.concat(tn);
        } else {
          this.readTranslationsFromServer(id);
        }
      });
    });

  }

  public readTranslationsFromServer(dictionaryId: string) {
    const subs = this.tbComponentService.readTranslationsFromServer(dictionaryId).subscribe(
      tn => {
        if (subs) {
          subs.unsubscribe();
        }
        if (tn) {
          this.translations = this.translations.concat(tn);
        }
        this.translations = tn;
        this.tbComponentService.saveToLocal(dictionaryId, this.translations);
      },
      err => {
        if (subs) {
          subs.unsubscribe();
        }
        //dictionary file may not exist on server
        if (err && err.status === 404) {
          this.tbComponentService.saveToLocal(dictionaryId, []);
        }
      });
  }
}
