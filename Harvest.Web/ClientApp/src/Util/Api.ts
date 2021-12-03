import { AppContextShape } from "../types";

declare var Harvest: AppContextShape;

export const authenticatedFetch = async (
  url: string,
  init?: RequestInit
): Promise<any> =>
  fetch(url, {
    ...init,
    credentials: "include",
    headers: [
      ["Accept", "application/json"],
      ["Content-Type", "application/json"],
      ["RequestVerificationToken", Harvest.antiForgeryToken],
    ],
  });
