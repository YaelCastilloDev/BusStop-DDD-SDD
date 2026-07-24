---
name: react-forms
description: React Hook Form with zod validation and shadcn/ui Form primitives. Use when building form components with validation.
---

# React Forms

Use **React Hook Form** with **zod v4** via `@hookform/resolvers` and **shadcn/ui `<Form>` wrapper**.

## Dependencies
```json
"react-hook-form": "^7.72.1"
"zod": "^4.3.6"
"@hookform/resolvers": "^5.2.2"
```

## shadcn/ui Form Pattern (Recommended)
The project ships a shadcn/ui `<Form>` wrapper at `@/components/ui/form.tsx` providing:
- `Form` (wraps `FormProvider`)
- `FormField` (wraps `Controller`)
- `FormItem`, `FormLabel`, `FormControl`, `FormDescription`, `FormMessage`

```tsx
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, FormField, FormItem, FormLabel, FormControl, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'

const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
})
type FormData = z.infer<typeof schema>

function MyForm() {
  const form = useForm<FormData>({
    resolver: zodResolver(schema),
  })

  function onSubmit(data: FormData) { /* ... */ }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)}>
        <FormField
          control={form.control}
          name='email'
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <button type='submit'>Submit</button>
      </form>
    </Form>
  )
}
```

## Manual State Form (Simple Cases)
For simple forms (e.g., login/register), manual `useState` + `FormEvent` is acceptable:
```tsx
const [email, setEmail] = useState('')
const handleSubmit = async (e: FormEvent) => {
  e.preventDefault()
  // ...
}
<form onSubmit={handleSubmit}>
  <input value={email} onChange={(e) => setEmail(e.target.value)} />
</form>
```

## Zod v4
- Schemas: `z.object()`, `z.string().email()`, `z.coerce.number()`.
- `z.infer<typeof schema>` for TypeScript types.
- Shareable schemas in feature-level `schemas.ts` or co-located in form file.

## Conventions
- Prefer shadcn `<Form>` wrapper for multi-field, validated forms.
- Manual `useState` is fine for 1-2 field auth forms.
- All error messages go through i18n `t()`.
- `@/` alias for all internal imports.
