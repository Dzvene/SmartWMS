import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { AutomationRuleForm, type AutomationRuleFormData } from './AutomationRuleForm';
import { useCreateAutomationRuleMutation } from '@/api/modules/automation';
import type { CreateAutomationRuleRequest } from '@/api/modules/automation';

export function AutomationCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createRule, { isLoading: isCreating }] = useCreateAutomationRuleMutation();

  const handleBack = () => {
    navigate(CONFIG.AUTOMATION);
  };

  const handleSubmit = async (data: AutomationRuleFormData) => {
    try {
      const actionConfigJson =
        Object.keys(data.actionConfig || {}).length > 0
          ? JSON.stringify(data.actionConfig)
          : undefined;

      const createData: CreateAutomationRuleRequest = {
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

      const result = await createRule(createData).unwrap();

      // Navigate to the created rule or back to list
      if (result.data?.id) {
        navigate(`${CONFIG.AUTOMATION}/${result.data.id}`);
      } else {
        navigate(CONFIG.AUTOMATION);
      }
    } catch (error) {
      console.error('Failed to create automation rule:', error);
    }
  };

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
              {t('automation.createRule', 'Create Automation Rule')}
            </h1>
            <span className="detail-page__subtitle">
              {t('automation.createRuleSubtitle', 'Fill in the details to create a new automation rule')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <AutomationRuleForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
