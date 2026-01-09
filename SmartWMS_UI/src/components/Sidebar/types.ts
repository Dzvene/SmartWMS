import { ReactNode } from 'react';

export interface NavItem {
  id: string;
  label: string;
  path: string;
  icon?: ReactNode;
  children?: NavItem[];
}

export interface NavGroup {
  id: string;
  label: string;
  items: NavItem[];
}

export interface SidebarProps {
  collapsed?: boolean;
  onToggle?: () => void;
}
