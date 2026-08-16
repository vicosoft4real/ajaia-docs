import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { sessionUser } from "../../mocks/fixtures";
import { renderWithApp } from "../../test/renderWithApp";
import { MobileNavDrawer } from "./MobileNavDrawer";

describe("MobileNavDrawer", () => {
  it("traps focus, closes on Escape, and restores focus to its trigger", async () => {
    const user = userEvent.setup(); renderWithApp(<MobileNavDrawer user={sessionUser} switching={false} onSwitchUser={vi.fn()} />);
    const trigger = screen.getByRole("button", { name: /open navigation/i }); await user.click(trigger);
    expect(screen.getByRole("navigation", { name: /mobile workspace/i })).toBeVisible();
    await user.keyboard("{Escape}"); expect(screen.queryByRole("navigation", { name: /mobile workspace/i })).not.toBeInTheDocument(); await waitFor(() => expect(trigger).toHaveFocus());
  });
});
