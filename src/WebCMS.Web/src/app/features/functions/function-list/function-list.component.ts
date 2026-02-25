import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FunctionService } from '../../../core/services/function.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { LanguageService } from '../../../core/services/language.service';
import { SlidePanelComponent } from '../../../shared/components/slide-panel/slide-panel.component';
import { FunctionDto } from '../../../core/models/function.model';
import { FunctionFormComponent } from '../function-form/function-form.component';

@Component({
  selector: 'app-function-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SlidePanelComponent,
    FunctionFormComponent,
    TranslatePipe
  ],
  templateUrl: './function-list.component.html',
  styleUrl: './function-list.component.scss'
})
export class FunctionListComponent implements OnInit {
  functions: FunctionDto[] = [];
  filteredFunctions: FunctionDto[] = [];
  parentFunctions: FunctionDto[] = [];
  searchTerm = '';
  isPanelOpen = false;
  isEditing = false;
  isLoading = false;
  selectedFunction: FunctionDto | null = null;
  expandedIds = new Set<number>();

  constructor(
    private functionService: FunctionService,
    private confirmDialog: ConfirmDialogService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    this.loadFunctions();
    this.loadParentFunctions();
  }

  loadFunctions(): void {
    this.isLoading = true;
    this.functionService.getMenuTree().subscribe({
      next: (result) => {
        this.functions = result;
        this.filteredFunctions = result;
        this.functions.forEach(f => {
          if (this.hasChildren(f)) {
            this.expandedIds.add(f.id);
          }
        });
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load functions:', err);
        this.isLoading = false;
      }
    });
  }

  loadParentFunctions(): void {
    this.functionService.getAllFunctions().subscribe({
      next: (result) => {
        this.parentFunctions = result;
      },
      error: (err) => {
        console.error('Failed to load parent functions:', err);
      }
    });
  }

  search(): void {
    if (!this.searchTerm.trim()) {
      this.filteredFunctions = this.functions;
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredFunctions = this.filterTree(this.functions, term);
    this.expandMatchingItems(this.filteredFunctions);
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.filteredFunctions = this.functions;
  }

  private filterTree(items: FunctionDto[], term: string): FunctionDto[] {
    const result: FunctionDto[] = [];
    
    for (const item of items) {
      const matches = item.name.toLowerCase().includes(term) || 
                      item.code.toLowerCase().includes(term);
      
      let filteredChildren: FunctionDto[] = [];
      if (item.children && item.children.length > 0) {
        filteredChildren = this.filterTree(item.children, term);
      }

      if (matches || filteredChildren.length > 0) {
        result.push({
          ...item,
          children: filteredChildren.length > 0 ? filteredChildren : item.children
        });
      }
    }
    
    return result;
  }

  private expandMatchingItems(items: FunctionDto[]): void {
    for (const item of items) {
      if (item.children && item.children.length > 0) {
        this.expandedIds.add(item.id);
        this.expandMatchingItems(item.children);
      }
    }
  }

  hasChildren(item: FunctionDto): boolean {
    return !!(item.children && item.children.length > 0);
  }

  isExpanded(id: number): boolean {
    return this.expandedIds.has(id);
  }

  toggleExpand(id: number): void {
    if (this.expandedIds.has(id)) {
      this.expandedIds.delete(id);
    } else {
      this.expandedIds.add(id);
    }
  }

  toggleAll(expand: boolean): void {
    if (expand) {
      this.expandAllItems(this.functions);
    } else {
      this.expandedIds.clear();
    }
  }

  private expandAllItems(items: FunctionDto[]): void {
    for (const item of items) {
      if (this.hasChildren(item)) {
        this.expandedIds.add(item.id);
        this.expandAllItems(item.children!);
      }
    }
  }

  openCreatePanel(): void {
    this.selectedFunction = null;
    this.isEditing = false;
    this.isPanelOpen = true;
  }

  openEditPanel(func: FunctionDto): void {
    this.functionService.getFunction(func.id).subscribe({
      next: (fullFunction) => {
        this.selectedFunction = fullFunction;
        this.isEditing = true;
        this.isPanelOpen = true;
      },
      error: (err) => {
        console.error('Failed to load function:', err);
      }
    });
  }

  closePanel(): void {
    this.isPanelOpen = false;
    this.selectedFunction = null;
  }

  onSave(_func: FunctionDto): void {
    this.closePanel();
    this.loadFunctions();
    this.loadParentFunctions();
  }

  deleteFunction(func: FunctionDto): void {
    this.confirmDialog.confirm({
      title: this.languageService.getTranslation('function.deleteFunction'),
      message: this.languageService.getTranslation('function.deleteFunctionConfirm').replace('{0}', func.name),
      confirmText: this.languageService.getTranslation('common.delete'),
      cancelText: this.languageService.getTranslation('common.cancel'),
      type: 'danger'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.functionService.deleteFunction(func.id).subscribe({
          next: () => {
            this.loadFunctions();
            this.loadParentFunctions();
          },
          error: (err) => {
            console.error('Failed to delete function:', err);
          }
        });
      }
    });
  }
}
