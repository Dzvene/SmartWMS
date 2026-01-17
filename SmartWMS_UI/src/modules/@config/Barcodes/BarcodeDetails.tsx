import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { BarcodeForm, type BarcodeFormData } from './BarcodeForm';
import { Modal } from '@/components';
import {
  useGetBarcodePrefixByIdQuery,
  useUpdateBarcodePrefixMutation,
  useDeleteBarcodePrefixMutation,
} from '@/api/modules/configuration';
import type { UpdateBarcodePrefixRequest } from '@/api/modules/configuration';

const typeLabels: Record<string, string> = {
  Product: 'Product',
  Location: 'Location',
  Container: 'Container',
  Pallet: 'Pallet',
  Order: 'Order',
  Transfer: 'Transfer',
  Receipt: 'Receipt',
  Shipment: 'Shipment',
  User: 'User',
  Equipment: 'Equipment',
  Other: 'Other',
};

export function BarcodeDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  const { data: barcodeResponse, isLoading: isLoadingBarcode } = useGetBarcodePrefixByIdQuery(id!, {
    skip: !id,
  });

  const [updateBarcodePrefix, { isLoading: isUpdating }] = useUpdateBarcodePrefixMutation();
  const [deleteBarcodePrefix, { isLoading: isDeleting }] = useDeleteBarcodePrefixMutation();

  const barcode = barcodeResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.BARCODES);
  };

  const handleSubmit = async (data: BarcodeFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateBarcodePrefixRequest = {
        prefix: data.prefix,
        description: data.description || undefined,
        isActive: data.isActive,
      };

      await updateBarcodePrefix({ id, data: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update barcode prefix:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteBarcodePrefix(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.BARCODES);
    } catch (error) {
      console.error('Failed to delete barcode prefix:', error);
    }
  };

  if (isLoadingBarcode) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!barcode) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('barcodes.barcodeNotFound', 'Barcode prefix not found')}
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
              {barcode.prefix}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${barcode.isActive ? 'success' : 'neutral'}`}>
                {barcode.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="detail-page__subtitle">
                {typeLabels[barcode.prefixType] || barcode.prefixType}
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
        <BarcodeForm
          initialData={barcode}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('barcodes.deletePrefix', 'Delete Barcode Prefix')}
      >
        <div className="modal__body">
          <p>
            {t(
              'barcodes.deleteConfirmation',
              `Are you sure you want to delete prefix "${barcode.prefix}"? This action cannot be undone.`
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

export default BarcodeDetails;
