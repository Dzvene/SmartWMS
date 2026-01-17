declare const __BUILD_TIME__: string;

declare module '*.svg' {
  import type { FC, SVGProps } from 'react';
  const SVGComponent: FC<SVGProps<SVGSVGElement>>;
  export default SVGComponent;
}

declare module '*.png' {
  const src: string;
  export default src;
}

declare module '*.jpg' {
  const src: string;
  export default src;
}

declare module '*.jpeg' {
  const src: string;
  export default src;
}

declare module '*.gif' {
  const src: string;
  export default src;
}

declare module '*.webp' {
  const src: string;
  export default src;
}

declare module '*.scss' {
  const classes: { [key: string]: string };
  export default classes;
}
