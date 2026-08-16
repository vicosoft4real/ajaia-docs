import { render } from "@testing-library/react";
import type React from "react";
import { Provider } from "react-redux";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { setupStore } from "../app/store";

export type RenderWithAppOptions = { initialEntry?: string; routePath?: string; extraRoutes?: React.ReactNode };

export function renderWithApp(ui: React.ReactElement, options: RenderWithAppOptions = {}): ReturnType<typeof render> {
  const { routePath = "*", extraRoutes } = options;
  const initialEntry = options.initialEntry ?? (routePath === "*" || routePath.includes(":") ? "/" : routePath);
  const store = setupStore();
  return render(
    <Provider store={store}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes><Route path={routePath} element={ui} />{extraRoutes}</Routes>
      </MemoryRouter>
    </Provider>,
  );
}
