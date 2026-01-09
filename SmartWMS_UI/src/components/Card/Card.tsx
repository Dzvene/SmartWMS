import { ReactNode } from 'react';
import MuiCard from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import CardHeader from '@mui/material/CardHeader';
import CardActions from '@mui/material/CardActions';
import Divider from '@mui/material/Divider';

export interface CardProps {
  title?: string;
  subtitle?: string;
  action?: ReactNode;
  children: ReactNode;
  footer?: ReactNode;
  elevation?: number;
  className?: string;
}

/**
 * Content card with optional header and footer sections
 */
export function Card({
  title,
  subtitle,
  action,
  children,
  footer,
  elevation = 1,
  className,
}: CardProps) {
  const hasHeader = title || subtitle || action;

  return (
    <MuiCard elevation={elevation} className={className}>
      {hasHeader && (
        <>
          <CardHeader
            title={title}
            subheader={subtitle}
            action={action}
            titleTypographyProps={{ variant: 'h6' }}
            subheaderTypographyProps={{ variant: 'body2' }}
          />
          <Divider />
        </>
      )}

      <CardContent>{children}</CardContent>

      {footer && (
        <>
          <Divider />
          <CardActions sx={{ justifyContent: 'flex-end', p: 2 }}>
            {footer}
          </CardActions>
        </>
      )}
    </MuiCard>
  );
}
