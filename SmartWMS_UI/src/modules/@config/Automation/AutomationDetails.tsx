import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { AutomationRuleForm, type AutomationRuleFormData, triggerLabels, actionLabels } from './AutomationRuleForm';
import {
  useGetAutomationRuleByIdQuery,
  useUpdateAutomationRuleMutation,
  useDeleteAutomationRuleMutation,
} from '@/api/modules/automation';
import type { UpdateAutomationRuleRequest } from '@/api/modules/automation';
import { Modal } from '@/components';

export function AutomationDetails() {
  const { id } = useParams<{ id: string }>();
  const t = useTranslate();
  const navigate = useNavigate();

  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);

  // Fetch rule data
  const { data: ruleResponse, isLoading: isLoadingRule } = useGetAutomationRuleByIdQuery(id!, {
    skip: !id,
  });

  // Mutations
  const [updateRule, { isLoading: isUpdating }] = useUpdateAutomationRuleMutation();
  const [deleteRule, { isLoading: isDeleting }] = useDeleteAutomationRuleMutation();

  const rule = ruleResponse?.data;

  const handleBack = () => {
    navigate(CONFIG.AUTOMATION);
  };

  const handleSubmit = async (data: AutomationRuleFormData) => {
    if (!id) return;

    try {
      const actionConfigJson =
        Object.keys(data.actionConfig || {}).length > 0
          ? JSON.stringify(data.actionConfig)
          : undefined;

      const updateData: UpdateAutomationRuleRequest = {
        name: data.name,
        description: data.description || undefined,
        isActive: data.isActive,
        triggerType: data.triggerType,
        entityType: data.entityType || undefined,
        actionType: data.actionType,
        priority: data.priority,
        cronExpression: data.cronExpression || undefined,
        conditions: data.conditions.length > 0 ? data.conditions : undefined,
        actionConfigJson,
      };

      await updateRule({ id, data: updateData }).unwrap();
    } catch (error) {
      console.error('Failed to update automation rule:', error);
    }
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteRule(id).unwrap();
      setDeleteConfirmOpen(false);
      navigate(CONFIG.AUTOMATION);
    } catch (error) {
      console.error('Failed to delete automation rule:', error);
    }
  };

  if (isLoadingRule) {
    return (
      <div className="detail-page">
        <div className="detail-page__loading">{t('common.loading', 'Loading...')}</div>
      </div>
    );
  }

  if (!rule) {
    return (
      <div className="detail-page">
        <div className="detail-page__error">
          {t('automation.ruleNotFound', 'Automation rule not found')}
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
            <h1 className="detail-page__title">{rule.name}</h1>
            <div className="detail-page__meta">
              <span
                className={`status-badge status-badge--${rule.isActive ? 'success' : 'neutral'}`}
              >
                {rule.isActive ? t('common.active', 'Active') : t('common.inactive', 'Inactive')}
              </span>
              <span className="badge badge--info">{triggerLabels[rule.triggerType]}</span>
              <span className="badge badge--secondary">{actionLabels[rule.actionType]}</span>
            </div>
          </div>
        </div>

        <div className="detail-page__header-actions">
          <button className="btn btn-danger" onClick={() => setDeleteConfirmOpen(true)}>
            {t('common.delete', 'Delete')}
          </button>
        </div>
      </header>

      <div className="detail-page__content">
        <AutomationRuleForm
          initialData={rule}
          onSubmit={handleSubmit}
          loading={isUpdating}
          isEditMode={true}
        />
      </div>

      {/* Delete Confirmation Modal */}
      <Modal
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
        title={t('automation.deleteRule', 'Delete Automation Rule')}
      >
        <div className="modal__body">
          <p>
            {t(
              'automation.deleteConfirmation',
              `Are you sure you want to delete "${rule.name}"? This action cannot be undone.`
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
