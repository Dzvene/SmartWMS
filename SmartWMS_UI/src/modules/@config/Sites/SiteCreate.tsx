import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { useCreateSiteMutation } from '@/api/modules/sites';
import { CONFIG } from '@/constants/routes';
import { SiteForm, type SiteFormData } from './SiteForm';
import './Sites.scss';

export function SiteCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createSite, { isLoading: isCreating }] = useCreateSiteMutation();

  const handleBack = () => {
    navigate(CONFIG.SITES);
  };

  const handleSubmit = async (data: SiteFormData) => {
    try {
      // tenantId is automatically injected by baseApi
      await createSite(data).unwrap();
      navigate(CONFIG.SITES);
    } catch (error) {
      console.error('Failed to create site:', error);
    }
  };

  return (
    <div className="site-details">
      {/* Header with back button */}
      <header className="site-details__header">
        <div className="site-details__header-left">
          <button className="btn btn-ghost" onClick={handleBack}>
            <span className="btn__icon">&larr;</span>
            {t('common.back', 'Back')}
          </button>
          <div className="site-details__title-section">
            <h1 className="site-details__title">{t('site.addSite', 'Add Site')}</h1>
          </div>
        </div>
      </header>

      {/* Main content */}
      <div className="site-details__content">
        <div className="site-details__form-container site-details__form-container--full">
          <SiteForm
            onSubmit={handleSubmit}
            loading={isCreating}
            isEditMode={false}
          />
        </div>
      </div>
    </div>
  );
}

export default SiteCreate;
