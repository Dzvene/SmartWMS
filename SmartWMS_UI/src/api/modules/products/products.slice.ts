/**
 * Products API Slice
 * RTK Query endpoints for Products module
 *
 * Endpoints:
 * - /products - Product CRUD
 * - /product-categories - Category CRUD
 *
 * Note: tenantId is automatically injected by baseApi
 */

import { baseApi } from '@/api/baseApi';
import type { ApiResponse } from '@/api/types';
import type {
  ProductsListParams,
  ProductsListResponse,
  ProductDetailResponse,
  CreateProductRequest,
  UpdateProductRequest,
  CategoriesListParams,
  CategoriesListResponse,
  CategoryDetailResponse,
  CategoryTreeResponse,
  CreateProductCategoryRequest,
  UpdateProductCategoryRequest,
} from './products.types';

export const productsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Products CRUD
    // ========================================================================

    getProducts: builder.query<ProductsListResponse, ProductsListParams | void>({
      query: ({
        page = 1,
        pageSize = 25,
        search,
        categoryId,
        isActive,
      }: ProductsListParams = {}) => ({
        url: `/products`,
        params: {
          page,
          pageSize,
          search,
          categoryId,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'Product' as const, id })),
              { type: 'Product', id: 'LIST' },
            ]
          : [{ type: 'Product', id: 'LIST' }],
    }),

    getProductById: builder.query<ProductDetailResponse, string>({
      query: (id: string) => `/products/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Product', id }],
    }),

    getProductBySku: builder.query<ProductDetailResponse, string>({
      query: (sku: string) => `/products/sku/${encodeURIComponent(sku)}`,
      providesTags: (result) =>
        result?.data ? [{ type: 'Product', id: result.data.id }] : [],
    }),

    getProductByBarcode: builder.query<ProductDetailResponse, string>({
      query: (barcode: string) => `/products/barcode/${encodeURIComponent(barcode)}`,
      providesTags: (result) =>
        result?.data ? [{ type: 'Product', id: result.data.id }] : [],
    }),

    createProduct: builder.mutation<ProductDetailResponse, CreateProductRequest>({
      query: (body: CreateProductRequest) => ({
        url: `/products`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [{ type: 'Product', id: 'LIST' }],
    }),

    updateProduct: builder.mutation<ProductDetailResponse, { id: string; data: UpdateProductRequest }>({
      query: ({ id, data }: { id: string; data: UpdateProductRequest }) => ({
        url: `/products/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'Product', id },
        { type: 'Product', id: 'LIST' },
      ],
    }),

    deleteProduct: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/products/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'Product', id },
        { type: 'Product', id: 'LIST' },
      ],
    }),

    // ========================================================================
    // Product Categories CRUD
    // ========================================================================

    getProductCategories: builder.query<CategoriesListResponse, CategoriesListParams | void>({
      query: ({
        page = 1,
        pageSize = 25,
        search,
        parentCategoryId,
        isActive,
      }: CategoriesListParams = {}) => ({
        url: `/product-categories`,
        params: {
          page,
          pageSize,
          search,
          parentCategoryId,
          isActive,
        },
      }),
      providesTags: (result) =>
        result?.data
          ? [
              ...result.data.items.map(({ id }) => ({ type: 'ProductCategory' as const, id })),
              { type: 'ProductCategory', id: 'LIST' },
            ]
          : [{ type: 'ProductCategory', id: 'LIST' }],
    }),

    getProductCategoryTree: builder.query<CategoryTreeResponse, void>({
      query: () => `/product-categories/tree`,
      providesTags: [{ type: 'ProductCategory', id: 'TREE' }],
    }),

    getProductCategoryById: builder.query<CategoryDetailResponse, string>({
      query: (id: string) => `/product-categories/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'ProductCategory', id }],
    }),

    createProductCategory: builder.mutation<CategoryDetailResponse, CreateProductCategoryRequest>({
      query: (body: CreateProductCategoryRequest) => ({
        url: `/product-categories`,
        method: 'POST',
        body,
      }),
      invalidatesTags: [
        { type: 'ProductCategory', id: 'LIST' },
        { type: 'ProductCategory', id: 'TREE' },
      ],
    }),

    updateProductCategory: builder.mutation<CategoryDetailResponse, { id: string; data: UpdateProductCategoryRequest }>({
      query: ({ id, data }: { id: string; data: UpdateProductCategoryRequest }) => ({
        url: `/product-categories/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: 'ProductCategory', id },
        { type: 'ProductCategory', id: 'LIST' },
        { type: 'ProductCategory', id: 'TREE' },
      ],
    }),

    deleteProductCategory: builder.mutation<ApiResponse<void>, string>({
      query: (id: string) => ({
        url: `/product-categories/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: 'ProductCategory', id },
        { type: 'ProductCategory', id: 'LIST' },
        { type: 'ProductCategory', id: 'TREE' },
      ],
    }),
  }),
});

// Export hooks
export const {
  // Products
  useGetProductsQuery,
  useLazyGetProductsQuery,
  useGetProductByIdQuery,
  useLazyGetProductByIdQuery,
  useGetProductBySkuQuery,
  useLazyGetProductBySkuQuery,
  useGetProductByBarcodeQuery,
  useLazyGetProductByBarcodeQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  // Categories
  useGetProductCategoriesQuery,
  useLazyGetProductCategoriesQuery,
  useGetProductCategoryTreeQuery,
  useLazyGetProductCategoryTreeQuery,
  useGetProductCategoryByIdQuery,
  useLazyGetProductCategoryByIdQuery,
  useCreateProductCategoryMutation,
  useUpdateProductCategoryMutation,
  useDeleteProductCategoryMutation,
} = productsApi;
