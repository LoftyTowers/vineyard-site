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
    const suffix = route ? `/${route}` : '';
    return this.http.get<PageData>(`/api/pages${suffix}`);
  }

  updateHeroImage(route: string, imageId: string): Observable<PageData> {
    const suffix = route ? `/${route}` : '';
    return this.http.put<PageData>(`/api/pages${suffix}/hero-image`, { imageId });
  }
}
