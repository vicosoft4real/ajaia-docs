import { expect, type Page } from "@playwright/test";

/** Fails a journey when the browser reports an application error. */
export function monitorBrowserErrors(page: Page): () => void {
  const errors: string[] = [];

  page.on("console", (message) => {
    if (message.type() === "error") {
      errors.push(`console.error: ${message.text()}`);
    }
  });
  page.on("pageerror", (error) => errors.push(`pageerror: ${error.message}`));

  return () => expect(errors, "browser console errors").toEqual([]);
}

export async function expectNoHorizontalOverflow(page: Page): Promise<void> {
  await expect
    .poll(() =>
      page.evaluate(
        () => document.documentElement.scrollWidth <= window.innerWidth,
      ),
    )
    .toBe(true);
}

export async function expectVisibleFocus(page: Page): Promise<void> {
  const newDocument = page.getByRole("button", { name: /new document/i });
  const maximumTabs = 20;

  for (let tab = 0; tab < maximumTabs; tab += 1) {
    if (await newDocument.evaluate((element) => document.activeElement === element)) {
      break;
    }

    await page.keyboard.press("Tab");
  }

  await expect(newDocument).toBeFocused();

  const focusTreatment = await newDocument.evaluate((element) => {
    const styles = getComputedStyle(element);
    return (
      (styles.outlineStyle !== "none" && styles.outlineWidth !== "0px") ||
      styles.boxShadow !== "none"
    );
  });

  expect(focusTreatment, "New document has a visible keyboard focus treatment").toBe(true);
}
