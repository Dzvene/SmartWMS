# Architecture Decision Records (ADR)

## SmartWMS_UI - December 2025

---

## ADR-001: TanStack Table vs Custom Table Implementation

**Status:** Accepted
**Date:** 2025-12-11

### Context
Need a flexible table component for data-heavy WMS application with sorting, pagination, and row selection.

### Decision
Use TanStack Table (React Table v8) as headless UI library.

### Rationale
- Headless approach allows full styling control
- Built-in sorting, filtering, pagination
- TypeScript-first with excellent type inference
- Large community, well-maintained
- No vendor lock-in on styling

### Alternatives Considered
- MUI DataGrid - too opinionated on styling
- AG Grid - overkill for requirements, licensing concerns
- Custom implementation - too time-consuming

---

## ADR-002: FullscreenModal Pattern vs Wizard Pattern

**Status:** Accepted
**Date:** 2025-12-11

### Context
Original design used multi-step wizards for create/edit flows. Need simpler alternative.

### Decision
Replace wizards with single FullscreenModal containing tabbed or sectioned forms.

### Rationale
- Simpler mental model for users
- Easier to implement and maintain
- Better for forms that don't require strict step sequence
- Can still section content logically with tabs

### Consequences
- Multi-step validation handled differently
- Need clear section navigation within modal

---

## ADR-003: Standard WMS Terminology

**Status:** Accepted
**Date:** 2025-12-11

### Context
Application uses domain-specific terminology. Need consistent naming.

### Decision
Use industry-standard WMS terminology:
- **Product** (not Item)
- **Location** (not Bin)
- **SKU** (not StockCode)
- **FulfillmentBatch** (not Proposal)

### Rationale
- Industry-standard terms are more recognizable
- Easier onboarding for users familiar with WMS systems
- Better alignment with ERP integrations

---

## ADR-004: Feature-Based Module Structure

**Status:** Accepted
**Date:** 2025-12-11

### Context
Need scalable folder structure for growing application.

### Decision
Organize by business domain with @ prefix:
```
src/modules/
  @core/        # App shell, auth, dashboard
  @inventory/   # Stock management
  @outbound/    # Sales, picking, shipping
  @inbound/     # Purchasing, receiving
  @warehouse/   # Locations, zones, equipment
  @config/      # System configuration
  @monitoring/  # Logs, sync status
```

### Rationale
- Clear domain boundaries
- Easy to locate related code
- Supports team ownership of domains
- @ prefix groups domains in file explorer

---

## ADR-005: RTK Query for API Layer

**Status:** Accepted
**Date:** 2025-12-11

### Context
Need data fetching solution with caching, loading states, and cache invalidation.

### Decision
Use RTK Query (part of Redux Toolkit).

### Rationale
- Already using Redux Toolkit for state
- Built-in caching and automatic refetching
- Tag-based cache invalidation
- Generated hooks reduce boilerplate
- TypeScript support

### Alternatives Considered
- React Query - excellent but adds another dependency
- SWR - simpler but less integrated with Redux
- Custom hooks - too much boilerplate

---

## ADR-006: SCSS with BEM Methodology

**Status:** Accepted
**Date:** 2025-12-11

### Context
Need styling approach that works with both custom components and MUI.

### Decision
Use SCSS with BEM naming convention for custom components, MUI for complex UI elements.

### Rationale
- SCSS provides variables, mixins, nesting
- BEM ensures predictable, non-conflicting class names
- MUI handles complex interactions (date pickers, autocomplete)
- Clear separation: custom styles for layout, MUI for widgets

### Pattern
```scss
.block {
  &__element { }
  &--modifier { }
}
```

---

## ADR-007: Colocated Types Strategy

**Status:** Accepted
**Date:** 2025-12-11

### Context
Where to place TypeScript types and interfaces.

### Decision
Colocate types with their usage:
- Component props: in component file
- API types: in API module
- Shared domain types: in `src/types/`
- Module-specific types: in module folder

### Rationale
- Types are close to where they're used
- Reduces import complexity
- Clear ownership of type definitions

---

## ADR-008: react-hook-form for Form Handling

**Status:** Accepted
**Date:** 2025-12-12

### Context
Need form library for validation, error handling, and submission.

### Decision
Use react-hook-form with inline validation.

### Rationale
- Uncontrolled inputs = better performance
- Simple API with useForm hook
- Built-in validation or integrate with Zod/Yup
- Works well with custom input components

### Consequences
- Need to create form-compatible input wrappers
- Validation can be inline or schema-based

---

*Document created: December 12, 2025*
