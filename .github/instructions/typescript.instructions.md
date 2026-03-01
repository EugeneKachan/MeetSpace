---
description: TypeScript coding standards — access modifiers, types, return types
applyTo: "UI/src/app/**/*.ts"
---

## TypeScript Coding Standards

These rules apply to all TypeScript files in the Angular project.

## 1. Access Modifiers

Always declare an explicit access modifier on every class member (property, getter, method):

- `public` — accessible from templates or externally
- `private` — internal implementation detail (not used in templates or subclasses)
- `protected` — accessible in subclasses only
- **Never** omit the modifier and rely on the implicit `public` default

```typescript
// ✅ correct
public isLoading: boolean = false;
private readonly apiUrl: string = '...';

// ❌ wrong — no modifier
isLoading = false;
```

## 2. Explicit Variable Types

Always annotate the type of class properties, even when it can be inferred:

```typescript
// ✅ correct
public errorMessage: string | null = null;
public readonly roles: string[] = USER_ROLES;
private readonly dataSource: MatTableDataSource<User> = new MatTableDataSource<User>();

// ❌ wrong — type omitted
errorMessage = null;
roles = USER_ROLES;
```

`const` / `let` locals inside method bodies may rely on inference when the type is obvious.

## 3. Return Types on Methods and Getters

Always annotate the return type of every method and getter:

```typescript
// ✅ correct
public onSubmit(): void { ... }
public get email(): AbstractControl { return this.form.get('email')!; }
private handleTokenResponse(response: TokenResponse): void { ... }

// ❌ wrong — return type omitted
onSubmit() { ... }
get email() { return this.form.get('email')!; }
```

## 4. `readonly` Where Applicable

Mark properties `readonly` when they are only ever assigned once (constructor or initializer):

```typescript
// ✅ correct
public readonly displayedColumns: string[] = ['name', 'email'];
private readonly jwtHelper: JwtHelperService = new JwtHelperService();
```

## 5. Implements Clause

Declare every lifecycle interface your class uses:

```typescript
// ✅ correct
export class MyComponent implements OnInit, AfterViewInit { ... }
```
