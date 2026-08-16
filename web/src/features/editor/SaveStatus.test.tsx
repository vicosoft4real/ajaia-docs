import { render, screen } from "@testing-library/react";
import { expect, it } from "vitest";
import { SaveStatus } from "./SaveStatus";

it.each([
  ["saved", "Saved"], ["saving", "Saving…"], ["changes-not-saved", "Changes not saved"], ["conflict", "Resolve conflict"],
] as const)("announces %s state", (state, label) => {
  render(<SaveStatus state={state} />);
  expect(screen.getByRole("status")).toHaveTextContent(label);
});
