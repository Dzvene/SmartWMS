/// <reference types="vite/client" />

declare module '*.svg' {
  import type { FC, SVGProps } from 'react';
  const SVGComponent: FC<SVGProps<SVGSVGElement>>;
  export default SVGComponent;
}

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_API_URL: string;
  readonly VITE_APP_NAME: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
