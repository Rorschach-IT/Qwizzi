# React + TypeScript + Vite

This template provides a minimal setup to get React working in Vite with HMR and some Oxlint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs).
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/).

## Environment setup

### Prerequisites

Install Node.js with npm.

Use the Node.js version specified by the project, for example in `.nvmrc` or the `engines` field in `package.json`. If no version is specified, choose a supported LTS release compatible with the project's Vite version.

Verify the installation:

```bash
node --version
npm --version
```

### Install dependencies

For a downloaded or cloned project with an existing lockfile, run:

```bash
npm ci
```

This command:

- Installs the dependencies recorded in `package-lock.json`.
- Includes development dependencies under the default development installation settings.
- Removes an existing `node_modules` directory before installation.
- Does not modify `package.json` or `package-lock.json`.
- Reports an error if the dependency declarations and lockfile are inconsistent.

If the lockfile is missing, or you intentionally need to update it after changing dependencies, use:

```bash
npm install
```

If `npm ci` reports that `package.json` and `package-lock.json` are inconsistent, check whether you have the correct versions of both files. Use `npm install` only when updating the lockfile is intended.

### Is npm init required?

Do not run `npm init` when setting up this existing application.

`npm init` initializes package metadata. It does not restore the project's dependencies or original scripts.

If `package.json` is missing, restore it from the repository or original archive. An existing `package-lock.json` does not replace the project's `package.json`.

## Running the application

Start the development server:

```bash
npm run dev
```

This requires a `dev` script in `package.json` that starts Vite, for example:

```json
{
  "scripts": {
    "dev": "vite"
  }
}
```

This example shows only the relevant script. Do not replace the entire existing `package.json` with this snippet.

Open the local address printed in the terminal, typically:

```text
http://localhost:5173
```

If the default port is already in use, Vite normally tries the next available port.

Keep the terminal running while working on the application. Changes to source files are reflected through Vite's development tooling and Hot Module Replacement where supported.

To stop the development server, press:

```text
Ctrl+C
```

## Development server options

When the `dev` script starts Vite, pass additional Vite options after `--`:

```bash
npm run dev -- <options>
```

The `--` separator forwards the following arguments to the command defined in the `dev` script.

### Open the browser automatically

```bash
npm run dev -- --open
```

### Use a custom port

```bash
npm run dev -- --port 3000
```

If port `3000` is occupied, Vite normally tries the next available port.

### Require a specific port

```bash
npm run dev -- --port 3000 --strictPort
```

With `--strictPort`, Vite exits if the requested port is unavailable.

### Access the application from another device

```bash
npm run dev -- --host 0.0.0.0
```

This makes the development server listen on all network interfaces.

On another device on the same trusted network, open the development computer's LAN address, for example:

```text
http://192.168.1.100:5173
```

Replace the example IP address and port with the actual values.

Access may require a firewall rule allowing connections to the selected port.

Use this option only on trusted networks. Do not expose the development server directly to the internet. The Vite development server is not a production hosting solution.

### Use a custom environment mode

```bash
npm run dev -- --mode staging
```

This starts the development server in `staging` mode and allows Vite to load mode-specific configuration from files such as:

```text
.env.staging
.env.staging.local
```

Changing the mode does not turn the development server into a production server.

Do not put secrets in client-exposed environment variables. Variables prefixed with `VITE_` can be exposed to browser code.

### Rebuild the dependency optimizer cache

```bash
npm run dev -- --force
```

Use this when troubleshooting dependency optimization or stale cached dependencies.

### Enable debug logging

```bash
npm run dev -- --debug
```

### Display available options

```bash
npm run dev -- --help
```

The available options depend on the installed Vite version.

### Combine multiple options

Start the application on port `3000`, require that exact port, and open the browser:

```bash
npm run dev -- --port 3000 --strictPort --open
```

Allow access from other devices on the same trusted network using port `3000`:

```bash
npm run dev -- --host 0.0.0.0 --port 3000 --strictPort
```

### Options overview

| Option           | Purpose                                          |
| ---------------- | ------------------------------------------------ |
| `--open`         | Automatically open the application in a browser. |
| `--port 3000`    | Request port `3000`.                             |
| `--strictPort`   | Exit if the requested port is unavailable.       |
| `--host 0.0.0.0` | Listen on all network interfaces.                |
| `--mode staging` | Use the `staging` environment mode.              |
| `--force`        | Rebuild the dependency optimizer cache.          |
| `--debug`        | Enable debug logging.                            |
| `--help`         | Display available CLI options.                   |

## Quick start

From the application directory:

```bash
npm ci
npm run dev
```

For a custom port and automatic browser launch:

```bash
npm run dev -- --port 3000 --open
```

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev and build performance.

To add it, see the [React Compiler installation documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the Oxlint configuration

If you are developing a production application, we recommend enabling type-aware lint rules by installing `oxlint-tsgolint`:

```bash
npm install --save-dev oxlint-tsgolint
```

Then update `.oxlintrc.json`:

```json
{
  "$schema": "./node_modules/oxlint/configuration_schema.json",
  "plugins": ["react", "typescript", "oxc"],
  "options": {
    "typeAware": true
  },
  "rules": {
    "react/rules-of-hooks": "error",
    "react/only-export-components": ["warn", { "allowConstantExport": true }]
  }
}
```

See the [Oxlint rules documentation](https://oxc.rs/docs/guide/usage/linter/rules) for the full list of rules and categories.
