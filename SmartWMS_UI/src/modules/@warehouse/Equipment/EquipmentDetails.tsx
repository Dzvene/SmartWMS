import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { WAREHOUSE } from '@/constants/routes';
import { EquipmentForm, type EquipmentFormData } from './EquipmentForm';
import {
  useGetEquipmentByIdQuery,
  useUpdateEquipmentMutation,
  useDeleteEquipmentMutation,
} from '@/api/modules/equipment';
import type { UpdateEquipmentRequest, EquipmentStatus } from '@/api/modules/equipment';
import { Modal } from '@/components';

const STATUS_CLASSES: Record<EquipmentStatus, string> = {
  Available: 'success',
  InUse: 'info',
  Maintenance: 'warning',
  OutOfService: 'error',
  Reserved: 'neutral',
};

export function EquipmentDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch equipment data
  const { data: equipmentResponse, isLoading: isLoadingEquipment } = useGetEquipmentByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateEquipment, { isLoading: isUpdating }] = useUpdateEquipmentMutation();
  const [deleteEquipment, { isLoading: isDeleting }] = useDeleteEquipmentMutation();

  const equipment = equipmentResponse?.data;

  const handleBack = () => {
    navigate(WAREHOUSE.EQUIPMENT);
  };

  const handleSubmit = async (data: EquipmentFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateEquipmentRequest = {
        name: data.name,
        description: data.description || undefined,
        type: data.type,
        status: data.status,
        warehouseId: data.warehouseId || undefined,
        serialNumber: data.serialNumber || undefined,
        manufacturer: data.manufacturer || undefined,
        model: data.model || undefined,
        purchaseDate: data.purchaseDate || undefined,
        warrantyExpiryDate: data.warrantyExpiryDate || undefined,
        nextMaintenanceDate: data.nextMaintenanceDate || undefined,
        isActive: data.isActive,
      };

      await updateEquipment({ id, body: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update equipment:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteEquipment(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(WAREHOUSE.EQUIPMENT);
    } catch (error) {
      console.error('Failed to delete equipment:', error);
    }
  };

  if (isLoadingEquipment) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!equipment) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('equipment.notFound', 'Equipment not found')}
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
              {equipment.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${STATUS_CLASSES[equipment.status]}`}>
                {equipment.statusName || equipment.status}
              </span>
              <span className="detail-page__subtitle">
                {equipment.code}
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
        <EquipmentForm
          initialData={equipment}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('equipment.deleteEquipment', 'Delete Equipment')}
      >
        <div className="modal__body">
          <p>
            {t(
              'equipment.deleteConfirmation',
              `Are you sure you want to delete "${equipment.name}"? This action cannot be undone.`
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
