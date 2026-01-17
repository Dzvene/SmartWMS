import { useNavigate } from 'react-router-dom';
import { useTranslate } from '@/hooks';
import { CONFIG } from '@/constants/routes';
import { ReasonCodeForm, type ReasonCodeFormData } from './ReasonCodeForm';
import { useCreateReasonCodeMutation } from '@/api/modules/configuration';
import type { CreateReasonCodeRequest } from '@/api/modules/configuration';

export function ReasonCodeCreate() {
  const t = useTranslate();
  const navigate = useNavigate();

  const [createReasonCode, { isLoading: isCreating }] = useCreateReasonCodeMutation();

  const handleBack = () => {
    navigate(CONFIG.REASON_CODES);
  };

  const handleSubmit = async (data: ReasonCodeFormData) => {
    try {
      const createData: CreateReasonCodeRequest = {
        code: data.code,
        name: data.name,
        description: data.description || undefined,
        reasonType: data.reasonType,
        requiresNotes: data.requiresNotes,
        sortOrder: data.sortOrder,
        isActive: data.isActive,
      };

      const result = await createReasonCode(createData).unwrap();

      // Navigate to the created reason code or back to list
      if (result.data?.id) {
        navigate(`${CONFIG.REASON_CODES}/${result.data.id}`);
      } else {
        navigate(CONFIG.REASON_CODES);
      }
    } catch (error) {
      console.error('Failed to create reason code:', error);
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
              {t('reasonCodes.createReasonCode', 'Create Reason Code')}
            </h1>
            <span className="detail-page__subtitle">
              {t('reasonCodes.createReasonCodeSubtitle', 'Fill in the details to create a new reason code')}
            </span>
          </div>
        </div>
      </header>

      <div className="detail-page__content">
        <ReasonCodeForm onSubmit={handleSubmit} loading={isCreating} isEditMode={false} />
      </div>
    </div>
  );
}
