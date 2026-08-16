import { expect, test, type Page } from "@playwright/test";
import { mkdir } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import {
  expectNoHorizontalOverflow,
  expectVisibleFocus,
  monitorBrowserErrors,
} from "./browserDiagnostics";

const screenshotsDirectory = resolve(
  dirname(fileURLToPath(import.meta.url)),
  "../../docs/screenshots",
);

let assertNoBrowserErrors: () => void;

test.beforeEach(async ({ page }) => {
  assertNoBrowserErrors = monitorBrowserErrors(page);
  await mkdir(screenshotsDirectory, { recursive: true });
});

test.afterEach(() => assertNoBrowserErrors());

async function loginAsAmina(page: Page): Promise<void> {
  await page.goto("/login");
  await page
    .getByRole("button", { name: /continue as amina okafor/i })
    .click();
  await expect(page).toHaveURL(/\/documents/);
}

async function captureLibrary(page: Page, filename: string): Promise<void> {
  await expectNoHorizontalOverflow(page);
  await expectVisibleFocus(page);
  await page.screenshot({
    path: resolve(screenshotsDirectory, filename),
    fullPage: false,
  });
}

async function captureEditor(page: Page, filename: string): Promise<void> {
  await expectNoHorizontalOverflow(page);
  const editor = page.getByRole("textbox", { name: /document content/i });
  await editor.focus();
  await expect(editor).toBeFocused();
  await page.screenshot({
    path: resolve(screenshotsDirectory, filename),
    fullPage: false,
  });
}

test("captures desktop and mobile library and editor evidence", async ({ page }) => {
  await loginAsAmina(page);

  await page.setViewportSize({ width: 1440, height: 1000 });
  await captureLibrary(page, "desktop-library.png");

  await page.getByRole("button", { name: /new document/i }).click();
  await page
    .getByRole("textbox", { name: /document title/i })
    .fill("Visual review brief");
  await page
    .getByRole("textbox", { name: /document content/i })
    .fill("A compact release review document.");
  await expect(page.getByText("Saved", { exact: true })).toBeVisible({
    timeout: 15_000,
  });
  await captureEditor(page, "desktop-editor.png");

  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto("/documents");
  await captureLibrary(page, "mobile-library.png");

  await page.getByRole("link", { name: /visual review brief/i }).click();
  await captureEditor(page, "mobile-editor.png");
});
