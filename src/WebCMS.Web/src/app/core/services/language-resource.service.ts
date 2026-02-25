import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import
{
    LanguageResource,
    LanguageResourceResponse,
    CreateOrUpdateLanguageResourceRequest,
    LanguageFileImportRequest,
    LanguageFileExport
} from '../models/language-resource.model';
import { environment } from '../../../environments/environment';

/**
 * 語言資源 API 服務
 */
@Injectable({
    providedIn: 'root'
})
export class LanguageResourceService
{
    private apiUrl = `${environment.apiUrl}/api/languageResource`;

    constructor(private http: HttpClient) { }

    /**
     * 取得特定語言的所有資源
     */
    getResourcesByLanguage(languageCode: string): Observable<LanguageResourceResponse<LanguageResource[]>>
    {
        return this.http.get<LanguageResourceResponse<LanguageResource[]>>(
            `${this.apiUrl}/${languageCode}`
        );
    }

    /**
     * 取得特定語言的單個資源
     */
    getResourceByKey(languageCode: string, resourceKey: string): Observable<LanguageResourceResponse<LanguageResource>>
    {
        return this.http.get<LanguageResourceResponse<LanguageResource>>(
            `${this.apiUrl}/${languageCode}/${resourceKey}`
        );
    }

    /**
     * 建立新的語言資源
     */
    createResource(request: CreateOrUpdateLanguageResourceRequest): Observable<LanguageResourceResponse<LanguageResource>>
    {
        return this.http.post<LanguageResourceResponse<LanguageResource>>(
            this.apiUrl,
            request
        );
    }

    /**
     * 更新語言資源
     */
    updateResource(id: number, request: CreateOrUpdateLanguageResourceRequest): Observable<LanguageResourceResponse<LanguageResource>>
    {
        return this.http.put<LanguageResourceResponse<LanguageResource>>(
            `${this.apiUrl}/${id}`,
            request
        );
    }

    /**
     * 刪除語言資源
     */
    deleteResource(id: string): Observable<LanguageResourceResponse<any>>
    {
        return this.http.delete<LanguageResourceResponse<any>>(
            `${this.apiUrl}/${id}`
        );
    }

    /**
     * 匯入語言資源（JSON）
     */
    importResources(request: LanguageFileImportRequest): Observable<LanguageResourceResponse<any>>
    {
        // 如果 fileContent 是物件，轉換為 JSON 字符串
        const payload = {
            languageCode: request.languageCode,
            fileContent: typeof request.fileContent === 'string'
                ? request.fileContent
                : JSON.stringify(request.fileContent),
            overwrite: request.overwrite || false
        };

        return this.http.post<LanguageResourceResponse<any>>(
            `${this.apiUrl}/${request.languageCode}/import`,
            payload
        );
    }

    /**
     * 匯出語言資源（JSON）
     */
    exportResources(languageCode: string): Observable<LanguageFileExport>
    {
        return this.http.get<LanguageFileExport>(
            `${this.apiUrl}/${languageCode}/export`
        );
    }

    /**
     * 驗證語言檔案格式
     */
    validateLanguageFile(fileContent: Record<string, any>): Observable<LanguageResourceResponse<any>>
    {
        return this.http.post<LanguageResourceResponse<any>>(
            `${this.apiUrl}/validate`,
            { fileContent }
        );
    }

    /**
     * 批次建立或更新資源
     */
    batchCreateOrUpdateResources(
        languageCode: string,
        resources: CreateOrUpdateLanguageResourceRequest[]
    ): Observable<LanguageResourceResponse<LanguageResource[]>>
    {
        return this.http.post<LanguageResourceResponse<LanguageResource[]>>(
            `${this.apiUrl}/${languageCode}/batch`,
            { resources }
        );
    }
}
