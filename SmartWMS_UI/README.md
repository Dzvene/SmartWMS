# SmartWMS UI

Modern Warehouse Management System - Web User Interface

## Overview

SmartWMS is a comprehensive warehouse management solution designed for modern logistics operations. This repository contains the React-based frontend application.

See [ARCHITECTURE_DECISION_RECORD.md](./ARCHITECTURE_DECISION_RECORD.md) for detailed architecture decisions and technology choices.

## Technology Stack

| Category       | Technology                              |
|----------------|----------------------------------------|
| Framework      | React 18 + TypeScript                  |
| Build Tool     | Vite 6                                 |
| State          | Redux Toolkit + RTK Query              |
| UI Library     | MUI v7                                 |
| Forms          | React Hook Form + Yup                  |
| Routing        | React Router v6                        |
| i18n           | react-intl                             |
| Styling        | SCSS + MUI theming                     |
| Date/Time      | Luxon                                  |
| DnD            | @hello-pangea/dnd                      |

## Getting Started

### Prerequisites

- Node.js 20+
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Start development server
npm run dev
```

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint with auto-fix
- `npm run format` - Format code with Prettier
- `npm test` - Run tests

## Project Structure

```
src/
├── api/                    # RTK Query API definitions
│   └── modules/            # Domain-specific API slices
├── assets/                 # Static assets (icons, images)
├── components/             # Reusable UI components
├── constants/              # App-wide constants, routes
├── hooks/                  # Custom React hooks
├── localization/           # i18n translations
├── models/                 # Shared TypeScript models
├── modules/                # Feature modules
│   ├── @core/              # App shell, auth, dashboard
│   ├── @configuration/     # System settings
│   ├── @inventory/         # Stock management
│   └── @operations/        # Order processing
├── store/                  # Redux store configuration
├── styles/                 # Global SCSS styles
├── types/                  # TypeScript type definitions
└── utils/                  # Utility functions
```

## License

Proprietary - All rights reserved
