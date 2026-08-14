import { defineConfig } from "vite";

export default defineConfig({
  build: {
    outDir: "dist",
    emptyOutDir: true,
    sourcemap: true,
    lib: {
      entry: {
        "uformkit-template.element": "src/uformkit-template.element.ts",
        "uformkit-submission.element": "src/uformkit-submission.element.ts",
        "uformkit-defaults.context": "src/uformkit-defaults.context.ts",
      },
      formats: ["es"],
      fileName: (_format, entryName) => `${entryName}.js`,
    },
    rollupOptions: {
      external: [/^@umbraco-cms\//],
    },
  },
});
