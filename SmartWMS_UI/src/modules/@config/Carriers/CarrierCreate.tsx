import { useIntl } from 'react-intl';
import { useNavigate } from 'react-router-dom';
import { CONFIG } from '@/constants/routes';
import { CarrierForm, type CarrierFormData } from './CarrierForm';
import { useCreateCarrierMutation } from '@/api/modules/carriers';
import type { CreateCarrierRequest } from '@/api/modules/carriers';

export function CarrierCreate() {
  const { formatMessage } = useIntl();
  const t = (id: string, defaultMessage?: string) => formatMessage({ id, defaultMessage });
  const navigate = useNavigate();

  const [createCarrier, { isLoading: isCreating }] = useCreateCarrierMutation();

  const handleBack = () => {
    navigate(CONFIG.CARRIERS);
  };

  const handleSubmit = async (data: CarrierFormData) => {
    try {
      const createData: CreateCarrierRequest = {
        code: data.code,
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
      };

      const result = await createCarrier(createData).unwrap();

      // Navigate to the created carrier or back to list
      if (result.data?.id) {
        navigate(`${CONFIG.CARRIERS}/${result.data.id}`);
      } else {
        navigate(CONFIG.CARRIERS);
      }
    } catch (error) {
      console.error('Failed to create carrier:', error);
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
              {t('carriers.createCarrier', 'Create Carrier')}
            </h1>
            <span className="detail-page__subtitle">
              {t('carriers.createCarrierSubtitle', 'Fill in the details to create a new carrier')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <CarrierForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
