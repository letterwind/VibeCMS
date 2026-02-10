import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ColumnDefinition {
  key: string;
  header: string;
  sortable?: boolean;
  width?: string;
  template?: string;
}

export interface PaginationConfig {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface PageEvent {
  pageNumber: number;
  pageSize: number;
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

export interface RowActionEvent<T> {
  action: string;
  item: T;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss'
})
export class DataTableComponent<T extends Record<string, any>> {
  @Input() data: T[] = [];
  @Input() columns: ColumnDefinition[] = [];
  @Input() pagination: PaginationConfig | null = null;
  @Input() showActions = true;

  @Output() onPageChange = new EventEmitter<PageEvent>();
  @Output() onSort = new EventEmitter<SortEvent>();
  @Output() onRowAction = new EventEmitter<RowActionEvent<T>>();

  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  get visiblePages(): number[] {
    if (!this.pagination) return [];
    
    const pages: number[] = [];
    const current = this.pagination.pageNumber;
    const total = this.pagination.totalPages;
    const range = 2;

    let start = Math.max(1, current - range);
    let end = Math.min(total, current + range);

    if (current - range < 1) {
      end = Math.min(total, end + (range - current + 1));
    }
    if (current + range > total) {
      start = Math.max(1, start - (current + range - total));
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  onSortClick(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.onSort.emit({ column: this.sortColumn, direction: this.sortDirection });
  }

  goToPage(page: number): void {
    if (!this.pagination) return;
    if (page < 1 || page > this.pagination.totalPages) return;
    if (page === this.pagination.pageNumber) return;

    this.onPageChange.emit({ 
      pageNumber: page, 
      pageSize: this.pagination.pageSize 
    });
  }

  onAction(action: string, item: T): void {
    this.onRowAction.emit({ action, item });
  }
}
