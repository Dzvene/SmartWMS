/**
 * API Request/Response Types
 * Standard structures for API communication
 */

/**
 * Pagination request parameters
 */
export interface QueryParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
  search?: string;
}

/**
 * Pagination parameters for API filters
 */
export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Paginated data structure
 */
export interface PaginatedData<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Paginated response from server (data structure only, wrap in ApiResponse when needed)
 */
export type PaginatedResponse<T> = PaginatedData<T>;

/**
 * Standard API response wrapper
 */
export interface ApiResponse<T> {
  data?: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

/**
 * API error details
 */
export interface ErrorResponse {
  code: string;
  message: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

/**
 * File upload response
 */
export interface FileUploadResponse {
  fileId: string;
  fileName: string;
  fileSize: number;
  contentType: string;
  url?: string;
}

/**
 * Bulk operation result
 */
export interface BulkOperationResult {
  successCount: number;
  failureCount: number;
  errors?: Array<{
    id: string;
    error: string;
  }>;
}
