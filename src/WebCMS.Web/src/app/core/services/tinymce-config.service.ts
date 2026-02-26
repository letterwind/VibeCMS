import { Injectable } from '@angular/core';
import { EditorComponent } from '@tinymce/tinymce-angular';

@Injectable({ providedIn: 'root' })
export class TinyMceConfigService {
  private readonly baseConfig: EditorComponent['init'] = {
    base_url: '/assets/js/tinymce',
    suffix: '.min',
    height: 400,
    menubar: false,
    plugins: [
      'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
      'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
      'insertdatetime', 'media', 'table', 'help', 'wordcount'
    ],
    toolbar: 'undo redo | blocks | bold italic forecolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | link image | code | help',
    content_style: 'body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif; font-size: 14px; }',
    file_picker_callback: function (cb: any, value: string, meta: any) {
      const input = document.createElement('input');
        input.setAttribute('type', 'file');
        //event listeners
        input.addEventListener('change', (e: Event) => {
          const file = (e.target as HTMLInputElement).files![0];

          const reader = new FileReader();
          reader.addEventListener('load', () => {
            const id = 'blobid' + (new Date()).getTime();
            const blobCache = (window as any).tinymce.activeEditor.editorUpload.blobCache;
            const base64 = (reader.result as string).split(',')[1];
            const blobInfo = blobCache.create(id, file, base64);
            blobCache.add(blobInfo);
            cb(blobInfo.blobUri(), { title: file.name });
          });
          reader.readAsDataURL(file);
        });
        input.click();    
    }

  };

  getConfig(overrides?: Partial<EditorComponent['init']>): EditorComponent['init'] {
    return { ...(this.baseConfig as object), ...(overrides as object) } as EditorComponent['init'];
  }
}
