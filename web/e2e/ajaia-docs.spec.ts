import { expect, test, type Locator, type Page } from "@playwright/test";
import { fileURLToPath } from "node:url";
import { monitorBrowserErrors } from "./browserDiagnostics";

const fixturePath = fileURLToPath(
  new URL("./fixtures/reviewer-brief.md", import.meta.url),
);

let assertNoBrowserErrors: () => void = () => {};

test.beforeEach(async ({ page }) => {
  assertNoBrowserErrors = monitorBrowserErrors(page);
});

test.afterEach(() => {
  try {
    assertNoBrowserErrors();
  } finally {
    assertNoBrowserErrors = () => {};
  }
});

async function loginAsAmina(page: Page): Promise<void> {
  await page.goto("/login");
  await page
    .getByRole("button", { name: /continue as amina okafor/i })
    .click();
  await expect(page).toHaveURL(/\/documents/);
}

async function expectSaved(page: Page): Promise<void> {
  await expect(page.getByText("Saved", { exact: true })).toBeVisible({
    timeout: 15_000,
  });
}

async function selectAll(editor: Locator): Promise<void> {
  const modifier = process.platform === "darwin" ? "Meta" : "Control";
  await editor.press(`${modifier}+A`);
}

test("owner creates, formats, shares, and collaborator safely edits", async ({
  page,
}) => {
  await loginAsAmina(page);
  await page.getByRole("button", { name: /new document/i }).click();

  const title = page.getByRole("textbox", { name: /document title/i });
  const editor = page.getByRole("textbox", { name: /document content/i });
  await title.fill("Launch brief");
  await editor.fill("Release plan");

  for (const name of [
    /bold/i,
    /italic/i,
    /underline/i,
    /heading 1|h1/i,
    /heading 2|h2/i,
    /bulleted list|bullets/i,
    /numbered list|numbers/i,
  ]) {
    await selectAll(editor);
    await page.getByRole("button", { name }).click();
  }

  await expectSaved(page);
  await page.reload();
  await expect(editor).toContainText("Release plan");

  await page.getByRole("button", { name: /^share$/i }).click();
  const shareDialog = page.getByRole("dialog", {
    name: /share this document/i,
  });
  await shareDialog.getByRole("button", { name: /share with chidi/i }).click();
  await expect(shareDialog.getByText(/chidi okeke has access/i)).toBeVisible();
  await shareDialog.getByRole("button", { name: /close dialog/i }).click();
  await expect(shareDialog).toHaveCount(0);

  await page.getByRole("button", { name: /switch user/i }).click();
  await page
    .getByRole("button", { name: /continue as chidi okeke/i })
    .click();
  await page.getByRole("tab", { name: /shared with me/i }).click();
  await page
    .getByRole("button", { name: /open launch brief/i })
    .first()
    .click();

  await expect(page.getByRole("button", { name: /^share$/i })).toHaveCount(0);
  await expect(
    page.getByRole("textbox", { name: /document title/i }),
  ).toHaveJSProperty("readOnly", true);
  await expect(page.getByRole("button", { name: /delete/i })).toHaveCount(0);

  const collaboratorEditor = page.getByRole("textbox", {
    name: /document content/i,
  });
  await collaboratorEditor.fill("Collaborator release update");
  await expectSaved(page);
  await page.reload();
  await expect(collaboratorEditor).toContainText("Collaborator release update");
});

test("imports Markdown and preserves normalized formatting after an edit", async ({
  page,
}) => {
  await loginAsAmina(page);
  await page.getByRole("button", { name: /import/i }).first().click();
  const importDialog = page.getByRole("dialog", { name: /import a document/i });
  await importDialog.getByLabel("Document file").setInputFiles(fixturePath);
  await importDialog
    .getByRole("button", { name: /import document/i })
    .click();

  await expect(
    page.getByRole("textbox", { name: /document title/i }),
  ).toHaveValue("reviewer-brief");
  await expect(
    page.getByRole("heading", { name: "Launch review brief" }),
  ).toBeVisible();
  await expect(
    page
      .getByRole("textbox", { name: /document content/i })
      .locator("li")
      .filter({ hasText: "Verify the first release journey." }),
  ).toBeVisible();

  const editor = page.getByRole("textbox", { name: /document content/i });
  await editor.press("End");
  await editor.press("Enter");
  await editor.type("Normalization confirmed.");
  await expectSaved(page);
  await page.reload();

  await expect(
    page.getByRole("heading", { name: "Launch review brief" }),
  ).toBeVisible();
  await expect(
    editor.locator("li").filter({
      hasText: "Confirm that imported Markdown becomes an editable document.",
    }),
  ).toBeVisible();
  await expect(editor).toContainText("Normalization confirmed.");
});
