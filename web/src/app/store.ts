import { configureStore } from "@reduxjs/toolkit";
import sessionReducer from "../features/auth/sessionSlice";
import { ajaiaApi } from "../store/api/ajaiaApi";

export const setupStore = () => configureStore({
  reducer: { session: sessionReducer, [ajaiaApi.reducerPath]: ajaiaApi.reducer },
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(ajaiaApi.middleware),
});

export const store = setupStore();
export type AppStore = ReturnType<typeof setupStore>;
export type RootState = ReturnType<AppStore["getState"]>;
export type AppDispatch = AppStore["dispatch"];
