import { InfoService } from './../../core/services/info.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Input, OnInit, ChangeDetectorRef } from '@angular/core';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId = '';

  public dictionaryId = '';
  public translations = [];

  constructor(
    public tbComponentService: TbComponentService, 
    protected changeDetectorRef: ChangeDetectorRef) {
    this.dictionaryId = tbComponentService.calculateDictionaryId(this);
    let s = this._TB("Ciao {1} {0}", 0, 1);
  }

  _TB(baseText: string, ...args: any[]) {
    return this.tbComponentService.translate(this.translations, baseText, args);
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
          this.changeDetectorRef.detectChanges();
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
          this.changeDetectorRef.detectChanges();
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
