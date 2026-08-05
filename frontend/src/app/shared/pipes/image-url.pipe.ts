import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../../environments/environment';

// The API stores/returns image paths as relative paths (e.g. "images/items/xxx.jpg"), served as
// static files by the backend. In dev, the Angular app and the API run on different origins/ports,
// so a bare relative path resolves against the wrong origin and 404s - this prefixes it with the
// API's static-files origin. In prod (same-origin deployment), filesBaseUrl is empty and the path
// resolves correctly as-is.
@Pipe({ name: 'imageUrl', standalone: false })
export class ImageUrlPipe implements PipeTransform {
  transform(path: string | null | undefined): string | null {
    if (!path) return null;
    if (/^https?:\/\//i.test(path)) return path;
    return `${environment.filesBaseUrl}/${path.replace(/^\/+/, '')}`;
  }
}
