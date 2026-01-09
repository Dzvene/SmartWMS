import { useState, useMemo } from 'react';
import { useIntl } from 'react-intl';
import type { ColumnDef } from '@tanstack/react-table';

import { DataTable } from '@/components/DataTable';
import { FullscreenModal } from '@/components/FullscreenModal';
import { useGetReasonCodesQuery } from '@/api/modules/configuration';
import type { ReasonCodeDto, ReasonCodeType } from '@/api/modules/configuration';

const categoryLabels: Record<ReasonCodeType, string> = {
  Adjustment: 'Adjustment',
  Return: 'Return',
  Damage: 'Damage',
  Expiry: 'Expiry',
  QualityHold: 'Quality Hold',
  Transfer: 'Transfer',
  Scrap: 'Scrap',
  Found: 'Found',
  Lost: 'Lost',
  Other: 'Other',
};

/**
 * Reason Codes Configuration Module
 *
 * Manages reason codes for inventory adjustments and returns.
 */
export function ReasonCodes() {
  const { formatMessage } = useIntl();
  const t = (id: string) => formatMessage({ id });

  const [selectedCode, setSelectedCode] = useState<ReasonCodeDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [paginationState, setPaginationState] = useState({ page: 1, pageSize: 25 });

  const { data, isLoading } = useGetReasonCodesQuery({
    page: paginationState.page,
    pageSize: paginationState.pageSize,
  });

  const reasonCodes = data?.data?.items ?? [];
  const totalRows = data?.data?.totalCount ?? 0;

  const columns = useMemo<ColumnDef<ReasonCodeDto, unknown>[]>(
    () => [
      {
        accessorKey: 'code',
        header: t('reasonCodes.code'),
        size: 100,
        cell: ({ getValue }) => (
          <span className="code">{getValue() as string}</span>
        ),
      },
      {
        accessorKey: 'name',
        header: t('common.name'),
        size: 160,
      },
      {
        accessorKey: 'description',
        header: t('reasonCodes.description'),
        size: 200,
        cell: ({ getValue }) => getValue() || '—',
      },
      {
        accessorKey: 'reasonType',
        header: t('reasonCodes.category'),
        size: 120,
        cell: ({ getValue }) => {
          const type = getValue() as ReasonCodeType;
          return (
            <span className={`tag tag--${type.toLowerCase()}`}>
              {categoryLabels[type]}
            </span>
          );
        },
      },
      {
        accessorKey: 'requiresNotes',
        header: 'Requires Note',
        size: 110,
        cell: ({ getValue }) => (getValue() ? 'Yes' : 'No'),
      },
      {
        accessorKey: 'isActive',
        header: t('common.status'),
        size: 80,
        cell: ({ getValue }) => (
          <span className={`status-badge status-badge--${getValue() ? 'active' : 'inactive'}`}>
            {getValue() ? t('status.active') : t('status.inactive')}
          </span>
        ),
      },
    ],
    [t]
  );

  const handleRowClick = (code: ReasonCodeDto) => {
    setSelectedCode(code);
    setIsModalOpen(true);
  };

  const handleAddNew = () => {
    setSelectedCode(null);
    setIsModalOpen(true);
  };

  return (
    <div className="page">
      <div className="page__header">
        <h1 className="page__title">{t('reasonCodes.title')}</h1>
        <div className="page__actions">
          <button className="btn btn--primary" onClick={handleAddNew}>
            {t('reasonCodes.addCode')}
          </button>
        </div>
      </div>

      <div className="page__content">
        <DataTable
          data={reasonCodes}
          columns={columns}
          pagination={{
            pageIndex: paginationState.page - 1,
            pageSize: paginationState.pageSize,
          }}
          onPaginationChange={({ pageIndex, pageSize }) => {
            setPaginationState({ page: pageIndex + 1, pageSize });
          }}
          totalRows={totalRows}
          onRowClick={handleRowClick}
          selectedRowId={selectedCode?.id}
          getRowId={(row) => row.id}
          loading={isLoading}
          emptyMessage={t('common.noData')}
        />
      </div>

      <FullscreenModal
        open={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={selectedCode ? 'Edit Reason Code' : 'Add Reason Code'}
      >
        <div className="form">
          <p>Reason code form will be implemented here.</p>
        </div>
      </FullscreenModal>
    </div>
  );
}

export default ReasonCodes;
