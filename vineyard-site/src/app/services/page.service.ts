import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PageData {
  blocks: any[];
}

@Injectable({ providedIn: 'root' })
export class PageService {
  constructor(private http: HttpClient) {}

  getPage(route: string): Observable<PageData> {
    return this.http.get<PageData>(`/api/pages/${route}`);
  }
}
