import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface PageData {
  blocks: any[];
}

@Injectable({ providedIn: 'root' })
export class PageService {
  constructor(private http: HttpClient, private auth: AuthService) {}

  getPage(route: string): Observable<PageData> {
    const suffix = route ? `/${route}` : '';
    return this.http.get<PageData>(`/api/pages${suffix}`);
  }

  getDraft(route: string): Observable<PageData> {
    const suffix = route ? `/${route}` : '';
    return this.http.get<PageData>(`/api/pages${suffix}/draft`);
  }

  updateHeroImage(route: string, imageId: string): Observable<PageData> {
    const suffix = route ? `/${route}` : '';
    return this.http.put<PageData>(`/api/pages${suffix}/hero-image`, { imageId });
  }

  autosaveDraft(route: string, content: PageData): Observable<void> {
    const suffix = route ? `/${route}` : '';
    const token = this.auth.token;
    const headers = token ? new HttpHeaders({ Authorization: `Bearer ${token}` }) : undefined;
    return this.http.post<void>(`/api/pages${suffix}/autosave`, { content }, { headers });
  }

  publishDraft(route: string): Observable<PageData> {
    const suffix = route ? `/${route}` : '';
    return this.http.post<PageData>(`/api/pages${suffix}/publish`, {});
  }

  discardDraft(route: string): Observable<void> {
    const suffix = route ? `/${route}` : '';
    return this.http.post<void>(`/api/pages${suffix}/discard`, {});
  }
}
