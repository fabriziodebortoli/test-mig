import { InfoService } from './../../core/services/info.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Input, OnInit, ChangeDetectorRef } from '@angular/core';

export abstract class TbComponent implements OnInit {
  @Input()
  public cmpId = '';
  public dictionaryId = '';
  public translations = [];
  private cmpCount = 0;
  protected destroyed = false;
  constructor(
    public tbComponentService: TbComponentService,
    protected changeDetectorRef: ChangeDetectorRef) {
    this.dictionaryId = "";
  }
  protected enableLocalization() {
    this.dictionaryId = this.tbComponentService.calculateDictionaryId(this);
  }
  _TB(baseText: string, ...args: any[]) {
    return this.tbComponentService.translate(this.translations, baseText, args);
  }
  ngOnInit() {
    if (!this.dictionaryId) {
      return;
    }
    const ids = this.dictionaryId.split('.');
    this.cmpCount = ids.length;
    ids.forEach(async id => {
      this.tbComponentService.readFromLocal(id).take(1).subscribe(tn => {
        if (tn) {
          this.translations = this.translations.concat(tn);
          this.checkIfReady();
        } else {
          this.readTranslationsFromServer(id);
        }
      });
    });
  }

  private checkIfReady() {
    if (--this.cmpCount === 0) {
      setTimeout(() => !this.destroyed && this.changeDetectorRef.detectChanges(), 0);
      this.onTranslationsReady();
    }
  }

  ngOnDestroy() {
    this.destroyed = true;
  }

  protected onTranslationsReady() { }

  public readTranslationsFromServer(dictionaryId: string) {
    const subs = this.tbComponentService.readTranslationsFromServer(dictionaryId).subscribe(
      tn => {
        if (subs) {
          subs.unsubscribe();
        }
        if (!tn) {
          tn = [];
        }
        this.translations = this.translations.concat(tn);
        this.checkIfReady();
        this.tbComponentService.saveToLocal(dictionaryId, tn);

      },
      err => {
        if (subs) {
          subs.unsubscribe();
        }
        this.checkIfReady();
        //dictionary file may not exist on server
        if (err && err.status === 404) {
          this.tbComponentService.saveToLocal(dictionaryId, []);
        }
      });
  }
}
