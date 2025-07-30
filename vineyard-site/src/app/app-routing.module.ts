import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { AboutComponent } from './pages/about/about.component';
import { GalleryComponent } from './pages/gallery/gallery.component';
import { ThemeEditorComponent } from './admin/theme-editor/theme-editor.component';
import { VersionHistoryComponent } from './admin/version-history/version-history.component';
import { ActivityLogComponent } from './admin/activity-log/activity-log.component';
import { authGuard } from './services/auth.guard';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { ServerErrorComponent } from './pages/server-error/server-error.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: 'gallery', component: GalleryComponent },
  {
    path: 'admin/theme-editor',
    component: ThemeEditorComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/versions',
    component: VersionHistoryComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'admin/activity',
    component: ActivityLogComponent,
    canActivate: [authGuard],
    data: { roles: ['Admin'] }
  },
  { path: 'error', component: ServerErrorComponent },
  { path: '**', component: NotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}

export default routes;
