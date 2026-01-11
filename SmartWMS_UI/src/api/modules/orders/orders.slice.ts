/**
 * Orders API Slice
 * RTK Query endpoints for Orders module (Sales Orders, Purchase Orders, Customers, Suppliers)
 */

import { baseApi } from '@/api/baseApi';
import type {
  // Sales Orders
  SalesOrderFilters,
  SalesOrderResponse,
  SalesOrderListResponse,
  SalesOrderLineResponse,
  CreateSalesOrderRequest,
  UpdateSalesOrderRequest,
  UpdateSalesOrderStatusRequest,
  AddSalesOrderLineRequest,
  UpdateSalesOrderLineRequest,
  // Purchase Orders
  PurchaseOrderFilters,
  PurchaseOrderResponse,
  PurchaseOrderListResponse,
  PurchaseOrderLineResponse,
  CreatePurchaseOrderRequest,
  UpdatePurchaseOrderRequest,
  UpdatePurchaseOrderStatusRequest,
  AddPurchaseOrderLineRequest,
  UpdatePurchaseOrderLineRequest,
  ReceivePurchaseOrderLineRequest,
  // Customers
  CustomerFilters,
  CustomerResponse,
  CustomerListResponse,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  // Suppliers
  SupplierFilters,
  SupplierResponse,
  SupplierListResponse,
  CreateSupplierRequest,
  UpdateSupplierRequest,
} from './orders.types';

export const ordersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    // ========================================================================
    // Sales Orders
    // ========================================================================

    getSalesOrders: builder.query<SalesOrderListResponse, SalesOrderFilters | void>({
      query: (params) => ({
        url: '/sales-orders',
        params: params || {},
      }),
      providesTags: ['SalesOrders'],
    }),

    getSalesOrderById: builder.query<SalesOrderResponse, string>({
      query: (id) => `/sales-orders/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'SalesOrders', id }],
    }),

    createSalesOrder: builder.mutation<SalesOrderResponse, CreateSalesOrderRequest>({
      query: (body) => ({
        url: '/sales-orders',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['SalesOrders'],
    }),

    updateSalesOrder: builder.mutation<SalesOrderResponse, { id: string; body: UpdateSalesOrderRequest }>({
      query: ({ id, body }) => ({
        url: `/sales-orders/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'SalesOrders', id }, 'SalesOrders'],
    }),

    updateSalesOrderStatus: builder.mutation<SalesOrderResponse, { id: string; body: UpdateSalesOrderStatusRequest }>({
      query: ({ id, body }) => ({
        url: `/sales-orders/${id}/status`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'SalesOrders', id }, 'SalesOrders'],
    }),

    deleteSalesOrder: builder.mutation<void, string>({
      query: (id) => ({
        url: `/sales-orders/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['SalesOrders'],
    }),

    // Sales Order Lines
    addSalesOrderLine: builder.mutation<SalesOrderLineResponse, { orderId: string; body: AddSalesOrderLineRequest }>({
      query: ({ orderId, body }) => ({
        url: `/sales-orders/${orderId}/lines`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'SalesOrders', id: orderId }],
    }),

    updateSalesOrderLine: builder.mutation<SalesOrderLineResponse, { orderId: string; lineId: string; body: UpdateSalesOrderLineRequest }>({
      query: ({ orderId, lineId, body }) => ({
        url: `/sales-orders/${orderId}/lines/${lineId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'SalesOrders', id: orderId }],
    }),

    deleteSalesOrderLine: builder.mutation<void, { orderId: string; lineId: string }>({
      query: ({ orderId, lineId }) => ({
        url: `/sales-orders/${orderId}/lines/${lineId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'SalesOrders', id: orderId }],
    }),

    // Sales Order Actions
    allocateSalesOrder: builder.mutation<SalesOrderResponse, string>({
      query: (id) => ({
        url: `/sales-orders/${id}/allocate`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'SalesOrders', id }, 'SalesOrders'],
    }),

    releaseSalesOrder: builder.mutation<SalesOrderResponse, string>({
      query: (id) => ({
        url: `/sales-orders/${id}/release`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'SalesOrders', id }, 'SalesOrders'],
    }),

    cancelSalesOrder: builder.mutation<SalesOrderResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/sales-orders/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'SalesOrders', id }, 'SalesOrders'],
    }),

    // ========================================================================
    // Purchase Orders
    // ========================================================================

    getPurchaseOrders: builder.query<PurchaseOrderListResponse, PurchaseOrderFilters | void>({
      query: (params) => ({
        url: '/purchase-orders',
        params: params || {},
      }),
      providesTags: ['PurchaseOrders'],
    }),

    getPurchaseOrderById: builder.query<PurchaseOrderResponse, string>({
      query: (id) => `/purchase-orders/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'PurchaseOrders', id }],
    }),

    createPurchaseOrder: builder.mutation<PurchaseOrderResponse, CreatePurchaseOrderRequest>({
      query: (body) => ({
        url: '/purchase-orders',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['PurchaseOrders'],
    }),

    updatePurchaseOrder: builder.mutation<PurchaseOrderResponse, { id: string; body: UpdatePurchaseOrderRequest }>({
      query: ({ id, body }) => ({
        url: `/purchase-orders/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PurchaseOrders', id }, 'PurchaseOrders'],
    }),

    updatePurchaseOrderStatus: builder.mutation<PurchaseOrderResponse, { id: string; body: UpdatePurchaseOrderStatusRequest }>({
      query: ({ id, body }) => ({
        url: `/purchase-orders/${id}/status`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PurchaseOrders', id }, 'PurchaseOrders'],
    }),

    deletePurchaseOrder: builder.mutation<void, string>({
      query: (id) => ({
        url: `/purchase-orders/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['PurchaseOrders'],
    }),

    // Purchase Order Lines
    addPurchaseOrderLine: builder.mutation<PurchaseOrderLineResponse, { orderId: string; body: AddPurchaseOrderLineRequest }>({
      query: ({ orderId, body }) => ({
        url: `/purchase-orders/${orderId}/lines`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'PurchaseOrders', id: orderId }],
    }),

    updatePurchaseOrderLine: builder.mutation<PurchaseOrderLineResponse, { orderId: string; lineId: string; body: UpdatePurchaseOrderLineRequest }>({
      query: ({ orderId, lineId, body }) => ({
        url: `/purchase-orders/${orderId}/lines/${lineId}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'PurchaseOrders', id: orderId }],
    }),

    deletePurchaseOrderLine: builder.mutation<void, { orderId: string; lineId: string }>({
      query: ({ orderId, lineId }) => ({
        url: `/purchase-orders/${orderId}/lines/${lineId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'PurchaseOrders', id: orderId }],
    }),

    receivePurchaseOrderLine: builder.mutation<PurchaseOrderLineResponse, { orderId: string; lineId: string; body: ReceivePurchaseOrderLineRequest }>({
      query: ({ orderId, lineId, body }) => ({
        url: `/purchase-orders/${orderId}/lines/${lineId}/receive`,
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { orderId }) => [{ type: 'PurchaseOrders', id: orderId }, 'PurchaseOrders'],
    }),

    // Purchase Order Actions
    confirmPurchaseOrder: builder.mutation<PurchaseOrderResponse, string>({
      query: (id) => ({
        url: `/purchase-orders/${id}/confirm`,
        method: 'POST',
      }),
      invalidatesTags: (_result, _error, id) => [{ type: 'PurchaseOrders', id }, 'PurchaseOrders'],
    }),

    cancelPurchaseOrder: builder.mutation<PurchaseOrderResponse, { id: string; reason?: string }>({
      query: ({ id, reason }) => ({
        url: `/purchase-orders/${id}/cancel`,
        method: 'POST',
        body: { reason },
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PurchaseOrders', id }, 'PurchaseOrders'],
    }),

    // ========================================================================
    // Customers
    // ========================================================================

    getCustomers: builder.query<CustomerListResponse, CustomerFilters | void>({
      query: (params) => ({
        url: '/customers',
        params: params || {},
      }),
      providesTags: ['Customers'],
    }),

    getCustomerById: builder.query<CustomerResponse, string>({
      query: (id) => `/customers/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Customers', id }],
    }),

    createCustomer: builder.mutation<CustomerResponse, CreateCustomerRequest>({
      query: (body) => ({
        url: '/customers',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Customers'],
    }),

    updateCustomer: builder.mutation<CustomerResponse, { id: string; body: UpdateCustomerRequest }>({
      query: ({ id, body }) => ({
        url: `/customers/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Customers', id }, 'Customers'],
    }),

    deleteCustomer: builder.mutation<void, string>({
      query: (id) => ({
        url: `/customers/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Customers'],
    }),

    // ========================================================================
    // Suppliers
    // ========================================================================

    getSuppliers: builder.query<SupplierListResponse, SupplierFilters | void>({
      query: (params) => ({
        url: '/suppliers',
        params: params || {},
      }),
      providesTags: ['Suppliers'],
    }),

    getSupplierById: builder.query<SupplierResponse, string>({
      query: (id) => `/suppliers/${id}`,
      providesTags: (_result, _error, id) => [{ type: 'Suppliers', id }],
    }),

    createSupplier: builder.mutation<SupplierResponse, CreateSupplierRequest>({
      query: (body) => ({
        url: '/suppliers',
        method: 'POST',
        body,
      }),
      invalidatesTags: ['Suppliers'],
    }),

    updateSupplier: builder.mutation<SupplierResponse, { id: string; body: UpdateSupplierRequest }>({
      query: ({ id, body }) => ({
        url: `/suppliers/${id}`,
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Suppliers', id }, 'Suppliers'],
    }),

    deleteSupplier: builder.mutation<void, string>({
      query: (id) => ({
        url: `/suppliers/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Suppliers'],
    }),
  }),
});

// Export hooks
export const {
  // Sales Orders
  useGetSalesOrdersQuery,
  useLazyGetSalesOrdersQuery,
  useGetSalesOrderByIdQuery,
  useLazyGetSalesOrderByIdQuery,
  useCreateSalesOrderMutation,
  useUpdateSalesOrderMutation,
  useUpdateSalesOrderStatusMutation,
  useDeleteSalesOrderMutation,
  useAddSalesOrderLineMutation,
  useUpdateSalesOrderLineMutation,
  useDeleteSalesOrderLineMutation,
  useAllocateSalesOrderMutation,
  useReleaseSalesOrderMutation,
  useCancelSalesOrderMutation,

  // Purchase Orders
  useGetPurchaseOrdersQuery,
  useLazyGetPurchaseOrdersQuery,
  useGetPurchaseOrderByIdQuery,
  useLazyGetPurchaseOrderByIdQuery,
  useCreatePurchaseOrderMutation,
  useUpdatePurchaseOrderMutation,
  useUpdatePurchaseOrderStatusMutation,
  useDeletePurchaseOrderMutation,
  useAddPurchaseOrderLineMutation,
  useUpdatePurchaseOrderLineMutation,
  useDeletePurchaseOrderLineMutation,
  useReceivePurchaseOrderLineMutation,
  useConfirmPurchaseOrderMutation,
  useCancelPurchaseOrderMutation,

  // Customers
  useGetCustomersQuery,
  useLazyGetCustomersQuery,
  useGetCustomerByIdQuery,
  useLazyGetCustomerByIdQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,

  // Suppliers
  useGetSuppliersQuery,
  useLazyGetSuppliersQuery,
  useGetSupplierByIdQuery,
  useLazyGetSupplierByIdQuery,
  useCreateSupplierMutation,
  useUpdateSupplierMutation,
  useDeleteSupplierMutation,
} = ordersApi;
