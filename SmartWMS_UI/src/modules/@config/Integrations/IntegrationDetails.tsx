import { useState } from 'react';
import { useTranslate } from '@/hooks';
import { useNavigate, useParams } from 'react-router-dom';
import { CONFIG } from '@/constants/routes';
import { IntegrationForm, type IntegrationFormData } from './IntegrationForm';
import {
  useGetIntegrationByIdQuery,
  useUpdateIntegrationMutation,
  useDeleteIntegrationMutation,
  useActivateIntegrationMutation,
  useDeactivateIntegrationMutation,
  useTestIntegrationConnectionMutation,
} from '@/api/modules/integrations';
import type { UpdateIntegrationRequest } from '@/api/modules/integrations';
import { Modal } from '@/components';

const statusClassMap: Record<string, string> = {
  Active: 'success',
  Inactive: 'neutral',
  Error: 'error',
  Pending: 'warning',
  Disconnected: 'neutral',
};

export function IntegrationDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);

  // Fetch integration data
  const { data: integrationResponse, isLoading: isLoadingIntegration } = useGetIntegrationByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateIntegration, { isLoading: isUpdating }] = useUpdateIntegrationMutation();
  const [deleteIntegration, { isLoading: isDeleting }] = useDeleteIntegrationMutation();
  const [activateIntegration] = useActivateIntegrationMutation();
  const [deactivateIntegration] = useDeactivateIntegrationMutation();
  const [testConnection, { isLoading: isTesting }] = useTestIntegrationConnectionMutation();

  const integration = integrationResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.INTEGRATIONS);
  };

  const handleSubmit = async (data: IntegrationFormData) => {
    if (!id) return;

    try {
      const updateData: UpdateIntegrationRequest = {
        name: data.name,
        description: data.description || undefined,
        baseUrl: data.baseUrl || undefined,
        apiVersion: data.apiVersion || undefined,
        syncSchedule: data.syncSchedule || undefined,
      };

      await updateIntegration({ id, data: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update integration:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteIntegration(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.INTEGRATIONS);
    } catch (error) {
      console.error('Failed to delete integration:', error);
    }
  };

  const handleToggleStatus = async () => {
    if (!id || !integration) return;

    try {
      if (integration.status === 'Active') {
        await deactivateIntegration(id).unwrap();
      } else {
        await activateIntegration(id).unwrap();
      }
    } catch (error) {
      console.error('Failed to toggle integration status:', error);
    }
  };

  const handleTestConnection = async () => {
    if (!id) return;

    try {
      const result = await testConnection(id).unwrap();
      setTestResult(result.data || { success: false, message: 'Unknown error' });
    } catch {
      setTestResult({ success: false, message: 'Connection test failed' });
    }
  };

  if (isLoadingIntegration) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">
          {t('common.loading', 'Loading...')}
        </div>
      </div>
    );
  }

  if (!integration) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('integrations.integrationNotFound', 'Integration not found')}
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
              {integration.name}
            </h1>
            <div className="detail-page__meta">
              <span className={`status-badge status-badge--${statusClassMap[integration.status] || 'neutral'}`}>
                {integration.status}
              </span>
              <span className="detail-page__subtitle">
                {integration.providerName}
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
        <IntegrationForm
          initialData={integration}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
          onTestConnection={handleTestConnection}
          isTesting={isTesting}
          testResult={testResult}
          onToggleStatus={handleToggleStatus}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('integrations.deleteIntegration', 'Delete Integration')}
      >
        <div className="modal__body">
          <p>
            {t(
              'integrations.deleteConfirmation',
              `Are you sure you want to delete "${integration.name}"? This will remove all sync history and webhooks.`
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
