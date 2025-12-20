import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ImageListItem {
  id: string;
  url: string;
  thumbnailUrl: string;
  alt?: string;
  caption?: string;
  width?: number;
  height?: number;
}

@Injectable({ providedIn: 'root' })
export class ImagesService {
  constructor(private http: HttpClient) {}

  getImages(): Observable<ImageListItem[]> {
    return this.http.get<ImageListItem[]>('/api/images');
  }
}
