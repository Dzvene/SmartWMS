import { useTranslate } from '@/hooks';
import { useNavigate } from 'react-router-dom';
import { CONFIG } from '@/constants/routes';
import { IntegrationForm, type IntegrationFormData } from './IntegrationForm';
import { useCreateIntegrationMutation } from '@/api/modules/integrations';
import type { CreateIntegrationRequest } from '@/api/modules/integrations';

export function IntegrationCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createIntegration, { isLoading: isCreating }] = useCreateIntegrationMutation();

  const handleBack = () => {
    navigate(CONFIG.INTEGRATIONS);
  };

  const handleSubmit = async (data: IntegrationFormData) => {
    try {
      const createData: CreateIntegrationRequest = {
        name: data.name,
        integrationType: data.integrationType,
        providerName: data.providerName,
        description: data.description || undefined,
        baseUrl: data.baseUrl || undefined,
        apiVersion: data.apiVersion || undefined,
        authType: data.authType || undefined,
        syncSchedule: data.syncSchedule || undefined,
      };

      const result = await createIntegration(createData).unwrap();

      // Navigate to the created integration or back to list
      if (result.data?.id) {
        navigate(`${CONFIG.INTEGRATIONS}/${result.data.id}`);
      } else {
        navigate(CONFIG.INTEGRATIONS);
      }
    } catch (error) {
      console.error('Failed to create integration:', error);
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
              {t('integrations.addIntegration', 'Add Integration')}
            </h1>
            <span className="detail-page__subtitle">
              {t('integrations.configureConnection', 'Configure external system connection')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <IntegrationForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
