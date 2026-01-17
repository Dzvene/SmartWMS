import { useState } from 'react';
import { useIntl } from 'react-intl';
import { useNavigate, useParams } from 'react-router-dom';
import { CONFIG } from '@/constants/routes';
import { CarrierForm, type CarrierFormData } from './CarrierForm';
import {
  useGetCarrierByIdQuery,
  useUpdateCarrierMutation,
  useDeleteCarrierMutation,
} from '@/api/modules/carriers';
import type { UpdateCarrierRequest } from '@/api/modules/carriers';
import { Modal } from '@/components';

export function CarrierDetails() {
  const { id } = useParams<{ id: string }>();
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch carrier data
  const { data: carrierResponse, isLoading: isLoadingCarrier } = useGetCarrierByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateCarrier, { isLoading: isUpdating }] = useUpdateCarrierMutation();
  const [deleteCarrier, { isLoading: isDeleting }] = useDeleteCarrierMutation();

  const carrier = carrierResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.CARRIERS);
  };

  const handleSubmit = async (data: CarrierFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateCarrierRequest = {
        name: data.name,
        description: data.description || undefined,
        contactName: data.contactName || undefined,
        phone: data.phone || undefined,
        email: data.email || undefined,
        website: data.website || undefined,
        accountNumber: data.accountNumber || undefined,
        integrationType: data.integrationType,
        defaultServiceCode: data.defaultServiceCode || undefined,
        notes: data.notes || undefined,
        isActive: data.isActive,
      };

      await updateCarrier({ id, body: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update carrier:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteCarrier(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.CARRIERS);
    } catch (error) {
      console.error('Failed to delete carrier:', error);
    }
  };

  if (isLoadingCarrier) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!carrier) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('carriers.carrierNotFound', 'Carrier not found')}
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
              {carrier.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${carrier.isActive ? 'success' : 'neutral'}`}>
                {carrier.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="detail-page__subtitle">
                {carrier.code}
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
        <CarrierForm
          initialData={carrier}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('carriers.deleteCarrier', 'Delete Carrier')}
      >
        <div className="modal__body">
          <p>
            {t(
              'carriers.deleteConfirmation',
              `Are you sure you want to delete ${carrier.name}? This will also remove all associated services.`
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
