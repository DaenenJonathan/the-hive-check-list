import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ImportPreview, ImportPreviewItem, ImportService } from '../services/import.service';

@Component({
  selector: 'app-import-page',
  templateUrl: './import-page.component.html',
  standalone: false
})
export class ImportPageComponent {
  preview: ImportPreview | null = null;
  selectedFile: File | null = null;
  loading = false;
  error = '';
  step: 'upload' | 'preview' | 'done' = 'upload';
  categories: string[] = [];
  itemsByCategory: Record<string, ImportPreviewItem[]> = {};

  constructor(
    private importService: ImportService,
    private router: Router
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.error = '';
    }
  }

  previewFile(): void {
    if (!this.selectedFile) return;
    this.loading = true;
    this.error = '';
    this.importService.preview(this.selectedFile).subscribe({
      next: data => {
        this.preview = data;
        this.buildCategoryIndex(data.items);
        this.step = 'preview';
        this.loading = false;
      },
      error: () => {
        this.error = 'IMPORT.ERROR_PREVIEW';
        this.loading = false;
      }
    });
  }

  confirmImport(): void {
    if (!this.selectedFile) return;
    this.loading = true;
    this.importService.confirm(this.selectedFile).subscribe({
      next: result => {
        this.loading = false;
        this.router.navigate(['/checklists', result.checklistId]);
      },
      error: () => {
        this.error = 'IMPORT.ERROR_CONFIRM';
        this.loading = false;
      }
    });
  }

  reset(): void {
    this.step = 'upload';
    this.selectedFile = null;
    this.preview = null;
    this.error = '';
    this.categories = [];
    this.itemsByCategory = {};
  }

  private buildCategoryIndex(items: ImportPreviewItem[]): void {
    this.itemsByCategory = {};
    for (const item of items) {
      const cat = item.category ?? 'Autres';
      if (!this.itemsByCategory[cat]) {
        this.itemsByCategory[cat] = [];
        this.categories.push(cat);
      }
      this.itemsByCategory[cat].push(item);
    }
  }
}
