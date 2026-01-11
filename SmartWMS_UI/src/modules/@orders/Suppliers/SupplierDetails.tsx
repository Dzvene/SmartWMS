import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { ORDERS } from '@/constants/routes';
import { SupplierForm, type SupplierFormData } from './SupplierForm';
import {
  useGetSupplierByIdQuery,
  useUpdateSupplierMutation,
  useDeleteSupplierMutation,
} from '@/api/modules/orders';
import type { UpdateSupplierRequest } from '@/api/modules/orders';
import { Modal } from '@/components';

export function SupplierDetails() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch supplier data
  const { data: supplierResponse, isLoading: isLoadingSupplier } = useGetSupplierByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateSupplier, { isLoading: isUpdating }] = useUpdateSupplierMutation();
  const [deleteSupplier, { isLoading: isDeleting }] = useDeleteSupplierMutation();

  const supplier = supplierResponse?.data;

  const handleBack = () => {
    navigate(ORDERS.SUPPLIERS);
  };

  const handleSubmit = async (data: SupplierFormData) => {
    if (!id) return;

    try {
      const leadTimeDays = data.leadTimeDays === '' ? undefined : Number(data.leadTimeDays);

      const updateData: UpdateSupplierRequest = {
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
        leadTimeDays,
        isActive: data.isActive,
      };

      await updateSupplier({ id, body: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update supplier:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteSupplier(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(ORDERS.SUPPLIERS);
    } catch (error) {
      console.error('Failed to delete supplier:', error);
    }
  };

  if (isLoadingSupplier) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!supplier) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('suppliers.supplierNotFound', 'Supplier not found')}
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
              {supplier.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${supplier.isActive ? 'success' : 'neutral'}`}>
                {supplier.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="detail-page__subtitle">
                {supplier.code}
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
        <SupplierForm
          initialData={supplier}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('suppliers.deleteSupplier', 'Delete Supplier')}
      >
        <div className="modal__body">
          <p>
            {t(
              'suppliers.deleteConfirmation',
              `Are you sure you want to delete ${supplier.name}? This action cannot be undone.`
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
