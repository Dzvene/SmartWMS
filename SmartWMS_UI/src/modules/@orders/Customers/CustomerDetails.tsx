import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { CustomerForm, type CustomerFormData } from './CustomerForm';
import {
  useGetCustomerByIdQuery,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
} from '@/api/modules/orders';
import type { UpdateCustomerRequest } from '@/api/modules/orders';
import { Modal } from '@/components';

export function CustomerDetails() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch customer data
  const { data: customerResponse, isLoading: isLoadingCustomer } = useGetCustomerByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateCustomer, { isLoading: isUpdating }] = useUpdateCustomerMutation();
  const [deleteCustomer, { isLoading: isDeleting }] = useDeleteCustomerMutation();

  const customer = customerResponse?.data;

  const handleBack = () => {
    navigate(ORDERS.CUSTOMERS);
  };

  const handleSubmit = async (data: CustomerFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateCustomerRequest = {
        name: data.name,
        contactName: data.contactName || undefined,
        email: data.email || undefined,
        phone: data.phone || undefined,
        addressLine1: data.addressLine1 || undefined,
        addressLine2: data.addressLine2 || undefined,
        city: data.city || undefined,
        region: data.region || undefined,
        postalCode: data.postalCode || undefined,
        countryCode: data.countryCode || undefined,
        taxId: data.taxId || undefined,
        paymentTerms: data.paymentTerms || undefined,
        isActive: data.isActive,
      };

      await updateCustomer({ id, body: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update customer:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteCustomer(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(ORDERS.CUSTOMERS);
    } catch (error) {
      console.error('Failed to delete customer:', error);
    }
  };

  if (isLoadingCustomer) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!customer) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('customers.customerNotFound', 'Customer not found')}
          <button className="btn btn-primary" onClick={handleBack}>
            {t('common.backToList', 'Back to List')}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="detail-page">
      <header className="detail-page__header">
        <div className="detail-page__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span>&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="detail-page__title-section">
            <h1 className="detail-page__title">
              {customer.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${customer.isActive ? 'success' : 'neutral'}`}>
                {customer.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="detail-page__subtitle">
                {customer.code}
              </span>
            </div>
          </div>
        </div>

        <div className="detail-page__header-actions">
          <button
            className="btn btn-danger"
            onClick={() => setDeleteConfirmOpen(true)}
          >
            {t('common.delete', 'Delete')}
          </button>
        </div>
      </header>

      <div className="detail-page__content">
        <CustomerForm
          initialData={customer}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('customers.deleteCustomer', 'Delete Customer')}
      >
        <div className="modal__body">
          <p>
            {t(
              'customers.deleteConfirmation',
              `Are you sure you want to delete ${customer.name}? This action cannot be undone.`
            )}
          </p>
        </div>
        <div className="modal__actions">
          <button className="btn btn-ghost" onClick={() => setDeleteConfirmOpen(false)}>
            {t('common.cancel', 'Cancel')}
          </button>
          <button className="btn btn-danger" onClick={handleDelete} disabled={isDeleting}>
            {isDeleting ? t('common.deleting', 'Deleting...') : t('common.delete', 'Delete')}
          </button>
        </div>
      </Modal>
    </div>
  );
}
