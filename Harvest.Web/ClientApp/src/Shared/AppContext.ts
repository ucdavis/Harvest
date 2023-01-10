import React from "react";
import { AppContextShape, User } from "../types";

const AppContext = React.createContext<AppContextShape>({
  antiForgeryToken: "",
  user: { detail: {} as User, roles: [] },
  usecoa: false,
});

export default AppContext;
