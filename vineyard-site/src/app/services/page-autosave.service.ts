import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject, Subscription, of } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap } from 'rxjs/operators';
import { PageData, PageService } from './page.service';

export type AutosaveStatus = 'idle' | 'dirty' | 'saving' | 'saved' | 'error';

export interface AutosaveState {
  status: AutosaveStatus;
  lastSavedUtc?: Date;
}

interface AutosavePayload {
  route: string;
  content: PageData;
  changeId: number;
}

@Injectable({ providedIn: 'root' })
export class PageAutosaveService {
  private readonly changes$ = new Subject<AutosavePayload>();
  private readonly stateSubject = new BehaviorSubject<AutosaveState>({ status: 'idle' });
  private readonly subscription: Subscription;
  private latestChangeId = 0;

  constructor(private pageService: PageService) {
    this.subscription = this.changes$
      .pipe(
        debounceTime(1000),
        tap(() => this.updateState({ status: 'saving' })),
        switchMap(payload =>
          this.pageService.autosaveDraft(payload.route, payload.content).pipe(
            map(() => ({ success: true, changeId: payload.changeId })),
            catchError(() => of({ success: false, changeId: payload.changeId }))
          )
        )
      )
      .subscribe(result => {
        if (result.changeId !== this.latestChangeId) {
          return;
        }

        if (result.success) {
          this.updateState({ status: 'saved', lastSavedUtc: new Date() });
        } else {
          this.updateState({ status: 'error' });
        }
      });
  }

  get state$(): Observable<AutosaveState> {
    return this.stateSubject.asObservable();
  }

  queueChange(route: string, content: PageData): void {
    this.latestChangeId += 1;
    this.updateState({ status: 'dirty' });
    this.changes$.next({ route, content, changeId: this.latestChangeId });
  }

  saveNow(route: string, content: PageData): Observable<void> {
    this.updateState({ status: 'saving' });
    this.latestChangeId += 1;
    const currentId = this.latestChangeId;
    return this.pageService.autosaveDraft(route, content).pipe(
      tap({
        next: () => {
          if (currentId === this.latestChangeId) {
            this.updateState({ status: 'saved', lastSavedUtc: new Date() });
          }
        },
        error: () => {
          if (currentId === this.latestChangeId) {
            this.updateState({ status: 'error' });
          }
        }
      }),
      map(() => void 0)
    );
  }

  reset(): void {
    this.latestChangeId = 0;
    this.stateSubject.next({ status: 'idle' });
  }

  destroy(): void {
    this.subscription.unsubscribe();
    this.stateSubject.complete();
    this.changes$.complete();
  }

  private updateState(next: AutosaveState): void {
    const current = this.stateSubject.value;
    this.stateSubject.next({
      status: next.status,
      lastSavedUtc: next.lastSavedUtc ?? current.lastSavedUtc
    });
  }
}
